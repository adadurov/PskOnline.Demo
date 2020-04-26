namespace PskOnline.Server.Plugins.RusHydro.DAL
{
  using System;
  using System.Threading.Tasks;
  using PskOnline.Server.Plugins.RusHydro.ObjectModel;

  public interface IDepartmentPsaReportService
  {
    Task<PsaReportDocument> GetCurrentShiftReportAsync(Guid departmentGuid);

    /// <summary>
    /// Add the Employee's summary to the corresponding department report.
    /// Following requests for GetCurrentShiftReportAsync() will include the added summary,
    /// if the Employee's summary belongs to the then-current shift at the moment
    /// GetCurrentShiftReportAsync is called
    /// </summary>
    /// <param name="summary"></param>
    Task AddSummaryAsync(SummaryDocument summary);
  }
}
