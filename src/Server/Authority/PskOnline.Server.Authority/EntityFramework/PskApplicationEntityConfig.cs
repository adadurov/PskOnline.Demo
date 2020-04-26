namespace PskOnline.Server.Authority.EntityFramework
{
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata.Builders;
  using PskOnline.Server.Authority.ObjectModel;

  internal sealed class PskApplicationEntityConfig : IEntityTypeConfiguration<PskApplication>
  {
    public void Configure(EntityTypeBuilder<PskApplication> application)
    {
      application.HasIndex(a => a.TenantId);
      application.HasIndex(a => a.DepartmentId);
      application.HasIndex(a => a.BranchOfficeId);
    }
  }
}