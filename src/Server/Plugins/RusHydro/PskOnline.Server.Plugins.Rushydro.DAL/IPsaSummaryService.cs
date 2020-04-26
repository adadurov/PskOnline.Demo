namespace PskOnline.Server.Plugins.RusHydro.DAL
{
  using PskOnline.Server.Plugins.RusHydro.ObjectModel;
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  public interface IPsaSummaryService
  {
    Task<Guid> AddAsync(SummaryDocument value);

    /// <summary>
    /// Throws 'ItemNotFoundException' if no summary for 
    /// the specified inspectionId could be found
    /// </summary>
    /// <param name="inspectionId"></param>
    /// <returns></returns>
    Task<SummaryDocument> GetSummaryForInspectionAsync(Guid inspectionId);

    IEnumerable<SummaryDocument> GetSummariesInDepartmentForPeriod(
      Guid departmentId,
      DateTimeOffset completedAfter,
      DateTimeOffset completedNotLaterThan);

    Task<List<SummaryDocument>> GetSummariesInDepartmentForShift(
      Guid departmentId, long absoluteShiftIndex);
  }
}
