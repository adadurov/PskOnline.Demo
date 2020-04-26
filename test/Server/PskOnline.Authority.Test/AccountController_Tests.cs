namespace PskOnline.Authority.Test
{
  using System;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Http;
  using NSubstitute;
  using NUnit.Framework;
  using PskOnline.Components.Log;
  using PskOnline.Server.Authority;
  using PskOnline.Server.Authority.API.Dto;
  using PskOnline.Server.Authority.Controllers;
  using PskOnline.Server.Authority.Interfaces;
  using PskOnline.Server.Shared.Exceptions;
  using PskOnline.Server.Shared.Multitenancy;
  using Shouldly;

  [TestFixture]
  public class AccountController_Tests
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

    private (AccountController controller, 
             IAccountManager urAcc,
             IRestrictedAccountManager acc) CreateInstance()
    {
      var accountManager = Substitute.For<IRestrictedAccountManager>();
      var unrestrictedAcc = Substitute.For<IAccountManager>();
      var authorizationService = Substitute.For<IAuthorizationService>();
      var claimsService = Substitute.For<IClaimsService>();
      var accessChecker = Substitute.For<ITenantEntityAccessChecker>();

      return (new AccountController(
          accountManager, unrestrictedAcc, authorizationService, claimsService, 
          accessChecker, new AutoMapperConfig()), 
        unrestrictedAcc, accountManager);
    }

    [Test]
    public async Task Reset_Password_Should_Call_Unrestricted_Account_Manager()
    {
      // Given
      var setup = CreateInstance();

      // When
      await setup.controller.ResetPasswordStart(
        new ResetPasswordStartDto { UserNameOrEmail = "qwerty" }
        );

      // Then
      await setup.urAcc.Received().SendPasswordResetLinkAsync(Arg.Any<string>(), Arg.Any<HttpRequest>());
    }

    [Test]
    public async Task Reset_Password_Should_NOT_Call_Restricted_Account_Manager()
    {
      // Given
      var setup = CreateInstance();

      // When
      await setup.controller.ResetPasswordStart(
        new ResetPasswordStartDto { UserNameOrEmail = "qwerty" }
        );

      // Then
      await setup.acc.DidNotReceive().SendPasswordResetLinkAsync(Arg.Any<string>(), Arg.Any<HttpRequest>());
    }
  }
}
