namespace PskOnline.Server.Service.ViewModels
{
  using System.ComponentModel.DataAnnotations;

  public class ContactInfoDto
  {
    [Required(ErrorMessage = "Full contact name is required")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Contact email is required")]
    public string Email { get; set; }

    public string OfficePhoneNumber { get; set; }

    public string MobilePhoneNumber { get; set; }

    public string StreetAddress { get; set; }

    public string City { get; set; }

    public string Comment { get; set; }

  }
}
