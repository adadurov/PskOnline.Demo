namespace PskOnline.Server.DAL
{
  using Microsoft.EntityFrameworkCore;
  using System;
  using System.Linq;
  using System.Threading.Tasks;
  using System.Threading;

  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Shared.ObjectModel;
  using PskOnline.Server.DAL.EntityFramework;

  /// <summary>
  /// Create initial migration via Package Manager Console:
  /// 
  /// Add-Migration Initial -OutputDir Migrations -Context ApplicationDbContext -Project PskOnline.Service
  /// </summary>
  public class ApplicationDbContext : DbContext
  {
    public string CurrentUserId { get; set; }

    public DbSet<Tenant> Tenants { get; set; }

    public DbSet<Employee> Employees { get; set; }

    public DbSet<BranchOffice> BranchOffices { get; set; }

    public DbSet<Department> Departments { get; set; }

    public DbSet<Position> Positions { get; set; }

    public DbSet<Inspection> Inspections { get; set; }

    public DbSet<Test> InspectionTests { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
      base.OnModelCreating(builder);

      builder.ApplyConfiguration(new TenantEntityConfig());

      builder.ApplyConfiguration(new EmployeeEntityConfig());
      builder.ApplyConfiguration(new InspectionEntityConfig());
      builder.ApplyConfiguration(new TestEntityConfig());
      builder.ApplyConfiguration<BranchOffice>(new OrgStructureEntityConfig());
      builder.ApplyConfiguration<Department>(new OrgStructureEntityConfig());
      builder.ApplyConfiguration<Position>(new OrgStructureEntityConfig());
    }

    public override int SaveChanges()
    {
      UpdateAuditEntities();
      return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
      UpdateAuditEntities();
      return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
      UpdateAuditEntities();
      return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
    {
      UpdateAuditEntities();
      return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void UpdateAuditEntities()
    {
      var modifiedEntries = ChangeTracker.Entries()
          .Where(x => x.Entity is IAuditableEntity && (x.State == EntityState.Added || x.State == EntityState.Modified));

      foreach (var entry in modifiedEntries)
      {
        var entity = (IAuditableEntity)entry.Entity;
        DateTime now = DateTime.UtcNow;

        if (entry.State == EntityState.Added)
        {
          entity.CreatedDate = now;
          entity.CreatedBy = CurrentUserId;
        }
        else
        {
          base.Entry(entity).Property(x => x.CreatedBy).IsModified = false;
          base.Entry(entity).Property(x => x.CreatedDate).IsModified = false;
          entity.UpdatedDate = now;
          entity.UpdatedBy = CurrentUserId;
        }
      }
    }

    /// <summary>
    /// Migrate the database to current version
    /// </summary>
    public void MigrateToCurrentVersion()
    {
      Database.Migrate();
    }
  }
}
