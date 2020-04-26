namespace PskOnline.Server.Service.Multitenant
{
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Primitives;
  using System.Linq;

  public class TenantSlugExtractor
  {
    public static string ExtractSlug(string host, StringValues serverNameValues, string devServerName, ILogger logger)
    {
      var serverName = serverNameValues.FirstOrDefault();

      if (string.IsNullOrWhiteSpace(host))
      {
        logger.LogDebug(
          "'Host' HTTP header not found or empty. Cannot infer the tenant slug. " +
          "Assuming empty slug -> default tenant.");
        return "";
      }

      // 'Host' HTTP header is not empty, but what about 'PSK-ServerName' header?
      if (!string.IsNullOrWhiteSpace(serverName))
      {
        var index = host.IndexOf(serverName);
        if (index != -1)
        {
          // serverName is a substring of Host HTTP header
          // likely for production, will either return some string or an empty slug
          return host.Substring(0, index).TrimEnd('.').ToLower();
        }
        var msg = $"Host = '{host}', ServerName = '{serverName}'";
        logger.LogDebug(
          "'Host' header doesn't contain the value of 'PSK-ServerName'. Check proxy configuration. " +
          "Assuming empty slug -> default tenant. " + msg);
        return "";
      }

      logger.LogDebug(
        "'PSK-ServerName' HTTP header not found or empty. Assuming development environment. " +
        $"DevServerName = '{devServerName}'");

      var idx = host.IndexOf(devServerName);
      if (idx != -1)
      {
        return host.Substring(0, idx).TrimEnd('.');
      }

      var err = $"Host = '{host}', Config.DevServerName = '{devServerName}'";
      logger.LogDebug(
        "'Host' header doesn't contain the value of 'DevServerName'. Check your environment. " +
        "Assuming empty slug -> default tenant. " + err);
      return "";
    }


  }
}
