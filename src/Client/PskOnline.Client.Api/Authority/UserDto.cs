namespace PskOnline.Client.Api.Authority
{
  using System.Collections.Generic;

  public class UserDto
  {
    public string Id { get; set; }

    public string UserName { get; set; }

    public string LastName { get; set; }

    public string FirstName { get; set; }

    public string Patronymic { get; set; }

    public string Email { get; set; }

    public string TenantId { get; set; }

    public string BranchOfficeId { get; set; }

    public string DepartmentId { get; set; }

    public string PhoneNumber { get; set; }

    public string EmployeeId { get; set; }

    public string WebUiPreferences { get; set; }

    public bool IsEnabled { get; set; }

    public bool IsLockedOut { get; set; }

    public List<UserRoleInfo> Roles { get; set; }
  }
}
