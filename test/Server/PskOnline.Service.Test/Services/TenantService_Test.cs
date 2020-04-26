namespace PskOnline.Service.Test.Services
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Threading.Tasks;
  using Microsoft.Extensions.Logging;
  using NSubstitute;
  using NUnit.Framework;

  using PskOnline.Components.Log;
  using PskOnline.Server.DAL.Multitenancy;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Shared.Permissions;
  using PskOnline.Server.Shared.Repository;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Shared.Exceptions;

  [TestFixture]
  class TenantService_Test
  {
    [SetUp]
    public void Setup()
    {
      LogHelper.ConfigureConsoleLogger();

      _accessChecker = Substitute.For<ITenantAccessChecker>();

      _entity = new Tenant { Id = Guid.NewGuid(), Name = "1", Slug = "1" };
      _wrongTenantEntity = new Tenant { Id = Guid.NewGuid(), Name = "wte1", Slug = "wte1" };
      _wrongTenantEntity2 = new Tenant { Id = Guid.NewGuid(), Name = "wte2", Slug = "wte2" };

      _tenantIdProvider = Substitute.For<ITenantIdProvider>();
      _tenantIdProvider.GetTenantId().Returns(_entity.Id);

      SetupRepositoryAndService();
    }

    [TearDown]
    public void TearDown()
    {
      LogHelper.ShutdownLogSystem();
    }

    private void SetupRepositoryAndService()
    {
      _repository = Substitute.For<IGuidKeyedRepository<Tenant>>();
      _repository.Get(_entity.Id).Returns(_entity);
      _repository.Get(_wrongTenantEntity.Id).Returns(_wrongTenantEntity);
      _repository.Get(_wrongTenantEntity2.Id).Returns(_wrongTenantEntity2);

      _repository.GetAsync(_entity.Id).Returns(_entity);
      _repository.GetAsync(_wrongTenantEntity.Id).Returns(_wrongTenantEntity);
      _repository.GetAsync(_wrongTenantEntity2.Id).Returns(_wrongTenantEntity2);

      _repository.Query().Returns(
        new[] { _entity, _wrongTenantEntity, _wrongTenantEntity2 }.AsEntityAsyncQueryable()
        );

      _tenantContext = Substitute.For<IAccessScopeFilter>();
      if( _tenantIdProvider.GetTenantId() == _entity.TenantId)
      {
        _tenantContext.AddScopeFilter(Arg.Any<IQueryable<Tenant>>()).Returns(
          a => ((IQueryable<Tenant>)a[0]).Where(e => e.TenantId == _tenantIdProvider.GetTenantId() )
          );
      }
      else
      {
        _tenantContext.AddScopeFilter(Arg.Any<IQueryable<Tenant>>()).Returns(
          a => ((IQueryable<Tenant>)a[0])
          );
      }
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

    IAccessScopeFilter _tenantContext;

    ITenantIdProvider _tenantIdProvider;

    [Test]
    public void Create_Should_Check_Access_And_Generate_New_Guid()
    {
      // Given
      var newTenant = new Tenant { Name = "4", Slug = "4" };

      // When
      var newGuid = _service.AddAsync(newTenant).Result;

      // Then
      _accessChecker.Received().ValidateAccessToEntityAsync(newTenant, EntityAction.Create);
      _repository.Received().Add(Arg.Is<Tenant>(e => e.Id != Guid.Empty));
      _repository.Received().SaveChangesAsync();
      Assert.That(newGuid, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public void Create_With_Duplicate_Name_Should_Throw_ItemAlreadyExistsException_Without_Id()
    {
      // Given
      // entity in repository with an Id assigned
      _entity.Id = Guid.NewGuid();
      _entity.Name = "123";
      _entity.Slug = "876";
      _repository.GetSingleOrDefaultAsync(
        Arg.Any<Expression<Func<Tenant, bool>>>()).Returns(_entity);
      _repository.Query().Returns((new [] { _entity }).AsEntityAsyncQueryable());

      var newTenant = new Tenant
      {
        Name = _entity.Name
      };
      // When
      AsyncTestDelegate action = async () => await _service.AddAsync(newTenant);

      // Then
      var ex = Assert.ThrowsAsync<BadRequestException>(action);
      _repository.DidNotReceive().SaveChangesAsync();
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
  }
}
