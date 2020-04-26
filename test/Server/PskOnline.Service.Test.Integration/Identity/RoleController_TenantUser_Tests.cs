namespace PskOnline.Service.Test.Integration.Identity
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Net.Http;
  using System.Threading.Tasks;
  using log4net;
  using Microsoft.AspNetCore.Mvc.Testing;
  using Newtonsoft.Json.Linq;
  using NUnit.Framework;
  using PskOnline.Client.Api;
  using PskOnline.Client.Api.Authority;
  using PskOnline.Components.Log;
  using PskOnline.Server.DAL.Multitenancy;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Shared.Permissions.Standard;
  using PskOnline.Service.Test.Integration.TestData;

  [TestFixture(
    Author = "Adadurov",
    Description =
    "Scenario test -- sensitive to order of test cases execution. " +
    "Be careful when editing test cases. \r\n" +
    "Verifies that Gryffindor Role data are accessible to Gryffindor user. " + 
    "Plus, it verifies that other tenant data are not accessible to the same user")]
  public class RoleController_TenantUser_Tests : IDisposable
  {
    ILog _log = LogManager.GetLogger(typeof(RoleController_TenantUser_Tests));
    DefaultWebApplicationFactory _app;
    HttpClient _httpClient;
    IApiClient _apiClient;

    Guid _gryffindorHouseGuid;
    Guid _otherTenantGuid;
    IList<RoleDto> _allSiteRoles;
    IList<RoleDto> _gryffindorRoles;

    Guid _gryffindorHouseAdminRoleId;
    string _newRoleInTenantId;
    RoleDto _newRoleInTenantDto;

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

      _gryffindorHouseGuid = Guid.NewGuid();
      _otherTenantGuid = Guid.NewGuid();
      _app = new DefaultWebApplicationFactory();

      var options = new WebApplicationFactoryClientOptions
      {
        AllowAutoRedirect = false
      };
      _httpClient = _app.CreateClient(options);

      _apiClient = new ApiClient(_httpClient, _app.GetLogger<ApiClient>());

      await _apiClient.AsSiteAdminAsync(_httpClient);
      _allSiteRoles = (await _apiClient.GetRolesAsync()).ToList();

      var gryff = await TestTenants.CreateTenantWithRolesAndUsers(_apiClient, _httpClient, TestTenants.GryffindorHouse);
      _gryffindorRoles = (await _apiClient.GetRolesAsync()).ToList();
      _gryffindorHouseAdminRoleId = gryff.TenantAdminRoleId;
      _newRoleInTenantId = _gryffindorRoles.First(r => r.Id != _gryffindorHouseAdminRoleId.ToString()).Id;
    }

    [Test]
    public void PostSiteRole_ShouldFail()
    {
      // currently, the controller doesn't permit posting of roles to other 
      // tenants (TenantId is replaced with the content of the current user's
      // TenantId claim
      //
      // so the proper handling of cross-tenant posting by tenant user
      // should be verified in a unit test
    }

    [Test]
    [Order(1)]
    public async Task PostTenantRole_ShouldSucceed()
    {
      // Given
      var newRoleInTenant = new RoleDto
      {
        Name = "Tenant Role 2",
        Description = "Tenant Role 2 Description",
        Permissions = new PermissionDto[]
        {
          new PermissionDto
          {
            Value = OrgStructurePermissions.Org_Tenant_View.Value
          }
        }
      };

      // When
      // Role is posted
      newRoleInTenant.Id = (await _apiClient.PostRoleAsync(newRoleInTenant)).ToString();

      // Then
      // Non-empty GUID is returned in response
      Assert.That(newRoleInTenant.Id, Is.Not.EqualTo(Guid.Empty));

      // Save for future use
      _newRoleInTenantDto = newRoleInTenant;
    }

    [Test]
    [Order(2)]
    public async Task GetAllRoles_ShouldReturnRolesInOwnTenant()
    {
      // Given

      // When
      var visibleRoles = await _apiClient.GetRolesAsync();

      // Then
      visibleRoles.Select(r => { Console.WriteLine(JObject.FromObject(r).ToString()); return r; } );

      Assert.That(
        visibleRoles.Count(), 
        Is.EqualTo(DefaultTenantRoles.GetStandardTenantNonAdminRoleTemplates().Count
        + 1 // admin role
        + 1 // "Tenant Role 2" added by PostTenantRole_ShouldSucceed
        ));
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
      foreach( var permission in adminTemplatePermissions)
      {
        Assert.That(role.Permissions.Any(p => p.Value == permission.Value));
      }
    }

    [Test]
    [Order(3)]
    public void GetTenantRole_ShouldFail_GivenRoleDoesntExist()//
    {
      // Given
      // a random role DTO
      var roleDto = new RoleDto
      {
        Id = Guid.NewGuid().ToString(),
        Name = "123 Name",
        Description = "123 Description"
      };

      // When
      AsyncTestDelegate action = async () => await _apiClient.GetRoleAsync(roleDto.Id);

      // Then
      Assert.ThrowsAsync<ItemNotFoundException>(action);
    }

    [Test]
    [Order(2)]
    public void UpdateSiteRole_ShouldFail()
    {
      // Given
      var role = _allSiteRoles.First();
      role.Name += " Modified";

      // When
      _log.Info($"Tenant user attempting to update SITE role '{role.Name}' ({role.Id})");
      AsyncTestDelegate action = async () => await _apiClient.PutRoleAsync(role);

      // Then
      // a 'Forbidden' exception is thrown
      Assert.ThrowsAsync<UnauthorizedAccessException>(action);
    }

    [Test]
    [Order(3)]
    public async Task UpdateTenantRole_ShouldSucceed()
    {
      var tenantNonAdminRoleId = _gryffindorRoles.First( 
        r => r.Name != DefaultTenantRoles.TenantAdministratorTemplate.Name).Id;

      var existingRole = await _apiClient.GetRoleAsync(tenantNonAdminRoleId);

      existingRole.Name += " modified";

      // When
      await _apiClient.PutRoleAsync(existingRole);
      var verifyingRole = await _apiClient.GetRoleAsync(existingRole.Id);

      // Then
      Assert.That(verifyingRole.Name, Is.EqualTo(existingRole.Name));
    }

    [Test]
    [Order(3)]
    public void UpdateTenantRole_ShouldFail_GivenRoleDoesntExist()//
    {
      // Given
      // a random role DTO
      var roleDto = new RoleDto
      {
        Id = Guid.NewGuid().ToString(),
        Name = "123 Name",
        Description = "123 Description"
      };

      // When
      // Update is requested
      AsyncTestDelegate action = async () => await _apiClient.PutRoleAsync(roleDto);

      // Then
      // the request should fail
      Assert.ThrowsAsync<ItemNotFoundException>(action);
    }

    [Test]
    [Order(10)]
    public async Task DeleteTenantRole_ShouldSucceed()
    {
      // Given
      var roleId = _newRoleInTenantId;

      // When
      await _apiClient.DeleteRoleAsync(roleId);

      // Then
      // 1. the request should succeed (not throwing any exceptions)
      AsyncTestDelegate action = async () => await _apiClient.GetRoleAsync(roleId);

      // 2. the request to get the deleted role should fail
      Assert.ThrowsAsync<ItemNotFoundException>(action);
    }

    [Test]
    [Order(11)]
    public async Task DeleteTenantRole_ShouldSucceed_GivenRoleDoesntExist()
    {
      // Given
      // The ID of the role that was deleted in Order(10) test before
      var roleId = _newRoleInTenantId;

      // When
      // I attempt to delete the same role again
      await _apiClient.DeleteRoleAsync(roleId.ToString());

      // Then
      // the request succeeds
    }

    [Test]
    [Order(12)]
    public void DeleteSiteRole_ShouldFail()
    {
      // Given
      var role = _allSiteRoles.First();
      var roleId = role.Id;

      // When
      _log.Info($"Attempting to remove role '{role.Name}' ({role.Id})");

      AsyncTestDelegate action = async () => await _apiClient.DeleteRoleAsync(roleId);

      // Then
      Assert.ThrowsAsync<UnauthorizedAccessException>(action);
    }
  }
}
