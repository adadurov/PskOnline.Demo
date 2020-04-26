namespace PskOnline.Server.Service
{
  using AutoMapper;
  using log4net;

  using PskOnline.Server.Service.ViewModels;

  /// <summary>
  /// performs configuration of static items
  /// once per app domain
  /// useful for using static assets like AutoMapper configuration
  /// in dynamic context (e.g. in integration testing context when app host
  /// is built multiple times in the same domain)
  /// </summary>
  public static class StaticStartup
  {
    private static ILog _log = LogManager.GetLogger(typeof(StaticStartup));

    static StaticStartup()
    {
      Mapper.Initialize(cfg =>
      {
        cfg.AddProfile<AutoMapperProfile>();
      });
    }

    public static void Startup()
    {
      _log.Info(nameof(Startup) + " - completed");
    }
  }
}
