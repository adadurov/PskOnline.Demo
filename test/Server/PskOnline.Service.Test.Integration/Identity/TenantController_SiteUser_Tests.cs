namespace PskOnline.Service.Test.Integration.Identity
{
  using System;
  using System.IdentityModel.Tokens.Jwt;
  using System.Linq;
  using System.Net;
  using System.Net.Http;
  using System.Net.Http.Headers;
  using System.Threading.Tasks;

  using log4net;
  using Microsoft.AspNetCore.Mvc.Testing;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;
  using Newtonsoft.Json.Linq;
  using NUnit.Framework;

  using PskOnline.Client.Api;
  using PskOnline.Client.Api.Authority;
  using PskOnline.Client.Api.Models;
  using PskOnline.Client.Api.Tenant;
  using PskOnline.Components.Log;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Service.Test.Integration.TestData;

  [TestFixture]
  public class TenantController_SiteUser_Tests : IDisposable
  {
    readonly ILog _log = LogManager.GetLogger(typeof(TenantController_SiteUser_Tests));
    const string _url = "/api/Tenant/";
    DefaultWebApplicationFactory _app;
    HttpClient _httpClient;
    IApiClient _apiClient;

    Guid _createdTenantGuid;

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
      _httpClient = _app.CreateClient(/* not using options */);

      _apiClient = new ApiClient(_httpClient, _app.GetLogger<ApiClient>());

      _apiClient.SignInWithUserPasswordAsync(
        TestUsers.DefaultSiteAdminName, TestUsers.DefaultSiteAdminPass).Wait();
    }

    [SetUp]
    public void SetUp()
    {
      InitOnce();

      _apiClient.SignInWithUserPassword_WithSlug_Async(
        TestUsers.DefaultSiteAdminName, TestUsers.DefaultSiteAdminPass,
        _httpClient, "").Wait();
    }

    [TearDown]
    public void TearDown()
    {
    }

    [Test]
    [Order(4)]
    public void Post_ShouldFail_WithoutContact()
    {
      // Given
      // an instance of TenantDto without PrimaryContact value
      var tenant = new TenantCreateDto
      {
        TenantDetails = new TenantDto
        {
          Id = Guid.NewGuid().ToString(),
          Name = "AAA",
          Slug = "aaa",
          ServiceDetails = new ServiceDetailsDto
          {
            ServiceMaxUsers = 10
          }
        },
        AdminUserDetails = new TenantCreateAdminDto
        {
          Email = "test@pskonline.ru",
          UserName = "AAA_Admin",
          NewPassword = "AAA_Admin_pass_2#",
          FirstName = "John",
          Patronymic = "Alfredovich",
          LastName = "Lennon",
          PhoneNumber = "+79991234567"
        }
      };
      // When 
      // a request is made to create a new tenant with the specified attributes
      var response = _httpClient.PostAsJsonAsync(_url, tenant);

      // Then
      // the request fails with BadRequest status code
      Assert.That(response.Result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    [Order(4)]
    public void Post_ShouldFail_WithoutSlug()
    {
      // Given
      // an instance of TenantDto without Slug value
      var tenant = new TenantCreateDto
      {
        TenantDetails = new TenantDto
        {
          Id = Guid.NewGuid().ToString(),
          Name = "AAA",
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
          Email = "test@pskonline.ru",
          UserName = "AAA_Admin",
          NewPassword = "AAA_Admin_pass2#",
          FirstName = "John",
          Patronymic = "Alfredovich",
          LastName = "Lennon",
          PhoneNumber = "+79991234567"
        }
      };
      // When 
      // a request is made to create a new tenant with the specified attributes
      var response = _httpClient.PostAsJsonAsync(_url, tenant);

      // Then
      // the request fails with BadRequest status code
      Assert.That(response.Result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    [Order(4)]
    public void Post_Should_Fail_With_Duplicate_Slug_Nocase()
    {
      // Given
      // a tenant with the specified slug value
      var tenant = new TenantCreateDto
      {
        TenantDetails = new TenantDto
        {
          Id = Guid.NewGuid().ToString(),
          Name = "AAA222",
          Slug = "aaa222",
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
          Email = "test@pskonline.ru",
          UserName = "AAA_Admin",
          NewPassword = "AAA_Admin_pass12#",
          FirstName = "John",
          Patronymic = "Alfredovich",
          LastName = "Lennon",
          PhoneNumber = "+79991234567"
        }
      };
      var response1 = _httpClient.PostAsJsonAsync(_url, tenant);
      response1.Result.EnsureSuccessStatusCode();

      // When 
      // a request is made to create a new tenant with the specified attributes
      tenant.TenantDetails.Name += "__nocase";
      tenant.TenantDetails.Slug = tenant.TenantDetails.Slug.ToUpper();

      var response2 = _httpClient.PostAsJsonAsync(_url, tenant);

      // Then
      // the request fails with BadRequest status code
      Assert.That(response2.Result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    [Order(4)]
    public void Post_ShouldFail_WithBadSlug()
    {
      // Given
      // an instance of TenantDto with a bad Slug value
      var tenant = new TenantCreateDto
      {
        TenantDetails = new TenantDto
        {
          Id = Guid.NewGuid().ToString(),
          Name = "Bad slug",
          Slug = "Bad sluG",
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
          Email = "test@pskonline.ru",
          UserName = "AAA_Admin",
          NewPassword = "AAA_Admin_pass12#",
          FirstName = "John",
          Patronymic = "Alfredovich",
          LastName = "Lennon",
          PhoneNumber = "+79991234567"
        }
      };
      // When 
      // a request is made to create a new tenant with the specified attributes
      var response = _httpClient.PostAsJsonAsync(_url, tenant);

      // Then
      // the request fails with BadRequest status code
      Assert.That(response.Result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    [Order(4)]
    public void Login_Should_Succeed_With_Slug_Case_Mismatch()
    {

      var lowerCaseSlug = "lower-case-slug";
      // Given
      // an instance of TenantDto with a Slug value in upper case
      var tenant = new TenantCreateDto
      {
        TenantDetails = new TenantDto
        {
          Id = Guid.NewGuid().ToString(),
          Name = "Bad slug",
          Slug = lowerCaseSlug,
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
          Email = "test@pskonline.ru",
          UserName = "AAA_Admin",
          NewPassword = "AAA_Admin_pass1",
          FirstName = "John",
          Patronymic = "Alfredovich",
          LastName = "Lennon",
          PhoneNumber = "+79991234567"
        }
      };
      var response = _httpClient.PostAsJsonAsync(_url, tenant);
      response.Result.EnsureSuccessStatusCode();

      // When 
      // an authentication request is made with upper-case slug

      var upperCaseSlug = lowerCaseSlug.ToUpper();

      _apiClient.SignInWithUserPassword_WithSlug_Async(
        tenant.AdminUserDetails.UserName, tenant.AdminUserDetails.NewPassword,
        _httpClient, upperCaseSlug
        ).Wait();

      // Then
      // the request should succeed
    }


    [Test]
    [Order(4)]
    public void Post_ShouldFail_WithoutAdminData()
    {
      // Given
      // an instance of TenantDto without PrimaryContact value
      var tenant = new TenantCreateDto
      {
        TenantDetails = new TenantDto
        {
          Id = Guid.NewGuid().ToString(),
          Name = "AAA-4",
          Slug = "aaa-4",
          PrimaryContact = new ContactInfoDto
          {
            FullName = "Neville Longbottom",
            Email = "longbottom@hogwarts.edu.uk"
          },
          ServiceDetails = new ServiceDetailsDto
          {
            ServiceMaxUsers = 10
          }
        }
      };
      // When 
      // a request is made to create a new tenant with the specified attributes
      var response = _httpClient.PostAsJsonAsync(_url, tenant);

      // Then
      // the request fails with BadRequest status code
      Assert.That(response.Result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    [Order(4)]
    public void Post_ShouldFail_GivenPasswordIsTooSimple()
    {
      var tenant = new TenantCreateDto
      {
        TenantDetails = new TenantDto
        {
          Id = Guid.NewGuid().ToString(),
          Name = "AAA-44",
          Slug = "aaa-4a",
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
          Email = "test@pskonline.ru",
          UserName = "AAA_Admin",
          NewPassword = "AAA_Admin_pass",
          FirstName = "John",
          Patronymic = "Alfredovich",
          LastName = "Lennon",
          PhoneNumber = "+79991234567"
        }
      };
      // When
      var response = _httpClient.PostAsJsonAsync(_url, tenant);

      // Then
      // 1.
      Assert.That(response.Result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

      // 2.
      var errorInfo = response.Result.Content.ReadAsJsonAsync<ApiErrorDto>().Result;
      Assert.That(!string.IsNullOrEmpty(errorInfo.Error));

      // should contain 'assword' (simple match for 'Password' and 'password')
      Assert.That(errorInfo.Error, Contains.Substring("assword"));
    }

    [Test]
    [Order(1)]
    public async Task Post_ShouldSucceed()
    {
      var tenant = new TenantCreateDto
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
          Email = "test@pskonline.ru",
          UserName = "AAA_Admin",
          NewPassword = "AAA_Admin_pass1",
          FirstName = "John",
          Patronymic = "Alfredovich",
          LastName = "Lennon",
          PhoneNumber = "+79991234567"
        }
      };
      // When
      var response = _httpClient.PostAsJsonAsync(_url, tenant);

      // Then
      // 1.
      response.Result.EnsureSuccessStatusCode();
      Assert.That(response.Result.StatusCode, Is.EqualTo(HttpStatusCode.Created));
      // 2.
      var createdEntityInfo = response.Result.Content
        .ReadAsJsonAsync<CreatedWithGuidDto>().Result;

      // remember the ID -- it will be used in next tests

      _createdTenantGuid = createdEntityInfo.Id;
      Console.WriteLine("Tenant created: " + _createdTenantGuid);
      // 3.
      var createdEntityResponse = _httpClient.GetAsync(_url + createdEntityInfo.Id.ToString());
      createdEntityResponse.Result.EnsureSuccessStatusCode();
      // 4. there is a user created in the new tenant
      var allUsersResponse = _httpClient.GetAsync("/api/account/users/").Result;
      allUsersResponse.EnsureSuccessStatusCode();
      var allUsers = allUsersResponse.Content.ReadAsJsonAsync<UserDto[]>().Result;
      Assert.That(allUsers.Any(
        u => 0 == string.Compare(u.TenantId, createdEntityInfo.Id.ToString(), true)));

      // 5. and the user can authenticate with the credentials
      await _apiClient.SignInWithUserPassword_WithSlug_Async(
        tenant.AdminUserDetails.UserName, tenant.AdminUserDetails.NewPassword,
        _httpClient, tenant.TenantDetails.Slug);

      var id_token = _apiClient.GetIdToken();

      // 6. and the id_token contains proper TenantId claim
      Console.WriteLine(JObject.FromObject(id_token).ToString());

      var tenantIdClaimValue = id_token.Claims.First(claim => claim.Type == CustomClaimTypes.TenantId).Value;
      Console.WriteLine("Token contains tenant ID = " + tenantIdClaimValue);

      Assert.That(tenantIdClaimValue, Is.EqualTo(createdEntityInfo.Id.ToString()));

      // 7. and there are only standard roles created for the tenant
      var rolesResponse = _httpClient.GetAsync("/api/account/roles/").Result;
      rolesResponse.EnsureSuccessStatusCode();

      Console.WriteLine(rolesResponse.Content.ReadAsStringAsync().Result);

      var roles = rolesResponse.Content.ReadAsJsonAsync<RoleDto[]>().Result;
      Assert.That(roles.Length, Is.EqualTo(4));
    }

    [Test]
    [Order(7)]
    public void Post_ShouldSucceed_WithSecondTenant()
    {
      var tenant = new TenantCreateDto
      {
        TenantDetails = new TenantDto
        {
          Id = Guid.NewGuid().ToString(),
          Name = "BBB",
          Slug = "bbb",
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
          Email = "test2@pskonline.ru",
          UserName = "BBB_Admin",
          NewPassword = "BBB_Admin_pass2",
          FirstName = "John",
          Patronymic = "Alfredovich",
          LastName = "Lennon",
          PhoneNumber = "+79992345678"
        }
      };
      var response = _httpClient.PostAsJsonAsync(_url, tenant);
      response.Result.EnsureSuccessStatusCode();
      Assert.That(response.Result.StatusCode, Is.EqualTo(HttpStatusCode.Created));
      var createdEntityInfo = response.Result.Content.ReadAsJsonAsync<PskOnline.Client.Api.Models.CreatedWithGuidDto>().Result;

      var createdEntityResponse = _httpClient.GetAsync(_url + createdEntityInfo.Id.ToString());
      createdEntityResponse.Result.EnsureSuccessStatusCode();
    }

    [Test]
    [Order(8)]
    public void Post_ShouldFail_WithDuplicateTenantName()
    {
      // Given
      var tenant = new TenantCreateDto
      {
        TenantDetails = new TenantDto {
          Id = Guid.NewGuid().ToString(),
          Name = "post_shouldfail_withduplicatetenantname",
          Slug = "post_shouldfail_withduplicatetenantname",
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
        AdminUserDetails = new TenantCreateAdminDto {
          Email = "test@pskonline.ru",
          UserName = "AAA_Admin",
          NewPassword = "AAA_Admin_pass2#",
          FirstName = "John",
          Patronymic = "Alfredovich",
          LastName = "Lennon",
          PhoneNumber = "+79991234567"
        }
      };
      var response = _httpClient.PostAsJsonAsync(_url, tenant);
      Console.WriteLine(response.Result.Content.ReadAsStringAsync().Result);
      response.Result.EnsureSuccessStatusCode();

      // When
      tenant.TenantDetails.Id = Guid.NewGuid().ToString();
      tenant.TenantDetails.Slug += "_duplicatename";
      var response2 = _httpClient.PostAsJsonAsync(_url, tenant);

      Console.WriteLine(response2.Result.Content.ReadAsStringAsync().Result);

      // Then
      Assert.That(response2.Result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    [Order(9)]
    public void Post_ShouldSucceed_WithDuplicateAdminUsername()
    {
      // Given
      var tenant = new TenantCreateDto
      {
        TenantDetails = new TenantDto
        {
          Id = Guid.NewGuid().ToString(),
          Name = "CCC123456789",
          Slug = "abcdefghijklmnopqrstuvwxyz-0123456789_",
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
          Email = "test_CCC@pskonline.ru",
          UserName = "admin",
          NewPassword = "CCC_Admin_pass_7$",
          FirstName = "John",
          Patronymic = "Alfredovich",
          LastName = "Lennon",
          PhoneNumber = "+79992345678"
        }
      };

      var response1 = _httpClient.PostAsJsonAsync(_url, tenant);
      Console.WriteLine(response1.Result.Content.ReadAsStringAsync().Result);
      response1.Result.EnsureSuccessStatusCode();

      // When
      tenant.TenantDetails.Name += "___";
      tenant.TenantDetails.Slug += "___";
      tenant.TenantDetails.Id = Guid.NewGuid().ToString();
      var response = _httpClient.PostAsJsonAsync(_url, tenant);

      Console.WriteLine(response.Result.Content.ReadAsStringAsync().Result);

      // Then
      response.Result.EnsureSuccessStatusCode();
    }

    [Test]
    [Order(10)]
    public void Get_ShouldSucceed_GivenTenantExists()
    {
      var response = _httpClient.GetAsync(_url + _createdTenantGuid.ToString());
      Assert.IsTrue(response.Result.IsSuccessStatusCode);
      var tenant = response.Result.Content.ReadAsJsonAsync<TenantDto>().Result;
      Assert.AreEqual(_createdTenantGuid.ToString(), tenant.Id);
    }

    [Test]
    [Order(11)]
    public void GetAll_ShouldSucceed()
    {
      var response = _httpClient.GetAsync(_url);
      Assert.IsTrue(response.Result.IsSuccessStatusCode);
      var tenants = response.Result.Content.ReadAsJsonAsync<TenantDto[]>().Result;
      Assert.IsTrue(tenants.Any(c => c.Id == _createdTenantGuid.ToString()));
      Assert.That(tenants.Length, Is.GreaterThan(1));
    }

    [Test]
    [Order(2)]
    public void Update_ShouldSucceed_GivenTenantExists()
    {
      var response = _httpClient.GetAsync(_url + _createdTenantGuid.ToString());
      Assert.IsTrue(response.Result.IsSuccessStatusCode);

      var tenant = response.Result.Content.ReadAsJsonAsync<TenantDto>().Result;
      Assert.AreEqual(_createdTenantGuid.ToString(), tenant.Id);
      tenant.Name = tenant.Name + "_modified";

      var putResponse = _httpClient.PutAsJsonAsync(_url + tenant.Id, tenant).Result;
      putResponse.EnsureSuccessStatusCode();

      var checkResponse = _httpClient.GetAsync(_url + _createdTenantGuid.ToString());
      Assert.IsTrue(checkResponse.Result.IsSuccessStatusCode);
      var checkTenant = checkResponse.Result.Content.ReadAsJsonAsync<TenantDto>().Result;
      Assert.That(checkTenant.Name, Is.EqualTo("AAA_modified"));
    }

    [Test]
    [Order(13)]
    public void Update_ShouldFail_GivenTenantDoesntExist()
    {
      // Given
      // a random TenantDto
      var tenantDto = new TenantDto()
      {
        Id = Guid.NewGuid().ToString(),
        Name = "123 Name",
        Slug = "zbcdef_123",
        ServiceDetails = new ServiceDetailsDto
        {
          ServiceExpireDate = DateTime.Now + TimeSpan.FromDays(1000),
          ServiceMaxStorageMegabytes = 10000,
          ServiceMaxUsers = 1000
        },
        PrimaryContact = new ContactInfoDto
        {
          FullName = "John Doe",
          Email = "alexei.adadurov@mail.ru",
          MobilePhoneNumber = "+79991234567"
        }
      };

      // When
      var updateTenantResponse = _httpClient.PutAsJsonAsync(_url + tenantDto.Id, tenantDto).Result;

      // Then
      Assert.That(updateTenantResponse.StatusCode == HttpStatusCode.NotFound);
    }


    [Test]
    [Order(16)]
    public async Task Delete_ShouldSucceed_GivenTenantExists()
    {
      // Given
      // an infrastructure with tenants

      // When
      // a specific tenant is deleted
      var response = await _httpClient.DeleteAsync(_url + _createdTenantGuid.ToString());

      // Then
      // 1. the DELETE request succeeds
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

      // 2. a following requests to get the tenant is completed with NotFound status code
      var response2 = await _httpClient.GetAsync(_url + _createdTenantGuid.ToString());
      Assert.That(response2.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

      // 3. The following request to list all the remaining tenants is completed successfully
      var response3 = await _httpClient.GetAsync(_url);
      response3.EnsureSuccessStatusCode();
      Assert.That(
        response3.StatusCode,
        Is.EqualTo(HttpStatusCode.OK)
        );

      var tenants = await response3.Content.ReadAsJsonAsync<TenantDto[]>();
      // ... and the returned collection doesn't contain the deleted tenant
      Assert.IsFalse(tenants.Any(c => c.Id == _createdTenantGuid.ToString()));
    }

    [Test]
    [Order(17)]
    public void Delete_ShouldSucceed_GivenTenantDoesntExist()
    {
      // Given
      // an infrastructure with a tenant deleted
      var response1 = _httpClient.DeleteAsync(_url + Guid.NewGuid().ToString());
      Assert.IsTrue(response1.Result.IsSuccessStatusCode);

      // When
      // a specific tenant is deleted
      var response2 = _httpClient.DeleteAsync(_url + Guid.NewGuid().ToString());

      // Then
      // 1. the DELETE request succeeds
      Assert.IsTrue(response2.Result.IsSuccessStatusCode);
      Assert.That(
        response2.Result.StatusCode,
        Is.EqualTo(HttpStatusCode.NoContent)
        );

    }

    [Test]
    [Order(20)]
    public void Post_And_Sign_In_As_Tenant_Admin_Should_Succeed()
    {
      var tenant = new TenantCreateDto
      {
        TenantDetails = new TenantDto
        {
          Id = Guid.NewGuid().ToString(),
          Name = "ZZZ",
          Slug = "zzz",
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
          Email = "zzz@pskonline.ru",
          UserName = "ZZZ_Admin",
          NewPassword = "ZZZ_Admin_pass1",
          FirstName = "John",
          Patronymic = "Alfredovich",
          LastName = "Lennon",
          PhoneNumber = "+79993456789"
        }
      };
      // When
      var response = _httpClient.PostAsJsonAsync(_url, tenant);

      // Then
      // 1.
      response.Result.EnsureSuccessStatusCode();
      Assert.That(response.Result.StatusCode, Is.EqualTo(HttpStatusCode.Created));
      // 2.
      var createdEntityInfo = response.Result.Content
        .ReadAsJsonAsync<CreatedWithGuidDto>().Result;
      // 3.
      var createdEntityResponse = _httpClient.GetAsync(_url + createdEntityInfo.Id.ToString());
      createdEntityResponse.Result.EnsureSuccessStatusCode();
      // 4.
      _apiClient.SignInWithUserPassword_WithSlug_Async(
        tenant.AdminUserDetails.UserName, tenant.AdminUserDetails.NewPassword,
        _httpClient, tenant.TenantDetails.Slug).Wait();
    }


  }
}