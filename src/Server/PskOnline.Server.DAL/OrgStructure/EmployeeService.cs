namespace PskOnline.Server.DAL.OrgStructure
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;

  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Logging;

  using PskOnline.Server.DAL.OrgStructure.Interfaces;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Shared.EFCore;
  using PskOnline.Server.Shared.Exceptions;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Shared.Repository;

  public class EmployeeService : BaseService<Employee>, IEmployeeService
  {
    private readonly IGuidKeyedRepository<BranchOffice> _branchOfficeRepository;
    private readonly IGuidKeyedRepository<Department> _departmentRepository;
    private readonly IGuidKeyedRepository<Position> _positionRepository;

    public EmployeeService(
      IGuidKeyedRepository<BranchOffice> branchOfficeRepository,
      IGuidKeyedRepository<Department> departmentRepository,
      IGuidKeyedRepository<Position> positionRepository,
      ITenantEntityAccessChecker tenantAccessChecker,
      IAccessScopeFilter tenantContext,
      IGuidKeyedRepository<Employee> repository,
      ILogger<EmployeeService> logger
      )
      : base(tenantAccessChecker, tenantContext, repository, logger)
    {
      // this is needed for validation of Position model
      _branchOfficeRepository = branchOfficeRepository;
      _positionRepository = positionRepository;
      _departmentRepository = departmentRepository;
    }

    protected override async Task CheckNewItemIsUnique(Employee value)
    {
      // is there already an Employee entity with the same id?
      if (await Repository.Query().AnyAsync(e => e.Id == value.Id))
      {
        throw ConflictException.IdConflict(nameof(Employee), value.Id);
      }

      // does the Department already have an Employee of the same name?
      if (await Repository.Query().AnyAsync(e => 
        e.FirstName == value.FirstName && 
        e.LastName == value.LastName &&
        e.Patronymic == value.Patronymic &&
        e.BranchOfficeId == value.BranchOfficeId &&
        e.DepartmentId == value.DepartmentId &&
        e.ExternalId == value.ExternalId &&
        e.TenantId == value.TenantId))
      {
        throw BadRequestException.NamedEntityExists(nameof(Employee), value.FullName);
      }
    }

    protected override async Task CheckUpdatedItemIsUnique(Employee value)
    {
      // does the Tenant already have a BranchOffice of the same name?
      if( await Repository.Query().AnyAsync(e =>
        e.FirstName == value.FirstName &&
        e.LastName == value.LastName &&
        e.Patronymic == value.Patronymic &&
        e.BranchOfficeId == value.BranchOfficeId &&
        e.DepartmentId == value.DepartmentId &&
        e.TenantId == value.TenantId &&
        e.ExternalId == value.ExternalId &&
        e.Id != value.Id
        ))
      {
        throw BadRequestException.NamedEntityExists(nameof(Employee), value.FullName);
      }
    }

    protected override async Task CheckEntityReferences(Employee employee)
    {
      // No need to validate BranchOfficeId, 'cause we trust it, because we assign it ourselves
      await ReferenceValidatorForTenant.ValidateRequiredEntityReference(employee, _departmentRepository, employee.DepartmentId);
      await ReferenceValidatorForTenant.ValidateRequiredEntityReference(employee, _positionRepository, employee.PositionId);
    }

    public IEnumerable<Employee> GetEmployeesInDepartment(Department department)
    {
      var query = Repository.Query();
      return AddScopeFilter(query).Where(e => e.DepartmentId == department.Id);
    }

    protected override async Task UpdateDenormalizedParameters(Employee employee)
    {
      var department = await _departmentRepository.GetAsync(employee.DepartmentId);
      if (department == null || department.TenantId != employee.TenantId)
      {
        throw BadRequestException.BadReference(
          "Employee refers to an invalid department: " + employee.DepartmentId.ToString()
        );
      }
      employee.BranchOfficeId = department.BranchOfficeId;
    }

  }
}
