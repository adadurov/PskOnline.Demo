namespace PskOnline.Client.Api.Authority
{

  public class UserEditDto : UserDto
  {
    public string CurrentPassword { get; set; }

    public string NewPassword { get; set; }

    new private bool IsLockedOut { get; }
  }
}
