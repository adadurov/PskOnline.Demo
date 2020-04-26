namespace PskOnline.Server.DAL.Multitenancy
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Shared.EFCore;

  public class TenantRepository : Repository<Tenant, ApplicationDbContext>
  {
    public TenantRepository(ApplicationDbContext context) : base(context)
    { }

    public IEnumerable<Tenant> GetTopActiveTenants(int count)
    {
      throw new NotImplementedException();
    }

    public IEnumerable<Tenant> GetAllTenantsData()
    {
      return _appContext.Tenants
          .OrderBy(c => c.Name)
          .ToList();
    }

    private ApplicationDbContext _appContext => (ApplicationDbContext)_context;
  }
}
