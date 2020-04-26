namespace PskOnline.Server.DAL.EntityFramework
{
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata.Builders;
  using PskOnline.Server.ObjectModel;

  internal sealed class EmployeeEntityConfig : IEntityTypeConfiguration<Employee>
  {
    public void Configure(EntityTypeBuilder<Employee> employee)
    {
      employee.ToTable($"App_{nameof(Employee)}");
      employee.HasKey(e => e.Id);
      employee.HasIndex(e => e.UserId);
      employee.HasIndex(e => e.TenantId);
      employee.HasIndex(e => e.BranchOfficeId);
      employee.HasIndex(e => e.DepartmentId);

      employee.Property(e => e.TenantId).IsRequired();
      employee.Property(e => e.FirstName).IsRequired();
      employee.Property(e => e.LastName).IsRequired();
    }
  }
}
