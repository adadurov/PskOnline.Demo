namespace PskOnline.Server.Plugins.RusHydro.DAL
{
  using System;

  public static class DepartmentShiftReportIdBuilder
  {
    /// <summary>
    /// build the identifier of the report for the department shift,
    /// that is the current for the point in time specified by the department's local time
    /// </summary>
    /// <param name="departmentId"></param>
    /// <param name="localTime"></param>
    /// <returns></returns>
    public static string BuildReportId(Guid departmentId, DateTime localTime)
    {
      return BuildReportId(departmentId, WorkingShiftAbsoluteIndex.GetAbsoluteIndex(localTime));
    }

    /// <summary>
    /// build the identifier of the report for the department shift,
    /// that is the current for the point in time specified by the department's local time
    /// </summary>
    /// <param name="departmentId"></param>
    /// <param name="absoluteShiftIndex"></param>
    /// <returns></returns>
    public static string BuildReportId(Guid departmentId, long absoluteShiftIndex)
    {
      return departmentId.ToString("N") + absoluteShiftIndex.ToString();
    }

  }
}
