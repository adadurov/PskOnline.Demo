namespace PskOnline.Service.Test.Integration.Identity
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

  using PskOnline.Components.Log;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Shared.Permissions;
  using PskOnline.Service.Test.Integration.TestData;
  using PskOnline.Client.Api.Authority;
  using PskOnline.Client.Api;
  using PskOnline.Client.Api.Models;
  using PskOnline.Server.DAL.Multitenancy;
  using PskOnline.Client.Api.OpenId;

  [TestFixture(
    Author = "Adadurov",
    Description =
    "Scenario test -- sensitive to order of test cases execution. " +
    "Be careful when editing test cases. \r\n")]
  public class AccountController_TenantUser_Tests : IDisposable
  {
    const string _url = "/api/account/users/";
    DefaultWebApplicationFactory _app;
    HttpClient _httpClient;
    private HttpClient _unauthorizedHttpClient;
    IApiClient _apiClient;
    IMapper _mapper;

    UserDto _siteAdminUser;
    UserDto _gryffindorAdminUser;
    UserDto _slytherinAdminUser;

    string _gryffindorAdminRoleId;
    string _slytherinAdminRoleId;

    IList<RoleDto> _siteRoles;
    IList<RoleDto> _gryffindorRoles;
    IList<RoleDto> _slytherinRoles;

    [SetUp]
    public async Task SetUp()
    {
      _mapper = TestMapperConfiguration.Config.CreateMapper();
      await InitOnce();
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

      _unauthorizedHttpClient = _app.CreateClient(options);
      _unauthorizedHttpClient.DefaultRequestHeaders.Add("accept", "application/json");

      _apiClient = new ApiClient(_httpClient, _app.GetLogger<ApiClient>());
      await _apiClient.AsSiteAdminAsync(_httpClient);

      _siteAdminUser = await _apiClient.GetCurrentUserAsync();
      _siteRoles = (await _apiClient.GetRolesAsync()).ToList();

      {
        var gryffindorTenant = await TestTenants.CreateTenantWithRolesAndUsers(
          _apiClient, _httpClient, TestTenants.GryffindorHouse);
        _gryffindorAdminUser = gryffindorTenant.AdminUser;

        await _apiClient.AsGryffindorAdminAsync(_httpClient);
        _gryffindorRoles = (await _apiClient.GetRolesAsync()).ToList();

        _gryffindorAdminRoleId = gryffindorTenant.TenantAdminRoleId.ToString();
      }

      await _apiClient.AsSiteAdminAsync(_httpClient);
      {
        var slytherinTenant = await TestTenants.CreateTenantWithRolesAndUsers(
          _apiClient, _httpClient, TestTenants.SlytherinHouse);
        _slytherinAdminUser = slytherinTenant.AdminUser;

        await _apiClient.AsSlytherinAdminAsync(_httpClient);
        _slytherinRoles = (await _apiClient.GetRolesAsync()).ToList();

        _slytherinAdminRoleId = slytherinTenant.TenantAdminRoleId.ToString();
      }
    }

    [Test]
    public void Update_ShouldFail_GivenUserDoesntExist()//
    {
      // Given
      // a random role DTO
      var userPatchDto = new UserEditDto
      {
        Id = Guid.NewGuid().ToString(),
        UserName = "123_Name"
      };

      // When
      // Update is requested
      AsyncTestDelegate action = async () => await _apiClient.PutUserAsync(userPatchDto);

      // Then
      // the request should throw exception
      Assert.ThrowsAsync<ItemNotFoundException>(action);
    }

    [Test]
    public void UpdateSiteUser_ShouldFail()
    {
      // Given
      // a random role DTO
      var userPatchDto = new UserEditDto
      {
        Id = _siteAdminUser.Id,
        UserName = "123_Name"
      };

      // When
      // Update is requested
      AsyncTestDelegate action = async () => await _apiClient.PutUserAsync(userPatchDto);

      // Then
      // the request should fail
      Assert.ThrowsAsync<UnauthorizedAccessException>(action);
    }

    [Test]
    public void AssignRole_ShouldFail_GivenTenantUserAndSiteRole()
    {
      // Given
      var patchedUser = Mapper.Map<UserEditDto>(_gryffindorAdminUser);
      // important -- the patch call will otherwise fail
      patchedUser.CurrentPassword = patchedUser.NewPassword = null;
      patchedUser.Roles.Add(Mapper.Map<UserRoleInfo>(_siteRoles.First()));

      // When
      // Update is requested
      AsyncTestDelegate action = async () => await _apiClient.PutUserAsync(patchedUser);

      // Then
      // the request should fail
      Assert.ThrowsAsync<BadRequestException>(action);
    }

    [Test]
    public async Task AssignRole_ShouldSucceed_GivenTenantUserAndTenantRole()
    {
      // Given
      var patchedUser = Mapper.Map<UserEditDto>(_gryffindorAdminUser);
      // important -- the patch call will otherwise fail
      patchedUser.CurrentPassword = patchedUser.NewPassword = null;
      patchedUser.Roles.Clear();
      patchedUser.Roles.AddRange(_mapper.Map<List<UserRoleInfo>>(_gryffindorRoles));

      // When
      // Update is requested
      await _apiClient.PutUserAsync(patchedUser);

      // Then
      // the request should succeed
    }

    [Test]
    public void RemoveRoleFromUser_ShouldSucceed_GivenTenantUserAndTenantRole()
    {
      // Given
      // all possible tenant roles added to the tenant admin user
      var patchedUser = _mapper.Map<UserEditDto>(_gryffindorAdminUser);
      patchedUser.Roles = _mapper.Map<List<UserRoleInfo>>(_gryffindorRoles);
      // important -- the patch call will otherwise fail
      patchedUser.NewPassword = patchedUser.CurrentPassword = null;

      var addAllRolesResponse = _httpClient.PutAsJsonAsync(_url + patchedUser.Id, patchedUser).Result;
      addAllRolesResponse.EnsureSuccessStatusCode();

      // When
      // all roles except for 'tenant admin' are removed
      patchedUser.Roles.Clear();
      patchedUser.Roles.Add(new UserRoleInfo { Id = _gryffindorAdminRoleId });

      var updateUserResponse = _httpClient.PutAsJsonAsync(_url + patchedUser.Id, patchedUser).Result;

      // Then
      // the request should succeed
      Assert.That(
        updateUserResponse.StatusCode,
        Is.EqualTo(HttpStatusCode.NoContent)
        );
    }

    [Test]
    public void AssignRole_ShouldFail_GivenTenantUserAndNonExistingRole()
    {
      // Given
      var patchedUser = Mapper.Map<UserDto>(_gryffindorAdminUser);
      patchedUser.Roles.AddRange(new[] { new UserRoleInfo { Id = Guid.NewGuid().ToString(), Name = "random role name" } });

      // When
      // Update is requested
      var updateUserResponse = _httpClient.PutAsJsonAsync(_url + patchedUser.Id, patchedUser).Result;

      // Then
      // the request should fail
      Assert.That(
        updateUserResponse.StatusCode,
        Is.EqualTo(HttpStatusCode.BadRequest)
        );
    }

    [Test]
    public void Get_ShouldFail_GivenUserDoesntExist()//
    {
      // Given
      // a random role DTO
      var userId = Guid.NewGuid().ToString();

      // When
      var getUserResponse = _httpClient.GetAsync(_url + userId).Result;

      // Then
      // the request should fail
      Assert.That(
        getUserResponse.StatusCode,
        Is.EqualTo(HttpStatusCode.NotFound)
        );
    }

    [Test]
    public void GetAllPermissions_ShouldReturnOnlyTenantPermissions_WithTenantUser()//
    {
      // Given

      // When
      var getPermissionsResponse = _httpClient.GetAsync("/api/account/permissions/").Result;

      // Then
      // the request should fail
      getPermissionsResponse.EnsureSuccessStatusCode();

      var permissions = getPermissionsResponse.Content.ReadAsJsonAsync<IEnumerable<PermissionDto>>().Result;

      Assert.IsFalse(permissions.Any(p => p.Value.Contains(PermScope.GLOBAL)));
      Assert.That(permissions.Count(), Is.EqualTo(17));

      // and the PsaSummary plugin added its permission
      Assert.That(permissions.Any(
        p => p.Value == DefaultTenantRoles.PsaSummary_Department_View_Stub_Permission.Value
        )
      );
    }

    [Test]
    public async Task Create_ShouldSucceed_GivenSameTenant()
    {
      // Given
      // Tenant admin user successfully authenticated
      var user = new UserEditDto
      {
        UserName = "UserToCreateSuccessfully",
        NewPassword = "Qwerty123$",
        FirstName = "First Name",
        LastName = "Last Name",
        Patronymic = "Patronymic",
        Email = "UserToCreateSuccessfully@psk-online.ru",
        IsEnabled = true,
        Roles = _mapper.Map<List<UserRoleInfo>>(_gryffindorRoles)
      };
      var tenantId = _apiClient.GetIdToken().GetTenantIdClaimValue();
      var opsStatBefore = await _apiClient.GetTenantOperationsSummaryAsync(tenantId);
      var usersBefore = opsStatBefore.ServiceActualUsers;

      // When
      // The current user attempts to create a new user
      var createUserResponse = _httpClient.PostAsJsonAsync(_url, user).Result;
      Console.WriteLine("create => " + createUserResponse.Content.ReadAsStringAsync().Result);

      // Then
      // the request should succeed
      Assert.That(
        createUserResponse.StatusCode,
        Is.EqualTo(HttpStatusCode.Created)
        );

      var opsStatAfter = await _apiClient.GetTenantOperationsSummaryAsync(tenantId);
      var usersAfter = opsStatAfter.ServiceActualUsers;

      Assert.That(
        usersBefore + 1,
        Is.EqualTo(usersAfter));
    }

    [Test]
    public async Task Create_ShouldFail_GivenDuplicateUserName()
    {
      // Given
      // Tenant admin user successfully authenticated
      // and created a new user

      var username = nameof(Create_ShouldFail_GivenDuplicateUserName);
      var user = new UserEditDto
      {
        UserName = username,
        NewPassword = "Qwerty123$",
        FirstName = "First Name",
        LastName = "Last Name",
        Patronymic = "Patronymic",
        Email = username + "@psk-online.ru",
        IsEnabled = true,
        Roles = _mapper.Map<List<UserRoleInfo>>(_gryffindorRoles)
      };

      {
        var createUserResponse = await _httpClient.PostAsJsonAsync(_url, user);
        Console.WriteLine("create #1 => " + createUserResponse.Content.ReadAsStringAsync().Result);
        Assert.That(
          createUserResponse.StatusCode,
          Is.EqualTo(HttpStatusCode.Created)
          );
      }
      // When
      // the admin attempts to create a new user with a duplicate name
      user.Email = "1" + user.Email;

      var createUserResponse2 = await _httpClient.PostAsJsonAsync(_url, user);
      Console.WriteLine("create #2 => " + createUserResponse2.Content.ReadAsStringAsync().Result);
      // Then
      // the request should fail
      Assert.That(
        createUserResponse2.StatusCode,
        Is.EqualTo(HttpStatusCode.BadRequest)
        );
    }

    [Test]
    public async Task Create_ShouldFail_GivenDuplicateEmail_SameTenante()
    {
      // Given
      // Tenant admin user successfully authenticated
      // and created a new user

      var username = nameof(Create_ShouldFail_GivenDuplicateEmail_SameTenante);
      var user = new UserEditDto
      {
        UserName = username,
        NewPassword = "Qwerty123$",
        FirstName = "First Name",
        LastName = "Last Name",
        Patronymic = "Patronymic",
        Email = username + "@psk-online.ru",
        IsEnabled = true,
        Roles = _mapper.Map<List<UserRoleInfo>>(_gryffindorRoles)
      };

      {
        var createUserResponse = await _httpClient.PostAsJsonAsync(_url, user);
        Console.WriteLine("create #1 => " + createUserResponse.Content.ReadAsStringAsync().Result);
        Assert.That(
          createUserResponse.StatusCode,
          Is.EqualTo(HttpStatusCode.Created)
          );
      }
      // When
      // the admin attempts to create a new user with a duplicate email
      user.UserName += "_dup_email";
      user.Email = user.Email;

      var createUserResponse2 = await _httpClient.PostAsJsonAsync(_url, user);
      Console.WriteLine("create #2 => " + createUserResponse2.Content.ReadAsStringAsync().Result);
      // Then
      // the request should fail
      Assert.That(
        createUserResponse2.StatusCode,
        Is.EqualTo(HttpStatusCode.BadRequest)
        );
    }

    [Test]
    public async Task Create_ShouldSucceed_SameNameDifferentTenants()
    {
      // Given
      // Gryffindor admin user having just created a user of the specified name
      var sharedUserName = "Albus.Dumbledore";

      {
        await _apiClient.AsGryffindorAdminAsync(_httpClient);
        var user = new UserEditDto
        {
          UserName = sharedUserName,
          NewPassword = "Qwerty123$",
          FirstName = "First Name",
          LastName = "Last Name",
          Patronymic = "Patronymic",
          Email = sharedUserName + "@psk-online.ru",
          IsEnabled = true,
          Roles = _mapper.Map<List<UserRoleInfo>>(_gryffindorRoles)
        };
        var tenantId = _apiClient.GetIdToken().GetTenantIdClaimValue();
        var opsStatBefore = await _apiClient.GetTenantOperationsSummaryAsync(tenantId);
        var usersBefore = opsStatBefore.ServiceActualUsers;

        var createUserResponse = _httpClient.PostAsJsonAsync(_url, user).Result;
        Console.WriteLine("create in Gryffindor => \r\n" + createUserResponse.Content.ReadAsStringAsync().Result);

        Assert.That(
          createUserResponse.StatusCode,
          Is.EqualTo(HttpStatusCode.Created)
          );

        var opsStatAfter = await _apiClient.GetTenantOperationsSummaryAsync(tenantId);
        var usersAfter = opsStatAfter.ServiceActualUsers;

        Assert.That(
          usersBefore + 1,
          Is.EqualTo(usersAfter));

        await _apiClient.SignInWithUserPassword_WithSlug_Async(
          user.UserName, user.NewPassword,
          _httpClient, TestData.TestTenants.GryffindorHouse.Slug);
      }

      // When
      // Slytherin admin atempts to create a user of the same name and email
      {
        await _apiClient.AsSlytherinAdminAsync(_httpClient);

        var user = new UserEditDto
        {
          UserName = sharedUserName,
          NewPassword = "Qwerty123$%^",
          FirstName = "First Name",
          LastName = "Last Name",
          Patronymic = "Patronymic",
          Email = sharedUserName + "@psk-online.ru",
          IsEnabled = true,
          Roles = _mapper.Map<List<UserRoleInfo>>(_slytherinRoles)
        };
        var tenantId = _apiClient.GetIdToken().GetTenantIdClaimValue();
        var opsStatBefore = await _apiClient.GetTenantOperationsSummaryAsync(tenantId);
        var usersBefore = opsStatBefore.ServiceActualUsers;

        var createUserResponse = _httpClient.PostAsJsonAsync(_url, user).Result;
        Console.WriteLine("create in Slytherin => \r\n=> " + createUserResponse.Content.ReadAsStringAsync().Result);

        // Then
        // the request should succeed
        Assert.That(
          createUserResponse.StatusCode,
          Is.EqualTo(HttpStatusCode.Created)
          );

        var opsStatAfter = await _apiClient.GetTenantOperationsSummaryAsync(tenantId);
        var usersAfter = opsStatAfter.ServiceActualUsers;

        Assert.That(
          usersBefore + 1,
          Is.EqualTo(usersAfter));

        await _apiClient.SignInWithUserPassword_WithSlug_Async(
          user.UserName, user.NewPassword,
          _httpClient, TestData.TestTenants.SlytherinHouse.Slug);
      }
    }

    [Test]
    public void Delete_ShouldSucceed_GivenSameTenant()
    {
      // Given
      // Tenant admin user having successfully created a new in the same tenant
      var user = new UserEditDto
      {
        UserName = "UserToDeleteSuccessfully",
        NewPassword = "Qwerty123$",
        FirstName = "First Name",
        LastName = "Last Name",
        Patronymic = "Patronymic",
        Email = "UserToDeleteSuccessfully@psk-online.ru",
        IsEnabled = true,
        Roles = _mapper.Map<List<UserRoleInfo>>(_gryffindorRoles)
      };

      var createUserResponse = _httpClient.PostAsJsonAsync(_url, user).Result;
      Console.WriteLine("create => " + createUserResponse.Content.ReadAsStringAsync().Result);
      createUserResponse.EnsureSuccessStatusCode();
      var createdUserDto = createUserResponse.Content.ReadAsJsonAsync<CreatedWithGuidDto>().Result;

      // When
      // The current user attempts to remove the user that has just been created
      var deleteUserResponse = _httpClient.DeleteAsync(_url + createdUserDto.Id).Result;
      Console.WriteLine("delete => " + deleteUserResponse.Content.ReadAsStringAsync().Result);

      // Then
      // the request should succeed
      Assert.That(
        deleteUserResponse.StatusCode,
        Is.EqualTo(HttpStatusCode.OK)
        );
    }

    [Test]
    public async Task RefreshUserToken_ShouldFail_GivenUserDeleted()
    {
      // Given
      // a new user in the current tenant, tht has successfully authenticated with the server
      var user = new UserEditDto
      {
        UserName = "UserToFailRefresh__AfterRemoval",
        NewPassword = "Qwerty123$",
        FirstName = "First Name",
        LastName = "Last Name",
        Patronymic = "Patronymic",
        Email = "UserToFailRefresh__AfterRemoval@psk-online.ru",
        IsEnabled = true,
        Roles = _mapper.Map<List<UserRoleInfo>>(_gryffindorRoles)
      };

      var createUserResponse = _httpClient.PostAsJsonAsync(_url, user).Result;
      Console.WriteLine("create => " + createUserResponse.Content.ReadAsStringAsync().Result);
      createUserResponse.EnsureSuccessStatusCode();
      var createdUserDto = createUserResponse.Content.ReadAsJsonAsync<CreatedWithGuidDto>().Result;

      var httpClient = _app.CreateClient();
      var userApiClient = new ApiClient(httpClient, _app.GetLogger<ApiClient>());
      await userApiClient.SignInWithUserPassword_WithSlug_Async(
        user.UserName, user.NewPassword,
        httpClient, TestTenants.GryffindorHouse.Slug);

      // When
      // The admin removes the user
      var deleteUserResponse = _httpClient.DeleteAsync(_url + createdUserDto.Id).Result;
      Console.WriteLine("delete => " + deleteUserResponse.Content.ReadAsStringAsync().Result);

      // Then
      // the attemp to refresh the token for the deleted user should fail
      AsyncTestDelegate refreshTokenAction = async () => await userApiClient.RefreshToken();

      Assert.ThrowsAsync<AuthenticationException>(refreshTokenAction);
    }

    [Test]
    public async Task Reset_Password_Start_Should_Succeed_Without_Auth()
    {
      // Given
      // Tenant admin user successfully authenticated
      // and created a new user
      var username = nameof(Reset_Password_Start_Should_Succeed_Without_Auth);
      var user = new UserEditDto
      {
        UserName = username,
        NewPassword = "Qwerty123$",
        FirstName = "First Name",
        LastName = "Last Name",
        Patronymic = "Patronymic",
        Email = username + "@psk-online.ru",
        IsEnabled = true,
        Roles = _mapper.Map<List<UserRoleInfo>>(_gryffindorRoles)
      };

      {
        var createUserResponse = await _httpClient.PostAsJsonAsync(_url, user);
        Console.WriteLine("create #1 => " + createUserResponse.Content.ReadAsStringAsync().Result);
        Assert.That(
          createUserResponse.StatusCode,
          Is.EqualTo(HttpStatusCode.Created)
          );
      }
      // When
      // a client requests a password reset for the user
      var dto = new { UserNameOrEmail = user.Email };

      var pwdResetStart = await _unauthorizedHttpClient.PostAsJsonAsync("/api/account/reset-password-start", dto);
      Console.WriteLine("reset start => " + pwdResetStart.Content.ReadAsStringAsync().Result);

      // Then
      // the request should succeed
      Assert.That(
        pwdResetStart.StatusCode,
        Is.EqualTo(HttpStatusCode.NoContent)
        );
    }

    [Test]
    public async Task Reset_Password_Start_Should_Succeed_For_Non_Existing_User()
    {
      // Given
      // n/a

      // When
      // a client requests a password reset for a non-registered email
      var email = nameof(Reset_Password_Start_Should_Succeed_For_Non_Existing_User) + "@psk-online.ru";
      var dto = new { UserNameOrEmail = email };

      var pwdResetStart = await _unauthorizedHttpClient.PostAsJsonAsync("/api/account/reset-password-start", dto);
      Console.WriteLine("reset start => " + pwdResetStart.Content.ReadAsStringAsync().Result);

      // Then
      // the request should succeed
      Assert.That(
        pwdResetStart.StatusCode,
        Is.EqualTo(HttpStatusCode.NoContent)
        );
    }

    [Test]
    public async Task Reset_Password_Complete_Should_Fail_With_Bad_Token()
    {
      // Given
      // Tenant admin user successfully authenticated
      // and created a new user
      var username = nameof(Reset_Password_Complete_Should_Fail_With_Bad_Token);
      var user = new UserEditDto
      {
        UserName = username,
        NewPassword = "Qwerty123$",
        FirstName = "First Name",
        LastName = "Last Name",
        Patronymic = "Patronymic",
        Email = username + "@psk-online.ru",
        IsEnabled = true,
        Roles = _mapper.Map<List<UserRoleInfo>>(_gryffindorRoles)
      };

      {
        var createUserResponse = await _httpClient.PostAsJsonAsync(_url, user);
        Console.WriteLine("create => " + createUserResponse.Content.ReadAsStringAsync().Result);
        Assert.That(
          createUserResponse.StatusCode,
          Is.EqualTo(HttpStatusCode.Created)
          );
      }
      // When
      // a client triese to complete a password reset for the user
      var dto = new
      {
        UserName = user.Email,
        Token = "bullshit",
        NewPassword = "Qwerty1234567*"
      };

      var pwdResetComplete = await _unauthorizedHttpClient.PostAsJsonAsync("/api/account/reset-password", dto);
      Console.WriteLine("reset complete => " + pwdResetComplete.Content.ReadAsStringAsync().Result);

      // Then
      // the request should fail with 'forbidden' status
      Assert.That(
        pwdResetComplete.StatusCode,
        Is.EqualTo(HttpStatusCode.Forbidden)
        );
    }

  }
}
