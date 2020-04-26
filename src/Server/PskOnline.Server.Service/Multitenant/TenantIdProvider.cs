namespace PskOnline.Server.Service.Multitenant
{
  using System;

  using Microsoft.AspNetCore.Http;
  using Microsoft.Extensions.Logging;
  using PskOnline.Server.Shared;
  using PskOnline.Server.Shared.Multitenancy;

  public class TenantIdProvider : ITenantIdProvider
  {
    private readonly Guid? _tenantId;
    private readonly ILogger<TenantIdProvider> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantIdProvider(
      IHttpContextAccessor httpContextAccessor,
      ILogger<TenantIdProvider> logger
      )
    {
      _httpContextAccessor = httpContextAccessor;
      _logger = logger;
      _tenantId = httpContextAccessor.HttpContext?.User?.GetUserTenant();
    }

    public Guid GetTenantId()
    {
      if( _tenantId.HasValue )
      {
        return _tenantId.Value;
      }
      var msg = $"User is not properly authenticated. No '{CustomClaimTypes.TenantId}' claim has been presented!";
      _logger.LogError(msg);
      throw new UnauthorizedAccessException(msg);
    }
  }
}
