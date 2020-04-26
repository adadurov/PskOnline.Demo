namespace PskOnline.Server.Plugins.RusHydro.DAL
{
  using PskOnline.Server.Shared.Repository;
  using System;
  using System.Collections.Generic;

  public interface IPsaSummaryRepository : IGuidKeyedRepository<Summary>
  {
    IEnumerable<Summary> GetSummariesInDepartmentForPeriod(
          Guid tenantId,
          Guid departmentId,
          DateTimeOffset completedAfter,
          DateTimeOffset completedNotLater);
  }
}
