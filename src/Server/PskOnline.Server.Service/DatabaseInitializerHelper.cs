namespace PskOnline.Server.Service
{
  using System;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;
  using PskOnline.Server.DAL;
  using PskOnline.Server.Shared.EFCore;

  public class DatabaseInitializerHelper
  {
    public static bool WaitForDbServerAndSeedDatabase(IServiceProvider services)
    {
      using (var scope = services.CreateScope())
      {
        var serviceProvider = scope.ServiceProvider;
        var logger = serviceProvider.GetRequiredService<ILogger<DatabaseInitializerHelper>>();

        if (!WaitForDbServer(serviceProvider, logger)) return false;
        try
        {
          var databaseInitializers = serviceProvider.GetServices<IDatabaseInitializer>();
          foreach (var pluginDbInitializer in databaseInitializers)
          {
            try
            {
              var seedTask = pluginDbInitializer.SeedAsync();
              logger.LogInformation($"Initializer '{pluginDbInitializer.GetType().FullName}' is seeding its DB.");
              seedTask.Wait();
              logger.LogInformation($"Initializer '{pluginDbInitializer.GetType().FullName}' finished seeding its DB.");
            }
            catch ( Exception ex)
            {
              logger.LogCritical(ex, "Error seeding the database!");
              return false;
            }
          }
          logger.LogInformation("All DB initializers completed seeding their databases.");
          return true;
        }
        catch (Exception ex)
        {
          logger.LogCritical(ex, "Error seeding the database!");
        }
        return false;
      }
    }

    private static bool WaitForDbServer(IServiceProvider serviceProvider, ILogger<DatabaseInitializerHelper> logger)
    {
      var databaseContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
      var timeout = TimeSpan.FromSeconds(60);
      var start = DateTime.UtcNow;
      while (DateTime.UtcNow - start < timeout)
      {
        logger.LogWarning("Trying to connect to database server...");
        try
        {
          databaseContext.MigrateToCurrentVersion();
          logger.LogInformation("Connected to database server.");
          return true;
        }
        catch (Exception ex)
        {
          // don't log the exception itself, as we are not really interesetd in the stack trace at this point
          logger.LogWarning($"Connection to database server failed with error \"{ex.Message}\", waiting for the next attempt");
        }
        System.Threading.Thread.Sleep(2000);
      }
      logger.LogCritical($"Timed out while connecting to database server (waited for more than {(int)timeout.TotalSeconds} seconds");
      return false;
    }
  }
}