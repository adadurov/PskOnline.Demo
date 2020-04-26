namespace PskOnline.Service.Test.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Net;
  using AutoMapper;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Logging;
  using NSubstitute;
  using NUnit.Framework;

  using PskOnline.Components.Log;
  using PskOnline.Server.Authority.API;
  using PskOnline.Server.Authority.API.Dto;
  using PskOnline.Server.DAL.Multitenancy;
  using PskOnline.Server.DAL.OrgStructure.Interfaces;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Service;
  using PskOnline.Server.Service.Controllers;
  using PskOnline.Server.Service.ViewModels;
  using PskOnline.Server.Shared.Contracts.Service;
  using PskOnline.Server.Shared.Exceptions;
  using PskOnline.Server.Shared.Service;

  [TestFixture]
  public class TenantContoller_Tests
  {
    [SetUp]
    public void Setup()
    {
      LogHelper.ConfigureConsoleLogger();

      StaticStartup.Startup();
    }

    [TearDown]
    public void TearDown()
    {
      LogHelper.ShutdownLogSystem();
    }

    [Test]
    public void Post_Should_Return_Created_Response_And_Call_Add()
    {
      // Given
      var newId = Guid.NewGuid();
      var tenantService = Substitute.For<IService<Tenant>>();
      var tenantCreator = Substitute.For<ITenantCreator>();
      var logger = Substitute.For<ILogger<TenantController>>();
      var accountService = Substitute.For<IAccountService>();
      var inspectionService = Substitute.For<IInspectionService>();
      var employeeService = Substitute.For<IEmployeeService>();
      var branchOfficeService = Substitute.For<IService<BranchOffice>>();
      var departmentService = Substitute.For<IService<Department>>();

      var tenantController = new TenantController(
        tenantService, 
        tenantCreator, 
        accountService, 
        inspectionService, 
        employeeService,
        branchOfficeService,
        departmentService,
        logger);

      var createTenantDto = new TenantCreateDto {
        TenantDetails = new TenantDto
        {
          Name = "Demo tenant 1",
          PrimaryContact = new ContactInfoDto
          {
            FullName = "John Doe"
          },
          ServiceDetails = new ServiceDetailsDto
          {
            ServiceExpireDate = DateTime.Now.Date + TimeSpan.FromDays(366)
          }
        },
        AdminUserDetails = new TenantCreateAdminDto
        {
          FirstName = "Ivanov",
          LastName = "Konstantin",
          Patronymic = "Mikhaylovich",
          UserName = "ivkomikh",
          Email = "ivkomikh@mail.to",
          NewPassword = "Qwerty123$"
        }
      };

      // When
      var response = tenantController.Post(createTenantDto);

      // Then
      tenantCreator.Received().CreateNewTenant(
          Arg.Is<Tenant>( v => v.Name == createTenantDto.TenantDetails.Name),
          Arg.Is<UserDto>( v => v.FirstName == createTenantDto.AdminUserDetails.FirstName),
          Arg.Is<string>( v => v == createTenantDto.AdminUserDetails.NewPassword)
        );
      var createdResponse = (CreatedResult)response.Result;
      Assert.That(createdResponse.Location, Is.EqualTo(nameof(TenantController.GetTenant)));
    }

    [Test]
    public void Get_Given_Id_Should_Return_OkObjectResult_With_Tenant()
    {
      // Given
      var id = Guid.NewGuid();
      var tenant = new Tenant() { Id = id };
      var tenantService = Substitute.For<IService<Tenant>>();
      tenantService.GetAsync(Arg.Any<Guid>()).Returns(tenant);
      var tenantCreator = Substitute.For<ITenantCreator>();
      var accountService = Substitute.For<IAccountService>();
      var inspectionService = Substitute.For<IInspectionService>();
      var employeeService = Substitute.For<IEmployeeService>();
      var branchOfficeService = Substitute.For<IService<BranchOffice>>();
      var departmentService = Substitute.For<IService<Department>>();

      var logger = Substitute.For<ILogger<TenantController>>();

      var tenantController = new TenantController(
        tenantService,
        tenantCreator,
        accountService,
        inspectionService,
        employeeService,
        branchOfficeService,
        departmentService,
        logger);

      // When
      var actionResult = tenantController.GetTenant(id).Result;

      // Then
      tenantService.Received().GetAsync(id);

      var objectResult = (OkObjectResult)actionResult;
      var tenantModel = (TenantDto)objectResult.Value;

      Assert.That(tenantModel.Id, Is.EqualTo(tenant.Id.ToString()));
    }

    [Test]
    public void Get_ShouldFail_GivenTenantDoesntExist()
    {
      // Given
      // TenantController over a mock service that would alway throw exceptions on GetAsync

      var tenantService = Substitute.For<IService<Tenant>>();
      tenantService.When(s => s.GetAsync(Arg.Any<Guid>()))
        .Do(v => throw new ItemNotFoundException(v[0].ToString(), "tenant"));
      var tenantCreator = Substitute.For<ITenantCreator>();
      var logger = Substitute.For<ILogger<TenantController>>();
      var accountService = Substitute.For<IAccountService>();
      var inspectionService = Substitute.For<IInspectionService>();
      var employeeService = Substitute.For<IEmployeeService>();
      var branchOfficeService = Substitute.For<IService<BranchOffice>>();
      var departmentService = Substitute.For<IService<Department>>();

      var tenantController = new TenantController(
        tenantService, 
        tenantCreator, 
        accountService, 
        inspectionService, 
        employeeService,
        branchOfficeService,
        departmentService,
        logger);

      var tenantId = Guid.NewGuid();

      // When
      // Get is called on the controller
      AsyncTestDelegate action = async () => await tenantController.GetTenant(tenantId);

      // Then
      // The controller should thrown ItemNotFoundException
      Assert.ThrowsAsync<ItemNotFoundException>(action);
    }

    [Test]
    public void GetTenants_Should_Return_OkObjectResult_With_EnumerableOfTenants()
    {
      // Given
      var id = Guid.NewGuid();
      var tenant = new Tenant() { Id = id, Name = "Tenant 1" };
      var id2 = Guid.NewGuid();
      var tenant2 = new Tenant() { Id = id2, Name = "Tenant 2" };

      var tenantService = Substitute.For<IService<Tenant>>();
      tenantService.GetAllAsync(Arg.Any<int?>(), Arg.Any<int?>()).Returns(new[] { tenant, tenant2 });

      var tenantCreator = Substitute.For<ITenantCreator>();
      var logger = Substitute.For<ILogger<TenantController>>();
      var accountService = Substitute.For<IAccountService>();
      var inspectionService = Substitute.For<IInspectionService>();
      var employeeService = Substitute.For<IEmployeeService>();
      var branchOfficeService = Substitute.For<IService<BranchOffice>>();
      var departmentService = Substitute.For<IService<Department>>();

      var tenantController = new TenantController(
        tenantService, 
        tenantCreator, 
        accountService, 
        inspectionService, 
        employeeService, 
        branchOfficeService,
        departmentService,
        logger);

      // When
      var actionResult = tenantController.GetAll().Result;

      // Then
      tenantService.Received().GetAllAsync(Arg.Any<int?>(), Arg.Any<int?>());

      var objectResult = (OkObjectResult)actionResult;
      var tenantModels = (IEnumerable<TenantDto>)objectResult.Value;

      Assert.That(tenantModels.Any(), Is.True);
      Assert.That(tenantModels.Where(c => c.Id == id.ToString()).Any());
      Assert.That(tenantModels.Where(c => c.Id == id2.ToString()).Any());
    }

    [Test]
    public void Update_ShouldSucceed_GivenTenantExists()
    {
      // Given
      var id = Guid.NewGuid();
      var tenant = new Tenant() {
        Id = id,
        Name = "Tenant 1",
        ServiceDetails = new TenantServiceDetails { },
        PrimaryContact = new ContactInfo { }
      };

      var tenantService = Substitute.For<IService<Tenant>>();
      tenantService.When(s => s.UpdateAsync(Arg.Any<Tenant>())).Do( c => { });
      tenantService.GetAsync(Arg.Any<Guid>()).Returns(tenant);
      var tenantCreator = Substitute.For<ITenantCreator>();
      var logger = Substitute.For<ILogger<TenantController>>();

      var accountService = Substitute.For<IAccountService>();
      var inspectionService = Substitute.For<IInspectionService>();
      var employeeService = Substitute.For<IEmployeeService>();
      var branchOfficeService = Substitute.For<IService<BranchOffice>>();
      var departmentService = Substitute.For<IService<Department>>();

      var tenantController = new TenantController(
        tenantService, 
        tenantCreator, 
        accountService, 
        inspectionService, 
        employeeService,
        branchOfficeService,
        departmentService,
        logger);

      var updatedTenantModel = Mapper.Map<TenantDto>(tenant);

      // When
      var actionResult = tenantController.Put(tenant.Id, updatedTenantModel).Result;

      // Then
      tenantService.Received().UpdateAsync(Arg.Is<Tenant>( c => c.Id == tenant.Id ));

      var result = (NoContentResult)actionResult;

      Assert.That(result.StatusCode, Is.EqualTo((int)HttpStatusCode.NoContent));
    }

    [Test]
    public void Update_ShouldFail_GivenTenantDoesntExist()
    {
      // Given
      // TenantController over a mock service that would alway throw exceptions on Get & GetAsync

      var tenantService = Substitute.For<IService<Tenant>>();
      tenantService.When(s => s.GetAsync(Arg.Any<Guid>()))
        .Do(v => throw new ItemNotFoundException(v[0].ToString(), "tenant"));
      var tenantCreator = Substitute.For<ITenantCreator>();
      var logger = Substitute.For<ILogger<TenantController>>();
      var accountService = Substitute.For<IAccountService>();
      var inspectionService = Substitute.For<IInspectionService>();
      var employeeService = Substitute.For<IEmployeeService>();
      var branchOfficeService = Substitute.For<IService<BranchOffice>>();
      var departmentService = Substitute.For<IService<Department>>();

      var tenantController = new TenantController(
        tenantService, 
        tenantCreator, 
        accountService, 
        inspectionService, 
        employeeService,
        branchOfficeService,
        departmentService,
        logger);

      var tenantId = Guid.NewGuid();
      var tenantDto = new TenantDto
      {
        Id = tenantId.ToString(),
        Name = "Tenant",
        PrimaryContact = new ContactInfoDto
        {
          Email = "210@mail.com"
        },
        ServiceDetails = new ServiceDetailsDto
        {

        }
      };

      // When
      // Put is called on the controller

      AsyncTestDelegate action = async () => await tenantController.Put(tenantId, tenantDto);

      // Then
      // The controller should thrown ItemNotFoundException
      Assert.ThrowsAsync<ItemNotFoundException>(action);
    }

    [Test]
    public void Delete_Should_Return_Deleted()
    {
      // Given
      var id = Guid.NewGuid();
      var tenant = new Tenant() { Id = id, Name = "Tenant 1" };

      var tenantService = Substitute.For<IService<Tenant>>();
      var tenantCreator = Substitute.For<ITenantCreator>();
      var logger = Substitute.For<ILogger<TenantController>>();
      var accountService = Substitute.For<IAccountService>();
      var inspectionService = Substitute.For<IInspectionService>();
      var employeeService = Substitute.For<IEmployeeService>();
      var branchOfficeService = Substitute.For<IService<BranchOffice>>();
      var departmentService = Substitute.For<IService<Department>>();

      var tenantController = new TenantController(
        tenantService, 
        tenantCreator, 
        accountService, 
        inspectionService,
        employeeService,
        branchOfficeService,
        departmentService,
        logger);

      var updatedTenantModel = Mapper.Map<TenantDto>(tenant);

      // When
      var actionResult = tenantController.Delete(tenant.Id).Result;

      // Then
      tenantService.Received().RemoveAsync(Arg.Is<Guid>(c => c == id));

      var okResult = (NoContentResult)actionResult;

      Assert.That(okResult.StatusCode, Is.EqualTo(204));
    }

    [Test]
    public void GetOpsSummary_Should_Collect_Summary()
    {
      // Given
      var id = Guid.NewGuid();
      var tenant = new Tenant() { Id = id, Name = "Tenant 1" };

      var tenantService = Substitute.For<IService<Tenant>>();
      tenantService.GetAsync(Arg.Any<Guid>()).Returns(tenant);
      var tenantCreator = Substitute.For<ITenantCreator>();
      var logger = Substitute.For<ILogger<TenantController>>();
      var accountService = Substitute.For<IAccountService>();
      var inspectionService = Substitute.For<IInspectionService>();
      var employeeService = Substitute.For<IEmployeeService>();
      var branchOfficeService = Substitute.For<IService<BranchOffice>>();
      var departmentService = Substitute.For<IService<Department>>();

      var tenantController = new TenantController(
        tenantService,
        tenantCreator,
        accountService,
        inspectionService,
        employeeService,
        branchOfficeService,
        departmentService,
        logger);

      var updatedTenantModel = Mapper.Map<TenantDto>(tenant);

      // When
      var actionResult = tenantController.GetOperationsSummary(tenant.Id).Result;

      // Then
      tenantService.Received().GetAsync(Arg.Is<Guid>(c => c == id));
      employeeService.Received().GetItemCountInTenantAsync(Arg.Is<Guid>(c => c == id));
      accountService.Received().GetUserCountInTenantAsync(Arg.Is<Guid>(c => c == id));
      inspectionService.Received(2).GetInspectionCountSinceAsync(Arg.Is<Guid>(c => c == id), Arg.Any<DateTimeOffset>());
    }

  }
}
