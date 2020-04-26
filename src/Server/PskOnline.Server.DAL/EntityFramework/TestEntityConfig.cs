namespace PskOnline.Server.DAL.EntityFramework
{
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata.Builders;
  using PskOnline.Server.ObjectModel;

  internal sealed class TestEntityConfig : IEntityTypeConfiguration<Test>
  {
    public void Configure(EntityTypeBuilder<Test> test)
    {
      test.ToTable($"App_{nameof(Test)}");
      test.HasKey(t => t.Id);
      test.HasIndex(t => t.EmployeeId);
      test.HasIndex(t => t.DepartmentId);
      test.HasIndex(t => t.BranchOfficeId);
      test.HasIndex(t => t.TenantId);
      test.HasIndex(t => t.StartTime);
      test.HasIndex(t => t.FinishTime);
    }
  }
}
