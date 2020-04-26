namespace PskOnline.Server.DAL.Multitenancy
{
  using System.Threading.Tasks;

  using PskOnline.Server.Authority.API.Dto;
  using PskOnline.Server.ObjectModel;

  public interface ITenantCreator
  {
    /// <summary>
    /// Creates tenant, standard roles for the tenant
    /// (including at least 'Tenant Admin' role)
    /// and a new user (with the specified password).
    /// The new user is assigned the 'Tenant Admin' role.
    /// </summary>
    /// <param name="newTenant"></param>
    /// <param name="newTenantAdmin"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    Task<TenantCreationResult> CreateNewTenant(
      Tenant newTenant, 
      UserDto newTenantAdmin,
      string password);
  }
}
