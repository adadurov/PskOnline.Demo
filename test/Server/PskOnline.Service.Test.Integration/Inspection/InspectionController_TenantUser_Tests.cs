namespace PskOnline.Service.Test.Integration.Inpection
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
  using Newtonsoft.Json.Linq;
  using NUnit.Framework;

  using PskOnline.Client.Api;
  using PskOnline.Client.Api.Inspection;
  using PskOnline.Client.Api.Organization;
  using PskOnline.Components.Log;
  using PskOnline.Service.Test.Integration.TestData;

  [TestFixture(
    Author = "Adadurov",
    Description = 
    "Verifies that Gryffindor data are accessible to Gryffindor user. " + 
    "Plus, it verifies that other tenant data are not accessible to the same user")]
  public class InspectionController_TenantUser_Tests : IDisposable
  {
    const string _url = "/api/Inspection/";
    DefaultWebApplicationFactory _app;
    HttpClient _httpClient;
    IApiClient _apiClient;

    TenantContainer _gryffindorHouse;
    TenantContainer _slytherinHouse;
    IList<EmployeeDto> _gryffindorEmployees;

    Guid _gryffindorInspectionId;
    Guid _slytherinInspectionId;

    [SetUp]
    public async Task SetUp()
    {
      LogHelper.ConfigureConsoleLogger();
      await InitOnce();
      await _apiClient.AsGryffindorAdminAsync(_httpClient);
    }

    [TearDown]
    public void TearDown()
    {
    }

    public void Dispose()
    {
      _httpClient?.Dispose();
      _httpClient = null;
      _app?.Dispose();
      _app = null;
    }

    private async Task InitOnce()
    {
      if (_app != null) return;

      _app = new DefaultWebApplicationFactory();
      var options = new WebApplicationFactoryClientOptions()
      {
        AllowAutoRedirect = false
      };
      _httpClient = _app.CreateClient(options);
      _apiClient = new ApiClient(_httpClient, _app.GetLogger<ApiClient>());

      // create tenants here
      await _apiClient.AsSiteAdminAsync(_httpClient);
      _gryffindorHouse = await TestTenants.CreateTenantWithRolesAndUsers(_apiClient, _httpClient, TestTenants.GryffindorHouse);
      await TestBranchOffice.SeedDefaultBranchOffice(_apiClient, _httpClient, _gryffindorHouse);
      await TestDepartment.SeedDefaultDepartments(_apiClient, _gryffindorHouse);
      await TestPosition.SeedDefaultPosition(_apiClient, _gryffindorHouse);
      await TestEmployees.SeedDefaultEmployees(_apiClient, _gryffindorHouse);
      _gryffindorEmployees = (await _apiClient.GetEmployeesAsync()).ToList();

      await _apiClient.AsSiteAdminAsync(_httpClient);
      _slytherinHouse = await TestTenants.CreateTenantWithRolesAndUsers(_apiClient, _httpClient, TestTenants.SlytherinHouse);
      await TestBranchOffice.SeedDefaultBranchOffice(_apiClient, _httpClient, _slytherinHouse);
      await TestDepartment.SeedDefaultDepartments(_apiClient, _slytherinHouse);
      await TestPosition.SeedDefaultPosition(_apiClient, _slytherinHouse);
      await TestEmployees.SeedDefaultEmployees(_apiClient, _slytherinHouse);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="apiClient"></param>
    /// <param name=""></param>
    private async Task<Guid> PostManyInspections(IApiClient apiClient, InspectionDto template)
    {
      var fiveMinutes = TimeSpan.FromMinutes(5);
      var lastInspectionId = Guid.Empty;
      var oneHundredKbytes = new string('X', 150*1024);
      for (int i = 0; i < 25; i++)
      {
        try { 
        // add more inspections to test paging options
        var newInspection = Mapper.Map<InspectionDto>(template);
        newInspection.Id = Guid.NewGuid().ToString();
        newInspection.EmployeeId = template.EmployeeId;
        newInspection.StartTime = DateTime.Now - TimeSpan.FromMinutes(i * 10);
        newInspection.FinishTime = null;
        var test = new TestDto
        {
          Id = Guid.NewGuid().ToString(),
          MethodId = "123",
          MethodVersion = "1.0",
          InspectionId = newInspection.Id,
          EmployeeId = newInspection.EmployeeId,
          StartTime = newInspection.StartTime,
          FinishTime = newInspection.StartTime + fiveMinutes,
          MethodProcessedDataJson = JObject.Parse("{}"),
          MethodRawDataJson = JObject.Parse("{ \"value\" : \"" + oneHundredKbytes + "\"}")
        };
        lastInspectionId = await apiClient.PostInspectionAsync(newInspection);

        Console.WriteLine($"posted {i}-th inspection");

        test.InspectionId = lastInspectionId.ToString();
        await apiClient.PostTestAsync(test);

        await apiClient.CompleteInspectionAsync(new InspectionCompleteDto
          {
            Id = lastInspectionId,
            FinishTime = newInspection.StartTime + fiveMinutes
          });
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Failed to post {i}-th inspection: {ex.Message}");
          throw;
        }
      }
      return lastInspectionId;
    }

    private async Task SeedTestInspections(IApiClient apiClient)
    {
      {
        await apiClient.AsGryffindorAdminAsync(_httpClient);

        var newEmployee = new EmployeeDto
        {
          FirstName = "John",
          LastName = "Doe",
          Patronymic = "Alan",
          PositionId = _gryffindorHouse.Position_Default.Id,
          DepartmentId = _gryffindorHouse.Department_1_1.Department.Id,
          BranchOfficeId = _gryffindorHouse.BranchOffice_One.Id
        };
        var employeeId = await apiClient.PostEmployeeAsync(newEmployee);

        var gryInspection = new InspectionDto
        {
          Id = Guid.NewGuid().ToString(),
          MethodSetId = "Method_Set_Id",
          StartTime = DateTime.Now - TimeSpan.FromMinutes(700),
          FinishTime = DateTime.Now - TimeSpan.FromMinutes(700),
          InspectionPlace = InspectionPlace.OnWorkplace,
          InspectionType = InspectionType.PreShift,
          EmployeeId = employeeId.ToString(),
          MachineName = "HOST002-gry",
          MethodSetVersion = "Version 16"
        };
        _gryffindorInspectionId = await PostManyInspections(apiClient, gryInspection);
      }
      {
        await apiClient.AsSlytherinAdminAsync(_httpClient);

        var newEmployee = new EmployeeDto
        {
          FirstName = "John",
          LastName = "Doe",
          Patronymic = "Alan",
          PositionId = _slytherinHouse.Position_Default.Id,
          DepartmentId = _slytherinHouse.Department_1_1.Department.Id,
          BranchOfficeId = _slytherinHouse.BranchOffice_One.Id
        };
        var employeeId = await apiClient.PostEmployeeAsync(newEmployee);

        var slyInspection = new InspectionDto
        {
          Id = Guid.NewGuid().ToString(),
          MethodSetId = "Method_Set_Id",
          StartTime = DateTime.Now - TimeSpan.FromMinutes(700),
          FinishTime = DateTime.Now - TimeSpan.FromMinutes(700),
          InspectionPlace = InspectionPlace.OnWorkplace,
          InspectionType = InspectionType.PreShift,
          EmployeeId = employeeId.ToString(),
          MachineName = "HOST002-sly",
          MethodSetVersion = "Version 16"
        };
        _slytherinInspectionId = await PostManyInspections(apiClient, slyInspection);
      }
    }

    [Test]
    [Order(0)]
    public void Init_Test_Fixture_Should_Succeed()
    {
      // This test does nothing but allows to estimate the time
      // required to post 100 inspections without the overhead
      // of the app starting up (in the next test)
    }

    [Test]
    [Order(1)]
    public async Task Post_50_Inspections_Should_Succeed()
    {
      await SeedTestInspections(_apiClient);
    }

    [Test]
    [Order(2)]
    public void ReadAll_ShouldReturnFromOwnTenantOnly()
    {
      // Given 
      // _client authenticated as a Gryffindor user in infrastructure
      // containing Gryffindor and Slytherin Inspections

      // When
      // tenant user requests list of inspections
      var visibleInspectionsResponse = _httpClient.GetAsync(_url).Result;
      var visibleInspections = visibleInspectionsResponse.Content.ReadAsJsonAsync<InspectionDto[]>().Result;

      // Then
      // only the tenant that the user is related to is returned
      Assert.That(visibleInspections.Length, Is.EqualTo(25));
      Assert.That(visibleInspections.Count(i => i.MachineName == "HOST002-gry"), Is.EqualTo(25));
      Assert.False(visibleInspections.Any( i => i.MachineName == "HOST002-sly"));
    }

    [Test]
    [Order(3)]
    public void ReadAll_WithPaging_ShouldReturnFromOwnTenantOnly()
    {
      // Given 
      // _client authenticated as a Gryffindor user in infrastructure
      // containing Gryffindor and Ministry Of Magic Inspections

      // When
      // tenant users requests list of tenants...
      var visibleInspectionsResponse = _httpClient.GetAsync(_url + "?skip=10&take=10").Result;
      var visibleInspections = visibleInspectionsResponse.Content.ReadAsJsonAsync<InspectionDto[]>().Result;

      // Then
      Assert.That(visibleInspections.Length, Is.EqualTo(10));
    }

    [Test]
    [Order(4)]
    public void PostInspection_ShouldSucceed()
    {
      var newInspectionModel = new InspectionDto
      {
        MethodSetId = "Method_Set_Id",
        StartTime = DateTime.Now - TimeSpan.FromMinutes(7),
        InspectionPlace = InspectionPlace.OnWorkplace,
        InspectionType = InspectionType.PreShift,
        EmployeeId = _gryffindorEmployees.First().Id,
        MachineName = "HOST002",
        MethodSetVersion = "Version 16"
      };

      var response = _httpClient.PostAsJsonAsync(_url, newInspectionModel);

      Assert.That(response.Result.StatusCode, Is.EqualTo(HttpStatusCode.Created));
      var createdEntityInfo = response.Result.Content.ReadAsJsonAsync<PskOnline.Client.Api.Models.CreatedWithGuidDto>().Result;

      var verifyResponse = _httpClient.GetAsync(_url + createdEntityInfo.Id.ToString());
      var verifyContent = verifyResponse.Result.Content;
      var verifyInspection = verifyContent.ReadAsJsonAsync<InspectionDto>().Result;
      Assert.That(verifyInspection.MachineName, Is.EqualTo(newInspectionModel.MachineName));
      Assert.That(verifyInspection.Id, Is.EqualTo(createdEntityInfo.Id.ToString()));
    }

    [Test]
    [Order(5)]
    public void PostDuplicateInspection_ShouldReturnFound()
    {
      var newInspectionModel = new InspectionDto
      {
        MethodSetId = "Method_Set_Id",
        StartTime = DateTime.Now - TimeSpan.FromMinutes(7),
        InspectionPlace = InspectionPlace.OnWorkplace,
        InspectionType = InspectionType.PreShift,
        EmployeeId = _gryffindorEmployees.First().Id,
        MachineName = "HOST002",
        MethodSetVersion = "Version 16"
      };

      // When
      // a duplicate exception is posted
      var response1 = _httpClient.PostAsJsonAsync(_url, newInspectionModel).Result;
      var response2 = _httpClient.PostAsJsonAsync(_url, newInspectionModel).Result;

      // Then
      // the 2nd result is 'Found' with a link to the existing inspection
      Assert.That(response1.StatusCode, Is.EqualTo(HttpStatusCode.Created));
      Assert.That(response2.StatusCode, Is.EqualTo(HttpStatusCode.Found));

      var newEntityInfo = response1.Content.ReadAsJsonAsync<PskOnline.Client.Api.Models.CreatedWithGuidDto>().Result;
      Assert.IsTrue(response2.Headers.Location.ToString().EndsWith(newEntityInfo.Id.ToString()));
    }

    [Test]
    [Order(6)]
    public void ReadOwnTenantInspection_ShouldSucceed()
    {
      // Given 
      // _client authenticated as a Gryffindor House user

      // When
      // the user requests an existing inspections belongind to Gryffindor...
      var gryffindorInspectionResponse = _httpClient.GetAsync(_url + _gryffindorInspectionId.ToString()).Result;

      // Then
      // the request is successful and returns the existing inspection
      gryffindorInspectionResponse.EnsureSuccessStatusCode();
      Assert.That(gryffindorInspectionResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var gryffindorInspection = gryffindorInspectionResponse.Content.ReadAsJsonAsync<InspectionDto>().Result;
      Assert.That(gryffindorInspection.Id, Is.EqualTo(_gryffindorInspectionId.ToString()));
    }

    [Test]
    [Order(7)]
    public void ReadOtherTenantInspection_ShouldFailWithForbidden()
    {
      // Given 
      // _client authenticated as a Gryffindor House user

      // When
      // the user requests an existing inspections belongind to a different tenant...
      var momInspectionResponse = _httpClient.GetAsync(_url + _slytherinInspectionId.ToString()).Result;

      // Then
      // the request is unsuccessful and returns the tenant
      Assert.That(momInspectionResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    [Order(8)]
    public void UpdateOtherTenantInspection_ShouldFail()
    {
      // Given
      // an infrastructure with tenants and client authorized as Gryffindor House user
      var updatedSlytherinInspection = new InspectionDto
      {
        Id = _slytherinInspectionId.ToString(),
        FinishTime = DateTime.Now
      };

      // When
      // the user attempts to update an inspection within Gryffindor House
      var putResponse = _httpClient.PutAsJsonAsync(_url + updatedSlytherinInspection.Id.ToString(), updatedSlytherinInspection);

      // Then
      // The server responds with 'Method Not Allowed'
      Assert.That(putResponse.Result.StatusCode, Is.EqualTo(HttpStatusCode.MethodNotAllowed));
    }

    [Test]
    [Order(9)]
    public void UpdateOwnTenantInspection_ShouldFail_GivenInspectionIsCompleted()
    {
      // Given
      // an infrastructure with tenants and client authorized as Gryffindor House user
      var updatedGryffindorInspection = new InspectionDto
      {
        Id = _gryffindorInspectionId.ToString()
      };
      updatedGryffindorInspection.FinishTime = DateTime.Now;

      // When
      // the user attempts to update an inspection within Gryffindor House
      var putResponse = _httpClient.PutAsJsonAsync(_url + updatedGryffindorInspection.Id.ToString(), updatedGryffindorInspection);

      // Then
      // The server responds with 'Method Not Allowed'
      Assert.That(putResponse.Result.StatusCode, Is.EqualTo(HttpStatusCode.MethodNotAllowed));
    }

    [Test]
    [Order(10)]
    public void DeleteOwnTenantInspection_ShouldSucceed()
    {
      // Given
      // an infrastructure with tenants and client authorized as Gryffindor House user
      
      // When
      // an attempt is made to delete inspection belonging to Gryffindor
      var response = _httpClient.DeleteAsync(_url + _gryffindorInspectionId.ToString());

      // Then
      Assert.That(response.Result.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    [Order(11)]
    public void DeleteOtherTenantInspection_ShouldFail()
    {
      // Given
      // an infrastructure with tenants

      // When
      // an attempt is made to delete inspection belonging to Ministry Of Magic
      var response = _httpClient.DeleteAsync(_url + _slytherinInspectionId.ToString());

      // Then
      Assert.That(response.Result.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    [Order(100)]
    public void PostTest_ShouldFail_GivenNoMethodId()
    {
      // Given
      var testDto = new TestPostDto
      {
        InspectionId = Guid.NewGuid().ToString(),
        MethodRawDataJson = JObject.FromObject(new object())
      };

      // When
      // an attempt is made to post test without mandatory attributes

      AsyncTestDelegate action = async () => await _apiClient.PostTestAsync(testDto);

      // Then
      Assert.ThrowsAsync<BadRequestException>(action);
    }

    [Test]
    [Order(100)]
    public void PostTest_ShouldFail_GivenMethodIdTooShort()
    {
      // Given
      var testDto = new TestPostDto
      {
        InspectionId = Guid.NewGuid().ToString(),
        MethodId = "",
        MethodRawDataJson = JObject.FromObject(new object())
      };

      // When
      // an attempt is made to post test without mandatory attributes

      AsyncTestDelegate action = async () => await _apiClient.PostTestAsync(testDto);

      // Then
      Assert.ThrowsAsync<BadRequestException>(action);
    }

    [Test]
    [Order(100)]
    public void PostTest_ShouldFail_GivenNoInspectionId()
    {
      // Given
      var testDto = new TestPostDto
      {
        MethodId = Guid.NewGuid().ToString(),
        MethodRawDataJson = JObject.FromObject(new object())
      };

      // When
      // an attempt is made to post test without mandatory attributes

      AsyncTestDelegate action = async () => await _apiClient.PostTestAsync(testDto);

      // Then
      Assert.ThrowsAsync<BadRequestException>(action);
    }

  }
}