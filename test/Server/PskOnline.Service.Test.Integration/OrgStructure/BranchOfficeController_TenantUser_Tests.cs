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
  using NUnit.Framework;

  using PskOnline.Client.Api;
  using PskOnline.Client.Api.Authority;
  using PskOnline.Client.Api.Models;
  using PskOnline.Client.Api.Organization;
  using PskOnline.Components.Log;
  using PskOnline.Service.Test.Integration.TestData;

  [TestFixture(
    Author = "Adadurov",
    Description = "Validates various scenarios related to Branch Offices. " +
                  "Notice that this test fixture is initialized only once!")]
  public class BranchOfficeController_TenantUser_Tests : IDisposable
  {
    DefaultWebApplicationFactory _app;

    const string _url = "/api/BranchOffice/";

    HttpClient _httpClient;

    IApiClient _apiClient;

    TenantContainer _gryffindorHouse;

    TenantContainer _slytherinHouse;

    UserDto _siteAdminUser;

    Guid _nonExistingTenantId = Guid.NewGuid();

    [SetUp]
    public async Task SetUp()
    {
      await InitOnce();

      await _apiClient.SignInWithUserPassword_WithSlug_Async(
        _gryffindorHouse.AdminName, _gryffindorHouse.AdminPassword,
        _httpClient, _gryffindorHouse.Tenant.Slug);
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

      _apiClient.SignInWithUserPassword_WithSlug_Async(
        TestUsers.DefaultSiteAdminName, TestUsers.DefaultSiteAdminPass,
        _httpClient, "").Wait();
      _siteAdminUser = await _apiClient.GetCurrentUserAsync();

      _gryffindorHouse = await TestTenants.CreateTenantWithRolesAndUsers(_apiClient, _httpClient, TestTenants.GryffindorHouse);

      await _apiClient.SignInWithUserPassword_WithSlug_Async(
        TestUsers.DefaultSiteAdminName, TestUsers.DefaultSiteAdminPass,
        _httpClient, "");
      _slytherinHouse = await TestTenants.CreateTenantWithRolesAndUsers(_apiClient, _httpClient, TestTenants.SlytherinHouse);

    }

    [Test]
    public async Task CreateAndReadBranchOffice_ShouldSucceed()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      var branchOfficeDto = new BranchOfficeDto
      {
        Id = Guid.NewGuid().ToString(),
        Name = "Branch Office #100",
        TimeZoneId = TimeZoneInfo.Local.Id
      };

      var numBranchOfficesBefore = 
        (await _apiClient.GetTenantOperationsSummaryAsync(
        _gryffindorHouse.TenantId.ToString())).BranchOfficesCount;

      // When
      // the user posts a new branch office...
      var newId = await _apiClient.PostBranchOfficeAsync(branchOfficeDto);

      // Then
      // the request should succeed
      // And the following 'read' request should succeed
      var createdBranchOfficeDto = await _apiClient.GetBranchOfficeAsync(newId);

      Assert.That(createdBranchOfficeDto.Name, Is.EqualTo(branchOfficeDto.Name));
      Assert.That(createdBranchOfficeDto.TimeZoneId, Is.EqualTo(TimeZoneInfo.Local.Id));

      var numBranchOfficesAfter =
        (await _apiClient.GetTenantOperationsSummaryAsync(
        _gryffindorHouse.TenantId.ToString())).BranchOfficesCount;

      Assert.That(numBranchOfficesAfter, Is.EqualTo(numBranchOfficesBefore + 1));

    }

    [Test]
    public async Task CreateBranchOffice_ShouldFail_GivenDuplicateName()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // branch office DTO
      var branchOfficeDto = new BranchOfficeDto
      {
        Name = "Branch Office #1",
        TimeZoneId = TimeZoneInfo.Local.Id
      };
      var createFirstBranchOfficeResponse = await _httpClient.PostAsJsonAsync(_url, branchOfficeDto);
      createFirstBranchOfficeResponse.EnsureSuccessStatusCode();

      // When
      // Update is requested
      var createSecondBranchOfficeResponse = await _httpClient.PostAsJsonAsync(_url, branchOfficeDto);

      // Then
      // the request should succeed
      Assert.That(
        createSecondBranchOfficeResponse.StatusCode,
        Is.EqualTo(HttpStatusCode.BadRequest)
        );
    }

    [Test]
    public async Task CreateBranchOffice_ShouldFail_GivenDuplicateId()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // created a branch office with name "Branch Office #11"
      var branchOfficeDto = new BranchOfficeDto
      {
        Id = Guid.NewGuid().ToString(),
        Name = "Branch Office #11",
        TimeZoneId = TimeZoneInfo.Local.Id
      };
      var createFirstBranchOfficeResponse = await _httpClient.PostAsJsonAsync(_url, branchOfficeDto);
      Console.WriteLine(await createFirstBranchOfficeResponse.Content.ReadAsStringAsync());

      // When
      // The user tries to create another branch office with the same Id
      // and a different name
      branchOfficeDto.Name += ".001";
      var createSecondBranchOfficeResponse = await _httpClient.PostAsJsonAsync(_url, branchOfficeDto);

      // Then
      // the request should fail with 'Conflict' status code
      Assert.That(
        createSecondBranchOfficeResponse.StatusCode,
        Is.EqualTo(HttpStatusCode.Conflict)
        );
    }

    [Test]
    public async Task CreateBranchOffice_ShouldSucceed_GivenDuplicateNames_InTwoTenants()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      var branchOfficeDto = new BranchOfficeDto
      {
        Name = "Branch Office #101",
        TimeZoneId = TimeZoneInfo.Local.Id
      };
      // created a tenant with the name "Branch Office #100"
      var createBranchOfficeResponse = _httpClient.PostAsJsonAsync(_url, branchOfficeDto).Result;
      Console.WriteLine(await createBranchOfficeResponse.Content.ReadAsStringAsync());
      createBranchOfficeResponse.EnsureSuccessStatusCode();

      // switch to tenant #2
      await _apiClient.SignInWithUserPassword_WithSlug_Async(
        _slytherinHouse.AdminName, _slytherinHouse.AdminPassword,
        _httpClient, _slytherinHouse.Tenant.Slug);

      // When
      // user from different tenant creates a branch office with the name
      // duplicating name of a branch office in a different tenant...
      var createBranchOffice2Response = _httpClient.PostAsJsonAsync(_url, branchOfficeDto).Result;

      // Then
      // the request should succeed
      Console.WriteLine(await createBranchOffice2Response.Content.ReadAsStringAsync());
      createBranchOffice2Response.EnsureSuccessStatusCode();

      Assert.That(
        createBranchOffice2Response.StatusCode,
        Is.EqualTo(HttpStatusCode.Created)
        );
    }

    [Test]
    public async Task GetAllBranchOffices_ShouldSucceed()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // 2 branch offices
      var branchOfficeDto1 = new BranchOfficeDto
      {
        Name = "Branch Office #1001",
        TimeZoneId = TimeZoneInfo.Local.Id
      };
      var createFirstBranchOfficeResponse = await _httpClient.PostAsJsonAsync(_url, branchOfficeDto1);
      Console.WriteLine("#1001 => " + await createFirstBranchOfficeResponse.Content.ReadAsStringAsync());
      createFirstBranchOfficeResponse.EnsureSuccessStatusCode();

      var branchOfficeDto = new BranchOfficeDto
      {
        Name = "Branch Office #1002",
        TimeZoneId = TimeZoneInfo.Utc.Id
      };
      var createSecondBranchOfficeResponse = await _httpClient.PostAsJsonAsync(_url, branchOfficeDto);
      Console.WriteLine("#1002 => " + await createSecondBranchOfficeResponse.Content.ReadAsStringAsync());
      createSecondBranchOfficeResponse.EnsureSuccessStatusCode();

      // When
      // User requests all branch offices
      var getBranchOfficesResponse = await _httpClient.GetAsync(_url);

      // Then
      // the request should succeed and return more than 1 branch office
      Assert.That(
        getBranchOfficesResponse.StatusCode,
        Is.EqualTo(HttpStatusCode.OK)
        );

      var items = await getBranchOfficesResponse.Content.ReadAsJsonAsync<List<BranchOfficeDto>>();
      Assert.That(items.Count(), Is.GreaterThan(1));
    }

    [Test]
    public async Task GetBranchOffice_ShouldFail_GivenWrongTenant()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // created the branch office
      var branchOfficeDto = new BranchOfficeDto
      {
        Id = Guid.NewGuid().ToString(),
        Name = "Branch Office To Be Forbidden",
        TimeZoneId = TimeZoneInfo.Local.Id
      };

      var postBranchOfficeResult = await _httpClient.PostAsJsonAsync(_url, branchOfficeDto);
      postBranchOfficeResult.EnsureSuccessStatusCode();
      Guid newId = (await postBranchOfficeResult.Content.ReadAsJsonAsync<CreatedWithGuidDto>()).Id;

      // When
      // Slytherin user tries to read the new Branch office...
      await _apiClient.SignInWithUserPassword_WithSlug_Async(
        _slytherinHouse.AdminName, _slytherinHouse.AdminPassword,
        _httpClient, _slytherinHouse.Tenant.Slug);
      var getBranchOffice = await _httpClient.GetAsync(_url + newId);

      // Then
      // the request should fail
      Console.WriteLine(await getBranchOffice.Content.ReadAsStringAsync());
      Assert.That(
        getBranchOffice.StatusCode,
        Is.EqualTo(HttpStatusCode.Forbidden)
        );
    }

    [Test]
    public async Task UpdateBranchOffice_ShouldSucceed()
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // and created branch office named Branch Office #5
      var bo2Name = "Branch Office #5";
      var branchOfficeDto = new BranchOfficeDto
      {
        Id = Guid.NewGuid().ToString(),
        Name = bo2Name,
        TimeZoneId = "MSK+1"
      };

      var branchOfficeDtoResponse = await _httpClient.PostAsJsonAsync(_url, branchOfficeDto);
      branchOfficeDtoResponse.EnsureSuccessStatusCode();

      var bo2Guid = (await branchOfficeDtoResponse.Content.ReadAsJsonAsync<CreatedWithGuidDto>()).Id;
      branchOfficeDto.Id = bo2Guid.ToString();

      branchOfficeDto.Name = "Branch Office #5.1";

      // When
      // the user attempts to rename Branch Office #5 to Branch Office #5.1
      var updateBranchOfficeResponse = await _httpClient.PutAsJsonAsync(_url + bo2Guid, branchOfficeDto);

      // Then
      // the request should succeed
      Assert.That(
        updateBranchOfficeResponse.StatusCode,
        Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task UpdateBranchOffice_ShouldFail_GivenBranchOfficeInOtherTenant()
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // and created branch office named "Branch Office To Fail Update"
      var branchOfficeDto = new BranchOfficeDto
      {
        Name = "Branch Office To Fail Update",
        TimeZoneId = "MSK+3"
      };

      var createBranchOfficeResponse = await _httpClient.PostAsJsonAsync(_url, branchOfficeDto);
      createBranchOfficeResponse.EnsureSuccessStatusCode();
      Guid newId = (await createBranchOfficeResponse.Content.ReadAsJsonAsync<CreatedWithGuidDto>()).Id;

      // When
      // a user from a different tenant attempts to delete the Branch Office
      await _apiClient.SignInWithUserPassword_WithSlug_Async(
        _slytherinHouse.AdminName, _slytherinHouse.AdminPassword,
        _httpClient, _slytherinHouse.Tenant.Slug);
      branchOfficeDto.Name += " updated";
      var updateBranchOfficeResponse = await _httpClient.PutAsJsonAsync(_url + newId.ToString(), branchOfficeDto);

      // Then
      // the request should fail
      Assert.That(
        updateBranchOfficeResponse.StatusCode,
        Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task UpdateBranchOffice_ShouldFail_GivenDuplicateName()
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // created 2 branch offices: "Branch Office #2" & "Branch Office #3"
      var bo2Name = "Branch Office #2";
      var branchOfficeDto = new BranchOfficeDto
      {
        Id = Guid.NewGuid().ToString(),
        Name = bo2Name,
        TimeZoneId = TimeZoneInfo.Local.Id
      };

      var branchOfficeDtoResponse = await _httpClient.PostAsJsonAsync(_url, branchOfficeDto);
      branchOfficeDtoResponse.EnsureSuccessStatusCode();

      branchOfficeDto.Name = "Branch Office #3";
      branchOfficeDto.Id = Guid.NewGuid().ToString();
      branchOfficeDtoResponse = await _httpClient.PostAsJsonAsync(_url, branchOfficeDto);
      branchOfficeDtoResponse.EnsureSuccessStatusCode();

      var bo3Guid = (await branchOfficeDtoResponse.Content.ReadAsJsonAsync<CreatedWithGuidDto>()).Id;
      branchOfficeDto.Id = bo3Guid.ToString();

      branchOfficeDto.Name = bo2Name;

      // When
      // user attempts to rename Branch Office #3 to Branch Office #2
      var updateBranchOfficeResponse = await _httpClient.PutAsJsonAsync(_url + bo3Guid, branchOfficeDto);

      // Then
      // the request should fail
      Assert.That(
        updateBranchOfficeResponse.StatusCode,
        Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task DeleteBranchOffice_ShouldSucceed_GivenBranchOfficeInUsersTenant()
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // and created branch office named Branch Office #5
      var bo2Name = "Branch Office #500";
      var branchOfficeDto = new BranchOfficeDto
      {
        Id = Guid.NewGuid().ToString(),
        Name = bo2Name,
        TimeZoneId = TimeZoneInfo.Local.Id
      };

      var createBranchOfficeResponse = await _httpClient.PostAsJsonAsync(_url, branchOfficeDto);
      createBranchOfficeResponse.EnsureSuccessStatusCode();

      // When
      // the user attempts to delete the Branch Office
      var updateBranchOfficeResponse = await _httpClient.DeleteAsync(_url + branchOfficeDto.Id);

      // Then
      // the request should succeed
      Assert.That(
        updateBranchOfficeResponse.StatusCode,
        Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task DeleteBranchOffice_ShouldFail_GivenBranchOfficeInOtherTenant()
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // and created branch office named Branch Office #50
      var bo2Name = "Branch Office #50";
      var branchOfficeDto = new BranchOfficeDto
      {
        Id = Guid.NewGuid().ToString(),
        Name = bo2Name,
        TimeZoneId = TimeZoneInfo.Local.Id
      };

      var createBranchOfficeResponse = await _httpClient.PostAsJsonAsync(_url, branchOfficeDto);
      createBranchOfficeResponse.EnsureSuccessStatusCode();

      // When
      // a user from a different tenant attempts to delete the Branch Office
      await _apiClient.SignInWithUserPassword_WithSlug_Async(
        _slytherinHouse.AdminName, _slytherinHouse.AdminPassword,
        _httpClient, _slytherinHouse.Tenant.Slug);

      var deleteBranchResponse = await _httpClient.DeleteAsync(_url + branchOfficeDto.Id);

      // Then
      // the request should fail
      Assert.That(
        deleteBranchResponse.StatusCode,
        Is.EqualTo(HttpStatusCode.Forbidden));
    }
  }
}
