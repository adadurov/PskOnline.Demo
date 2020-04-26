namespace PskOnline.Service.Test.Controllers
{
  using Microsoft.Extensions.Logging;
  using NSubstitute;
  using NUnit.Framework;
  using PskOnline.Components.Log;
  using PskOnline.Server.DAL.OrgStructure.Interfaces;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Service;
  using PskOnline.Server.Service.Controllers;
  using PskOnline.Server.Service.ViewModels;
  using PskOnline.Server.Shared.Service;
  using PskOnline.Server.Shared.Multitenancy;
  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Threading.Tasks;
  using PskOnline.Server.Shared.Exceptions;
  using PskOnline.Server.Authority.API;

  [TestFixture]
  public class DepartmentController_Tests
  {
    IService<Department> _departmentService;
    ITenantIdProvider _tenantIdProvider;
    Guid _newId;
    Guid _tenantId;

    [SetUp]
    public void Setup()
    {
      LogHelper.ConfigureConsoleLogger();

      StaticStartup.Startup();

      _newId = Guid.NewGuid();
      _tenantId = Guid.NewGuid();
      _departmentService = Substitute.For<IService<Department>>();
      _tenantIdProvider = Substitute.For<ITenantIdProvider>();
      _tenantIdProvider.GetTenantId().Returns(_tenantId);

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
      _departmentService.When(s => s.GetAsync(Arg.Any<Guid>()))
        .Do(v => throw new ItemNotFoundException(v[0].ToString(), "department"));

      var employeeService = Substitute.For<IEmployeeService>();
      var logger = Substitute.For<ILogger<DepartmentController>>();

      var workplaceCredentialsService = Substitute.For<IWorkplaceCredentialsService>();

      var departmentController = new DepartmentController(
        _departmentService, employeeService, workplaceCredentialsService, _tenantIdProvider, logger);

      var departmentId = Guid.NewGuid();
      var departmentDto = new DepartmentDto
      {
        Id = departmentId.ToString(),
        Name = "Department"
      };

      // When
      // Put is called on the controller
      async Task action() => await departmentController.Put(departmentId, departmentDto);

      // Then
      // The controller should thrown ItemNotFoundException
      Assert.ThrowsAsync<ItemNotFoundException>(action);
    }

    [Test]
    public void Get_ShouldFail_GivenDepartmentDoesntExist()
    {
      // Given
      // PositionController over a mock service that would alway throw exceptions on Get & GetAsync
      _departmentService.When(s => s.GetAsync(Arg.Any<Guid>()))
        .Do(v => throw new ItemNotFoundException(v[0].ToString(), "position"));

      var employeeService = Substitute.For<IEmployeeService>();
      var logger = Substitute.For<ILogger<DepartmentController>>();
      var workplaceCredentialsService = Substitute.For<IWorkplaceCredentialsService>();


      var departmentController = new DepartmentController(
        _departmentService, employeeService, workplaceCredentialsService, _tenantIdProvider, logger);

      var departmentId = Guid.NewGuid();

      // When
      // Get is called on the controller

      async Task action() => await departmentController.GetDepartment(departmentId);

      // Then
      // The controller should thrown ItemNotFoundException
      Assert.ThrowsAsync<ItemNotFoundException>(action);
    }

    [Test]
    public async Task GetDepartmentEmployees_ShouldGetDepartment_AndCallEmployeeService()
    {
      // Given
      // DepartmentController over a mock service that would alway throw exceptions on Get & GetAsync
      var department = new Department();
      _departmentService.GetAsync(Arg.Any<Guid>()).Returns(department);

      var employeeService = Substitute.For<IEmployeeService>();
      var logger = Substitute.For<ILogger<DepartmentController>>();

      var workplaceCredentialsService = Substitute.For<IWorkplaceCredentialsService>();

      var departmentController = new DepartmentController(
        _departmentService, employeeService, workplaceCredentialsService, _tenantIdProvider, logger);

      var departmentId = Guid.NewGuid();

      // When
      // GetEmployees is called on the controller
      await departmentController.GetDepartmentEmployees(departmentId);

      // Then
      // The controller should call departmentService
      await _departmentService.Received().GetAsync(departmentId);

      // And the controller should call employeeservice
      employeeService.Received().GetEmployeesInDepartment(department);
    }

  }
}
