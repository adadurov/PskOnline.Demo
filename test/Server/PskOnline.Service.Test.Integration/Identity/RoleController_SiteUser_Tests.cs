namespace PskOnline.Service.Test.Integration.Identity
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Net;
  using System.Net.Http;
  using System.Net.Http.Headers;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Mvc.Testing;
  using NUnit.Framework;

  using PskOnline.Client.Api;
  using PskOnline.Client.Api.Authority;
  using PskOnline.Components.Log;
  using PskOnline.Server.Authority.Roles;
  using PskOnline.Server.DAL.Multitenancy;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Shared.Permissions.Standard;
  using PskOnline.Service.Test.Integration.TestData;

  [TestFixture(
    Author = "Adadurov",
    Description = 
    "Scenario test -- sensitive to order of test cases execution. " + 
    "Be careful when editing test cases. \r\n" +
    "Verifies that Site data are accessible to Site user. " + 
    "Plus, it verifies that other tenant data are ALSO accessible to the same user")]
  public class RoleController_SiteUser_Tests : IDisposable
  {
    DefaultWebApplicationFactory _app;

    HttpClient _httpClient;

    IApiClient _apiClient;

    IEnumerable<RoleDto> _siteRoles;

    IEnumerable<RoleDto> _tenantRoles;

    Guid _gryffindorHouseAdminRoleId;

    [SetUp]
    public async Task SetUp()
    {
      await InitOnce();
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

      var options = new WebApplicationFactoryClientOptions
      {
        AllowAutoRedirect = false
      };
      _httpClient = _app.CreateClient(options);

      _apiClient = new ApiClient(_httpClient, _app.GetLogger<ApiClient>());

      // admin creates Gryffindor tenant and initializes default branch office
      await _apiClient.AsSiteAdminAsync(_httpClient);

      var tenant = await TestTenants.CreateTenantWithRolesAndUsers(_apiClient, _httpClient, TestTenants.GryffindorHouse);
      _gryffindorHouseAdminRoleId = tenant.TenantAdminRoleId;

      _tenantRoles = await _apiClient.GetRolesAsync();

      // Create a site role
      await _apiClient.AsSiteAdminAsync(_httpClient);
      var siteRole = new RoleDto() {
        Name = "Qwertyasdfzcv 123",
        Description = "Hello, world"
      };
      await _apiClient.PostRoleAsync(siteRole);
      _siteRoles = await _apiClient.GetRolesAsync();
      _siteRoles = _siteRoles.Where(sr => ! _tenantRoles.Any(tr => tr.Id == sr.Id));
    }

    [Test]
    [Order(11)]
    public async Task PostSiteRole_ShouldSucceed()
    {
      // Given
      var newSiteRoleDto = new RoleDto
      {
        Name = "Tenant Viewer Role",
        Description = "Tenant Viewer Role Description",
        Permissions = new PermissionDto[]
        {
          new PermissionDto
          {
            Value = TenantPermissions.Tenants_GLOBAL_View.Value
          }
        }
      };

      // When
      var newRoleId = await _apiClient.PostRoleAsync(newSiteRoleDto);

      // Then
      // 1. The request succeeds

      // 2. The server returns a role when the client requests role by the new id
      var verifyRole = await _apiClient.GetRoleAsync(newRoleId);

      Assert.That(verifyRole.Name, Is.EqualTo(newSiteRoleDto.Name));
    }

    [Test]
    [Order(11)]
    public async Task PostSiteRole_ShouldFail_GivenDuplicateName()
    {
      // Given
      var newSiteRoleDto = new RoleDto
      {
        Name = "Duplicate role name for test",
        Description = "Duplicate role description for test",
        Permissions = new PermissionDto[]
        {
          new PermissionDto
          {
            Value = TenantPermissions.Tenants_GLOBAL_View.Value
          }
        }
      };
      var newRoleId = await _apiClient.PostRoleAsync(newSiteRoleDto);

      // When
      // a user attempts to create a role with the same ID 
      newSiteRoleDto.Id = null;
      AsyncTestDelegate action = async () => await _apiClient.PostRoleAsync(newSiteRoleDto);

      // Then
      // The request fails with BAD REQUEST status code
      Assert.ThrowsAsync<BadRequestException>(action);
    }

    [Test]
    [Order(1)]
    [Ignore("Creation of tenant role is not implemented")]
    public void PostTenantRole_ShouldSucceed()
    {
      // currently, the controller doesn't permit posting of roles to other 
      // tenants (TenantId is replaced with the content of the current user's
      // TenantId claim
      //
      // so the proper handling of cross-tenant posting by tenant user
      // should be verified in a unit test
    }

    [Test]
    [Order(2)]
    public async Task GetAllRoles_ShouldReturnAllRolesOnSite()
    {
      // Given
      var numRolesInInfrastructure = _siteRoles.Count() + _tenantRoles.Count();

      // When
      var visibleRoles = await _apiClient.GetRolesAsync();

      // Then
      Assert.That(
        visibleRoles.Count(),
        Is.EqualTo(numRolesInInfrastructure)
        );
    }

    [Test]
    [Order(2)]
    public async Task GetSiteRole_ShouldSucceed()
    {
      // Given
      var siteRoleId = _siteRoles.First().Id;

      // When
      var receivedSiteRole = await _apiClient.GetRoleAsync(siteRoleId);

      // Then
      Assert.That(receivedSiteRole.Id, Is.EqualTo(siteRoleId));
    }

    [Test]
    [Order(2)]
    public async Task GetTenantRole_ShouldSucceed()
    {
      // Given

      // When
      var role = await _apiClient.GetRoleAsync(_gryffindorHouseAdminRoleId);

      // Then
      Assert.That(role.Id, Is.EqualTo(_gryffindorHouseAdminRoleId.ToString()));

      var adminTemplatePermissions = DefaultTenantRoles.TenantAdministratorTemplate.Permissions;
      foreach (var permission in adminTemplatePermissions)
      {
        Assert.That(role.Permissions.Any(p => p.Value == permission.Value));
      }
    }

    [Test]
    [Order(9)]
    public async Task UpdateSiteRole_ShouldSucceed()
    {
      // Given
      var siteRoleId = _siteRoles.First().Id;
      var existingRole = await _apiClient.GetRoleAsync(siteRoleId);

      existingRole.Name += " modified";

      // When
      await _apiClient.PutRoleAsync(existingRole);
      var verifyRole = await _apiClient.GetRoleAsync(existingRole.Id);

      // Then
      Assert.That(verifyRole.Name, Is.EqualTo(existingRole.Name));
    }

    [Test]
    [Order(9)]
    public async Task UpdateTenantRole_ShouldSucceed()
    {
      // Given
      var tenantRoleId = _tenantRoles.First().Id;
      var existingRole = await _apiClient.GetRoleAsync(tenantRoleId);

      existingRole.Name += " modified";

      // When
      await _apiClient.PutRoleAsync(existingRole);

      // Then
      var verifyRole = await _apiClient.GetRoleAsync(existingRole.Id);

      Assert.That(verifyRole.Name, Is.EqualTo(existingRole.Name));
    }

    [Test]
    [Order(10)]
    public async Task DeleteSiteRole_ShouldSucceed()
    {
      // Given
      // a site role that is not Site Admin
      var role = _siteRoles.First();
      var roleId = role.Id;

      // When
      await _apiClient.DeleteRoleAsync(roleId);

      // Then
      AsyncTestDelegate action = async () => await _apiClient.GetRoleAsync(roleId);
      Assert.ThrowsAsync<ItemNotFoundException>(action);
    }

    [Test]
    [Order(10)]
    public async Task DeleteTenantRole_ShouldSucceed()
    {
      // Given
      // a role belonging to Gryffindor House, Hogwarts
      var role = _tenantRoles.First();
      var roleId = role.Id;

      // When
      await _apiClient.DeleteRoleAsync(roleId);

      // Then
      // The following attempt to get the same role
      // fails with 'NotFound' status
      AsyncTestDelegate action = async () => await _apiClient.GetRoleAsync(roleId);

      Assert.ThrowsAsync<ItemNotFoundException>(action);
    }
  }
}
