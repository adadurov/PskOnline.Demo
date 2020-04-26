namespace PskOnline.Service.Test.Tenant
{
  using System;
  using Microsoft.Extensions.Logging;
  using NSubstitute;
  using NUnit.Framework;

  using PskOnline.Components.Log;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Service.Multitenant;
  using PskOnline.Server.Shared.Permissions;
  using PskOnline.Server.Shared.Multitenancy;

  [TestFixture]
  class TenantAccessChecker_TenantUser_Tests
  {
    [SetUp]
    public void Setup()
    {
      LogHelper.ConfigureConsoleLogger();
    }

    [TearDown]
    public void TearDown()
    {
      LogHelper.ShutdownLogSystem();
    }

    private ITenantIdProvider BuildTenantIdProviderForTenantUser()
    {
      var tenantIdProvider = Substitute.For<ITenantIdProvider>();
      tenantIdProvider.GetTenantId().Returns(Guid.NewGuid());
      return tenantIdProvider;
    }

    private IAccessChecker GetAccessCheckerInstance(ITenantIdProvider tenantIdProvider)
    {
      return new TenantEntityAccessChecker(
              tenantIdProvider,
              Substitute.For<ILogger<TenantEntityAccessChecker>>());
    }

    [Test]
    public void AccessChecker_Should_NOT_Permit_SiteUser_Create_Entity_Without_Tenant()
    {
      // Given
      var tenantUserContext = Substitute.For<IAccessScopeFilter>();
      var tenantIdProvider = BuildTenantIdProviderForTenantUser();

      var accessChecker = GetAccessCheckerInstance(tenantIdProvider);

      var entity = new TenantOwnedEntity() { TenantId = Guid.Empty };

      // When
      TestDelegate action = () => accessChecker.ValidateAccessToEntityAsync(entity, EntityAction.Create);

      // Then
      // exception is thrown
      Assert.Throws<UnauthorizedAccessException>(action);
    }

    [Test]
    public void AccessChecker_Should_Permit_All_Operations_On_Null_Entity()
    {
      // Given
      var tenantUserContext = Substitute.For<IAccessScopeFilter>();
      var tenantIdProvider = BuildTenantIdProviderForTenantUser();

      var accessChecker = GetAccessCheckerInstance(tenantIdProvider);

      TenantOwnedEntity entity = null;

      // When
      accessChecker.ValidateAccessToEntityAsync(entity, EntityAction.Create);
      accessChecker.ValidateAccessToEntityAsync(entity, EntityAction.Read);
      accessChecker.ValidateAccessToEntityAsync(entity, EntityAction.Update);
      accessChecker.ValidateAccessToEntityAsync(entity, EntityAction.Delete);

      // Then
      // no exception is thrown
    }

    [Test]
    public void AccessChecker_Should_NOT_Permit_SiteUser_Read_Entity_Without_Tenant()
    {
      // Given
      var tenantIdProvier = BuildTenantIdProviderForTenantUser();

      var accessChecker = GetAccessCheckerInstance(tenantIdProvier);

      var entity = new TenantOwnedEntity() { TenantId = Guid.Empty };

      // When
      TestDelegate action = () => accessChecker.ValidateAccessToEntityAsync(entity, EntityAction.Read);

      // Then
      // exception is thrown
      Assert.Throws<UnauthorizedAccessException>(action);
    }

    [Test]
    public void AccessChecker_Should_NOT_Permit_SiteUser_Update_Entity_Without_Tenant()
    {
      // Given
      var tenantIdProvider = BuildTenantIdProviderForTenantUser();

      var accessChecker = GetAccessCheckerInstance(tenantIdProvider);

      var entity = new TenantOwnedEntity() { TenantId = Guid.Empty };

      // When
      TestDelegate action = () => accessChecker.ValidateAccessToEntityAsync(entity, EntityAction.Update);

      // Then
      // exception is thrown
      Assert.Throws<UnauthorizedAccessException>(action);
    }

    [Test]
    public void AccessChecker_Should_NOT_Permit_SiteUser_Delete_Entity_Without_Tenant()
    {
      // Given
      var tenantIdProvider = BuildTenantIdProviderForTenantUser();

      var accessChecker = GetAccessCheckerInstance(tenantIdProvider);

      var entity = new TenantOwnedEntity() { TenantId = Guid.Empty };

      // When
      TestDelegate action = () => accessChecker.ValidateAccessToEntityAsync(entity, EntityAction.Delete);

      // Then
      // exception is thrown
      Assert.Throws<UnauthorizedAccessException>(action);
    }

    [Test]
    public void AccessChecker_Should_NOT_Permit_SiteUser_Create_Entity_Wrong_Tenant()
    {
      // Given
      var tenantIdProvider = BuildTenantIdProviderForTenantUser();

      var accessChecker = GetAccessCheckerInstance(tenantIdProvider);

      var entity = new TenantOwnedEntity() { TenantId = Guid.NewGuid() };

      // When
      TestDelegate action = () => accessChecker.ValidateAccessToEntityAsync(entity, EntityAction.Create);

      // Then
      // exception is thrown
      Assert.Throws<UnauthorizedAccessException>(action);
    }

    [Test]
    public void AccessChecker_Should_NOT_Permit_SiteUser_Read_Entity_Wrong_Tenant()
    {
      // Given
      var tenantIdProvider = BuildTenantIdProviderForTenantUser();

      var accessChecker = GetAccessCheckerInstance(tenantIdProvider);

      var entity = new TenantOwnedEntity() { TenantId = Guid.NewGuid() };

      // When
      TestDelegate action = () => accessChecker.ValidateAccessToEntityAsync(entity, EntityAction.Read);

      // Then
      // exception is thrown
      Assert.Throws<UnauthorizedAccessException>(action);
    }

    [Test]
    public void AccessChecker_Should_NOT_Permit_SiteUser_Update_Entity_Wrong_Tenant()
    {
      // Given
      var tenantIdProvider = BuildTenantIdProviderForTenantUser();

      var accessChecker = GetAccessCheckerInstance(tenantIdProvider);

      var entity = new TenantOwnedEntity() { TenantId = Guid.NewGuid() };

      // When
      TestDelegate action = () => accessChecker.ValidateAccessToEntityAsync(entity, EntityAction.Update);

      // Then
      // exception is thrown
      Assert.Throws<UnauthorizedAccessException>(action);
    }

    [Test]
    public void AccessChecker_Should_NOT_Permit_SiteUser_Delete_Entity_Wrong_Tenant()
    {
      // Given
      var tenantUserContext = BuildTenantIdProviderForTenantUser();

      var tenantIdProvider = GetAccessCheckerInstance(tenantUserContext);

      var entity = new TenantOwnedEntity() { TenantId = Guid.NewGuid() };

      // When
      TestDelegate action = () => tenantIdProvider.ValidateAccessToEntityAsync(entity, EntityAction.Delete);

      // Then
      // exception is thrown
      Assert.Throws<UnauthorizedAccessException>(action);
    }

    [Test]
    public void AccessChecker_Should_Permit_SiteUser_Create_Entity_Own_Tenant()
    {
      // Given
      var tenantIdProvider = BuildTenantIdProviderForTenantUser();

      var accessChecker = GetAccessCheckerInstance(tenantIdProvider);

      var entity = new TenantOwnedEntity() { TenantId = tenantIdProvider.GetTenantId() };

      // When
      accessChecker.ValidateAccessToEntityAsync(entity, EntityAction.Create);

      // Then
      // no exception is thrown
    }

    [Test]
    public void AccessChecker_Should_Permit_SiteUser_Read_Entity_Own_Tenant()
    {
      // Given
      var tenantIdProvider = BuildTenantIdProviderForTenantUser();

      var accessChecker = GetAccessCheckerInstance(tenantIdProvider);

      var entity = new TenantOwnedEntity() { TenantId = tenantIdProvider.GetTenantId() };

      // When
      accessChecker.ValidateAccessToEntityAsync(entity, EntityAction.Read);

      // Then
      // no exception is thrown
    }

    [Test]
    public void AccessChecker_Should_Permit_SiteUser_Update_Entity_Own_Tenant()
    {
      // Given
      var tenantIdProvider = BuildTenantIdProviderForTenantUser();

      var accessChecker = GetAccessCheckerInstance(tenantIdProvider);

      var entity = new TenantOwnedEntity() { TenantId = tenantIdProvider.GetTenantId() };

      // When
      accessChecker.ValidateAccessToEntityAsync(entity, EntityAction.Update);

      // Then
      // no exception is thrown
    }

    [Test]
    public void AccessChecker_Should_Permit_SiteUser_Delete_Entity_Own_Tenant()
    {
      // Given
      var tenantIdProvider = BuildTenantIdProviderForTenantUser();

      var accessChecker = GetAccessCheckerInstance(tenantIdProvider);

      var entity = new TenantOwnedEntity() { TenantId = tenantIdProvider.GetTenantId() };

      // When
      accessChecker.ValidateAccessToEntityAsync(entity, EntityAction.Delete);

      // Then
      // no exception is thrown
    }

  }
}
