namespace PskOnline.Service.Test.Integration.OrgStructure
{
  using System;
  using System.Linq;
  using System.Collections.Generic;
  using System.Net.Http;
  using System.Threading.Tasks;

  using AutoMapper;
  using Microsoft.AspNetCore.Mvc.Testing;
  using NUnit.Framework;

  using PskOnline.Client.Api;
  using PskOnline.Client.Api.Authority;
  using PskOnline.Client.Api.Organization;
  using PskOnline.Components.Log;
  using PskOnline.Server.Authority.API.Dto;
  using PskOnline.Service.Test.Integration.TestData;
  using Newtonsoft.Json.Linq;
  using Newtonsoft.Json;

  [TestFixture(
    Author = "Adadurov",
    Description = "Validates various scenarios related to Departments. " +
                  "Notice that this test fixture is initialized only once!")]
  public class DepartmentController_TenantUser_Tests : IDisposable
  {
    DefaultWebApplicationFactory _app;
    HttpClient _httpClient;
    IApiClient _apiClient;
    IMapper _mapper;

    TenantContainer _gryffindorHouse;

    TenantContainer _slytherinHouse;

    PskOnline.Client.Api.Authority.UserDto _siteAdminUser;
    Guid _nonExistingTenantId = Guid.NewGuid();

    [SetUp]
    public async Task SetUp()
    {
      _mapper = TestMapperConfiguration.Config.CreateMapper();

      await InitOnce();

      _apiClient = new ApiClient(_httpClient, _app.GetLogger<ApiClient>());

      // the tests start with a Gryffindor user
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

      var options = new WebApplicationFactoryClientOptions()
      {
        AllowAutoRedirect = false
      };
      _httpClient = _app.CreateClient(options);

      _apiClient = new ApiClient(_httpClient, _app.GetLogger<ApiClient>());
      // admin creates Gryffindor tenant and initializes default branch office
      await _apiClient.AsSiteAdminAsync(_httpClient);
      _siteAdminUser = await _apiClient.GetCurrentUserAsync();

      _gryffindorHouse = await TestTenants.CreateTenantWithRolesAndUsers(_apiClient, _httpClient, TestTenants.GryffindorHouse);
      await _apiClient.AsGryffindorAdminAsync(_httpClient);
      await TestBranchOffice.SeedDefaultBranchOffice(_apiClient, _httpClient, _gryffindorHouse);

      // admin creates Slytherin tenant and initializes default branch office
      await _apiClient.AsSiteAdminAsync(_httpClient);
      _slytherinHouse = await TestTenants.CreateTenantWithRolesAndUsers(_apiClient, _httpClient, TestTenants.SlytherinHouse);
      await _apiClient.AsSlytherinAdminAsync(_httpClient);
      await TestBranchOffice.SeedDefaultBranchOffice(_apiClient, _httpClient, _slytherinHouse);

    }

    [Test]
    public async Task CreateDepartment_ShouldSucceed()
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      var departmentDto = new DepartmentDto
      {
        Id = Guid.NewGuid().ToString(),
        Name = "Department #100 with keys",
        BranchOfficeId = _gryffindorHouse.BranchOffice_One.Id.ToString(),
      };

      var numDepartmentsBefore =
        (await _apiClient.GetTenantOperationsSummaryAsync(
        _gryffindorHouse.TenantId.ToString())).DepartmentsCount;

      // When
      // the user posts a new department...
      var newDepartmentId = await _apiClient.PostDepartmentAsync(departmentDto);

      // Then
      // 1. the request should succeed...

      // ... AND return the id of the new department
      Assert.That(newDepartmentId, Is.Not.EqualTo(Guid.Empty));

      // ... AND then 'read' request using the ID should succeed
      var readDepartmentDto = await _apiClient.GetDepartmentAsync(newDepartmentId);

