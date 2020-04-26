namespace PskOnline.Service.Test.Services
{
  using System;
  using System.Linq;
  using System.Threading.Tasks;
  using Microsoft.Extensions.Logging;
  using NSubstitute;
  using NUnit.Framework;

  using PskOnline.Components.Log;
  using PskOnline.Server.DAL.Multitenancy;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Service.Multitenant;
  using PskOnline.Server.Shared.Repository;
  using PskOnline.Server.Shared.Multitenancy;

  [TestFixture]
  class TenantService_Integration
  {
    [SetUp]
    public void Setup()
    {
      LogHelper.ConfigureConsoleLogger();

      _entity = new Tenant { Id = Guid.NewGuid(), Name = "1", Slug = "1" };
      _wrongTenantEntity = new Tenant { Id = Guid.NewGuid(), Name = "2", Slug = "2" };
      _wrongTenantEntity2 = new Tenant { Id = Guid.NewGuid(), Name = "3", Slug = "3" };

      var tenantGuid = _entity.Id;

      SetupRepositoryAndService(tenantGuid);
    }

    [TearDown]
    public void TearDown()
    {
      LogHelper.ShutdownLogSystem();
    }

    private void SetupRepositoryAndService(Guid userTenant)
    {
      _tenantIdProvider = Substitute.For<ITenantIdProvider>();
      _tenantIdProvider.GetTenantId().Returns(userTenant);

      _repository = Substitute.For<IGuidKeyedRepository<Tenant>>();
      _repository.Get(_entity.Id).Returns(_entity);
      _repository.GetAsync(_entity.Id).Returns(_entity);
      _repository.Get(_wrongTenantEntity.Id).Returns(_wrongTenantEntity);
      _repository.GetAsync(_wrongTenantEntity.Id).Returns(_wrongTenantEntity);
      _repository.Get(_wrongTenantEntity2.Id).Returns(_wrongTenantEntity2);
      _repository.GetAsync(_wrongTenantEntity2.Id).Returns(_wrongTenantEntity2);

      _accessChecker = new TenantAccessChecker(
        _tenantIdProvider, Substitute.For<ILogger<TenantEntityAccessChecker>>());

      _repository.Query().Returns(
        new[] { _entity, _wrongTenantEntity, _wrongTenantEntity2 }.AsEntityAsyncQueryable()
        );

      _tenantContext = new TenantScopeFilter(_tenantIdProvider,
        Substitute.For<ILogger<TenantScopeFilter>>());

      _service = new TenantService(
        _accessChecker,
        _tenantContext,
        _repository,
        Substitute.For<ILogger<TenantService>>(),
        Substitute.For<ITenantSlugProvider>()
        );
    }

    IGuidKeyedRepository<Tenant> _repository;

    Tenant _entity;

    Tenant _wrongTenantEntity;

    Tenant _wrongTenantEntity2;

    TenantService _service;

    ITenantAccessChecker _accessChecker;

    ITenantIdProvider _tenantIdProvider;

    IAccessScopeFilter _tenantContext;

    [Test]
    public void Create_By_Site_User_Should_Succeed()
    {
      // Given
      SetupRepositoryAndService(userTenant: Guid.Empty);
      var newTenant = new Tenant { Name = "4" };

      // When
      _service.AddAsync(newTenant).Wait();
      
      // Then
      _repository.Received().Add(newTenant);
      _repository.Received().SaveChangesAsync();
    }

    [Test]
    public void Update_By_Site_User_Should_Succeed()
    {
      // Given
      SetupRepositoryAndService(userTenant: Guid.Empty);

      // When
      _service.UpdateAsync(_entity).Wait();

      // Then
      _repository.Received().Update(_entity);
    }

    [Test]
    public void Delete_By_Site_User_Should_Succeed()
    {
      // Given
      SetupRepositoryAndService(userTenant: Guid.Empty);

      // When
      _service.RemoveAsync(_entity.Id).Wait();

      // Then
      _repository.Received().Remove(_entity);
    }

    [Test]
    public void Read_Any_Tenant_By_Site_User_Should_Succeed()
    {
      // Given
      SetupRepositoryAndService(userTenant: Guid.Empty);

      // When
      var e1 = _service.Get(_entity.Id);
      var e2 = _service.Get(_wrongTenantEntity.Id);
      var e3 = _service.Get(_wrongTenantEntity2.Id);

      // Then
      Assert.That(e1, Is.EqualTo(_entity));
      Assert.That(e2, Is.EqualTo(_wrongTenantEntity));
      Assert.That(e3, Is.EqualTo(_wrongTenantEntity2));
    }

    [Test]
    public void Read_All_Tenants_By_Site_User_Should_Return_All_Tenants()
    {
      // Given
      SetupRepositoryAndService(userTenant: Guid.Empty);

      // When
      var e = _service.GetAllAsync(null, null).Result;

      // Then
      _repository.Received().Query();
      Assert.That(e.Any(v => v.Id == _entity.Id));
      Assert.That(e.Any(v => v.Id == _wrongTenantEntity.Id));
      Assert.That(e.Any(v => v.Id == _wrongTenantEntity2.Id));
      Assert.That(e.Count(), Is.EqualTo(3));
    }

    [Test]
    public async Task Create_ByTenant_User_Should_Throw_Unauthorized()
    {
      // Given
      SetupRepositoryAndService(userTenant: _entity.Id);

      // When
      Func<Task> action = async () => await _service.AddAsync(_wrongTenantEntity);

      // Then
      await AsyncAssert.ThrowsAsync<UnauthorizedAccessException>(action);
    }

    [Test]
    public void Read_Own_Tenant_By_Tenant_User_Should_Succeed()
    {
      // Given
      SetupRepositoryAndService(userTenant: _entity.Id);

      // When
      var e = _service.Get(_entity.Id);

      // Then
      _repository.Received().Get(_entity.Id);
      Assert.That(e, Is.EqualTo(_entity));
    }

    [Test]
    public void Read_All_Tenants_By_Tenant_User_Should_Return_Own_Tenant()
    {
      // Given
      SetupRepositoryAndService(userTenant: _entity.Id);

      // When
      var e = _service.GetAllAsync(null, null).Result;

      // Then
      _repository.Received().Query();
      Assert.That(e.Any(v => v.Id == _entity.Id));
      Assert.That(e.Count(), Is.EqualTo(1) );
    }

    [Test]
    public void Read_Wrong_Tenant_By_Tenant_User_Should_Throw_Unauthorized()
    {
      // Given
      SetupRepositoryAndService(userTenant: _entity.Id);

      // When
      TestDelegate action = () => _service.Get(_wrongTenantEntity.Id);

      // Then
      Assert.Throws<UnauthorizedAccessException>(action);
    }

    [Test]
    public async Task Update_Own_Tenant_By_Tenant_User_Should_Throw_Unauthorized()
    {
      // Given
      SetupRepositoryAndService(userTenant: _entity.Id);

      // When
      Func<Task> action = async () => await _service.UpdateAsync(_entity);

      // Then
      await AsyncAssert.ThrowsAsync<UnauthorizedAccessException>(action);
    }

    [Test]
    public async Task Delete_Own_Tenant_By_Tenant_User_Should_Throw_Unauthorized()
    {
      // Given
      SetupRepositoryAndService(userTenant: _entity.Id);

      // When
      Func<Task> action = async () => await _service.RemoveAsync(_entity.Id);

      // Then
      await AsyncAssert.ThrowsAsync<UnauthorizedAccessException>(action);
    }
  }
}
