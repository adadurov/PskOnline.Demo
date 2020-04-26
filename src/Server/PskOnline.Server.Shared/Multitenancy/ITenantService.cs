namespace PskOnline.Server.Shared.Multitenancy
{
  using System;
  using System.Threading.Tasks;

  public interface ITenantService
  {
    Task<Guid> GetTenantIdBySlug(string slug);
  }
}
