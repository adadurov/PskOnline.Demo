namespace PskOnline.Server.Service.Multitenant
{
  using Microsoft.AspNetCore.Http;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Primitives;
  using PskOnline.Server.Shared.Multitenancy;
  using System.Linq;

  public class TenantSlugProvider : ITenantSlugProvider
  {
    private string _slug;
    private readonly string _devServerName;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TenantSlugProvider> _logger;

    public TenantSlugProvider(
      IHttpContextAccessor httpContextAccessor,
      IConfiguration configuration,
      ILogger<TenantSlugProvider> logger
      )
    {
      _devServerName = configuration["DevServerName"];
      _httpContextAccessor = httpContextAccessor;
      _logger = logger;
    }

    public string GetSlug()
    {
      if (string.IsNullOrEmpty(_slug))
      {
        var host = _httpContextAccessor.HttpContext?.Request.Host.Host;
        var serverNameValues = _httpContextAccessor.HttpContext?.Request.Headers["PSK-ServerName"] ?? StringValues.Empty;

        var slug = _httpContextAccessor.HttpContext?.Request.Headers["X-PSK-Slug"].FirstOrDefault() ?? "";
        if (!string.IsNullOrEmpty(slug))
        {
          // likely for auto testing
          _logger.LogDebug(
            $"Tenant slug inferred from X-PSK-Slug HTTP header: '{slug}'");
          _slug = slug;
        }
        else
        {
          // for dev/stage/prod environments
          _slug = TenantSlugExtractor.ExtractSlug(
            host?.ToString() ?? "",
            serverNameValues,
            _devServerName,
            _logger);
        }
      }
      return _slug;
    }
  }
}
