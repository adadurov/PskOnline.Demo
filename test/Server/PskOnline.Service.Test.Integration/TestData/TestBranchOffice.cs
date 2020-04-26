namespace PskOnline.Service.Test.Integration.TestData
{
  using System;
  using System.Net.Http;
  using System.Threading.Tasks;

  using PskOnline.Client.Api;
  using PskOnline.Client.Api.Organization;

  public class TestBranchOffice
  {
    internal static async Task SeedDefaultBranchOffice(IApiClient client, HttpClient httpClient, TenantContainer tenantInformation)
    {
      var defaultBranchOffice = new BranchOfficeDto
      {
        Id = Guid.NewGuid().ToString(),
        Name = "Head Office",
        TimeZoneId = "MSK+5" // "Irkutsk time"
      };

      await client.SignInWithUserPassword_WithSlug_Async(
        tenantInformation.AdminName, tenantInformation.AdminPassword,
        httpClient, tenantInformation.Tenant.Slug);
      var result = await client.PostBranchOfficeAsync(defaultBranchOffice);

      tenantInformation.BranchOffice_One = defaultBranchOffice;
    }
  }
}
