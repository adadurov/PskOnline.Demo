namespace PskOnline.Server.Authority.API.Dto
{
  using System.ComponentModel.DataAnnotations;

  public class ResetPasswordStartDto
  {
    [Required]
    public string UserNameOrEmail { get; set; }
  }
}
