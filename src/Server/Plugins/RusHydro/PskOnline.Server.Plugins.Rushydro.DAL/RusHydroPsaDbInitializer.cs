namespace PskOnline.Server.Plugins.RusHydro.DAL
{
  using System.Threading.Tasks;
  using Microsoft.Extensions.Logging;
  using PskOnline.Server.Shared.EFCore;

  public class RusHydroPsaDbInitializer : IDatabaseInitializer
  {
    private readonly ILogger _logger;
    private readonly RusHydroPsaDbContext _context;

    public RusHydroPsaDbInitializer(
      RusHydroPsaDbContext context,
      ILogger<RusHydroPsaDbInitializer> logger)
    {
      _logger = logger;
      _context = context;
    }

    public Task SeedAsync()
    {
      _context.MigrateToCurrentVersion();
      return Task.CompletedTask;
    }
  }
}
