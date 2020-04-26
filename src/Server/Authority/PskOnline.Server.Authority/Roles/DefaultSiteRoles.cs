namespace PskOnline.Server.Authority.Roles
{
  using System.Collections.Generic;

  using PskOnline.Server.Shared.Roles;
  using PskOnline.Server.Shared.Permissions.Standard;

  public static class DefaultSiteRoles
  {
    public static RoleSpecification GetBuiltInAdminRoleDefinition()
    {
      return new RoleSpecification(
              "FE2C22A8-7F59-443F-8005-2EEDC2812D74",
              "Site Admin",
              "Default Site Administrator",
              CoreAppPermissions.AllPermissions
              );
    }

    public static IReadOnlyCollection<RoleSpecification> GetStandardSiteRoles()
    {
      var roles = new List<RoleSpecification> {
        GetBuiltInAdminRoleDefinition(),
      };
      return roles;
    }
  }

}
