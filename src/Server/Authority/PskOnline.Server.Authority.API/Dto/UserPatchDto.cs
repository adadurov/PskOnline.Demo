namespace PskOnline.Server.Authority.API.Dto
{
  public class UserPatchDto
  {
    public string LastName { get; set; }

    public string FirstName { get; set; }

    public string Patronymic{ get; set; }

    public string PhoneNumber { get; set; }

    public string Email { get; set; }

    public string BranchOfficeId { get; set; }

    public string DepartmentId { get; set; }

    public string PositionId { get; set; }

    public string WebUiPreferences { get; set; }
  }
}
