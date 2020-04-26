namespace PskOnline.Server.Authority.API.Dto
{
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;

  public class UserDto
  {
    public string Id { get; set; }

    [Required(ErrorMessage = "Username is required"),
     StringLength(200, MinimumLength = 2, ErrorMessage = "Username must be between 2 and 200 characters")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Last name is required")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "First name is required")]
    public string FirstName { get; set; }

    public string Patronymic { get; set; }

    [Required(ErrorMessage = "Email is required"),
     StringLength(200, ErrorMessage = "Email must be at most 200 characters"),
     EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; }

    public string TenantId { get; set; }

    public string BranchOfficeId { get; set; }

    public string DepartmentId { get; set; }

    public string PhoneNumber { get; set; }

    public string EmployeeId { get; set; }

    public string WebUiPreferences { get; set; }

    public bool IsEnabled { get; set; }

    public bool IsLockedOut { get; set; }

    /// <summary>
    /// this flag is used to identify special accounts;
    /// modifying this flag is not recommended
    /// </summary>
    public bool IsDepartmentSpecialUser { get; set; }

    public List<UserRoleInfo> Roles { get; set; }
  }
}
