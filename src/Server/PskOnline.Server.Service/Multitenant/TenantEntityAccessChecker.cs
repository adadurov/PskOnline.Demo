namespace PskOnline.Server.Service.Multitenant
{
  using System;
  using System.Threading.Tasks;
  using Microsoft.Extensions.Logging;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Shared.ObjectModel;
  using PskOnline.Server.Shared.Permissions;

  /// <summary>
  /// implements access checking rules for entities belonging
  /// to tenants except for the actual tenants
  /// </summary>
  public class TenantEntityAccessChecker : ITenantEntityAccessChecker
  {
    private readonly ILogger<TenantEntityAccessChecker> _logger;
    private readonly ITenantIdProvider _tenantIdProvider;

    public TenantEntityAccessChecker(
      ITenantIdProvider tenantIdProvider,
      ILogger<TenantEntityAccessChecker> logger)
    {
      _logger = logger;
      _tenantIdProvider = tenantIdProvider;
    }

    public Task ValidateAccessToEntityAsync(
      ITenantEntity entity, EntityAction desiredAction)
    {
      if (null == entity)
      {
        return Task.CompletedTask;
      }

      // this call may throw exception if the user doesn't have
      // the validated TenantId claim
      var _userTenantId = _tenantIdProvider.GetTenantId();

      if (TenantSpec.BelongsToSite(_userTenantId))
      {
        // simple tenant access (no permissions)
        // 'site user' can access entities within any tenant
        // for the purposes of administration
        return Task.CompletedTask;
      }
      if (_userTenantId != entity.TenantId )
      {
        var message =
          $"User belonging to tenant {_userTenantId} has been denied access " +
          $"to entity '{entity.GetType().Name}' belonging to tenant {entity.TenantId}";
        throw new UnauthorizedAccessException(message);
      }
      return Task.CompletedTask;
    }

  }
}
