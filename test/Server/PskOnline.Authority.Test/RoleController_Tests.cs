namespace PskOnline.Authority.Test
{
  using System;

  using Microsoft.AspNetCore.Authorization;
  using NSubstitute;
  using NUnit.Framework;
  using PskOnline.Components.Log;
  using PskOnline.Server.Authority;
  using PskOnline.Server.Authority.API.Dto;
  using PskOnline.Server.Authority.Controllers;
  using PskOnline.Server.Authority.Interfaces;
  using PskOnline.Server.Shared;
  using PskOnline.Server.Shared.Exceptions;
  using PskOnline.Server.Shared.Multitenancy;
  using Shouldly;

  [TestFixture]
  public class RoleController_Tests
  {
    ITenantIdProvider _tenantIdProvider;
    IRestrictedRoleService _roleService;

    Guid _newId;
    Guid _tenantMockId;

    [SetUp]
    public void Setup()
    {
      LogHelper.ConfigureConsoleLogger();

      _newId = Guid.NewGuid();
      _tenantMockId = Guid.NewGuid();

      _roleService = Substitute.For<IRestrictedRoleService>();
      _tenantIdProvider = Substitute.For<ITenantIdProvider>();
      _tenantIdProvider.GetTenantId().Returns(_tenantMockId);
    }

    [TearDown]
    public void TearDown()
    {
      LogHelper.ShutdownLogSystem();
    }

    private RolesController CreateInstance()
    {
      var accountManager = Substitute.For<IRestrictedAccountManager>();
      var authorizationService = Substitute.For<IAuthorizationService>();
      var claimsService = Substitute.For<IClaimsService>();

      return new RolesController(
        accountManager, _roleService, claimsService, 
        authorizationService, _tenantIdProvider, new AutoMapperConfig());
    }

    [Test]
    public void ShouldAddTenantId_ToNewEntity()
    {

    }

    [Test]
    public void Get_ShouldFail_GivenRoleDoesntExist()
    {
      // Given
      // RolesController over a mock service that would alway throw exceptions on GetAsync
      var accountManager = Substitute.For<IRestrictedAccountManager>();
      var roleService = Substitute.For<IRestrictedRoleService>();
      var claimsService = Substitute.For<IClaimsService>();
      var authorizationService = Substitute.For<IAuthorizationService>();
      roleService.When(s => s.GetRoleByIdAsync(Arg.Any<Guid>()))
        .Do(v => throw new ItemNotFoundException(v[0].ToString(), "tenant"));
      var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<RolesController>>();
      var tenantIdProvider = Substitute.For<ITenantIdProvider>();

      var roleController = new RolesController(
        accountManager, roleService, claimsService, authorizationService, tenantIdProvider, new AutoMapperConfig());

      var roleId = Guid.NewGuid();

      // When
      // Get is called on the controller
      AsyncTestDelegate action = async () => await roleController.GetRoleById(roleId);

      // Then
      // The controller should thrown ItemNotFoundException
      Assert.ThrowsAsync<ItemNotFoundException>(action);
    }

    [Test]
    public void Update_ShouldFail_GivenRoleDoesntExist()
    {
      // Given
      // RolesController over a mock service that would alway throw exceptions on GetAsync

      var accountManager = Substitute.For<IRestrictedAccountManager>();
      var roleService = Substitute.For<IRestrictedRoleService>();
      var authorizationService = Substitute.For<IAuthorizationService>();
      roleService.When(s => s.GetRoleByIdAsync(Arg.Any<Guid>()))
          .Do(v => throw new ItemNotFoundException(v[0].ToString(), "role"));
      roleService.When(s => s.GetRoleByIdInternalAsync(Arg.Any<Guid>()))
          .Do(v => throw new ItemNotFoundException(v[0].ToString(), "role"));
      var claimsService = Substitute.For<IClaimsService>();
      var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<RolesController>>();
      var tenantIdProvider = Substitute.For<ITenantIdProvider>();

      var roleController = new RolesController(
        accountManager, roleService, claimsService, authorizationService, tenantIdProvider, new AutoMapperConfig());

      var roleId = Guid.NewGuid();
      var roleDto = new RoleDto
      {
        Id = roleId.ToString(),
        Name = "Role",
        Description = "Role description"
      };

      // When
      // Get is called on the controller
      AsyncTestDelegate action = async () => await roleController.UpdateRole(roleId, roleDto);

//      roleService.Received(1).GetRoleByIdInternalAsync(Arg.Any<Guid>());
      roleService.DidNotReceive().GetRoleByIdAsync(Arg.Any<Guid>());

      // Then
      // The controller should thrown ItemNotFoundException
      Assert.ThrowsAsync<ItemNotFoundException>(action);
    }
  }
}
