namespace PskOnline.Server.Service.Multitenant
{
  using System;
  using System.Threading.Tasks;
  using Microsoft.Extensions.Logging;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Shared.ObjectModel;
  using PskOnline.Server.Shared.Permissions;

  public class TenantAccessChecker : ITenantAccessChecker
  {
    private readonly ITenantIdProvider _tenantIdProvider;
    private readonly ILogger<TenantEntityAccessChecker> _logger;

    public TenantAccessChecker(
      ITenantIdProvider tenantIdProvider,
      ILogger<TenantEntityAccessChecker> logger)
    {
      _tenantIdProvider = tenantIdProvider;
      _logger = logger;
    }

    public Task ValidateAccessToEntityAsync(
      ITenantEntity entity, EntityAction desiredAction)
    {
      if( null == entity )
      {
        return Task.CompletedTask;
      }

      var userTenantId = _tenantIdProvider.GetTenantId();

      if (TenantSpec.BelongsToSite(userTenantId))
      {
        // simplistic access rules for prototype phase:
        // 'site user' can access any tenant
        // for the purposes of administration
        return Task.CompletedTask;
      }
      
      // non-'site user'
      if (userTenantId != entity.TenantId )
      {
        string message = BuildAccessDeniedMessage(entity, desiredAction);
        throw new UnauthorizedAccessException(message);
      }

      // 'tenant user' can perform any modification read its own tenant information
      if ( desiredAction != EntityAction.Read )
      {
        string message = BuildAccessDeniedMessage(entity, desiredAction);
        throw new UnauthorizedAccessException(message);
      }
      return Task.CompletedTask;
    }

    private string BuildAccessDeniedMessage(ITenantEntity entity, EntityAction desiredAction)
    {
      // deny the attempt to access a tenant
      // that the user doesn't belong to
      return $"User belonging to tenant {_tenantIdProvider.GetTenantId()} has been denied '{desiredAction}' access " +
        $"to entity '{entity.GetType().Name}' belonging to tenant {entity.TenantId}";
    }
  }
}
