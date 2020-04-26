namespace PskOnline.Server.Authority
{
  using System;
  using System.Linq;
  using System.Security.Claims;

  using AspNet.Security.OpenIdConnect.Primitives;

  public static class ClaimsPrincipalExtensions
  {
    public static string GetUserId(this ClaimsPrincipal user)
    {
      return user.FindFirst(OpenIdConnectConstants.Claims.Subject)?.Value?.Trim();
    }

    public static string[] GetRoles(ClaimsPrincipal identity)
    {
      return identity.Claims
          .Where(c => c.Type == OpenIdConnectConstants.Claims.Role)
          .Select(c => c.Value)
          .ToArray();
    }

  }
}
