namespace PskOnline.Server.Authority
{
  using System.Collections.Generic;

  using PskOnline.Server.Shared.Permissions;

  public interface IClaimsService
  {
    IApplicationPermission GetPermissionByValue(string value);

    IReadOnlyCollection<IApplicationPermission> GetAllPermissions();
  }
}
