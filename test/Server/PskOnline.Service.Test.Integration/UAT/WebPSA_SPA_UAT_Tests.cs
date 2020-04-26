namespace PskOnline.Service.Test.Integration.UAT
{
  using System;
  using System.Linq;
  using System.Net.Http;
  using System.Threading.Tasks;

  using Microsoft.AspNetCore.Mvc.Testing;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;
  using Newtonsoft.Json.Linq;
  using NUnit.Framework;

  using PskOnline.Client.Api;
  using PskOnline.Client.Api.Authority;
  using PskOnline.Client.Api.Inspection;
  using PskOnline.Client.Api.OpenId;
  using PskOnline.Client.Api.Organization;
  using PskOnline.Components.Log;
  using PskOnline.Server.Authority.API.Dto;
  using PskOnline.Service.Test.Integration.TestData;

  [TestFixture(
    Author = "Adadurov",
    Description = "Performs end-to-end UAT steps." +
                  "Notice that this test fixture is initialized only once and tests should run in order!")]
  public class WebPSA_SPA_UAT_Tests : IDisposable
  {
    DefaultWebApplicationFactory _app;

    HttpClient _httpClient;

    IApiClient _apiClient;

    TenantContainer _gryffindorHouse;

    TenantContainer _slytherinHouse;

    string _workDepartmentId;
    string _workBranchId;

    WorkplaceCredentialsDto _operatorWorkplaceCredentials;
    WorkplaceCredentialsDto _auditorWorkplaceCredentials;
    WorkplaceCredentialsDto _branchAuditWorkplaceCredentials;

    [SetUp]
    public async Task SetUp()
    {
      await InitOnce();

      // start each test as Gryffindor user
      await _apiClient.AsGryffindorAdminAsync(_httpClient);

      if (_operatorWorkplaceCredentials == null)
      {
        await CreateDepartmentCredentialsAsync();
        await CreateBranchCredentialsAsync();
      }
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

      // Slytherin House admin creates branch office and departments
      await _apiClient.AsSlytherinAdminAsync(_httpClient);
      TestBranchOffice.SeedDefaultBranchOffice(_apiClient, _httpClient, _slytherinHouse).Wait();
      TestDepartment.SeedDefaultDepartments(_apiClient, _slytherinHouse).Wait();
      TestPosition.SeedDefaultPosition(_apiClient, _slytherinHouse).Wait();
      TestEmployees.SeedDefaultEmployees(_apiClient, _slytherinHouse).Wait();

      // Gryffindor House admin creates branch office and departments
      await _apiClient.AsGryffindorAdminAsync(_httpClient);
      TestBranchOffice.SeedDefaultBranchOffice(_apiClient, _httpClient, _gryffindorHouse).Wait();
      TestDepartment.SeedDefaultDepartments(_apiClient, _gryffindorHouse).Wait();
      TestPosition.SeedDefaultPosition(_apiClient, _gryffindorHouse).Wait();
      TestEmployees.SeedDefaultEmployees(_apiClient, _gryffindorHouse).Wait();

      _workDepartmentId = _gryffindorHouse.Department_1_2.Department.Id;
      _workBranchId = _gryffindorHouse.BranchOffice_One.Id;
    }

    private async Task CreateDepartmentCredentialsAsync()
    {
      var uri = $"/api/department/{_workDepartmentId}/workplace";
      var workplaceDescriptor = new WorkplaceCredentialsRequestDto
      {
        Scopes = PskOnlineScopes.DeptOperatorWorkplace
      };
      var response = await _httpClient.PostAsJsonAsync(uri, workplaceDescriptor);

      // Then
      // the request should succeed
      response.EnsureSuccessStatusCode();
      _operatorWorkplaceCredentials = await response.Content.ReadAsJsonAsync<WorkplaceCredentialsDto>();
    }

    private async Task CreateBranchCredentialsAsync()
    {
      var uri = $"/api/branchoffice/{_workBranchId}/workplace";
      var workplaceDescriptor = new WorkplaceCredentialsRequestDto
      {
        Scopes = PskOnlineScopes.BranchAuditorWorkplace
      };
      var response = await _httpClient.PostAsJsonAsync(uri, workplaceDescriptor);

      // Then
      // the request should succeed
      response.EnsureSuccessStatusCode();
      _branchAuditWorkplaceCredentials = await response.Content.ReadAsJsonAsync<WorkplaceCredentialsDto>();
    }


    [Test]
    [Order(10)]
    public async Task CreateDepartmentOperatorWorkplace_ShouldSucceed_GivenUserIsAdmin()//
    {
      // Given
      // user authenticated as admin in tenant #1 (Gryffindor House) -- see Setup()
      // and created an operator workplace credentials for existing department
      Assert.That(_operatorWorkplaceCredentials.ClientId, Is.Not.Null);
      Assert.That(_operatorWorkplaceCredentials.ClientId, Is.Not.Empty);

      Assert.That(_operatorWorkplaceCredentials.ClientSecret, Is.Not.Null);
      Assert.That(_operatorWorkplaceCredentials.ClientSecret, Is.Not.Empty);

      // When
      //

      await _apiClient.SignInWithWorkplaceCredentialsAsync(
        _operatorWorkplaceCredentials.ClientId,
        _operatorWorkplaceCredentials.ClientSecret,
        PskOnlineScopes.DeptOperatorWorkplace);

      // Then
      // the workplace should authenticate successfully using the credentials
      // in client_credentials OpenId flow
    }

    [Test]
    [Order(11)]
    public async Task Should_Refresh_Tokens_Given_Operator_Workplace_User()//
    {
      // Given
      // user authenticated as operator workplace
      await _apiClient.SignInWithWorkplaceCredentialsAsync(
        _operatorWorkplaceCredentials.ClientId,
        _operatorWorkplaceCredentials.ClientSecret,
        PskOnlineScopes.DeptOperatorWorkplace);

      // When
      //
      await _apiClient.RefreshToken();

      // Then
      // the API client should get its own Department data successfully
      var workplaceIdToken = _apiClient.GetIdToken();
      var departmentId = workplaceIdToken.Claims.First(c => c.Type == CustomClaimTypes.DepartmentId).Value;

      var departmentDetails = await _apiClient.GetDepartmentAsync(departmentId);
    }

    [Test]
    [Order(15)]
    public async Task CreateBranchAuditorWorkplace_ShouldSucceed_GivenUserIsAdmin()//
    {
      // Given
      // user authenticated as admin in tenant #1 (Gryffindor House) -- see Setup()
      // and created a branch auditor workplace credentials for existing branch
      Assert.That(_branchAuditWorkplaceCredentials.ClientId, Is.Not.Null);
      Assert.That(_branchAuditWorkplaceCredentials.ClientId, Is.Not.Empty);

      Assert.That(_branchAuditWorkplaceCredentials.ClientSecret, Is.Not.Null);
      Assert.That(_branchAuditWorkplaceCredentials.ClientSecret, Is.Not.Empty);

      // When
      //
      await _apiClient.SignInWithWorkplaceCredentialsAsync(
        _branchAuditWorkplaceCredentials.ClientId,
        _branchAuditWorkplaceCredentials.ClientSecret,
        PskOnlineScopes.BranchAuditorWorkplace);

      // Then
      // the workplace should authenticate successfully using the credentials
      // in client_credentials OpenId flow
    }

    [Test]
    [Order(20)]
    public async Task GetDepartmentDetails_ShouldSucceed_GivenOperatorWorkplace()
    {
      // Given
      // client authenticated as operator workplace
      await _apiClient.SignInWithWorkplaceCredentialsAsync(
        _operatorWorkplaceCredentials.ClientId,
        _operatorWorkplaceCredentials.ClientSecret, 
        PskOnlineScopes.DeptOperatorWorkplace);

      var workplaceIdToken = _apiClient.GetIdToken();
      var departmentId = workplaceIdToken.Claims.First(c => c.Type == CustomClaimTypes.DepartmentId).Value;

      // When
      // the operator workplace reads department details
      await _apiClient.GetDepartmentAsync(departmentId);

      // Then
      // the request should succeed
    }

    [Test]
    [Order(30)]
    public async Task GetDepartmentEmployees_ShouldSucceed_GivenOperatorWorkplace()
    {
      // Given
      // client authenticated as operator workplace
      await _apiClient.SignInWithWorkplaceCredentialsAsync(
        _operatorWorkplaceCredentials.ClientId, _operatorWorkplaceCredentials.ClientSecret, PskOnlineScopes.DeptOperatorWorkplace);

      var workplaceIdToken = _apiClient.GetIdToken();
      var departmentId = workplaceIdToken.Claims.First(c => c.Type == CustomClaimTypes.DepartmentId).Value;

      // When
      // the operator workplace reads department employees
      var employees = await _apiClient.GetDepartmentEmployeesAsync(departmentId);

      // Then
      // the request should succeed and return the expected number of employees
      Assert.That(employees.Count(), Is.EqualTo(3));
    }

    [Test]
    [Order(40)]
    public async Task GetBranchOfficeDetails_ShouldSucceed_GivenOperatorWorkplace()
    {
      // Given
      // client authenticated as operator workplace
      await _apiClient.SignInWithWorkplaceCredentialsAsync(
        _operatorWorkplaceCredentials.ClientId,
        _operatorWorkplaceCredentials.ClientSecret,
        PskOnlineScopes.DeptOperatorWorkplace);

      var workplaceIdToken = _apiClient.GetIdToken();
      var branchOfficeId = workplaceIdToken.Claims.First(c => c.Type == CustomClaimTypes.BranchOfficeId).Value;

      // When
      // the operator workplace reads branch office details
      await _apiClient.GetBranchOfficeAsync(branchOfficeId);


      // Then
      // the request should succeed

    }

    [Test]
    [Order(50)]
    public async Task GetTenantSharedInfo_ShouldSucceed_GivenOperatorWorkplace()
    {
      // Given
      // client authenticated as operator workplace
      await _apiClient.SignInWithWorkplaceCredentialsAsync(
        _operatorWorkplaceCredentials.ClientId,
        _operatorWorkplaceCredentials.ClientSecret,
        PskOnlineScopes.DeptOperatorWorkplace);

      var workplaceIdToken = _apiClient.GetIdToken();
      var tenantId = workplaceIdToken.Claims.First(c => c.Type == CustomClaimTypes.TenantId).Value;

      // When
      // the operator workplace reads tenant details
      await _apiClient.GetTenantSharedInfoAsync(tenantId);

      // Then
      // the request should succeed
    }

    [Test]
    [Order(55)]
    public async Task GetTenantSharedInfo_ShouldSucceed_GivenOperatorWorkplace_NewApiClientInstanceWithTokens()
    {
      // Given
      // client authenticated as operator workplace
      var tokens = await _apiClient.SignInWithWorkplaceCredentialsAsync(
                              _operatorWorkplaceCredentials.ClientId,
                              _operatorWorkplaceCredentials.ClientSecret,
                              PskOnlineScopes.DeptOperatorWorkplace);

      var workplaceIdToken = _apiClient.GetIdToken();
      var tenantId = workplaceIdToken.Claims.First(c => c.Type == CustomClaimTypes.TenantId).Value;

      var localApiClient = new ApiClient(_app.CreateClient(), tokens, _app.GetLogger<ApiClient>());

      // When
      // the operator workplace reads tenant details
      var tenantInfo = await localApiClient.GetTenantSharedInfoAsync(tenantId);

      // Then
      // the request should succeed
      Assert.That(tenantInfo.Id, Is.EqualTo(tenantId));
    }

    [Test]
    [Order(56)]
    public async Task GetTenantSharedInfo_ShouldSucceed_GivenOperatorWorkplace_WithRenewalHandler()
    {
      // Given
      // client authenticated as operator workplace
      var tokens = await _apiClient.SignInWithWorkplaceCredentialsAsync(
                              _operatorWorkplaceCredentials.ClientId,
                              _operatorWorkplaceCredentials.ClientSecret,
                              PskOnlineScopes.DeptOperatorWorkplace);

      var workplaceIdToken = _apiClient.GetIdToken();
      var tenantId = workplaceIdToken.Claims.First(c => c.Type == CustomClaimTypes.TenantId).Value;

      {
        var httpClient = _app.CreateClient();
        var renewalHandler = new OpenIdRenewalHandler(httpClient, tokens, null, _app.GetLogger<ApiClient>());
        var localApiClient = new ApiClient(httpClient, renewalHandler, _app.GetLogger<ApiClient>());

        // When
        // the operator workplace reads 'shared' tenant info
        var tenantInfo = await localApiClient.GetTenantSharedInfoAsync(tenantId);

        // Then
        // the request should succeed
        Assert.That(tenantInfo.Id, Is.EqualTo(tenantId));
      }
    }

    [Test]
    [Order(60)]
    public async Task SubmitInspection_ShouldSucceed_GivenOperatorWorkplace()
    {
      // Given
      // client authenticated as operator workplace
      await _apiClient.SignInWithWorkplaceCredentialsAsync(
        _operatorWorkplaceCredentials.ClientId,
        _operatorWorkplaceCredentials.ClientSecret,
        PskOnlineScopes.DeptOperatorWorkplace);

      var workplaceIdToken = _apiClient.GetIdToken();
      var departmentId = workplaceIdToken.Claims.First(c => c.Type == CustomClaimTypes.DepartmentId).Value;

      var employees = await _apiClient.GetDepartmentEmployeesAsync(departmentId);

      var employee = employees.First();

      // When
      // the department operator workplace submits inspection & test data for an employee in the department

      var inspectionDto = new InspectionDto
      {
        MethodSetId = "Method_Set_Id",
        StartTime = DateTime.Now - TimeSpan.FromMinutes(7),
        InspectionPlace = InspectionPlace.OnWorkplace,
        InspectionType = InspectionType.PreShift,
        EmployeeId = employee.Id,
        MachineName = "HOST002",
        MethodSetVersion = "Version 16"
      };

      var inspectionId = await _apiClient.PostInspectionAsync(inspectionDto);

      // Then
      // the request should succeed
      var verifyInspection = await _apiClient.GetInspectionAsync(inspectionId);

      // And the attempt to post a test for the inspectioin should succeed
      var testDto = new TestDto
      {
        Id = Guid.NewGuid().ToString(),
        MethodId = "123",
        MethodVersion = "1.0",
        InspectionId = inspectionId.ToString(),
        EmployeeId = inspectionDto.EmployeeId,
        StartTime = inspectionDto.StartTime,
        FinishTime = inspectionDto.StartTime + TimeSpan.FromMinutes(5),
        MethodProcessedDataJson = JObject.Parse("{}"),
        MethodRawDataJson = JObject.Parse("{ \"value\" : \"Qw2er6ty1as6df36k347j3q2w2er8o9iu2234wer\"}")
      };

      var testId = await _apiClient.PostTestAsync(testDto);

      // And the attempt to complete the inspection should succeed

      var responseDto = await _apiClient.CompleteInspectionAsync(new InspectionCompleteDto
      {
        Id = inspectionId,
        FinishTime = inspectionDto.StartTime + TimeSpan.FromMinutes(5)
      });

      Assert.NotNull(responseDto);

      // and the Tenant Operations Statistics should return +1 completed inspection
      await _apiClient.AsGryffindorAdminAsync(_httpClient);
      var tenantId = workplaceIdToken.GetTenantIdClaimValue();
      var opsStat = await _apiClient.GetTenantOperationsSummaryAsync(tenantId);

      Assert.That(opsStat.NewInspectionsLast24Hours, Is.EqualTo(1));
      Assert.That(opsStat.NewInspectionsLastWeek, Is.EqualTo(1));
    }

    [Test]
    [Order(210)]
    public async Task CreateDepartmentAuditorWorkplace_ShouldSucceed_GivenUserIsAdmin()
    {
      // Given
      // user authenticated as admin in tenant #1 (Gryffindor House) -- see Setup()

      // When
      // the admin creates an auditor workplace credentials for an existing department
      var uri = $"/api/department/{_workDepartmentId}/workplace";
      var workplaceDescriptor = new WorkplaceCredentialsRequestDto
      {
        Scopes = PskOnlineScopes.DeptAuditorWorkplace
      };

      // Then
      // the request should succeed
      var response = await _httpClient.PostAsJsonAsync(uri, workplaceDescriptor);
      response.EnsureSuccessStatusCode();

      _auditorWorkplaceCredentials = await response.Content.ReadAsJsonAsync<WorkplaceCredentialsDto>();

      // and the workplace should authenticate successfully using the credentials
      // in client_credentials OpenId flow

    }

    [Test]
    [Order(220)]
    public async Task GetDepartmentDetails_ShouldSucceed_GivenAuditorWorkplace()
    {
      // Given
      // client authenticated as auditor workplace
      await _apiClient.SignInWithWorkplaceCredentialsAsync(
        _auditorWorkplaceCredentials.ClientId,
        _auditorWorkplaceCredentials.ClientSecret,
        PskOnlineScopes.DeptAuditorWorkplace);

      var workplaceIdToken = _apiClient.GetIdToken();
      var departmentId = workplaceIdToken.Claims.First(c => c.Type == CustomClaimTypes.DepartmentId).Value;

      // When
      // the auditor workplace reads department details
      await _apiClient.GetDepartmentAsync(departmentId);

      // Then
      // the request should succeed
    }

    [Test]
    [Order(230)]
    public async Task GetDepartmentEmployees_ShouldSucceed_GivenAuditorWorkplace()
    {
      // Given
      // client authenticated as auditor workplace
      await _apiClient.SignInWithWorkplaceCredentialsAsync(
        _auditorWorkplaceCredentials.ClientId,
        _auditorWorkplaceCredentials.ClientSecret,
        PskOnlineScopes.DeptAuditorWorkplace);

      var workplaceIdToken = _apiClient.GetIdToken();
      var departmentId = workplaceIdToken.Claims.First(c => c.Type == CustomClaimTypes.DepartmentId).Value;

      // When
      // the auditor workplace reads department employees
      var employees = await _apiClient.GetDepartmentEmployeesAsync(departmentId);

      // Then
      // the request should succeed and return the expected number of employees
      Assert.That(employees.Count(), Is.EqualTo(3));
    }

    [Test]
    [Order(240)]
    public async Task GetBranchOfficeDetails_ShouldSucceed_GivenAuditorWorkplace()
    {
      // Given
      // client authenticated as auditor workplace
      await _apiClient.SignInWithWorkplaceCredentialsAsync(
        _auditorWorkplaceCredentials.ClientId,
        _auditorWorkplaceCredentials.ClientSecret,
        PskOnlineScopes.DeptAuditorWorkplace);

      var workplaceIdToken = _apiClient.GetIdToken();
      var branchOfficeId = workplaceIdToken.Claims.First(c => c.Type == CustomClaimTypes.BranchOfficeId).Value;

      // When
      // the auditor workplace reads branch office details
      await _apiClient.GetBranchOfficeAsync(branchOfficeId);

      // Then
      // the request should succeed

    }

    [Test]
    [Order(250)]
    public async Task GetTenantInfo_ShouldSucceed_GivenAuditorWorkplace()
    {
      // Given
      // client authenticated as auditor workplace
      await _apiClient.SignInWithWorkplaceCredentialsAsync(
        _auditorWorkplaceCredentials.ClientId,
        _auditorWorkplaceCredentials.ClientSecret,
        PskOnlineScopes.DeptAuditorWorkplace);

      var workplaceIdToken = _apiClient.GetIdToken();
      var tenantId = workplaceIdToken.Claims.First(c => c.Type == CustomClaimTypes.TenantId).Value;

      // When
      // the auditor workplace reads 'shared' tenant details
      var tenantSharedInfo = await _apiClient.GetTenantSharedInfoAsync(tenantId);

      // Then
      // the request should succeed

      Assert.That(tenantSharedInfo.Id, Is.EqualTo(tenantId));
    }
  }
}
