namespace PskOnline.Server.Plugins.RusHydro.DAL
{
  using PskOnline.Server.Shared.Repository;
  using System;
  using System.Threading.Tasks;

  public interface IPsaReportRepository : IGuidKeyedRepository<Report>
  {
    Task<Report> GetReportAsync(Guid tenantId, Guid departmentId, long shiftAbsoluteIndex);
  }
}