      // ... AND return the DTO with properly specified attributes
      Assert.That(readDepartmentDto.Name, Is.EqualTo(departmentDto.Name));
      Assert.That(readDepartmentDto.BranchOfficeId, Is.EqualTo(departmentDto.BranchOfficeId));

      var numDepartmentsAfter =
        (await _apiClient.GetTenantOperationsSummaryAsync(
        _gryffindorHouse.TenantId.ToString())).DepartmentsCount;

      Assert.That(numDepartmentsAfter, Is.EqualTo(numDepartmentsBefore + 1));

      // ... AND 'create credentials' call should succeed
      var opCredRequest = new WorkplaceCredentialsRequestDto
      {
        Scopes = PskOnlineScopes.DeptOperatorWorkplace
      };
      var opCredResponse = await _httpClient.PostAsJsonAsync($"/api/department/{newDepartmentId}/workplace", opCredRequest);
      opCredResponse.EnsureSuccessStatusCode();
      var opCred = await opCredResponse.Content.ReadAsJsonAsync<WorkplaceCredentialsDto>();

      Assert.That(opCred.ClientSecret, Has.Length.InRange(16, 24));
      Assert.That(opCred.ClientId, Has.Length.InRange(16, 24));

      Console.WriteLine("client_id: " + opCred.ClientId);
      Console.WriteLine("client_secret: " + opCred.ClientSecret);
      {
        // ... AND client can authenticate with the credentials
        using (var apiClient = new ApiClient(
          _app.CreateClient(), _app.GetLogger<ApiClient>()))
        {
          var TokenHolder = await apiClient.SignInWithWorkplaceCredentialsAsync(
            opCred.ClientId, opCred.ClientSecret, PskOnlineScopes.DeptOperatorWorkplace);
        }
      }

      {
        // ... AND list workspaces call should succeed
        var deptWorkpacesResponse = await _httpClient.GetAsync($"/api/department/{newDepartmentId}/workplace");
        deptWorkpacesResponse.EnsureSuccessStatusCode();
        var workplaces = await deptWorkpacesResponse.Content.ReadAsJsonAsync<List<WorkplaceDto>>();

        Console.WriteLine("Workplaces: " + JsonConvert.SerializeObject(workplaces));

        Assert.That(workplaces, Has.Count.InRange(1, 2));
        Assert.That(workplaces.First().DepartmentId, Is.EqualTo(newDepartmentId.ToString()));
        Assert.That(workplaces.First().Scopes, Is.EqualTo("psk_dept_operator_bench"));

        {
          // ... AND update a workspace secret call should succeed
          var workplace = workplaces.First();
          var updateSecretResponse = await _httpClient.PostAsJsonAsync(
            $"/api/workplace/{workplace.ClientId}/new-secret", new { });

          updateSecretResponse.EnsureSuccessStatusCode();
          var newCredentials = await updateSecretResponse.Content.ReadAsJsonAsync<WorkplaceCredentialsDto>();

          Assert.That(newCredentials.ClientId, Is.EqualTo(workplace.ClientId));
          Assert.That(newCredentials.ClientSecret, Has.Length.GreaterThan(15));

          // ... AND client can authenticate with new secret
          using (var apiClient = new ApiClient(
            _app.CreateClient(), _app.GetLogger<ApiClient>()))
          {
            var TokenHolder = await apiClient.SignInWithWorkplaceCredentialsAsync(
              newCredentials.ClientId, newCredentials.ClientSecret, PskOnlineScopes.DeptOperatorWorkplace);
          }

          {
            // ... AND client can NOT authenticate with the OLD credentials

            AsyncTestDelegate code = async () => {
              using (var apiClient = new ApiClient(
                _app.CreateClient(), _app.GetLogger<ApiClient>()))
              {
                var TokenHolder = await apiClient.SignInWithWorkplaceCredentialsAsync(
                  opCred.ClientId, opCred.ClientSecret, PskOnlineScopes.DeptOperatorWorkplace);
              }
            };

            Assert.ThrowsAsync(typeof(AuthenticationException), code);
          }

        }

      }
    }

