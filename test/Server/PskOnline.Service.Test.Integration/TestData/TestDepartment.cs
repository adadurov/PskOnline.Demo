namespace PskOnline.Service.Test.Integration.TestData
{
  using System;
  using System.Threading.Tasks;

  using PskOnline.Client.Api;
  using PskOnline.Client.Api.Organization;

  public class TestDepartment
  {
    internal static async Task SeedDefaultDepartments(IApiClient client, TenantContainer tenantInfo)
    {
      await client.SignInWithUserPasswordAsync(tenantInfo.AdminName, tenantInfo.AdminPassword);

      {
        // create department 1.1
        var defaultDepartment11 = new DepartmentDto
        {
          Id = Guid.NewGuid().ToString(),
          Name = "Head Office Operations",
          BranchOfficeId = tenantInfo.BranchOffice_One.Id.ToString(),
        };

        await client.PostDepartmentAsync(defaultDepartment11);
        tenantInfo.Department_1_1.Department = defaultDepartment11;
      }
      {
        // create department 1.2
        var defaultDepartment12 = new DepartmentDto
        {
          Id = Guid.NewGuid().ToString(),
          Name = "Head Office Sales",
          BranchOfficeId = tenantInfo.BranchOffice_One.Id.ToString()
        };

        var createdDto12 = await client.PostDepartmentAsync(defaultDepartment12);

        tenantInfo.Department_1_2.Department = defaultDepartment12;
      }
    }
  }
}
