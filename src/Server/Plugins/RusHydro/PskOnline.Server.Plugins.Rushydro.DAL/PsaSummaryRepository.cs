namespace PskOnline.Server.Plugins.RusHydro.DAL
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using PskOnline.Server.Shared.EFCore;

  public class PsaSummaryRepository : Repository<Summary, RusHydroPsaDbContext>, IPsaSummaryRepository
  {
    public PsaSummaryRepository(RusHydroPsaDbContext context) : base(context)
    {
    }

    public IEnumerable<Summary> GetSummariesInDepartmentForPeriod(
      Guid tenantId,
      Guid departmentId,
      DateTimeOffset completedAfter,
      DateTimeOffset completedNotLater)
    {
      return base.Query().Where(
        s => s.TenantId == tenantId &&
             s.DepartmentId == departmentId && 
             s.CompletionTime > completedAfter  && 
             s.CompletionTime <= completedNotLater);
    }

  }
}