    [Test]
    public async Task CreateDepartment_ShouldFail_GivenDuplicateName()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // and created a department with the name "Department #1"
      var departmentDto = new DepartmentDto
      {
        Name = "Department #1",
        BranchOfficeId = _gryffindorHouse.BranchOffice_One.Id.ToString(),
      };
      var createdDeptId = await _apiClient.PostDepartmentAsync(departmentDto);

      // When
      // the user tries to create another department
      // with the same name within the same branch office
      departmentDto.Id = null; // reset the id to avoid other failures
      AsyncTestDelegate action = async () => await _apiClient.PostDepartmentAsync(departmentDto);

      // Then
      // the request should fail with BAD REQUEST status
      Assert.ThrowsAsync<BadRequestException>(action);
    }

    [Test]
    public void CreateDepartment_ShouldFail_GivenMissingReferenceToBranchOffice()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)

      var departmentDto = new DepartmentDto
      {
        Id = Guid.NewGuid().ToString(),
        Name = "Department Without Reference to Branch Office",
        // BranchOfficeId = _gryffindorHouse.BranchOffice_One.Id.ToString(),
      };

      // When
      // The user tries to create a department without reference to Branch Office (as above)

      AsyncTestDelegate action = async () => await _apiClient.PostDepartmentAsync(departmentDto);

      // Then
      // the request should fail with 'BadRequest' status code
      Assert.ThrowsAsync<BadRequestException>(action);
    }

    [Test]
    public void CreateDepartment_ShouldFail_GivenReferenceToARandomBranchOffice()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)

      var departmentDto = new DepartmentDto
      {
        Id = Guid.NewGuid().ToString(),
        Name = "Department Without Reference to Branch Office",
        BranchOfficeId = Guid.NewGuid().ToString(),
      };

      // When
      // The user tries to create a department without reference to Branch Office (as above)

      AsyncTestDelegate action = async () => await _apiClient.PostDepartmentAsync(departmentDto);

      // Then
      // the request should fail with 'BadRequest' status code
      Assert.ThrowsAsync<BadRequestException>(action);
    }

    [Test]
    public void CreateDepartment_ShouldFail_GivenReferenceToBranchOfficeInOtherTenant()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)

      // When
      // The user tries to create a department without reference to Branch Office
      var departmentDto = new DepartmentDto
      {
        Id = Guid.NewGuid().ToString(),
        Name = "Department With Reference to Other Tenant Branch Office",
        BranchOfficeId = _slytherinHouse.BranchOffice_One.Id.ToString(),
      };

      AsyncTestDelegate action = async () => await _apiClient.PostDepartmentAsync(departmentDto);

      // Then
      // the request should fail with 'BadRequest' status code
      Assert.ThrowsAsync<BadRequestException>(action);
    }

    [Test]
    public async Task CreateDepartment_ShouldFail_GivenDuplicateId()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // created a Department with name "Department #11"
      var departmentDto = new DepartmentDto
      {
        Id = Guid.NewGuid().ToString(),
        Name = "Department #11",
        BranchOfficeId = _gryffindorHouse.BranchOffice_One.Id.ToString()
      };
      var firstDepartmentId = await _apiClient.PostDepartmentAsync(departmentDto);
      Console.WriteLine($"Create {departmentDto.Name} => {firstDepartmentId}");

      // When
      // The user tries to create another Department with the same Id
      // and a different name
      departmentDto.Name += ".001";

      AsyncTestDelegate action = async () => await _apiClient.PostDepartmentAsync(departmentDto);

      // Then
      // the request should fail with 'Conflict' status code
      Assert.ThrowsAsync<ConflictException>(action);
    }

    [Test]
    public async Task CreateDepartment_ShouldSucceed_GivenDuplicateNames_InTwoTenants()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      var departmentDto = new DepartmentDto
      {
        Name = "Department #101",
        BranchOfficeId = _gryffindorHouse.BranchOffice_One.Id.ToString()
      };
      // created a tenant with the name "Department #100"
      var createdDepartmentId_t1 = await _apiClient.PostDepartmentAsync(departmentDto);
      Console.WriteLine($"Create {departmentDto} => {createdDepartmentId_t1}");

      // switch to tenant #2
      await _apiClient.AsSlytherinAdminAsync(_httpClient);

      // When
      // user from different tenant creates a Department with the name
      // duplicating name of a Department in a different tenant...
      departmentDto.BranchOfficeId = _slytherinHouse.BranchOffice_One.Id.ToString();
      departmentDto.Id = null;
      var createdDepartmentId_t2 = await _apiClient.PostDepartmentAsync(departmentDto); ;

      // Then
      // the request should succeed -- no exception has been thrown
      Console.WriteLine($"Create {departmentDto} => {createdDepartmentId_t2}");

    }

    [Test]
    public async Task GetAllDepartments_ShouldSucceed()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // created 2 departments just created in Gryffindor House
      // and 1 department just created in Slytherin House
      var departmentDto1 = new DepartmentDto
      {
        Name = "Department #1001",
        BranchOfficeId = _gryffindorHouse.BranchOffice_One.Id
      };
      var createFirstDepartmentid = await _apiClient.PostDepartmentAsync(departmentDto1);
      Console.WriteLine($"#1001 => {createFirstDepartmentid}");

      var departmentDto = new DepartmentDto
      {
        Name = "Department #1002",
        BranchOfficeId = _gryffindorHouse.BranchOffice_One.Id
      };

      var createSecondDepartmentId = await _apiClient.PostDepartmentAsync(departmentDto);
      Console.WriteLine($"#1002 => {createSecondDepartmentId}");

      await _apiClient.AsSlytherinAdminAsync(_httpClient);
      var slytherinDepartmentDto = new DepartmentDto
      {
        Name = "Slytherin Department #1002",
        BranchOfficeId = _slytherinHouse.BranchOffice_One.Id
      };
      var createSlytherinDepartmentId = await _apiClient.PostDepartmentAsync(slytherinDepartmentDto);
      Console.WriteLine($" slytherin => {createSlytherinDepartmentId}");

      // When
      // Gryffindor user requests all departments
      await _apiClient.AsGryffindorAdminAsync(_httpClient);
      var availableGryffindorDepartments = await _apiClient.GetDepartmentsAsync();

      // Then
      // the request should succeed and return more than 1 departments
      Assert.That(availableGryffindorDepartments.Count(), Is.GreaterThanOrEqualTo(2));
    }

    [Test]
    public async Task GetDepartment_ShouldFail_GivenWrongTenant()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // Department DTO
      var departmentDto = new DepartmentDto
      {
        Id = Guid.NewGuid().ToString(),
        Name = "Department To Be Forbidden",
        BranchOfficeId = _gryffindorHouse.BranchOffice_One.Id.ToString()
      };

      var newDepartmentId = await _apiClient.PostDepartmentAsync(departmentDto);

      // When
      // Slytherin user tries to read the new Branch office...
      await _apiClient.AsSlytherinAdminAsync(_httpClient);

      AsyncTestDelegate action = async () => await _apiClient.GetDepartmentAsync(newDepartmentId);

      // Then
      // the request should fail
      Assert.ThrowsAsync<UnauthorizedAccessException>(action);
    }

    [Test]
    public async Task UpdateDepartment_ShouldSucceed()
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // and created Department named Department #5
      var bo2Name = "Department #5";
      var departmentDto = new DepartmentDto
      {
        Id = Guid.NewGuid().ToString(),
        Name = bo2Name,
        BranchOfficeId = _gryffindorHouse.BranchOffice_One.Id.ToString()
      };

      var newDepartmentId = await _apiClient.PostDepartmentAsync(departmentDto);

      departmentDto.Id = newDepartmentId.ToString();
      departmentDto.Name = "Department #5.1";

      // When
      // the user attempts to rename Department #5 to Department #5.1
      await _apiClient.PutDepartmentAsync(departmentDto);

      // Then
      // the request should succeed
    }

    [Test]
    public async Task UpdateDepartment_ShouldFail_GivenDepartmentInOtherTenant()
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // and created Department named "Department To Fail Update"
      var departmentDto = new DepartmentDto
      {
        Name = "Department To Fail Update",
        BranchOfficeId = _gryffindorHouse.BranchOffice_One.Id.ToString()
      };

      departmentDto.Id = (await _apiClient.PostDepartmentAsync(departmentDto)).ToString();

      // When
      // a user from a different tenant attempts to delete the Branch Office
      await _apiClient.AsSlytherinAdminAsync(_httpClient);

      departmentDto.Name += " updated";

      AsyncTestDelegate action = async () => await _apiClient.PutDepartmentAsync(departmentDto);

      // Then
      // the request should fail
      Assert.ThrowsAsync<UnauthorizedAccessException>(action);
    }

    [Test]
    public async Task UpdateDepartment_ShouldFail_GivenDuplicateName()
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // created 2 branch offices: "Department #2" & "Department #3"
      var bo2Name = "Department #2";
      var departmentDto = new DepartmentDto
      {
        Id = Guid.NewGuid().ToString(),
        Name = bo2Name,
        BranchOfficeId = _gryffindorHouse.BranchOffice_One.Id.ToString()
      };

      var departmentDtoResponse = await _apiClient.PostDepartmentAsync(departmentDto);

      departmentDto.Name = "Department #3";
      departmentDto.Id = Guid.NewGuid().ToString();

      var bo3Guid = await _apiClient.PostDepartmentAsync(departmentDto);
      departmentDto.Id = bo3Guid.ToString();
      departmentDto.Name = bo2Name;

      // When
      // user attempts to rename Department #3 to Department #2
      AsyncTestDelegate action = async () => await _apiClient.PutDepartmentAsync(departmentDto);

      // Then
      // the request should fail
      Assert.ThrowsAsync<BadRequestException>(action);
    }

    [Test]
    public async Task DeleteDepartment_ShouldSucceed_GivenDepartmentInUsersTenant()
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // and created Department named Department #5
      var bo2Name = "Department #500";
      var departmentDto = new DepartmentDto
      {
        Id = Guid.NewGuid().ToString(),
        Name = bo2Name,
        BranchOfficeId = _gryffindorHouse.BranchOffice_One.Id.ToString()
      };

      var newDepId = await _apiClient.PostDepartmentAsync(departmentDto);

      // When
      // the user attempts to delete the Branch Office
      await _apiClient.DeleteDepartmentAsync(newDepId);

      // Then
      // 1. the request should succeed
      Assert.That(newDepId.ToString(), Is.EqualTo(departmentDto.Id));

      // 2. the department shall not be available for 'get'

      AsyncTestDelegate action = async () => await _apiClient.GetDepartmentAsync(departmentDto.Id);

      Assert.ThrowsAsync<ItemNotFoundException>(action);
    }

    [Test]
    public async Task DeleteDepartment_ShouldFail_GivenDepartmentInOtherTenant()
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // and created Department named Department #50
      var bo2Name = "Department #50";
      var departmentDto = new DepartmentDto
      {
        Id = Guid.NewGuid().ToString(),
        Name = bo2Name,
        BranchOfficeId = _gryffindorHouse.BranchOffice_One.Id.ToString()
      };

      var createdDepartmentId = await _apiClient.PostDepartmentAsync(departmentDto);

      // When
      // a user from a different tenant attempts to delete the Branch Office
      await _apiClient.AsSlytherinAdminAsync(_httpClient);

      AsyncTestDelegate action = async () => await _apiClient.DeleteDepartmentAsync(departmentDto.Id);

      // Then
      // the request should fail
      Assert.ThrowsAsync<UnauthorizedAccessException>(action);
    }
  }
}
