namespace PskOnline.Service.Test.Integration.TestData
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Net.Http;
  using System.Threading.Tasks;
  using AutoMapper;
  using PskOnline.Client.Api;
  using PskOnline.Client.Api.Authority;
  using PskOnline.Client.Api.Tenant;

  public static class TestTenants
  {
    private static Guid GryffindorGuid = Guid.NewGuid();
    private static Guid SlytherinGuid = Guid.NewGuid();

    public static async Task<TenantContainer> CreateTenantWithRolesAndUsers(IApiClient client, HttpClient httpClient, TenantDto t)
    {
      IMapper mapper = new AutoMapperConfig().CreateMapper();
      // pretend we are the site user...

      var tenantSpecification = new TenantContainer(t);

      var newT = new TenantCreateDto
      {
        TenantDetails = new TenantDto
        {
          Name = t.Name,
          Slug = t.Slug,
          Comment = t.Comment,
          PrimaryContact = new ContactInfoDto
          {
            FullName = "John Smith",
            MobilePhoneNumber = "+79998887766",
            StreetAddress = "931 Baker street",
            Email = "someone@psk-online.ru"
          },
          ServiceDetails = new ServiceDetailsDto
          {
            ServiceMaxEmployees = 1000,
            ServiceMaxUsers = 10,
            ServiceExpireDate = DateTime.Now + TimeSpan.FromDays(730),
            ServiceMaxStorageMegabytes = 40960
          }
        },
        AdminUserDetails = new TenantCreateAdminDto
        {
          UserName = tenantSpecification.AdminName,
          FirstName = "John",
          LastName = "Doe",
          Email = tenantSpecification.AdminName + "@somedomain.com",
          NewPassword = tenantSpecification.AdminPassword
        }

      };

      var newTenantId = client.PostTenantAsync(newT).Result;
      tenantSpecification.Tenant.Id = newTenantId.ToString();
      tenantSpecification.TenantId = newTenantId;

      // now read any details of the Tenant
      await client.SignInWithUserPassword_WithSlug_Async(newT.AdminUserDetails.UserName, newT.AdminUserDetails.NewPassword, 
        httpClient, newT.TenantDetails.Slug);

      var tenantRoles = await client.GetRolesAsync();

      tenantSpecification.TenantAdminRole = tenantRoles.First(r => r.Name.Contains("admin", StringComparison.InvariantCultureIgnoreCase));
      tenantSpecification.TenantAdminRoleId = Guid.Parse(tenantSpecification.TenantAdminRole.Id);

      var tenantUsers = await client.GetUsersAsync();

      var tenantAdminUser = tenantUsers.First(
        u => !string.IsNullOrEmpty(u.UserName) && u.UserName.Contains("admin", StringComparison.InvariantCultureIgnoreCase));

      tenantSpecification.AdminUser = mapper.Map<UserEditDto>(tenantAdminUser);
      tenantSpecification.AdminUser.CurrentPassword = newT.AdminUserDetails.NewPassword;
      newT.AdminUserDetails.NewPassword = null;

      return tenantSpecification;
    }

    public static TenantDto GryffindorHouse
    {
      get => new TenantDto
      {
        Id = GryffindorGuid.ToString(),
        Name = "Gryffindor House (Hogwarts)",
        Slug = "gryffindor",
        PrimaryContact = new ContactInfoDto
        {
          FullName = "Nevile Longbottom"
        },
        ServiceDetails = new ServiceDetailsDto
        {
          ServiceMaxUsers = 10
        }
      };
    }

    public static TenantDto SlytherinHouse
    {
      get => new TenantDto
      {
        Id = SlytherinGuid.ToString(),
        Name = "Slytherin House (Hogwarts)",
        Slug = "slytherin",
        PrimaryContact = new ContactInfoDto
        {
          FullName = "Severus Snape"
        },
        ServiceDetails = new ServiceDetailsDto
        {
          ServiceMaxUsers = 10
        }
      };
    }
  }
}
