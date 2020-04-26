namespace PskOnline.Server.DAL.EntityFramework
{
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata.Builders;
  using PskOnline.Server.ObjectModel;

  internal sealed class TenantEntityConfig : IEntityTypeConfiguration<Tenant>
  {
    public void Configure(EntityTypeBuilder<Tenant> tenantEntity)
    {
      tenantEntity.ToTable($"App_{nameof(Tenant)}");
      tenantEntity.HasKey(c => c.Id);
      // we don't require a unique index here, but must check it in the business logic;
      // this will allow migration of the existing environment to the new version 
      // which uses subdomain slugs for tenants
      tenantEntity.HasIndex(c => c.Slug);
      tenantEntity.Ignore(c => c.TenantId);
      tenantEntity.Property(c => c.Name).IsRequired().HasMaxLength(100);
      tenantEntity.HasIndex(c => c.Name).IsUnique();

      var primaryContact = tenantEntity.OwnsOne(c => c.PrimaryContact);
      primaryContact.Property(c => c.Email).HasMaxLength(128);
      primaryContact.Property(c => c.City).HasMaxLength(128);
      primaryContact.Property(c => c.StreetAddress).HasMaxLength(256);

      tenantEntity.OwnsOne(c => c.ServiceDetails);
    }
  }
}
