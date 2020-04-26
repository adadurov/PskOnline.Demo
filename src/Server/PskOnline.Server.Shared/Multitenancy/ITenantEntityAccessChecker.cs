namespace PskOnline.Server.Shared.Multitenancy
{
  using PskOnline.Server.Shared.Permissions;

  /// <summary>
  /// implements rules for access to entities belonging to tenants
  /// </summary>
  public interface ITenantEntityAccessChecker : IAccessChecker
  {
  }
}
