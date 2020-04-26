namespace PskOnline.Service.Test.Integration
{
  using System;
  using System.Net.Http;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.Mvc.Testing;
  using Microsoft.AspNetCore.TestHost;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;
  using PskOnline.Server.Authority.Interfaces;
  using PskOnline.Server.DAL;
  using PskOnline.Server.Service;

  public class DefaultWebApplicationFactory : WebApplicationFactory<Startup>
  {
    public static string DefaultPskServerName = "dev.psk-online.ru";
    public static string DefaultHostHeader = DefaultPskServerName;
    public static string TenantHostHeader = "tenant.dev.psk-online.ru";

    protected override TestServer CreateServer(IWebHostBuilder builder)
    {
      var server = base.CreateServer(builder);

      var sp = server.Host.Services;

      var environment = GetCurrentEnvironmentName();
      Console.WriteLine($"Using environment: {environment}");

      // Create a scope to obtain a reference to the database
      // context (ApplicationDbContext).
      using (var scope = sp.CreateScope())
      {
        var scopedServices = scope.ServiceProvider;

        var db = scopedServices.GetRequiredService<ApplicationDbContext>();
        // Ensure the database is re-created.
        db.Database.EnsureDeleted();

        DatabaseInitializerHelper.WaitForDbServerAndSeedDatabase(server.Host.Services);
      }
      return server;
    }

    /// <summary>
    /// current implementation returns an NSubstitute's mock of ILogger&lt;T&gt;
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public ILogger GetLogger<T>()
    {
      return NSubstitute.Substitute.For<ILogger<T>>();
      //var fac = Server.Host.Services.GetRequiredService<ILoggerFactory>();
      //return fac.CreateLogger<T>();
    }

    protected override IWebHostBuilder CreateWebHostBuilder()
    {
      string environment = GetCurrentEnvironmentName();
      return base.CreateWebHostBuilder().UseEnvironment(environment);
    }

    private static string GetCurrentEnvironmentName()
    {
      string environment = Environment.GetEnvironmentVariable("PSKONLINE_ENV");
      if (string.IsNullOrEmpty(environment))
      {
        environment = "Test";
      }
      return environment;
    }
  }
}
