namespace PskOnline.Server.Service.ViewModels
{
  using System.ComponentModel.DataAnnotations;

  public class BranchOfficeDto
  {
    public string Id { get; set; }

    [Required(ErrorMessage ="Branch office name is required")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the ID of the time zone
    /// that the branch office is located in.
    /// </summary>
    [Required(ErrorMessage = "Time zone ID is required")]
    public string TimeZoneId { get; set; }
  }
}
