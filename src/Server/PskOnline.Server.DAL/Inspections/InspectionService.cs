namespace PskOnline.Server.DAL.Inspections
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Logging;

  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Shared.Contracts.Service;
  using PskOnline.Server.Shared.EFCore;
  using PskOnline.Server.Shared.Exceptions;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Shared.Repository;

  public class InspectionService : IInspectionService
  {
    private readonly IGuidKeyedRepository<Inspection> _repository;
    private readonly IGuidKeyedRepository<Test> _testRepository;
    private readonly InspectionServiceImpl _serviceImpl;
    private readonly IInspectionCompletionEventHandler _completionHandler;

    public InspectionService(
      ITenantEntityAccessChecker tenantAccessChecker,
      IAccessScopeFilter tenantContext,
      IGuidKeyedRepository<Inspection> repository,
      IGuidKeyedRepository<Test> testRepository,
      IGuidKeyedRepository<Employee> employeeRepository,
      IInspectionCompletionEventHandler completionHandler,
      ILoggerFactory loggerFactory)
    {
      _serviceImpl = new InspectionServiceImpl(
        tenantAccessChecker, tenantContext, repository, employeeRepository, loggerFactory.CreateLogger<BaseService<Inspection>>());
      _completionHandler = completionHandler;
      _repository = repository;
      _testRepository = testRepository;
    }


    public async Task<Guid> BeginInspectionAsync(Inspection inspection)
    {
      if (inspection.IsFinished)
      {
        throw new BadRequestException("Starting a finished inspection (FinishTime != null) is not allowed");
      }
      return await _serviceImpl.AddAsync(inspection);
    }

    public Task<long> GetInspectionCountSinceAsync(Guid tenantId, DateTimeOffset timeStamp)
    {
      return _repository.Query().LongCountAsync(i => i.TenantId == tenantId && i.FinishTime > timeStamp);
    }

    public async Task<IEnumerable<PluginOutputDescriptor>> CompleteInspectionAsync(Guid inspectionId, DateTimeOffset completionTime, CancellationToken cancellationToken)
    {
      var inspection = await _repository.GetAsync(inspectionId);
      if (null == inspection)
      {
        throw new ItemNotFoundException(inspectionId.ToString(), nameof(Inspection));
      }

      if (inspection.IsFinished)
      {
        return await HandleCompletionAsync(inspectionId, cancellationToken);
      }
      var numTests = _testRepository.Query().Count(t => t.InspectionId == inspectionId);

      if (numTests == 0)
      {
        throw new BadRequestException("Inspection doesn't have tests and may not be completed");
      }

      inspection.FinishTime = completionTime;
 
      await _serviceImpl.UpdateAsync(inspection);
      return await HandleCompletionAsync(inspectionId, cancellationToken);
    }

    private async Task<IEnumerable<PluginOutputDescriptor>> HandleCompletionAsync(Guid inspectionId, CancellationToken cancellationToken)
    {
      return await _completionHandler.HandleInspectionCompletionAsync(inspectionId, cancellationToken);
    }

    public Task<IEnumerable<Inspection>> GetAllAsync(int? skip, int? take)
    {
      return _serviceImpl.GetAllAsync(skip, take); 
    }

    public Task<Inspection> GetAsync(Guid id)
    {
      return _serviceImpl.GetAsync(id);
    }

    public Task RemoveAsync(Guid id)
    {
      return _serviceImpl.RemoveAsync(id);
    }

    public async Task<Inspection> GetInspectionWithTestsAsync(Guid inspectionId)
    {
      return await _repository.Query()
        .Where(i => i.Id == inspectionId)
        .Include(i => i.Tests).FirstOrDefaultAsync();
    }
  }
}
