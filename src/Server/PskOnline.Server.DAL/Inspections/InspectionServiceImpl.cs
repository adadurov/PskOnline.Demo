namespace PskOnline.Server.DAL.Inspections
{
  using System.Threading.Tasks;

  using Microsoft.Extensions.Logging;

  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Shared.EFCore;
  using PskOnline.Server.Shared.Exceptions;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Shared.Permissions;
  using PskOnline.Server.Shared.Repository;

  internal class InspectionServiceImpl : BaseService<Inspection>
  {
    private readonly IGuidKeyedRepository<Employee> _employeeRepository;

    internal InspectionServiceImpl(
      IAccessChecker tenantAccessChecker,
      IAccessScopeFilter tenantContext,
      IGuidKeyedRepository<Inspection> repository,
      IGuidKeyedRepository<Employee> employeeRepository,
      ILogger<BaseService<Inspection>> logger)
      : base(tenantAccessChecker, tenantContext, repository, logger)
    {
      _employeeRepository = employeeRepository;
    }

    protected override async Task CheckNewItemIsUnique(Inspection value)
    {
      var existing = await Repository.GetSingleOrDefaultAsync(
        i => i.StartTime == value.StartTime &&
        i.EmployeeId == value.EmployeeId
        );
      if (existing != null)
      {
        throw ItemAlreadyExistsException.MatchingEntityExists(nameof(Inspection), existing.Id);
      }
    }

    protected override async Task UpdateDenormalizedParameters(Inspection inspection)
    {
      var employee = await _employeeRepository.GetAsync(inspection.EmployeeId);
      if (employee == null)
      {
        // we won't let client know about wrong tenants
        throw BadRequestException.BadReference(
          "Inspection refers to an non-existing employee: " + inspection.EmployeeId.ToString()
        );
      }

      inspection.DepartmentId = employee.DepartmentId;
      inspection.BranchOfficeId = employee.BranchOfficeId;
    }

  }
}
