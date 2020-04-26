namespace PskOnline.Client.Api.OpenId
{
  using System.IdentityModel.Tokens.Jwt;
  using System.Threading.Tasks;

  public interface IAuthenticationHandler
  {
    /// <summary>
    /// Authenticates the client
    /// Upon failure, throws 'BadRequestException'
    /// </summary>
    Task<TokenHolder> AuthenticateAsync();
  }
}
