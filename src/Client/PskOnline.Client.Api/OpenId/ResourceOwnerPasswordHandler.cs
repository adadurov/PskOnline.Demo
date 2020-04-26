namespace PskOnline.Client.Api.OpenId
{
  using System.Collections.Generic;
  using System.Net.Http;
  using System.Security;
  using System.Threading.Tasks;

  public class ResourceOwnerPasswordHandler : IAuthenticationHandler
  {
    private readonly HttpClient _httpClient;
    private readonly SecureString _username;
    private readonly SecureString _password;

    public ResourceOwnerPasswordHandler(HttpClient httpClient, SecureString username, SecureString password)
    {
      _httpClient = httpClient;
      _username = username;
      _password = password;
    }

    public async Task<TokenHolder> AuthenticateAsync()
    {
      var scopes = "email phone profile roles";
      var values = new Dictionary<string, string>
      {
        ["grant_type"] = "password",
        ["username"] = _username.ToInsecureString(),
        ["password"] = _password.ToInsecureString(),
        ["scope"] = "openid offline_access " + scopes
      };
      var tokenInfo = await OpenIdUtils.ExecuteTokenRequest(_httpClient, values);
      return new TokenHolder(tokenInfo, scopes);
    }
  }
}
