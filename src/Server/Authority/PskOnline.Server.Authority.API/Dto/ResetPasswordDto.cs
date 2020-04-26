namespace PskOnline.Server.Authority.API.Dto
{
  using System.ComponentModel.DataAnnotations;

  public class ResetPasswordDto
  {
    [Required]
    public string UserName { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }

    [Required]
    public string Token { get; set; }
  }
}
