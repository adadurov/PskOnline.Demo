namespace PskOnline.Service.Test.Integration.Identity
{
  using System;
  using System.Net;
  using System.Net.Http;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Mvc.Testing;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;
  using NUnit.Framework;
  using PskOnline.Client.Api;
  using PskOnline.Components.Log;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Service.ViewModels;
  using PskOnline.Service.Test.Integration.TestData;


  [TestFixture]
  public class TenantController_TenantUser_Tests : IDisposable
  {
    const string _url = "/api/Tenant/";
    DefaultWebApplicationFactory _app;
    HttpClient _httpClient;
    IApiClient _apiClient;

    string _gryffindorTenantId;
    string _slytherinTenantId;

    [SetUp]
    public async Task SetUp()
    {
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
      _httpClient = _app.CreateClient(/* not using options */);
      _apiClient = new ApiClient(_httpClient, _app.GetLogger<ApiClient>());

      await _apiClient.AsSiteAdminAsync(_httpClient);
      var tenant = await TestTenants.CreateTenantWithRolesAndUsers(_apiClient, _httpClient, TestTenants.GryffindorHouse);
      _gryffindorTenantId = tenant.Tenant.Id;

      await _apiClient.AsSiteAdminAsync(_httpClient);
      var tenant2 = await TestTenants.CreateTenantWithRolesAndUsers(_apiClient, _httpClient, TestTenants.SlytherinHouse);

      _slytherinTenantId = tenant2.Tenant.Id;

    }

    [Test]
    public void PostTenant_ShouldFail_Given_Tenant_User()
    {
      // Given
      // _client authenticated as a Gryffindor user

      // When
      // the user tries to post a new tenant
      var newTenant = new TenantCreateDto
      {
        TenantDetails = new TenantDto
        {
          Id = Guid.NewGuid().ToString(),
          Name = "AAA",
          Slug = "aaa",
          PrimaryContact = new ContactInfoDto
          {
            FullName = "Neville Longbottom",
            Email = "longbottom@hogwarts.edu.uk"
          },
          ServiceDetails = new ServiceDetailsDto
          {
            ServiceMaxUsers = 10
          }
        },
        AdminUserDetails = new TenantCreateAdminDto
        {
          Email = "123@test.com",
          UserName = "Neville.Longbottom",
          FirstName = "Neville",
          Patronymic = "Frankovich",
          LastName = "Longbottom",
          PhoneNumber = "+79990987654",
          NewPassword = "123$%^&*"
        }
      };
      var response = _httpClient.PostAsJsonAsync(_url, newTenant);
      
      // Then
      // Server returns 'Forbidden' status code
      Assert.That(response.Result.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public void ReadOwnTenantTenant_ShouldSucceed()
    {
      // Given 
      // _client authenticated as a Gryffindor user

      // When
      // the user attempts to get details of their own tenant
      var tenantResponse = _httpClient.GetAsync(_url + _gryffindorTenantId.ToString()).Result;
      var tenant = tenantResponse.Content.ReadAsJsonAsync<TenantDto>().Result;

      // Then
      // the request is successful and the tenant with the expected Id is returned
      tenantResponse.EnsureSuccessStatusCode();
      Assert.That(tenant.Id, Is.EqualTo(_gryffindorTenantId.ToString()));
    }

    [Test]
    public void ReadOtherTenant_ShouldFailWithForbidden()
    {
      // Given 
      // _client authenticated as a Gryffindor user

      // When
      // the user attempts to get details of another tenant (Ministry Of Magic)
      var tenantResponse = _httpClient.GetAsync(_url + _slytherinTenantId.ToString()).Result;

      // Then
      // the request is unsuccessful and returns the tenant
      Assert.That(tenantResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public void UpdateOwnTenant_ShouldFail()
    {
      // Given 
      // _client authenticated as a Gryffindor user obtained its own tenant details
      var response = _httpClient.GetAsync(_url + _gryffindorTenantId.ToString());
      var tenant = response.Result.Content.ReadAsJsonAsync<TenantDto>().Result;

      // When
      // the user attemps to modify details of their own tenant
      tenant.Name = tenant.Name + "_modified";
      var putResponse = _httpClient.PutAsJsonAsync(_url + $"{tenant.Id}", tenant);

      // Then
      // the server returns 'Forbidden'
      Assert.That(putResponse.Result.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public void UpdateOtherTenant_ShouldFail()
    {
      // Given 
      // _client authenticated as a Gryffindor user obtained its own tenant details
      var response = _httpClient.GetAsync(_url + _gryffindorTenantId.ToString());
      response.Result.EnsureSuccessStatusCode();
      var tenant = response.Result.Content.ReadAsJsonAsync<TenantDto>().Result;

      // When
      // the user attemps to modify details of a different tenant
      // using the content of the own tenant (maybe by mistake?)
      tenant.Name = tenant.Name + "_modified";
      tenant.Id = _slytherinTenantId.ToString();
      var putResponse = _httpClient.PutAsJsonAsync(_url + $"{_slytherinTenantId}", tenant);

      // Then
      // the server returns 'Forbidden'
      Assert.That(putResponse.Result.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public void DeleteOwnTenant_ShouldFail()
    {
      // Given
      // _client authenticated as a Gryffindor user

      // When
      // the user attemps to delete their own tenant
      var response = _httpClient.DeleteAsync(_url + _gryffindorTenantId.ToString()).Result;

      // Then
      // the server returns 'Forbidden'
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public void DeleteOtherTenant_ShouldFail()
    {
      // Given
      // _client authenticated as a Gryffindor user

      // When
      // the user attemps to delete another tenant
      var response = _httpClient.DeleteAsync(_url + _slytherinTenantId.ToString()).Result;

      // Then
      // the server returns 'Forbidden'
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public void ReadAllTenants_ShouldReturnOwnTenantOnly()
    {
      // Given 
      // _client authenticated as a Gryffindor user

      // When
      // tenant users requests list of tenants...
      var visibleTenantsResponse = _httpClient.GetAsync(_url).Result;
      var visibleTenants = visibleTenantsResponse.Content.ReadAsJsonAsync<TenantDto[]>().Result;

      // Then
      // only the tenant that the user belongs to is returned
      Assert.That(visibleTenants.Length, Is.EqualTo(1));
      Assert.That(visibleTenants[0].Id, Is.EqualTo(_gryffindorTenantId.ToString()));
    }
  }
}