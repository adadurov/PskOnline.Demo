namespace PskOnline.Server.DAL.EntityFramework
{
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata.Builders;
  using PskOnline.Server.ObjectModel;

  internal sealed class OrgStructureEntityConfig : IEntityTypeConfiguration<BranchOffice>,
                                   IEntityTypeConfiguration<Department>,
                                   IEntityTypeConfiguration<Position>
  {
    public void Configure(EntityTypeBuilder<BranchOffice> branchOffice)
    {
      branchOffice.ToTable($"App_{nameof(BranchOffice)}");
      branchOffice.HasKey(b => b.Id);
      branchOffice.HasIndex(b => b.TenantId);
      branchOffice.Property(b => b.TenantId).IsRequired();
      branchOffice.HasMany(b => b.Departments).WithOne().HasForeignKey(d => d.BranchOfficeId).IsRequired();
      branchOffice.HasMany(b => b.Positions).WithOne().HasForeignKey(p => p.BranchOfficeId).IsRequired(false);
    }

    public void Configure(EntityTypeBuilder<Department> department)
    {
      department.ToTable($"App_{nameof(Department)}");
      department.HasKey(d => d.Id);
      department.Property(d => d.TenantId).IsRequired();
      department.HasIndex(d => d.TenantId);
      department.Property(d => d.BranchOfficeId).IsRequired();
      department.HasIndex(d => d.BranchOfficeId);
      department.Property(d => d.Name).IsRequired().HasMaxLength(256);
    }

    public void Configure(EntityTypeBuilder<Position> position)
    {
      position.ToTable($"App_{nameof(Position)}");
      position.HasKey(p => p.Id);
      position.Property(p => p.TenantId).IsRequired();
      position.HasIndex(p => p.TenantId);
      position.Property(p => p.BranchOfficeId);
      position.HasIndex(p => p.BranchOfficeId);
      position.Property(p => p.Name).IsRequired().HasMaxLength(256);
    }
  }
}
