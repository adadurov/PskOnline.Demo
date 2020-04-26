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

  public class DepartmentService : BaseService<Department>
  {
    private readonly IGuidKeyedRepository<BranchOffice> _branchOfficeRepository;

    public DepartmentService(
      IGuidKeyedRepository<BranchOffice> branchOfficeRepository,
      ITenantEntityAccessChecker tenantAccessChecker,
      IAccessScopeFilter tenantContext,
      IGuidKeyedRepository<Department> repository,
      ILogger<DepartmentService> logger
      )
      : base(tenantAccessChecker, tenantContext, repository, logger)
    {
      // this is needed for validation of Position model
      _branchOfficeRepository = branchOfficeRepository;
    }

    protected override async Task CheckNewItemIsUnique(Department value)
    {
      // does the Tenant already have a Department with the same id?
      if (await Repository.Query().AnyAsync(e => e.Id == value.Id))
      {
        throw ConflictException.IdConflict(nameof(Department), value.Id);
      }

      // does the Tenant already have a BranchOffice of the same name?
      if (await Repository.Query().AnyAsync(e => 
        e.Name == value.Name && 
        e.BranchOfficeId == value.BranchOfficeId && 
        e.TenantId == value.TenantId))
      {
        throw BadRequestException.NamedEntityExists(nameof(Department), value.Name);
      }
    }

    protected override async Task CheckUpdatedItemIsUnique(Department value)
    {    
      // does the Tenant already have a BranchOffice of the same name?
      if ( await Repository.Query().AnyAsync(e =>
        e.Name == value.Name && 
        e.BranchOfficeId == value.BranchOfficeId && 
        e.TenantId == value.TenantId &&
        e.Id != value.Id
        ))
      {
        throw BadRequestException.NamedEntityExists(nameof(Department), value.Name);
      }
    }

    protected override async Task CheckEntityReferences(Department value)
    {
      // it is not allowed to refer to a branch office
      // that does not belong to the same tenant
      await ReferenceValidatorForTenant.ValidateRequiredEntityReference(value, _branchOfficeRepository, value.BranchOfficeId);
    }
  }
}
