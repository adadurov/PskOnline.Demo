namespace PskOnline.Server.Authority.Interfaces
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using PskOnline.Server.Authority.API.Dto;
  using PskOnline.Server.Authority.ObjectModel;

  public interface IRoleService : API.IRoleService
  {
    Task<Guid> CreateRoleAsync(ApplicationRole role, IEnumerable<string> claims);

    Task<ApplicationRole> GetRoleByIdInternalAsync(Guid roleId);

    Task<ApplicationRole> GetRoleInTenantByNameAsync(string name, Guid tenantId);

    Task<ApplicationRole> GetRoleByIdLoadRelatedAsync(Guid id);

    Task<List<ApplicationRole>> GetRolesLoadRelatedAsync(int page, int pageSize);

    Task UpdateRoleAsync(ApplicationRole role, IEnumerable<string> claims);

    Task DeleteRoleAsync(ApplicationRole role);

    Task DeleteRoleByIdAsync(Guid id);

    Task<bool> TestCanDeleteRoleAsync(Guid id);
  }
}
