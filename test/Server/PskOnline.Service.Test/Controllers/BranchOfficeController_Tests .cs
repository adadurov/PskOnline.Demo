namespace PskOnline.Service.Test.Controllers
{
  using Microsoft.Extensions.Logging;
  using NSubstitute;
  using NUnit.Framework;
  using PskOnline.Components.Log;
  using PskOnline.Server.Authority.API;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Service;
  using PskOnline.Server.Service.Controllers;
  using PskOnline.Server.Service.ViewModels;
  using PskOnline.Server.Shared.Exceptions;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Shared.Service;
  using System;
  using System.Threading.Tasks;

  [TestFixture]
  public class BranchOfficeController_Tests
  {
    IService<BranchOffice> _branchOfficeService;
    ITenantIdProvider _tenantIdProvider;
    ITimeService _timeService;
    ILogger<BranchOfficeController> _logger;
    IWorkplaceCredentialsService _credentialsService;

    Guid _newId;
    Guid _tenantId;

    [SetUp]
    public void Setup()
    {
      LogHelper.ConfigureConsoleLogger();

      StaticStartup.Startup();

      _newId = Guid.NewGuid();
      _tenantId = Guid.NewGuid();
      _branchOfficeService = Substitute.For<IService<BranchOffice>>();
      _tenantIdProvider = Substitute.For<ITenantIdProvider>();
      _logger = Substitute.For<ILogger<BranchOfficeController>>();
      _timeService = Substitute.For<ITimeService>();

      _credentialsService = Substitute.For<IWorkplaceCredentialsService>();
      _tenantIdProvider.GetTenantId().Returns(_tenantId);
      _timeService.CheckTimeZoneId(Arg.Any<string>()).Returns(true);
    }

    [TearDown]
    public void TearDown()
    {
      LogHelper.ShutdownLogSystem();
    }

    [Test]
    public void Update_ShouldFail_GivenDepartmentDoesntExist()
    {
      // Given
      // PositionController over a mock service that would alway throw exceptions on Get & GetAsync
      _branchOfficeService.When(s => s.GetAsync(Arg.Any<Guid>()))
        .Do(v => throw new ItemNotFoundException(v[0].ToString(), "branch office"));

      var branchOfficeController = new BranchOfficeController(
        _branchOfficeService, _timeService, _credentialsService, _tenantIdProvider, _logger);

      var branchOfficeId = Guid.NewGuid();
      var branchOfficeDto = new BranchOfficeDto
      {
        Id = branchOfficeId.ToString(),
        Name = "BranchOffice",
        TimeZoneId = "MSK+3"
      };

      // When
      // Put is called on the controller
      async Task action() => await branchOfficeController.Put(branchOfficeId, branchOfficeDto);

      // Then
      // The controller should thrown ItemNotFoundException
      Assert.ThrowsAsync<ItemNotFoundException>(action);
    }

    [Test]
    public async Task Post_ShouldCheckTimeZoneId()
    {
      // Given
      // PositionController over mock services

      var branchOfficeController = new BranchOfficeController(
        _branchOfficeService, _timeService, _credentialsService, _tenantIdProvider, _logger);

      var branchOfficeId = Guid.NewGuid();
      var branchOfficeDto = new BranchOfficeDto
      {
        Id = branchOfficeId.ToString(),
        Name = "BranchOffice",
        TimeZoneId = "XYZ"
      };

      // When
      // Put is called on the controller
      await branchOfficeController.Post(branchOfficeDto);

      // Then
      // The controller should thrown ItemNotFoundException
      _timeService.Received().CheckTimeZoneId(Arg.Any<string>());
    }

    [Test]
    public async Task Update_ShouldCheckTimeZoneId()
    {
      // Given
      // PositionController over mock services

      var branchOfficeController = new BranchOfficeController(
        _branchOfficeService, _timeService, _credentialsService, _tenantIdProvider, _logger);

      var branchOfficeId = Guid.NewGuid();
      var branchOfficeDto = new BranchOfficeDto
      {
        Id = branchOfficeId.ToString(),
        Name = "BranchOffice",
        TimeZoneId = "XYZ"
      };

      // When
      // Put is called on the controller
      await branchOfficeController.Put(branchOfficeId, branchOfficeDto);

      // Then
      // The controller should thrown ItemNotFoundException
      _timeService.Received().CheckTimeZoneId(Arg.Any<string>());
    }

    [Test]
    public void Get_ShouldFail_GivenDepartmentDoesntExist()
    {
      // Given
      // PositionController over a mock service that would alway throw exceptions on Get & GetAsync
      _branchOfficeService.When(s => s.GetAsync(Arg.Any<Guid>()))
        .Do(v => throw new ItemNotFoundException(v[0].ToString(), "branch office"));

      var branchOfficeController = new BranchOfficeController(
        _branchOfficeService, _timeService, _credentialsService, _tenantIdProvider, _logger);

      var branchOfficeId = Guid.NewGuid();

      // When
      // Put is called on the controller

      async System.Threading.Tasks.Task action() => await branchOfficeController.GetBranchOffice(branchOfficeId);

      // Then
      // The controller should thrown ItemNotFoundException
      Assert.ThrowsAsync<ItemNotFoundException>(action);
    }
  }
}
