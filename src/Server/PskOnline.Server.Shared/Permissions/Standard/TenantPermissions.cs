namespace PskOnline.Server.Shared.Permissions.Standard
{
  using System.Collections.Generic;
  using System.Collections.ObjectModel;

  using PskOnline.Server.Shared.Permissions;

  /// <summary>
  /// Defines permissions related to Tenant objects
  /// </summary>
  public static class TenantPermissions
  {
    public const string TenantGroupPrefix = "tenants";

    public const string TenantPermissionGroupName = "Tenant Permissions";

    public static ReadOnlyCollection<ApplicationPermission> AllPermissions;

    static TenantPermissions()
    {
      AllPermissions = new List<ApplicationPermission>()
      {
        Tenants_GLOBAL_View,
        Tenants_GLOBAL_Manage
      }.AsReadOnly();
    }

    public static ApplicationPermission Tenants_GLOBAL_View =>
      new ApplicationPermission("View GLOBAL Tenants",
        TenantGroupPrefix, PermScope.GLOBAL, PermAction.View,
        TenantPermissionGroupName,
        "Permission to view details of tenants");

    public static ApplicationPermission Tenants_GLOBAL_Manage =>
      new ApplicationPermission(
        "Manage GLOBAL Tenants", 
        TenantGroupPrefix, PermScope.GLOBAL, PermAction.Manage,
        TenantPermissionGroupName, 
        "Permission to create, delete and modify tenants");

  }
}
