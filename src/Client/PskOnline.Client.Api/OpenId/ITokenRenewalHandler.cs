namespace PskOnline.Client.Api.OpenId
{
  using System;
  using System.IdentityModel.Tokens.Jwt;
  using System.Threading.Tasks;

  public interface ITokenRenewalHandler
  {
    /// <summary>
    /// returns the current id_token
    /// </summary>
    JwtSecurityToken GetIdToken();

    /// <summary>
    /// returns all tokens
    /// </summary>
    /// <returns></returns>
    TokenHolder GetTokens();

    /// <summary>
    /// Refreshes the current access token if its validity period has expired
    /// and attaches the updated access token as the default Authorization header
    /// Upon failure, throws 'AuthenticationException'
    /// </summary>
    Task RefereshAuthenticationIfNeededAsync();

    /// <summary>
    /// Refreshes the current access token
    /// and attaches the updated access token as the default Authorization header.
    /// Upon failure, throws 'AuthenticationException'
    /// </summary>
    Task RefereshAuthenticationAsync();

    /// <summary>
    /// This event is fired after the renewal handler sucesfully refreshes
    /// the access token. Use this event to get notified about a client 
    /// receiving an updated token, so that the factory could create new 
    /// instances of the ApiClient with the most up-to-date tokens, at the 
    /// same time reducing the number of requests to the server to refresh
    /// a token or issue a new token based on credentials.
    /// </summary>
    event Action<TokenHolder> TokenUpdated;

    /// <summary>
    /// Returns true if the last received access token has expired and false otherwise
    /// </summary>
    /// <returns></returns>
    bool IsSessionsExpired();
  }
}
