namespace PskOnline.Server.Shared.Multitenancy
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Security.Claims;
  using System.Threading.Tasks;

  public static class UserExtension
  {
    /// <summary>
    /// Returns a Guid by retrieving a 'TenantId' claim and converting 
    /// the string to a Guid. If there is none, or if the string is not
    /// convertible to Guid, returns null.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static Guid? GetUserTenant(this ClaimsPrincipal user)
    {
      var tenantClaim = CustomClaimTypes.TenantId;
      var tenantIdString = user.FindFirst(tenantClaim)?.Value?.Trim();
      try
      {
        return Guid.Parse(tenantIdString);
      }
      catch( Exception )
      {
        return null;
      }
    }

    public static void AddUserTenantAndOrgStructureClaims(this ClaimsPrincipal user, Guid tenantId, Guid? branchOfficeId, Guid? departmentId)
    {
      user.AddUserTenantAndOrgStructureClaims(tenantId.ToString(), branchOfficeId?.ToString(), departmentId?.ToString());
    }

    public static void AddUserTenantAndOrgStructureClaims(this ClaimsPrincipal user, string tenantId, string branchOfficeId, string departmentId)
    {
      var claims = new List<Claim>()
      {
        new Claim(CustomClaimTypes.TenantId, tenantId.ToString())
      };
      if (!string.IsNullOrEmpty(branchOfficeId))
      {
        claims.Add(new Claim(CustomClaimTypes.BranchOfficeId, branchOfficeId));
      }
      if (!string.IsNullOrEmpty(departmentId))
      {
        claims.Add(new Claim(CustomClaimTypes.DepartmentId, departmentId));
      }
      user.Identities.First().AddClaims(claims);
    }

  }
}
