namespace PskOnline.Server.Plugins.RusHydro.DAL
{
  using PskOnline.Server.Plugins.RusHydro.ObjectModel;
  using Microsoft.EntityFrameworkCore;
  using Newtonsoft.Json;

  public class RusHydroPsaDbContext : DbContext
  {
    public DbSet<Summary> Sumaries { get; set; }

    public RusHydroPsaDbContext(DbContextOptions<RusHydroPsaDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
      base.OnModelCreating(builder);

      {
        // Configure summary entity
        builder.Entity<Summary>().ToTable($"RusHydro_{nameof(Summary)}");

        var serializerSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        builder.Entity<Summary>().Property(s => s.SummaryDocument).HasConversion(
          v => JsonConvert.SerializeObject(v, serializerSetting),
          v => JsonConvert.DeserializeObject<SummaryDocument>(v, serializerSetting)
          );

        builder.Entity<Summary>().Property(s => s.InspectionId).IsRequired();
        // preventy creating multiple summaries for a single inspection
        builder.Entity<Summary>().HasIndex(s => s.InspectionId).IsUnique();
        builder.Entity<Summary>().HasIndex(s => s.DepartmentId);
        builder.Entity<Summary>().HasIndex(s => s.ShiftAbsoluteIndex);
        builder.Entity<Summary>().HasIndex(s => s.CompletionTime);
        builder.Entity<Summary>().HasIndex(s => s.BranchOfficeId);
        builder.Entity<Summary>().HasIndex(s => s.TenantId);
        builder.Entity<Summary>().HasIndex(s => s.EmployeeId);
      }
    }

    internal void MigrateToCurrentVersion()
    {
      Database.Migrate();
    }
  }
}
