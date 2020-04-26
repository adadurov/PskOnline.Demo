namespace PskOnline.Server.Service.ViewModels
{
  using System.ComponentModel.DataAnnotations;

  public class WorkplaceCredentialsRequestDto
  {
    /// <summary>
    /// Space-separated scopes that the client is permitted to use.
    /// </summary>
    [Required]
    public string Scopes { get; set; }
  }
}
