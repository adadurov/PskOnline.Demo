namespace PskOnline.Client.Api.Authority
{
  public class RoleDto
  {
    public string Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public int UsersCount { get; set; }

    public PermissionDto[] Permissions { get; set; }
  }
}
