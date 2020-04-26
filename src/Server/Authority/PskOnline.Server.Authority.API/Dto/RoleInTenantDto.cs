namespace PskOnline.Server.Authority.API.Dto
{
  using System.ComponentModel.DataAnnotations;

  public class RoleInTenantDto
  {
    public string Id { get; set; }

    public string TenantId { get; set; }

    [Required(ErrorMessage = "Role name is required"), StringLength(200, MinimumLength = 2, ErrorMessage = "Role name must be between 2 and 200 characters")]
    public string Name { get; set; }

    public string Description { get; set; }

    public int UsersCount { get; set; }

    public PermissionDto[] Permissions { get; set; }
  }
}
