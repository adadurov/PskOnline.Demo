namespace PskOnline.Server.Shared.Permissions
{
  public interface IPluginPermissionsProvider
  {
    IApplicationPermission[] GetPermissions();
  }
}
