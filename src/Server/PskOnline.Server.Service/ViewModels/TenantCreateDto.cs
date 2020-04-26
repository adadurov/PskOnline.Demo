using System.ComponentModel.DataAnnotations;

namespace PskOnline.Server.Service.ViewModels
{
  public class TenantCreateDto
  {
    [Required]
    public TenantDto TenantDetails { get; set; }

    [Required]
    public TenantCreateAdminDto AdminUserDetails { get; set; }
  }
}
