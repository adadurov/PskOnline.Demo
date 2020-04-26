namespace PskOnline.Server.Service.Multitenant
{
  using System;
  using System.Linq;
  using Microsoft.Extensions.Logging;

  using PskOnline.Server.Shared.ObjectModel;
  using PskOnline.Server.Shared.Permissions;
  using PskOnline.Server.Shared.Multitenancy;

  public class TenantScopeFilter : IAccessScopeFilter
  {
    private readonly ITenantIdProvider _tenantIdProvider;

    public TenantScopeFilter(
      ITenantIdProvider tenantIdProvider,
      ILogger<TenantScopeFilter> logger)
    {
      _tenantIdProvider = tenantIdProvider;
    }

    public IQueryable<T> AddScopeFilter<T>(IQueryable<T> entityQuery) where T : class, ITenantEntity
    {
      var tenantId = _tenantIdProvider.GetTenantId();
      if (tenantId != Guid.Empty)
      {
        return entityQuery.Where(e => e.TenantId == tenantId);
      }
      return entityQuery;
    }
  }
}
