namespace PskOnline.Service.Test.Integration.TestData
{
  using System;
  using System.Threading.Tasks;

  using PskOnline.Client.Api;
  using PskOnline.Client.Api.Organization;

  public class TestEmployees
  {
    /// <summary>
    /// creates 2 employees in each of departments (dept_1_1 and dept_1_2)
    /// </summary>
    /// <param name="client"></param>
    /// <param name="tenantInfo"></param>
    /// <returns></returns>
    internal static async Task SeedDefaultEmployees(IApiClient client, TenantContainer tenantInfo)
    {
      await client.SignInWithUserPasswordAsync(tenantInfo.AdminName, tenantInfo.AdminPassword);

      {
        var deptId = tenantInfo.Department_1_1.Department.Id.ToString();
        var branchId = tenantInfo.BranchOffice_One.Id.ToString();

        var employee1 = new EmployeeDto
        {
          Id = Guid.NewGuid().ToString(),
          FirstName = "John",
          LastName = "Doe",
          Gender = PskOnline.Client.Api.Models.Gender.Male,
          BirthDate = DateTime.UtcNow - TimeSpan.FromDays(30 * 365),
          DepartmentId = deptId,
          BranchOfficeId = branchId,
          PositionId = tenantInfo.Position_Default.Id
        };

        var employee2 = new EmployeeDto
        {
          Id = Guid.NewGuid().ToString(),
          FirstName = "Dorothy",
          LastName = "Lovecraft",
          Gender = PskOnline.Client.Api.Models.Gender.Female,
          BirthDate = DateTime.UtcNow - TimeSpan.FromDays(26 * 365),
          DepartmentId = deptId,
          BranchOfficeId = branchId,
          PositionId = tenantInfo.Position_Default.Id
        };

        await Task.WhenAll(client.PostEmployeeAsync(employee1), client.PostEmployeeAsync(employee2));
      }

      {
        var deptId = tenantInfo.Department_1_2.Department.Id.ToString();
        var branchId = tenantInfo.BranchOffice_One.Id.ToString();

        var employee1 = new EmployeeDto
        {
          Id = Guid.NewGuid().ToString(),
          FirstName = "Sherlock",
          LastName = "Holmes",
          Gender = PskOnline.Client.Api.Models.Gender.Male,
          BirthDate = DateTime.UtcNow - TimeSpan.FromDays(34 * 365),
          DepartmentId = deptId,
          BranchOfficeId = branchId,
          PositionId = tenantInfo.Position_Default.Id
        };

        var employee2 = new EmployeeDto
        {
          Id = Guid.NewGuid().ToString(),
          FirstName = "James",
          LastName = "Watson",
          Gender = PskOnline.Client.Api.Models.Gender.Male,
          BirthDate = DateTime.UtcNow - TimeSpan.FromDays(36 * 365),
          DepartmentId = deptId,
          BranchOfficeId = branchId,
          PositionId = tenantInfo.Position_Default.Id
        };

        var employee3 = new EmployeeDto
        {
          Id = Guid.NewGuid().ToString(),
          FirstName = "Martha Louise",
          LastName = "Hudson",
          Gender = PskOnline.Client.Api.Models.Gender.Female,
          BirthDate = DateTime.UtcNow - TimeSpan.FromDays(55 * 365),
          DepartmentId = deptId,
          BranchOfficeId = branchId,
          PositionId = tenantInfo.Position_Default.Id
        };

        await Task.WhenAll(
          client.PostEmployeeAsync(employee1), 
          client.PostEmployeeAsync(employee2),
          client.PostEmployeeAsync(employee3));
      }

    }
  }
}
