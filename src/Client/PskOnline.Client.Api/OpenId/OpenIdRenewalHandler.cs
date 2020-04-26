namespace PskOnline.Client.Api.OpenId
{
  using Microsoft.Extensions.Logging;
  using System;
  using System.Collections.Generic;
  using System.IdentityModel.Tokens.Jwt;
  using System.Net.Http;
  using System.Net.Http.Headers;
  using System.Security;
  using System.Threading.Tasks;

  public sealed class OpenIdRenewalHandler : ITokenRenewalHandler
  {
    private TokenHolder _tokenHolder;
    private readonly HttpClient _httpClient;
    private readonly IAuthenticationHandler _authHandler;
    private readonly ILogger _logger;

    public OpenIdRenewalHandler(
      HttpClient httpClient, 
      TokenHolder tokens, 
      IAuthenticationHandler authenticationHandler,
      ILogger logger)
    {
      _authHandler = authenticationHandler;
      _httpClient = httpClient;
      _logger = logger;
      SetTokens(tokens);
    }

    public TokenHolder GetTokens()
    {
      return _tokenHolder;
    }

    /// <summary>
    /// This event is fired after the renewal handler sucesfully refreshes
    /// the access token. Use this event to get notified about a client 
    /// receiving an updated token, so that the factory could create new 
    /// instances of the ApiClient with the most up-to-date tokens, at the 
    /// same time reducing the number of requests to the server to refresh
    /// a token or issue a new token based on credentials.
    /// </summary>
    public event Action<TokenHolder> TokenUpdated;

    private void SetTokens(TokenHolder tokens)
    {
      _tokenHolder = tokens;
      // use the provided access token from now on
      _httpClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", _tokenHolder.AccessToken.ToInsecureString());
    }

    public JwtSecurityToken GetIdToken()
    {
      if (_tokenHolder != null)
      {
        var handler = new JwtSecurityTokenHandler();
        return handler.ReadToken(_tokenHolder.IdToken) as JwtSecurityToken;
      }
      return null;
    }

    public bool IsSessionsExpired()
    {
      return DateTime.UtcNow > _tokenHolder.NotValidAfterUtc;
    }

    public async Task RefereshAuthenticationIfNeededAsync()
    {
      if (IsSessionsExpired())
      {
        _logger.LogInformation("Refreshing access token as the current token has expired (based on its lifetime, as advised by the server)");
        await RefereshAuthenticationAsync();
      }
    }

    public async Task RefereshAuthenticationAsync()
    {
      _httpClient.DefaultRequestHeaders.Remove("Authorization");

      if (!string.IsNullOrEmpty(_tokenHolder.RefreshToken.ToInsecureString()))
      {
        try
        {
          _logger.LogInformation($"Getting a new access token using refresh token");
          var scopes = _tokenHolder.Scopes;
          var tokenInfoResponse = await RefreshOpenIdTokenAsync(_tokenHolder.RefreshToken, scopes);
          _logger.LogInformation($"Obtained a new access token using refresh token");
          var tokens = new TokenHolder(tokenInfoResponse, scopes);
          SetTokens(tokens);

          // fire the 'token updated' event
          TokenUpdated?.Invoke(tokens);

          return;
        }
        catch (Exception ex)
        {
          _logger.LogInformation(ex, "Could not refersh access token using the refresh_token");
        }
      }

      // authenticate again (using credentials or whatever the handler can do for us)
      if (_authHandler != null)
      {
        _logger.LogInformation($"Getting a new access token using {_authHandler.GetType()}");
        var tokens = await _authHandler.AuthenticateAsync();
        _logger.LogInformation($"Obtained a new access token using {_authHandler.GetType()}");
        SetTokens(tokens);

        // fire the 'token updated' event
        TokenUpdated?.Invoke(tokens);
      }
      else
      {
        throw new AuthenticationException(
          "Unable to refresh access token and there is no way to re-authenticate.");
      }
    }

    private async Task<TokenInfoResponse> RefreshOpenIdTokenAsync(SecureString refreshToken, string scopes)
    {
      var values = new Dictionary<string, string>
      {
        ["refresh_token"] = refreshToken.ToInsecureString(),
        ["grant_type"] = "refresh_token",
        ["scope"] = "openid offlince_access " + scopes
        //["resource"] = "http://localhost:63333"
      };
      return await ExecuteTokenRequest(values);
    }

    protected async Task<TokenInfoResponse> ExecuteTokenRequest(Dictionary<string, string> values)
    {
      var request = new HttpRequestMessage
      {
        Method = HttpMethod.Post,
        RequestUri = new Uri(_httpClient.BaseAddress.AbsoluteUri + "connect/token"),
        Content = new FormUrlEncodedContent(values)
      };

      var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
      if (!response.IsSuccessStatusCode)
      {
        throw await ExceptionHelper.CreateExceptionAsync(response);
      }

      try
      {
        var payload = await response.Content.ReadAsJsonAsync<TokenInfoResponse>();
        return payload;
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException("Could not parse a successful authentication response.", ex);
      }
    }
  }
}
