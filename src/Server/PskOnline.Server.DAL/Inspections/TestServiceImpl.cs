namespace PskOnline.Server.DAL.Inspections
{
  using Microsoft.Extensions.Logging;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Shared.EFCore;
  using PskOnline.Server.Shared.Exceptions;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Shared.Repository;
  using System;
  using System.Threading.Tasks;

  internal class TestServiceImpl : BaseService<Test>
  {
    private readonly IGuidKeyedRepository<Inspection> _inspectionRepository;

    public TestServiceImpl(
      ITenantEntityAccessChecker tenantAccessChecker,
      IAccessScopeFilter tenantContext,
      IGuidKeyedRepository<Test> repository,
      IGuidKeyedRepository<Inspection> inspectionRepository,
      ILogger<TestServiceImpl> logger)
      : base(tenantAccessChecker, tenantContext, repository, logger)
    {
      _inspectionRepository = inspectionRepository;
    }

    protected override async Task UpdateDenormalizedParameters(Test test)
    {
      var inspection = await _inspectionRepository.GetAsync(test.InspectionId);
      if (inspection == null || inspection.TenantId != test.TenantId)
      {
        // we won't let client know about wrong tenants
        throw BadRequestException.BadReference(
          "Test refers to an invalid inspection " + test.InspectionId.ToString());
      }

      test.EmployeeId = inspection.EmployeeId;
      test.DepartmentId = inspection.DepartmentId;
      test.BranchOfficeId = inspection.BranchOfficeId;
    }

    public async override Task<Guid> AddAsync(Test test)
    {
      var existing = await Repository.GetSingleOrDefaultAsync(
        i => i.StartTime == test.StartTime &&
        i.EmployeeId == test.EmployeeId
      );
      if (existing != null)
      {
        throw ItemAlreadyExistsException.MatchingEntityExists(nameof(Test), existing.Id);
      }
      return await base.AddAsync(test);
    }

  }
}
