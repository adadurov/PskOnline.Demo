namespace PskOnline.Server.DAL.Multitenancy
{
  using System;
  using System.Linq;
  using System.Threading.Tasks;

  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Logging;

  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Shared.EFCore;
  using PskOnline.Server.Shared.Exceptions;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Shared.Repository;

  public class TenantService : BaseService<Tenant>, ITenantService
  {
    public TenantService(
      ITenantAccessChecker tenantAccessChecker,
      IAccessScopeFilter tenantContext,
      IGuidKeyedRepository<Tenant> repository,
      ILogger<TenantService> logger,
      ITenantSlugProvider slugProvider
      )
      : base(tenantAccessChecker, tenantContext, repository, logger)
    {
    }

    public async Task<Guid> GetTenantIdBySlug(string slug)
    {
      slug = slug.ToLower();
      var tenant = await Repository.Query().FirstOrDefaultAsync(t => t.Slug == slug);
      if (tenant != null) return tenant.Id;
      return TenantSpec.EntireSiteTenantId;
    }

    protected override async Task CheckNewItemIsUnique(Tenant value)
    {
      // is there already a Tenant with the same Id, name or slug?
      if (await Repository.Query().AnyAsync(e => e.Id == value.Id))
      {
        throw new BadRequestException(
          $"An entity with the same ID ({value.Id}) already exists.");
      }
      if (await Repository.Query().AnyAsync(e => e.Name == value.Name))
      {
        throw BadRequestException.NamedEntityExists(nameof(Tenant), value.Name);
      }
      if (await Repository.Query().AnyAsync(e => e.Slug == value.Slug))
      {
        throw GetExceptionForBadSlug(value.Slug);
      }
    }

    protected override async Task CheckUpdatedItemIsUnique(Tenant value)
    {
      // is there already a Tenant with the same name
      // except for the tenant itself?
      if (await Repository.Query().AnyAsync( e => e.Name == value.Name && e.Id != value.Id ))
      {
        throw BadRequestException.NamedEntityExists(nameof(Tenant), value.Name);
      }
      if (await Repository.Query().AnyAsync(e => e.Slug == value.Slug && e.Id != value.Id))
      {
        throw GetExceptionForBadSlug(value.Slug);
      }
    }

    private Exception GetExceptionForBadSlug(string slug)
    {
      return new BadRequestException(
        $"A tenant with the same slug ('{slug}') already exists.");
    }
  }
}
