namespace PskOnline.Server.DAL.Inspections
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Microsoft.Extensions.Logging;

  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Shared.Repository;

  public class TestService : ITestService
  {
    private readonly TestServiceImpl _serviceImpl;
    private readonly IGuidKeyedRepository<Test> _repository;
    private readonly IGuidKeyedRepository<Inspection> _inspectionRepository;

    public TestService(
      ITenantEntityAccessChecker tenantAccessChecker,
      IAccessScopeFilter tenantContext,
      IGuidKeyedRepository<Test> repository,
      IGuidKeyedRepository<Inspection> inspectionRepository,
      ILoggerFactory logFactory)
    {
      _serviceImpl = new TestServiceImpl(tenantAccessChecker, tenantContext, repository, inspectionRepository, logFactory.CreateLogger<TestServiceImpl>());
      _repository = repository;
      _inspectionRepository = inspectionRepository;
    }

    public Task<Guid> AddAsync(Test test)
    {
      return _serviceImpl.AddAsync(test);
    }

    public Task<IEnumerable<Test>> GetAllAsync(int? skip, int? take)
    {
      return _serviceImpl.GetAllAsync(skip, take);
    }

    public Task<Test> GetAsync(Guid id)
    {
      return _serviceImpl.GetAsync(id);
    }

    public Task RemoveAsync(Guid id)
    {
      return _serviceImpl.RemoveAsync(id);
    }
  }
}
