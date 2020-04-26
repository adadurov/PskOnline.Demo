namespace PskOnline.Server.DAL.Multitenancy
{
  using System;
  using PskOnline.Server.Authority.API.Dto;

  public class TenantCreationResult
  {
    public Guid TenantId { get; set; }

    public RoleDto AdminRole { get; set; }

    private UserDto AdminUser { get; set; }
  }
}
