namespace PskOnline.Server.Plugins.RusHydro.Web
{
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;

  using PskOnline.Server.Plugins.RusHydro.DAL;
  using PskOnline.Server.Plugins.RusHydro.Web.Logic;
  using PskOnline.Server.Shared.Plugins;
  using PskOnline.Server.Shared.Permissions;
  using PskOnline.Server.Shared.EFCore;

  public class RusHydroPlugin : IServerPlugin
  {
    public void AddServices(IConfiguration configuration, IServiceCollection services, ILoggerFactory _loggerFactory)
    {
      services.AddDbContext<RusHydroPsaDbContext>(options =>
      {
        options.UseSqlServer(
          configuration["ConnectionStrings:DefaultConnection"], 
          b => b.MigrationsAssembly("PskOnline.Server.Plugins.RusHydro.DAL"));
      });

      services.AddScoped<IDatabaseInitializer, RusHydroPsaDbInitializer>();

      services.AddScoped<IPsaSummaryService, PsaSummaryService>();
      services.AddScoped<IPsaSummaryRepository, PsaSummaryRepository>();
      services.AddScoped<IDepartmentPsaReportService, PsaReportService>();

      services.AddScoped<IPluginPermissionsProvider, PsaSummaryPermissionService>();
      services.AddScoped<IOrgStructureReference, OrgStructureReference>();
      services.AddScoped<IInspectionResultsHandler, RusHydroInspectionHandler>();
    }
  }
}
