namespace PskOnline.Server.Service.Helpers
{
  using System;
  using Microsoft.Extensions.Logging;

  public static class Utilities
  {
    static ILoggerFactory _loggerFactory;


    public static void ConfigureLogger(ILoggerFactory loggerFactory)
    {
      _loggerFactory = loggerFactory;
    }

    public static ILogger CreateLogger<T>()
    {
      if (_loggerFactory == null)
      {
        throw new InvalidOperationException($"{nameof(ILogger)} is not configured. {nameof(ConfigureLogger)} must be called before use");
        //_loggerFactory = new LoggerFactory().AddConsole().AddDebug();
      }

      return _loggerFactory.CreateLogger<T>();
    }
  }
}
