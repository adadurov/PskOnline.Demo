namespace PskOnline.Server.DAL
{
  using System;
  using System.Threading.Tasks;

  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Logging;

  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Shared.EFCore;

  public class DatabaseInitializer : IDatabaseInitializer
  {
    private readonly ApplicationDbContext _context;
    private readonly ILogger _logger;

    public DatabaseInitializer(
      ApplicationDbContext context,
      ILogger<DatabaseInitializer> logger)
    {
      _context = context;
      _logger = logger;
    }

    public async Task SeedAsync()
    {
      _context.MigrateToCurrentVersion();

      if (!await _context.Tenants.AnyAsync())
      {
        _logger.LogInformation("Seeding initial data");

        var cust_1 = new Tenant
        {
          Name = "DEMO_Irkutskenergo",
          PrimaryContact = new ContactInfo {
            Email = "contact@irkutskenergo.ru",
            FullName = "John Doe",
            MobilePhoneNumber = "+79995554433",
            OfficePhoneNumber = "+79995554422"
          },
          ServiceDetails = new TenantServiceDetails
          {
            ServiceMaxUsers = 10,
            // 31 days for 1 full month
            // 1 more day for max UTC date lag compared to local date
            ServiceExpireDate = DateTime.UtcNow + TimeSpan.FromDays(32),
            ServiceMaxStorageMegabytes = 1000, // 1 Gigabyte
          },
          CreatedDate = DateTime.UtcNow,
          UpdatedDate = DateTime.UtcNow,
          CreatedBy = nameof(DatabaseInitializer),
          UpdatedBy = nameof(DatabaseInitializer)
        };

        var cust_2 = new Tenant
        {
          Name = "DEMO_Mosenergo",
          PrimaryContact = new ContactInfo {
            Email = "contact@mosenergo.ru",
            FullName = "Archibald Dendy",
            MobilePhoneNumber = "+79995554433",
            OfficePhoneNumber = "+79985554422"
          },
          ServiceDetails = new TenantServiceDetails {
            ServiceMaxUsers = 5,
            ServiceExpireDate = DateTime.Now + TimeSpan.FromDays(60),
            ServiceMaxStorageMegabytes = 4000, // 4 Gigabytes
          },
          CreatedDate = DateTime.UtcNow,
          UpdatedDate = DateTime.UtcNow,
          CreatedBy = nameof(DatabaseInitializer),
          UpdatedBy = nameof(DatabaseInitializer)
        };

        var cust_3 = new Tenant
        {
          Name = "DEMO_RusHydro",
          PrimaryContact = new ContactInfo {
            Email = "contact@rushydro.ru",
            FullName = "Christopher B. Wrongel",
            MobilePhoneNumber = "+79995554433",
            OfficePhoneNumber = "+79985554422"
          },
          ServiceDetails = new TenantServiceDetails {
            ServiceMaxUsers = 15,
            ServiceExpireDate = DateTime.Now + TimeSpan.FromDays(60),
            ServiceMaxStorageMegabytes = 2000, // 2 Gigabytes
          },
          CreatedDate = DateTime.UtcNow,
          UpdatedDate = DateTime.UtcNow,
          CreatedBy = nameof(DatabaseInitializer),
          UpdatedBy = nameof(DatabaseInitializer)
        };

        _context.Tenants.Add(cust_1);
        _context.Tenants.Add(cust_2);
        _context.Tenants.Add(cust_3);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeding initial data completed");
      }
    }
  }
}
