namespace PskOnline.Server.Authority
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  using Microsoft.Extensions.DependencyInjection;
  using PskOnline.Server.Shared.Permissions.Standard;
  using PskOnline.Server.Shared.Permissions;

  public sealed class ClaimsService : IClaimsService
  {
    private readonly IReadOnlyCollection<IApplicationPermission> _permissions;

    public ClaimsService(IServiceProvider serviceProvider)
    {
      var permissions = new List<IApplicationPermission>(40);
      permissions.AddRange(CoreAppPermissions.AllPermissions);
      // add permissions from plugins...

      var pluginPermissionsProviders = serviceProvider.GetServices<IPluginPermissionsProvider>();
      foreach( var provider in pluginPermissionsProviders )
      {
        permissions.AddRange(provider.GetPermissions());
      }
      _permissions = permissions.AsReadOnly();
    }

    public IReadOnlyCollection<IApplicationPermission> GetAllPermissions()
    {
      return _permissions;
    }

    public IApplicationPermission GetPermissionByValue(string value)
    {
      return _permissions.Where(p => p.Value == value).FirstOrDefault();
    }
  }
}
