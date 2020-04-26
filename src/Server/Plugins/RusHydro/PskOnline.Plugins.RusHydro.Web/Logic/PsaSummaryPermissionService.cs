namespace PskOnline.Server.Plugins.RusHydro.Web.Logic
{
  using PskOnline.Server.Shared.Permissions;

  public class PsaSummaryPermissionService : IPluginPermissionsProvider
  {
    public IApplicationPermission[] GetPermissions()
    {
      return PsaSummaryPermissions.AllPermissions();
    }
  }
}
