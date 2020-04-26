namespace PskOnline.Server.Authority.API
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  using PskOnline.Server.Authority.API.Dto;

  public interface IAccountService
  {
    Task<(bool success, string[] errors)> DeleteUserAsync(string userId);

    Task<(bool success, string[] errors)> CreateUserAsync(UserDto newUser, IEnumerable<Guid> roleIds, string password);

    Task<long> GetUserCountInTenantAsync(Guid tenantId);
  }
}
