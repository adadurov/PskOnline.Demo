namespace PskOnline.Client.Api.OpenId
{
  using PskOnline.Client.Api.Authority;
  using System.IdentityModel.Tokens.Jwt;
  using System.Linq;

  public static class JwtSecurityTokenExtensions
  {
    public static string GetDepartmentIdClaimValue(this JwtSecurityToken token)
    {
      var claim = token.Claims.FirstOrDefault(c => c.Type == CustomClaimTypes.DepartmentId);
      if( claim != null )
      {
        return claim.Value;
      }
      return null;
    }

    public static string GetBranchOfficeIdClaimValue(this JwtSecurityToken token)
    {
      var claim = token.Claims.FirstOrDefault(c => c.Type == CustomClaimTypes.BranchOfficeId);
      if (claim != null)
      {
        return claim.Value;
      }
      return null;
    }

    public static string GetTenantIdClaimValue(this JwtSecurityToken token)
    {
      var claim = token.Claims.FirstOrDefault(c => c.Type == CustomClaimTypes.TenantId);
      if (claim != null)
      {
        return claim.Value;
      }
      return null;
    }
  }
}
