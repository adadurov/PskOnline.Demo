namespace PskOnline.Server.Shared.Permissions.Standard
{
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Linq;

  using PskOnline.Server.Shared.Permissions;

  public static class CoreAppPermissions
  {
    public static ReadOnlyCollection<IApplicationPermission> AllPermissions { get; private set; }

    static CoreAppPermissions()
    {
      var allAppPerm = new List<IApplicationPermission>();

      allAppPerm.AddRange(OrgStructurePermissions.AllPermissions);
      allAppPerm.AddRange(TestDataPermissions.AllPermissions);
      allAppPerm.AddRange(UserPermissions.AllPermissions);
      allAppPerm.AddRange(RolePermissions.AllPermissions);
      allAppPerm.AddRange(TenantPermissions.AllPermissions);
      allAppPerm.AddRange(EmployeePermissions.AllPermissions);

      AllPermissions = allAppPerm.AsReadOnly();
    }

    public static string[] GetAdministrativePermissionValues()
    {
      return new string[] {
        UserPermissions.Users_GLOBAL_Manage,
        UserPermissions.Users_GLOBAL_View,
        RolePermissions.Roles_GLOBAL_Manage,
        RolePermissions.Roles_GLOBAL_Assign,
        RolePermissions.Roles_GLOBAL_View,
        TenantPermissions.Tenants_GLOBAL_Manage,
        TenantPermissions.Tenants_GLOBAL_View
      };
    }

    public static IApplicationPermission GetPermissionByName(string permissionName)
    {
      return AllPermissions.Where(p => p.Name == permissionName).FirstOrDefault();
    }

    public static IApplicationPermission GetPermissionByValue(string permissionValue)
    {
      return AllPermissions.Where(p => p.Value == permissionValue).FirstOrDefault();
    }

    public static ICollection<IApplicationPermission> GetPermissionsByType(string type)
    {
      return AllPermissions.Where(p => p.Type == type).ToList();
    }

    public static ICollection<IApplicationPermission> GetPermissionsByScope(string scope)
    {
      return AllPermissions.Where(p => p.Scope == scope).ToList();
    }

    public static ICollection<IApplicationPermission> GetPermissionsByAction(string action)
    {
      return AllPermissions.Where(p => p.Action == action).ToList();
    }

    public static string[] GetAllPermissionValues()
    {
      return AllPermissions.Select(p => p.Value).ToArray();
    }

  }
}
