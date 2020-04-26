namespace PskOnline.Server.DAL.OrgStructure
{
  using System.Threading.Tasks;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Logging;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Shared.Exceptions;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Shared.Repository;
  using PskOnline.Server.Shared.EFCore;

  public class BranchOfficeService : BaseService<BranchOffice>
  {
    public BranchOfficeService(
      ITenantEntityAccessChecker tenantAccessChecker,
      IAccessScopeFilter tenantContext,
      IGuidKeyedRepository<BranchOffice> repository,
      ILogger<BranchOfficeService> logger
      )
      : base(tenantAccessChecker, tenantContext, repository, logger)
    {
    }

    protected override async Task CheckNewItemIsUnique(BranchOffice value)
    {
      // does the Tenant already have a BranchOffice of the same name?
      if (await Repository.Query().AnyAsync(e => e.Id == value.Id ) )
      {
        throw ConflictException.IdConflict(nameof(BranchOffice), value.Id);
      }

      // does the Tenant already have a BranchOffice of the same name?
      if (await Repository.Query().AnyAsync(
        e => e.Name == value.Name && 
        e.TenantId == value.TenantId
        ))
      {
        throw BadRequestException.NamedEntityExists(nameof(BranchOffice), value.Name);
      }
    }

    protected override async Task CheckUpdatedItemIsUnique(BranchOffice value)
    {
      // does the Tenant already have a BranchOffice
      // of the same name (except for the updated one)?
      if (await Repository.Query().AnyAsync(
        e => e.Name == value.Name && 
        e.TenantId == value.TenantId &&
        e.Id != value.Id
        ))
      {
        throw BadRequestException.NamedEntityExists(nameof(BranchOffice), value.Name);
      }
    }
  }
}
