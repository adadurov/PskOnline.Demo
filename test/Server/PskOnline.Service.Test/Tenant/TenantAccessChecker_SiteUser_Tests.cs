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
  class TenantAccessChecker_SiteUser_Tests
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

    private ITenantIdProvider BuildEntireSiteUserTenantIdMock()
    {
      var context = Substitute.For<ITenantIdProvider>();
      context.GetTenantId().Returns(Guid.Empty);
      return context;
    }

    private IAccessChecker GetAccessCheckerInstance(ITenantIdProvider siteUserTenantIdProvider)
    {
      return new TenantEntityAccessChecker(
              siteUserTenantIdProvider,
              Substitute.For<ILogger<TenantEntityAccessChecker>>());
    }

    [Test]
    public void AccessChecker_Should_Permit_SiteUser_Create_Entity_Without_Tenant()
    {
      // Given
      var siteUserTenantIdProvider = BuildEntireSiteUserTenantIdMock();

      var accessChecker = GetAccessCheckerInstance(siteUserTenantIdProvider);
      
      var entity = new TenantOwnedEntity() { TenantId = Guid.Empty };

      // When
      accessChecker.ValidateAccessToEntityAsync(entity, EntityAction.Create);

      // Then
      // no exception is thrown
    }

    [Test]
    public void AccessChecker_Should_Permit_SiteUser_Read_Entity_Without_Tenant()
    {
      // Given
      var siteUserTenantIdProvider = BuildEntireSiteUserTenantIdMock();

      var accessChecker = GetAccessCheckerInstance(siteUserTenantIdProvider);

      var entity = new TenantOwnedEntity() { TenantId = Guid.Empty };

      // When
      accessChecker.ValidateAccessToEntityAsync(entity, EntityAction.Read);

      // Then
      // no exception is thrown
    }

    [Test]
    public void AccessChecker_Should_Permit_SiteUser_Update_Entity_Without_Tenant()
    {
      // Given
      var siteUserTenantIdProvider = BuildEntireSiteUserTenantIdMock();

      var accessChecker = GetAccessCheckerInstance(siteUserTenantIdProvider);

      var entity = new TenantOwnedEntity() { TenantId = Guid.Empty };

      // When
      accessChecker.ValidateAccessToEntityAsync(entity, EntityAction.Update);

      // Then
      // no exception is thrown
    }

    [Test]
    public void AccessChecker_Should_Permit_SiteUser_Delete_Entity_Without_Tenant()
    {
      // Given
      var siteUserTenantIdProvider = BuildEntireSiteUserTenantIdMock();

      var accessChecker = GetAccessCheckerInstance(siteUserTenantIdProvider);

      var entity = new TenantOwnedEntity() { TenantId = Guid.Empty };

      // When
      accessChecker.ValidateAccessToEntityAsync(entity, EntityAction.Delete);

      // Then
      // no exception is thrown
    }

    [Test]
    public void AccessChecker_Should_Permit_SiteUser_Create_Entity_With_Tenant()
    {
      // Given
      var siteUserTenantIdProvider = BuildEntireSiteUserTenantIdMock();

      var accessChecker = GetAccessCheckerInstance(siteUserTenantIdProvider);

      var entity = new TenantOwnedEntity() { TenantId = Guid.NewGuid() };

      // When
      accessChecker.ValidateAccessToEntityAsync(entity, EntityAction.Create);

      // Then
      // no exception is thrown
    }

    [Test]
    public void AccessChecker_Should_Permit_SiteUser_Read_Entity_With_Tenant()
    {
      // Given
      var siteUserTenantIdProvider = BuildEntireSiteUserTenantIdMock();

      var accessChecker = GetAccessCheckerInstance(siteUserTenantIdProvider);

      var entity = new TenantOwnedEntity() { TenantId = Guid.NewGuid() };

      // When
      accessChecker.ValidateAccessToEntityAsync(entity, EntityAction.Read);

      // Then
      // no exception is thrown
    }

    [Test]
    public void AccessChecker_Should_Permit_SiteUser_Update_Entity_With_Tenant()
    {
      // Given
      var siteUserTenantIdProvider = BuildEntireSiteUserTenantIdMock();

      var accessChecker = GetAccessCheckerInstance(siteUserTenantIdProvider);

      var entity = new TenantOwnedEntity() { TenantId = Guid.NewGuid() };

      // When
      accessChecker.ValidateAccessToEntityAsync(entity, EntityAction.Update);

      // Then
      // no exception is thrown
    }

    [Test]
    public void AccessChecker_Should_Permit_SiteUser_Delete_Entity_With_Tenant()
    {
      // Given
      var siteUserTenantIdProvider = BuildEntireSiteUserTenantIdMock();

      var accessChecker = GetAccessCheckerInstance(siteUserTenantIdProvider);

      var entity = new TenantOwnedEntity() { TenantId = Guid.NewGuid() };

      // When
      accessChecker.ValidateAccessToEntityAsync(entity, EntityAction.Delete);

      // Then
      // no exception is thrown
    }

  }
}
