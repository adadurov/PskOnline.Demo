namespace PskOnline.Server.DAL.EntityFramework
{
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata.Builders;
  using PskOnline.Server.ObjectModel;

  internal sealed class InspectionEntityConfig : IEntityTypeConfiguration<Inspection>
  {
    public void Configure(EntityTypeBuilder<Inspection> builder)
    {
      builder.ToTable($"App_{nameof(Inspection)}");
      builder.HasKey(i => i.Id);
      builder.HasIndex(i => i.MethodSetId);
      builder.HasIndex(i => i.EmployeeId);
      builder.HasIndex(i => i.DepartmentId);
      builder.HasIndex(i => i.BranchOfficeId);
      builder.HasIndex(i => i.TenantId);
      builder.HasIndex(i => i.StartTime);
      builder.HasIndex(i => i.FinishTime);
      builder.HasMany(i => i.Tests).WithOne().HasForeignKey(t => t.InspectionId);
    }
  }
}
