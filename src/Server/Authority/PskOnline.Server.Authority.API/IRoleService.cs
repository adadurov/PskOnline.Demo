namespace PskOnline.Server.Authority.API
{
  using System;
  using System.Threading.Tasks;

  using PskOnline.Server.Authority.API.Dto;

  public interface IRoleService
  {
    /// <summary>
    /// only permission values are used (description and everything else is ignored)
    /// </summary>
    /// <param name="roleDef"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    Task<Guid> CreateRoleAsync(RoleDto roleDef, Guid tenantId);

    Task<RoleDto> GetRoleByIdAsync(Guid roleId);

    Task DeleteAllTenantRoles(Guid tenantId);
  }
}
