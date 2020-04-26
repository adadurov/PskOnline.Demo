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
  using PskOnline.Server.Shared.Permissions;
  using PskOnline.Server.Shared.Repository;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Shared.EFCore;

  [TestFixture]
  public sealed class BaseService_Test
  {
    [SetUp]
    public void Setup()
    {
      LogHelper.ConfigureConsoleLogger();

      _accessChecker = Substitute.For<IAccessChecker>();
      var tenantGuid = Guid.NewGuid();

      _tenantIdProvider = Substitute.For<ITenantIdProvider>();
      _tenantIdProvider.GetTenantId().Returns(tenantGuid);

      _entity = new TenantOwnedEntity { Id = Guid.NewGuid(), TenantId = tenantGuid };
      _wrongTenantEntity = new TenantOwnedEntity { Id = Guid.NewGuid(), TenantId = Guid.NewGuid() };
      _nonTenantEntity = new TenantOwnedEntity { Id = Guid.NewGuid(), TenantId = Guid.Empty };

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

      _tenantContext = Substitute.For<IAccessScopeFilter>();
      if( _tenantIdProvider.GetTenantId() == _entity.TenantId)
      {
        _tenantContext.AddScopeFilter(Arg.Any<IQueryable<TenantOwnedEntity>>()).Returns(
          a => ((IQueryable<TenantOwnedEntity>)a[0]).Where(e => e.TenantId == _tenantIdProvider.GetTenantId() )
          );
      }
      else
      {
        _tenantContext.AddScopeFilter(Arg.Any<IQueryable<TenantOwnedEntity>>()).Returns(
          a => ((IQueryable<TenantOwnedEntity>)a[0])
          );
      }
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

    IAccessScopeFilter _tenantContext;

    ITenantIdProvider _tenantIdProvider;

    [Test]
    public void Create_Should_Check_Access_And_Generate_New_Guid()
    {
      // Given
      _entity.Id = Guid.Empty;

      // When
      _service.AddAsync(_entity).Wait();

      // Then
      _accessChecker.Received().ValidateAccessToEntityAsync(_entity, EntityAction.Create);
      _repository.Received().Add(Arg.Is<TenantOwnedEntity>(e => e.Id != Guid.Empty));
      _repository.Received().SaveChangesAsync();
    }

    [Test]
    public void Get_Should_Check_Access_And_Get_From_Repo()
    {
      // Given
      _repository.Get(_entity.Id).Returns(_entity);

      // When
      _service.Get(_entity.Id);

      // Then
      _accessChecker.Received().ValidateAccessToEntityAsync(_entity, EntityAction.Read);
      _repository.Received().Get(Arg.Is<Guid>(v => v == _entity.Id));
    }

    [Test]
    public async Task GetAsync_Should_Check_Access_And_Get_From_Repo()
    {
      // Given
      _repository.GetAsync(_entity.Id).Returns(_entity);

      // When
      var v = await _service.GetAsync(_entity.Id);

      // Then
      Assert.AreSame(_entity, v);
      await _accessChecker.Received().ValidateAccessToEntityAsync(_entity, EntityAction.Read);
      Received.InOrder(async () => await _repository.GetAsync(Arg.Is<Guid>(g => g == _entity.Id)));
    }

    [Test]
    public void Update_Should_Check_Access_And_Update_Repo()
    {
      // Given

      // When
      _service.UpdateAsync(_entity).Wait();

      // Then
      _accessChecker.Received().ValidateAccessToEntityAsync(_entity, EntityAction.Update);
      _repository.Received().Update(_entity);
    }

    [Test]
    public void Delete_Should_Check_Access_And_Delete_From_Repo()
    {
      // Given
      _repository.Get(_entity.Id).Returns(_entity);

      // When
      _service.RemoveAsync(_entity.Id).Wait();

      // Then
      _accessChecker.Received().ValidateAccessToEntityAsync(_entity, EntityAction.Delete);
      _repository.Received().Get(_entity.Id);
      _repository.Received().Remove(_entity);
      _repository.Received().SaveChangesAsync();
    }

    [Test]
    public void GetAll_With_Tenant_User_Should_Return_Only_Tenant_Entities()
    {
      // Given

      // When
      var availableEntities = _service.GetAllAsync(null, null).Result;

      // Then
      Assert.That(availableEntities.Count(), Is.EqualTo(1));
      Assert.That(availableEntities.First().TenantId, Is.EqualTo(_entity.TenantId));
      Assert.That(availableEntities.First(), Is.EqualTo(_entity));
      _repository.Received().Query();
    }

    [Test]
    public void Get_All_With_Site_User_Should_Return_All_Entities()
    {
      // Given
      _tenantIdProvider.GetTenantId().Returns(Guid.Empty);

      SetupRepositoryAndService();

      // When
      var entities = _service.GetAllAsync(null, null).Result;

      // Then
      Assert.That(entities.Count(), Is.EqualTo(3));
      _repository.Received().Query();
    }

    [Test]
    public void Get_All_With_Site_User_With_Paging_Should_Succeed()
    {
      // Given
      _tenantIdProvider.GetTenantId().Returns(Guid.Empty);

      SetupRepositoryAndService();

      // When
      var entities1 = _service.GetAllAsync(0, 1).Result;
      var entities2 = _service.GetAllAsync(1, 1).Result;
      var entities3 = _service.GetAllAsync(2, 1).Result;

      // Then
      _repository.Received().Query();

      Assert.That(entities1.Count(), Is.EqualTo(1));
      Assert.That(entities2.Count(), Is.EqualTo(1));
      Assert.That(entities3.Count(), Is.EqualTo(1));

      Assert.That(entities1.First(), Is.Not.EqualTo(entities2.First()));
      Assert.That(entities1.First(), Is.Not.EqualTo(entities3.First()));
      Assert.That(entities2.First(), Is.Not.EqualTo(entities3.First()));
    }

    [Test]
    public void Get_All_With_Site_User_Skip_All_Should_Return_0_Entities()
    {
      // Given
      _tenantIdProvider.GetTenantId().Returns(Guid.Empty);

      SetupRepositoryAndService();

      // When
      var entities4 = _service.GetAllAsync(3, 1).Result;

      // Then
      _repository.Received().Query();

      Assert.That(entities4.Count(), Is.EqualTo(0));
    }
  }
}
