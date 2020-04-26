namespace PskOnline.Server.Authority.EntityFramework
{
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata.Builders;
  using PskOnline.Server.Authority.ObjectModel;

  internal sealed class IdentityEntityConfig : IEntityTypeConfiguration<ApplicationUser>,
                               IEntityTypeConfiguration<ApplicationRole>
  {
    public void Configure(EntityTypeBuilder<ApplicationUser> user)
    {
      user.HasMany(u => u.Claims).WithOne()
        .HasForeignKey(c => c.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);
      user.HasMany(u => u.Roles).WithOne()
        .HasForeignKey(r => r.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);
      user.HasIndex(u => u.DepartmentId);
      user.HasIndex(u => u.BranchOfficeId);
    }

    public void Configure(EntityTypeBuilder<ApplicationRole> role)
    {
      role.HasMany(r => r.Claims).WithOne().HasForeignKey(c => c.RoleId)
        .IsRequired().OnDelete(DeleteBehavior.Cascade);
      role.HasMany(r => r.Users).WithOne().HasForeignKey(r => r.RoleId)
        .IsRequired().OnDelete(DeleteBehavior.Cascade);

      // override the 'unique normalized name' requirement
      role.HasIndex(r => r.NormalizedName).HasName("RoleNameIndex").IsUnique(false);

      // override the 'unique normalized name' requirement
      role.HasIndex(
        new[] { nameof(ApplicationRole.TenantId), nameof(ApplicationRole.NormalizedName) })
        .HasName("RoleNamePerTenantIndex").IsUnique();
    }
  }
}