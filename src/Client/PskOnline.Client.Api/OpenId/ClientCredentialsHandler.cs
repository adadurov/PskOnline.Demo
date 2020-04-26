namespace PskOnline.Client.Api.OpenId
{
  using System.Collections.Generic;
  using System.Net.Http;
  using System.Security;
  using System.Threading.Tasks;

  public class ClientCredentialsHandler : IAuthenticationHandler
  {
    private readonly HttpClient _httpClient;
    private readonly SecureString _clientId;
    private readonly SecureString _clientSecret;
    private readonly string _applicationScopes;

    public ClientCredentialsHandler(
      HttpClient httpClient, SecureString clientId, SecureString clientSecret, string applicationScopes)
    {
      _httpClient = httpClient;
      _clientId = clientId;
      _clientSecret = clientSecret;
      _applicationScopes = applicationScopes;
    }

    public async Task<TokenHolder> AuthenticateAsync()
    {
      var values = new Dictionary<string, string>
      {
        ["grant_type"] = "client_credentials",
        ["client_id"] = _clientId.ToInsecureString(),
        ["client_secret"] = _clientSecret.ToInsecureString(),
        ["scope"] = "openid " + _applicationScopes
      };
      var tokenInfo = await OpenIdUtils.ExecuteTokenRequest(_httpClient, values);
      return new TokenHolder(tokenInfo, _applicationScopes);
    }

    public string GetScopes()
    {
      return _applicationScopes;
    }
  }
}
