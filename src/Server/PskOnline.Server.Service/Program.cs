namespace PskOnline.Server.Service
{
  using Microsoft.AspNetCore;
  using Microsoft.AspNetCore.Hosting;

  public class Program
  {
    public static int Main(string[] args)
    {
      var host = CreateWebHostBuilder(args).Build();

      var dbInitialized = DatabaseInitializerHelper.WaitForDbServerAndSeedDatabase(host.Services);

      if( ! dbInitialized )
      {
        return 1;
      }
      host.Run();
      return 0;
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args)
    {
      return WebHost.CreateDefaultBuilder(args).UseStartup<Startup>();
    }
  }
}
