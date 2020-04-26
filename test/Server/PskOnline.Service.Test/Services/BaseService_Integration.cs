namespace PskOnline.Service.Test.Services
{
  using System;
  using System.Linq;
  using System.Threading.Tasks;
  using Microsoft.Extensions.Logging;
  using NSubstitute;
  using NUnit.Framework;

  using PskOnline.Components.Log;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Service.Multitenant;
  using PskOnline.Server.Shared;
  using PskOnline.Server.Shared.Permissions;
  using PskOnline.Server.Shared.Repository;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Shared.Service;
  using PskOnline.Server.Shared.EFCore;

  [TestFixture]
  class BaseService_Integration
  {
    [SetUp]
    public void Setup()
    {
      LogHelper.ConfigureConsoleLogger();

      var tenantGuid = Guid.NewGuid();
      _tenantIdProvider = Substitute.For<ITenantIdProvider>();
      _tenantIdProvider.GetTenantId().Returns(tenantGuid);

      _accessChecker = new TenantEntityAccessChecker(
        _tenantIdProvider, Substitute.For<ILogger<TenantEntityAccessChecker>>());

      _entity = new TenantOwnedEntity { Id = Guid.NewGuid(), TenantId = tenantGuid };
      _wrongTenantEntity = new TenantOwnedEntity { Id = Guid.NewGuid(), TenantId = Guid.NewGuid() };
      _nonTenantEntity = new TenantOwnedEntity { Id = Guid.NewGuid(), TenantId = Guid.Empty };

      _tenantContext = Substitute.For<IAccessScopeFilter>();
      _tenantContext.AddScopeFilter(Arg.Any<IQueryable<TenantOwnedEntity>>())
        .Returns(e => throw new NotImplementedException());

      SetupRepositoryAndService();
    }

    [TearDown]
    public void TearDown()
    {
      LogHelper.ShutdownLogSystem();
    }

    private void SetupRepositoryAndService()
    {
      _repository = Substitute.For<IGuidKeyedRepository<TenantOwnedEntity>>();
      _repository.Get(_entity.Id).Returns(_entity);
      _repository.GetAsync(_entity.Id).Returns(_entity);
      _repository.Get(_wrongTenantEntity.Id).Returns(_wrongTenantEntity);
      _repository.GetAsync(_wrongTenantEntity.Id).Returns(_wrongTenantEntity);
      _repository.Get(_nonTenantEntity.Id).Returns(_nonTenantEntity);
      _repository.GetAsync(_nonTenantEntity.Id).Returns(_nonTenantEntity);

      _repository.Query().Returns(
        new[] { _entity, _wrongTenantEntity, _nonTenantEntity }.AsQueryable()
        );

      _service = new BaseService<TenantOwnedEntity>(
        _accessChecker,
        _tenantContext,
        _repository,
        Substitute.For<ILogger<BaseService<TenantOwnedEntity>>>());
    }

    IGuidKeyedRepository<TenantOwnedEntity> _repository;

    TenantOwnedEntity _entity;

    TenantOwnedEntity _wrongTenantEntity;

    TenantOwnedEntity _nonTenantEntity;

    BaseService<TenantOwnedEntity> _service;

    IAccessChecker _accessChecker;

    ITenantIdProvider _tenantIdProvider;

    IAccessScopeFilter _tenantContext;

    [Test]
    public void ShouldThrowOnWrongTenantCreate()
    {
      // Given

      // When
      Func<Task> action = async () => await _service.AddAsync(_wrongTenantEntity);

      // Then
      var ex = AsyncAssert.ThrowsAsync<UnauthorizedAccessException>(action).Result;
      //Assert.That(ex.InnerExceptions.Any(e => e is UnauthorizedAccessException));
    }

    [Test]
    public void ShouldThrowOnWrongTenantRead()
    {
      // Given

      // When
      TestDelegate action = () => _service.Get(_wrongTenantEntity.Id);

      // Then
      Assert.Throws<UnauthorizedAccessException>(action);
    }

    [Test]
    public async Task ShouldThrowOnWrongTenantReadAsync()
    {
      // Given

      // When
      Func<Task> action = async () => await _service.GetAsync(_wrongTenantEntity.Id);

      // Then
      var ex = await AsyncAssert.ThrowsAsync<UnauthorizedAccessException>(action);
    }

    [Test]
    public async Task ShouldThrowOnWrongTenantUpdateAsync()
    {
      // Given

      // When
      Func<Task> action = async () => await _service.UpdateAsync(_wrongTenantEntity);

      // Then
      var ex = await AsyncAssert.ThrowsAsync<UnauthorizedAccessException>(action);
    }

    [Test]
    public async Task ShouldThrowOnWrongTenantDelete()
    {
      // Given

      // When
      Func<Task> action = async () => await _service.RemoveAsync(_wrongTenantEntity.Id);

      // Then
      var ex = await AsyncAssert.ThrowsAsync<UnauthorizedAccessException>(action);
    }
  }
}
