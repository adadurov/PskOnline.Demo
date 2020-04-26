namespace PskOnline.Server.Authority.API.Dto
{
  using System.ComponentModel.DataAnnotations;

  public class UserEditDto : UserDto
  {
    public string CurrentPassword { get; set; }

    [MinLength(8, ErrorMessage = "New Password must be at least 8 characters")]
    public string NewPassword { get; set; }

    new private bool IsLockedOut { get; } //Hide base member
  }
}
