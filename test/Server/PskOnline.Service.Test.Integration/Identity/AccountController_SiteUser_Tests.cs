namespace PskOnline.Service.Test.Integration.Identity
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Net;
  using System.Net.Http;
  using System.Threading.Tasks;

  using AutoMapper;
  using log4net;
  using Microsoft.AspNetCore.Mvc.Testing;
  using NUnit.Framework;

  using PskOnline.Client.Api;
  using PskOnline.Client.Api.Authority;
  using PskOnline.Components.Log;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Shared.Permissions;
  using PskOnline.Service.Test.Integration.TestData;

  [TestFixture(
    Author = "Adadurov",
    Description =
    "Scenario test -- sensitive to order of test cases execution. " +
    "Be careful when editing test cases. \r\n")]
  public class AccountController_SiteUser_Tests : IDisposable
  {
    ILog _log = log4net.LogManager.GetLogger(typeof(AccountController_SiteUser_Tests));
    DefaultWebApplicationFactory _app;
    IApiClient _apiClient;
    IMapper _mapper;

    HttpClient _httpClient;
    IList<RoleDto> _allRoles;
    UserEditDto _siteAdminUser;

    [SetUp]
    public void SetUp()
    {
      _mapper = TestMapperConfiguration.Config.CreateMapper();
      InitOnce();
    }

    public void Dispose()
    {
      _httpClient?.Dispose();
      _httpClient = null;
      _app?.Dispose();
      _app = null;
      LogHelper.ShutdownLogSystem();
    }

    private void InitOnce()
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
      _apiClient.AsSiteAdminAsync(_httpClient).Wait();

      _siteAdminUser = _mapper.Map<UserEditDto>(_apiClient.GetUserByUsernameAsync(TestUsers.DefaultSiteAdminName).Result);
      _allRoles = _apiClient.GetRolesAsync().Result.ToList();
    }

    [Test]
    [Order(1)]
    public async Task GetAllPermissions_ShouldReturnAllPermissions_ToSiteUser()
    {
      // Given
      // Client authenticated as site admin

      // When
      var permissions = await _apiClient.GetPermissionsAsync();

      // Then
      // the request should succeed and return global permissions

      Assert.IsTrue( permissions.Any(p => p.Value.Contains(PermScope.GLOBAL)) );
      Assert.That( permissions.Count(), Is.EqualTo(24) );
    }

    [Test]
    [Order(2)]
    public void GetUserByUserName_ShouldSucceed()
    {
      // Given
      // Application initialized with default admin account

      // When
      var user = _apiClient.GetUserByUsernameAsync(TestUsers.DefaultSiteAdminName).Result;

      // Then
      Assert.That(user, Is.Not.Null);
    }

    [Test]
    [Order(3)]
    public async Task UpdateSiteUser_ShouldSucceed()
    {
      // Given
      // a random role DTO
      var userPatchDto = await _apiClient.GetUserAsync(_siteAdminUser.Id);

      userPatchDto.FirstName += "New first name";

      // When
      // Update is requested
      await _apiClient.PutUserAsync(userPatchDto);

      // Then
      // the request should succeed
      var updatedUser = await _apiClient.GetUserAsync(userPatchDto.Id);
      Assert.That(updatedUser.FirstName, Is.EqualTo(userPatchDto.FirstName));
    }
  }
}
