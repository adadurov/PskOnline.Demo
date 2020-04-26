namespace PskOnline.Service.Test.Integration.OrgStructure
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Net;
  using System.Net.Http;
  using System.Threading.Tasks;

  using AutoMapper;
  using Microsoft.AspNetCore.Mvc.Testing;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;
  using NUnit.Framework;

  using PskOnline.Client.Api;
  using PskOnline.Client.Api.Authority;
  using PskOnline.Client.Api.Models;
  using PskOnline.Client.Api.OpenId;
  using PskOnline.Client.Api.Organization;
  using PskOnline.Components.Log;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Service.Test.Integration.TestData;

  [TestFixture(
    Author = "Adadurov",
    Description = "Validates various scenarios related to Employees. " +
                  "Notice that this test fixture is initialized only once!")]
  public class EmployeeController_TenantUser_Tests : IDisposable
  {
    DefaultWebApplicationFactory _app;

    HttpClient _httpClient;
    IApiClient _apiClient;

    TenantContainer _gryffindorHouse;

    TenantContainer _slytherinHouse;

    Guid _nonExistingTenantId = Guid.NewGuid();

    [SetUp]
    public async Task SetUp()
    {
      await InitOnce();

      // start each test as Gryffindor user
      await _apiClient.AsGryffindorAdminAsync(_httpClient);
    }

    public void Dispose()
    {
      _httpClient?.Dispose();
      _httpClient = null;
      _app?.Dispose();
      _app = null;
      LogHelper.ShutdownLogSystem();
    }

    private async Task InitOnce()
    {
      if (_app != null) return;

      LogHelper.ConfigureConsoleLogger();

      _app = new DefaultWebApplicationFactory();

      var options = new WebApplicationFactoryClientOptions {AllowAutoRedirect = false };
      _httpClient = _app.CreateClient(options);

      _apiClient = new ApiClient(_httpClient, _app.GetLogger<ApiClient>());

      // site admin creates 2 tenants
      await _apiClient.AsSiteAdminAsync(_httpClient);
      _gryffindorHouse = await TestTenants.CreateTenantWithRolesAndUsers(_apiClient, _httpClient, TestTenants.GryffindorHouse);

      await _apiClient.AsSiteAdminAsync(_httpClient);
      _slytherinHouse = await TestTenants.CreateTenantWithRolesAndUsers(_apiClient, _httpClient, TestTenants.SlytherinHouse);

      // Gryffindor House admin creates branch office and departments
      await _apiClient.AsSlytherinAdminAsync(_httpClient);
      TestBranchOffice.SeedDefaultBranchOffice(_apiClient, _httpClient, _slytherinHouse).Wait();
      TestDepartment.SeedDefaultDepartments(_apiClient, _slytherinHouse).Wait();
      TestPosition.SeedDefaultPosition(_apiClient, _slytherinHouse).Wait();

      await _apiClient.AsGryffindorAdminAsync(_httpClient);
      TestBranchOffice.SeedDefaultBranchOffice(_apiClient, _httpClient, _gryffindorHouse).Wait();
      TestDepartment.SeedDefaultDepartments(_apiClient, _gryffindorHouse).Wait();
      TestPosition.SeedDefaultPosition(_apiClient, _gryffindorHouse).Wait();
    }

    [Test]
    public async Task CreateAndReadEmployee_ShouldSucceed()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      var employeeDto = new EmployeePostDto
      {
        Id = Guid.NewGuid().ToString(),
        FirstName = "John #100",
        LastName = "Smith #100",
        DepartmentId = _gryffindorHouse.Department_1_1.Department.Id,
        PositionId = _gryffindorHouse.Position_Default.Id
      };

      var tenantId = _apiClient.GetIdToken().GetTenantIdClaimValue();
      var opsStatBefore = await _apiClient.GetTenantOperationsSummaryAsync(tenantId);
      var employeesBefore = opsStatBefore.ServiceActualEmployees;

      // When
      // the user posts a new employee...
      var createEmployeeId = await _apiClient.PostEmployeeAsync(employeeDto);

      // Then
      // the request should succeed

      // And the following 'read' request should succeed
      var createdEmployee = await _apiClient.GetEmployeeAsync(createEmployeeId);

      Assert.That(createdEmployee.LastName, Is.EqualTo(employeeDto.LastName));

      var opsStatAfter = await _apiClient.GetTenantOperationsSummaryAsync(tenantId);
      var employeesAfter = opsStatAfter.ServiceActualEmployees;

      // And the following request for the number of employees should return 1 more
      Assert.That(
        employeesBefore + 1,
        Is.EqualTo(employeesAfter));
    }

    [Test]
    [Ignore("No clear business rules for duplicate Employees")]
    public async Task CreateEmployee_ShouldFail_GivenDuplicateName()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // Employee DTO
      var employeeDto = new EmployeePostDto
      {
        FirstName = "John #1",
        LastName = "Smith #1",
        ExternalId = "123345",
        DepartmentId = _gryffindorHouse.Department_1_1.Department.Id,
        PositionId = _gryffindorHouse.Position_Default.Id
      };
      var createFirstEmployeeResponse = await _apiClient.PostEmployeeAsync(employeeDto);

      // When
      // An employee with the same name and ExternalId is posted to the same department
      employeeDto.Id = null;
      AsyncTestDelegate action = async () => await _apiClient.PostEmployeeAsync(employeeDto);

      // Then
      // the request should succeed
      Assert.ThrowsAsync<BadRequestException>(action);
    }

    [Test]
    public void CreateEmployee_ShouldFail_GivenReferenceToDepartmentMissing()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)

      // When
      // The user tries to create a employee without reference to Department
      var employeeDto = new EmployeeDto
      {
        Id = Guid.NewGuid().ToString(),
        FirstName = "Employee Without Reference to Branch Office",
        LastName = "Last Name without reference to branch office",
//        DepartmentId = _gryffindorHouse.Department_One_One.Id,
        PositionId = _gryffindorHouse.Position_Default.Id
      };

      AsyncTestDelegate action = async () => await _apiClient.PostEmployeeAsync(employeeDto);

      // Then
      // the request should fail with 'BadRequest' status code
      Assert.ThrowsAsync<BadRequestException>(action);
    }

    [Test]
    public void CreateEmployee_ShouldFail_GivenMissingReferenceToPosition()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)

      // When
      // The user tries to create a employee without reference to Position
      var employeeDto = new EmployeePostDto
      {
        Id = Guid.NewGuid().ToString(),
        FirstName = "Employee Without Reference to Branch Office",
        LastName = "Last Name without reference to branch office",
        DepartmentId = _gryffindorHouse.Department_1_1.Department.Id,
        //PositionId = _gryffindorHouse.Position_Default.Id
      };

      AsyncTestDelegate action = async () => await _apiClient.PostEmployeeAsync(employeeDto);

      // Then
      // the request should fail with 'BadRequest' status code
      Assert.ThrowsAsync<BadRequestException>(action);
    }

    [Test]
    public void CreateEmployee_ShouldFail_GivenReferenceToDepartmentInOtherTenant()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)

      // When
      // The user tries to create a employee with reference to Branch Office of a different tenant
      var employeeDto = new EmployeePostDto
      {
        Id = Guid.NewGuid().ToString(),
        FirstName = "Employee With Reference to Other Tenant Branch Office",
        LastName = "Employee With Reference to Other Tenant Branch Office",
        DepartmentId = _slytherinHouse.Department_1_1.Department.Id, // this is the problematic one
        PositionId = _gryffindorHouse.Position_Default.Id
      };

      AsyncTestDelegate action = async () => await _apiClient.PostEmployeeAsync(employeeDto);

      // Then
      // the request should fail with 'BadRequest' status code
      Assert.ThrowsAsync<BadRequestException>(action);
    }

    [Test]
    public async Task CreateEmployee_ShouldFail_GivenDuplicateId()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // created a Employee with name "Employee #11"
      var employeeDto = new EmployeePostDto
      {
        Id = Guid.NewGuid().ToString(),
        FirstName = "Employee #11",
        LastName = "Smith #11",
        DepartmentId = _gryffindorHouse.Department_1_1.Department.Id,
        PositionId = _gryffindorHouse.Position_Default.Id
      };
      await _apiClient.PostEmployeeAsync(employeeDto);

      // When
      // The user tries to create another Employee with the same Id
      // and a different name
      employeeDto.FirstName += ".001";
      AsyncTestDelegate action = async () => await _apiClient.PostEmployeeAsync(employeeDto);

      // Then
      // the request should fail with 'Conflict' status code
      Assert.ThrowsAsync<ConflictException>(action);
    }

    [Test]
    public async Task CreateEmployee_ShouldSucceed_GivenDuplicateNames_InTwoTenants()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      var employeeDto = new EmployeePostDto
      {
        FirstName = "First Name #101",
        LastName = "Last Name #101",
        DepartmentId = _gryffindorHouse.Department_1_1.Department.Id,
        PositionId = _gryffindorHouse.Position_Default.Id
      };
      // created an employee with the name above
      await _apiClient.PostEmployeeAsync(employeeDto);

      // switch to tenant #2
      await _apiClient.AsSlytherinAdminAsync(_httpClient);

      // When
      // user from different tenant creates a Employee with the name
      // duplicating name of a Employee in a different tenant...

      var employeeDto2 = new EmployeePostDto
      {
        FirstName = "First Name #101",
        LastName = "Last Name #101",
        DepartmentId = _slytherinHouse.Department_1_1.Department.Id,
        PositionId = _slytherinHouse.Position_Default.Id
      };

      await _apiClient.PostEmployeeAsync(employeeDto2);

      // Then
      // the request should succeed
    }

    [Test]
    public async Task GetAllEmployees_ShouldReturn_OnlythisTenantEmployees()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // created 2 employees just created in Gryffindor House
      // and 1 employee just created in Slytherin House
      var employeeDto1 = new EmployeePostDto
      {
        FirstName = "Employee #1001",
        LastName = "Employee #1001",
        DepartmentId = _gryffindorHouse.Department_1_1.Department.Id,
        PositionId = _gryffindorHouse.Position_Default.Id
      };
      var createFirstEmployeeId = await _apiClient.PostEmployeeAsync(employeeDto1);
      Console.WriteLine("#1001 => " + createFirstEmployeeId);

      var employeeDto = new EmployeePostDto
      {
        FirstName = "Employee #1002",
        LastName = "Employee #1002",
        DepartmentId = _gryffindorHouse.Department_1_1.Department.Id,
        PositionId = _gryffindorHouse.Position_Default.Id
      };
      var createSecondEmployeeId = await _apiClient.PostEmployeeAsync(employeeDto);
      Console.WriteLine("#1002 => " + createSecondEmployeeId);

      await _apiClient.AsSlytherinAdminAsync(_httpClient);

      var slytherinEmployeeDto = new EmployeePostDto
      {
        FirstName = "Slytherin Employee #1002",
        LastName = "Slytherin Employee #1002",
        DepartmentId = _slytherinHouse.Department_1_1.Department.Id,
        PositionId = _slytherinHouse.Position_Default.Id
      };
      var createSlytherinEmployeeId = await _apiClient.PostEmployeeAsync(slytherinEmployeeDto);
      Console.WriteLine(" slytherin => " + createSlytherinEmployeeId);

      // When
      // Gryffindor user requests all employees
      await _apiClient.AsGryffindorAdminAsync(_httpClient);

      var allemployees = await _apiClient.GetEmployeesAsync();

      // Then
      // the request should succeed and return more than 1 employees
      Assert.That(allemployees.Count(), Is.GreaterThanOrEqualTo(2));
    }

    [Test]
    public async Task GetEmployee_ShouldFail_GivenWrongTenant()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // Employee DTO
      var employeeDto = new EmployeeDto
      {
        FirstName = "Employee To Be Forbidden",
        LastName = "Employee To Be Forbidden",
        DepartmentId = _gryffindorHouse.Department_1_1.Department.Id,
        PositionId = _gryffindorHouse.Position_Default.Id
      };

      var newId = await _apiClient.PostEmployeeAsync(employeeDto);

      // When
      // Slytherin user tries to read the new employee created by Gryffindor user...
      await _apiClient.SignInWithUserPassword_WithSlug_Async(
        _slytherinHouse.AdminName, _slytherinHouse.AdminPassword,
        _httpClient, _slytherinHouse.Tenant.Slug);
      AsyncTestDelegate action = async () => await _apiClient.GetEmployeeAsync(newId);

      // Then
      // the request should fail
      Assert.ThrowsAsync<UnauthorizedAccessException>(action);
    }

    [Test]
    public async Task UpdateEmployee_ShouldSucceed()
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // and created Employee named Employee #5
      var employeeName = "Employee #5";
      var employeeDto = new EmployeePostDto
      {
        Id = Guid.NewGuid().ToString(),
        FirstName = employeeName,
        LastName = employeeName,
        DepartmentId = _gryffindorHouse.Department_1_1.Department.Id,
        PositionId = _gryffindorHouse.Position_Default.Id
      };

      var newEmployeeGuid = await _apiClient.PostEmployeeAsync(employeeDto);

      // When
      // the user attempts to rename Employee #5 to Employee #5.1
      employeeDto.FirstName += " Renamed";
      await _apiClient.PutEmployeeAsync(employeeDto);

      // Then
      // the request should succeed and the next get request should return updated data
      var employee = await _apiClient.GetEmployeeAsync(newEmployeeGuid);
      Assert.That(employee.FirstName, Is.EqualTo(employeeDto.FirstName));
    }

    [Test]
    public async Task UpdateEmployee_ShouldFail_GivenEmployeeInOtherTenant()
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // and created Employee named "Employee To Fail Update"
      var employeeDto = new EmployeePostDto
      {
        FirstName = "Employee To Fail Update",
        LastName = "Employee To Fail Update",
        DepartmentId = _gryffindorHouse.Department_1_1.Department.Id,
        PositionId = _gryffindorHouse.Position_Default.Id
      };

      var newId = await _apiClient.PostEmployeeAsync(employeeDto);
      employeeDto.Id = newId.ToString();

      // When
      // a user from a different tenant attempts to update the patient
      await _apiClient.AsSlytherinAdminAsync(_httpClient);
      employeeDto.FirstName += " updated";
      AsyncTestDelegate action = async () => await _apiClient.PutEmployeeAsync(employeeDto);

      // Then
      // the request should fail
      Assert.ThrowsAsync<UnauthorizedAccessException>(action);
    }

    [Test]
    [Ignore("No clear business rules for duplicate Employees")]
    public async Task UpdateEmployee_ShouldFail_GivenDuplicateName()
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // created 2 patients: "Employee #2" & "Employee #3"
      var bo2Name = "Employee #2";
      var employeeDto = new EmployeePostDto
      {
        Id = Guid.NewGuid().ToString(),
        FirstName = bo2Name,
        LastName = bo2Name,
        DepartmentId = _gryffindorHouse.Department_1_1.Department.Id,
        PositionId = _gryffindorHouse.Position_Default.Id
      };

      var employee2Guid = await _apiClient.PostEmployeeAsync(employeeDto);

      employeeDto.FirstName = employeeDto.LastName = "Employee #3";
      employeeDto.Id = Guid.NewGuid().ToString();

      var employee3Guid = await _apiClient.PostEmployeeAsync(employeeDto);

      employeeDto.Id = employee3Guid.ToString();
      employeeDto.FirstName = employeeDto.LastName = bo2Name;

      // When
      // user attempts to rename Employee #3 to Employee #2
      await _apiClient.PutEmployeeAsync(employeeDto);

      // Then
      // the request should not
    }

    [Test]
    public async Task DeleteEmployee_ShouldSucceed_GivenEmployeeInUsersTenant()
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // and created Employee named Employee #5
      var bo2Name = "Employee #500";
      var employeeDto = new EmployeePostDto
      {
        Id = Guid.NewGuid().ToString(),
        FirstName = bo2Name,
        LastName = bo2Name,
        DepartmentId = _gryffindorHouse.Department_1_1.Department.Id,
        PositionId = _gryffindorHouse.Position_Default.Id
      };

      var createEmployeeResponse = await _apiClient.PostEmployeeAsync(employeeDto);

      // When
      // the user attempts to delete the Employee
      await _apiClient.DeleteEmployeeAsync(employeeDto.Id);

      // Then
      // the request should succeed
      // and the next 'get' request should throw 'not found'
      AsyncTestDelegate action = async () => await _apiClient.GetEmployeeAsync(employeeDto.Id);
      Assert.ThrowsAsync<ItemNotFoundException>(action);
    }

    [Test]
    public async Task DeleteEmployee_ShouldFail_GivenEmployeeInOtherTenant()
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // and created Employee named Employee #50
      var bo2Name = "Employee #50";
      var employeeDto = new EmployeePostDto
      {
        Id = Guid.NewGuid().ToString(),
        FirstName = bo2Name,
        LastName = bo2Name,
        DepartmentId = _gryffindorHouse.Department_1_1.Department.Id,
        PositionId = _gryffindorHouse.Position_Default.Id
      };

      var createEmployeeResponse = await _apiClient.PostEmployeeAsync(employeeDto);

      // When
      // a user from a different tenant attempts to delete the Employee
      await _apiClient.AsSlytherinAdminAsync(_httpClient);

      AsyncTestDelegate action = async () => await _apiClient.DeleteEmployeeAsync(employeeDto.Id);

      // Then
      // the request should fail
      Assert.ThrowsAsync<UnauthorizedAccessException>(action);
    }
  }
}
