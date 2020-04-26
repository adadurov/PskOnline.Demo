namespace PskOnline.Server.Shared.Plugins
{
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;

  public interface IServerPlugin
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="servicesCollection"></param>
    void AddServices(
      IConfiguration configuration, 
      IServiceCollection servicesCollection,
      ILoggerFactory _loggerFactory);
  }
}
