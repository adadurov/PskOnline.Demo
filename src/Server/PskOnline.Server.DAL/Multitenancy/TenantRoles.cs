namespace PskOnline.Server.DAL.Multitenancy
{
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  
  using PskOnline.Server.Shared.Permissions;
  using PskOnline.Server.Shared.Permissions.Standard;

  public class TenantRoles
  {
    public static ReadOnlyCollection<IApplicationPermission> Patient =>
        new List<IApplicationPermission>()
        {
          TestDataPermissions.Test_Self_Submit,
          TestDataPermissions.Test_Self_View,
          OrgStructurePermissions.Org_TenantUpstream_View
        }.AsReadOnly();

    public static ReadOnlyCollection<ApplicationPermission> Operator =>
        new List<ApplicationPermission>()
        {
          TestDataPermissions.Test_Department_Submit,
          OrgStructurePermissions.Org_TenantUpstream_View
        }.AsReadOnly();

    public static ReadOnlyCollection<ApplicationPermission> NSS =>
        new List<ApplicationPermission>()
        {
          TestDataPermissions.Test_Department_View,
          OrgStructurePermissions.Org_TenantUpstream_View
        }.AsReadOnly();

    /// <summary>
    /// Auditor may be a Dept-, Org-, or Tenant-level role
    /// </summary>
    public static ReadOnlyCollection<ApplicationPermission> TenantAuditor =>
        new List<ApplicationPermission>()
        {
          TestDataPermissions.Test_Tenant_View,
          OrgStructurePermissions.Org_Tenant_View
        }.AsReadOnly();

    /// <summary>
    /// Auditor may be a Dept-, Org-, or Tenant-level role
    /// </summary>
    public static ReadOnlyCollection<ApplicationPermission> OrgAuditor =>
        new List<ApplicationPermission>()
        {
          TestDataPermissions.Test_Branch_View,
          OrgStructurePermissions.Org_Tenant_View
        }.AsReadOnly();

    public static ReadOnlyCollection<ApplicationPermission> PsyPhysiologist =>
        new List<ApplicationPermission>()
        {
          TestDataPermissions.Test_Tenant_View,
          TestDataPermissions.Test_Tenant_Submit,
          OrgStructurePermissions.Org_Tenant_View,
          UserPermissions.Users_Tenant_View
        }.AsReadOnly();

    public static ReadOnlyCollection<ApplicationPermission> TenantAdmin =>
        new List<ApplicationPermission>()
        {
          OrgStructurePermissions.Org_Tenant_View,
          OrgStructurePermissions.Org_Tenant_Manage,
          UserPermissions.Users_Tenant_View,
          UserPermissions.Users_Tenant_Manage
        }.AsReadOnly();

  }
}
