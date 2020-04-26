namespace PskOnline.Server.DAL.OrgStructure
{
  using System.Threading.Tasks;

  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Logging;

  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Shared.EFCore;
  using PskOnline.Server.Shared.Exceptions;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Shared.Repository;

  public class PositionService : BaseService<Position>
  {
    private readonly IGuidKeyedRepository<BranchOffice> _branchOfficeRepository;

    public PositionService(
      IGuidKeyedRepository<BranchOffice> branchOfficeRepository,
      ITenantEntityAccessChecker tenantAccessChecker,
      IAccessScopeFilter tenantContext,
      IGuidKeyedRepository<Position> repository,
      ILogger<PositionService> logger
      )
      : base(tenantAccessChecker, tenantContext, repository, logger)
    {
      // this is needed for validation of Position model
      _branchOfficeRepository = branchOfficeRepository;
    }

    protected override async Task CheckNewItemIsUnique(Position value)
    {
      // is there already a Position with the same id?
      if (await Repository.Query().AnyAsync(e => e.Id == value.Id))
      {
        throw ConflictException.IdConflict(nameof(Position), value.Id);
      }

      // does the Tenant already have a Position of the same name?
      if (await Repository.Query().AnyAsync(e =>
        e.Name == value.Name &&
        e.TenantId == value.TenantId
        ))
      {
        throw BadRequestException.NamedEntityExists(nameof(Department), value.Name);
      }
    }

    protected override async Task CheckUpdatedItemIsUnique(Position value)
    {
      // does the Tenant already have a Position of the same name?
      if (await Repository.Query().AnyAsync(e =>
        e.Name == value.Name &&
        e.TenantId == value.TenantId &&
        e.Id != value.Id
        ))
      {
        throw BadRequestException.NamedEntityExists(nameof(Department), value.Name);
      }
    }

    protected override async Task CheckEntityReferences(Position value)
    {
      // it is not allowed to refer to a branch office
      // that does not belong to the same tenant
      if (value.BranchOfficeId.HasValue)
      {
        await ReferenceValidatorForTenant.ValidateOptionalEntityReference(value, _branchOfficeRepository, value.BranchOfficeId.Value);
      }
    }

  }
}
