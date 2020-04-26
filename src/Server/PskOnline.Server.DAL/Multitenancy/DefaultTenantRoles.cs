namespace PskOnline.Server.DAL.Multitenancy
{
  using System.Collections.Generic;

  using PskOnline.Server.Shared.Permissions;
  using PskOnline.Server.Shared.Permissions.Standard;
  using PskOnline.Server.Shared.Roles;

  public static class DefaultTenantRoles
  {
    public static IReadOnlyCollection<RoleSpecification> GetStandardTenantNonAdminRoleTemplates()
    {
      var roles = new List<RoleSpecification>
      {
        new RoleSpecification(
          "6584F8AE-5291-4F00-AE8F-1172BBA52AE0",
          "Employee",
          "Employee role (submit & view own test data)",
          new [] { TestDataPermissions.Test_Self_Submit,
                    TestDataPermissions.Test_Self_View,
                    OrgStructurePermissions.Org_TenantUpstream_View }
          ),

        new RoleSpecification(
          "0B34C602-B6D7-40B5-B13C-7232FCE70C49",
          "Organization Auditor",
          "Organization-wide auditor (General Manager or Principal Engineer)",
          new [] { TestDataPermissions.Test_Tenant_View,
                    OrgStructurePermissions.Org_Tenant_View,
                    UserPermissions.Users_Tenant_View }
          ),

        new RoleSpecification(
          "A78A848C-13D5-4B37-B7C9-8229013A40A7",
          "Psychophysiologist",
          "Psychophysiologist",
          new [] { TestDataPermissions.Test_Tenant_View,
                    TestDataPermissions.Test_Tenant_Submit,
                    OrgStructurePermissions.Org_Tenant_View,
                    UserPermissions.Users_Tenant_View }
          ),
      };
      return roles;
    }

    /// <summary>
    /// Duplicates the definition of the original permissions defined by a plugin.
    /// </summary>
    /// <remarks>
    /// The value is that we don't duplicate the 'value' field in too many places.</remarks>
    public static IApplicationPermission PsaSummary_Department_View_Stub_Permission => new ApplicationPermission(
          "STUB View PSA summary for department",
          "psasummary", PermScope.Department, PermAction.View,
          "STUB premissions related to PSA Summary",
          "STUB permission to view PSA summary for own department");

    public static RoleSpecification TenantAdministratorTemplate => 
      new RoleSpecification(
        "C65F0F55-4A93-474A-9CC4-896FCE073A76",
        "Tenant Administrator",
        "Tenant Administrator (structure & users)",
        new[] { OrgStructurePermissions.Org_Tenant_View,
            OrgStructurePermissions.Org_Tenant_Manage,
            RolePermissions.Roles_Tenant_View,
            RolePermissions.Roles_Tenant_Assign,
            RolePermissions.Roles_Tenant_Manage,
            UserPermissions.Users_Tenant_Manage,
            UserPermissions.Users_Tenant_View }
        );
  }
}
