namespace PskOnline.Server.Plugins.RusHydro.Web.Dto
{
  using System;

  /// <summary>
  /// The report about the employees conditions during a working shift
  /// </summary>
  public class PsaReportDto
  {
    public string DepartmentId { get; set; }

    public string DepartmentName { get; set; }

    public string BranchOfficeId { get; set; }

    public string BranchOfficeName { get; set; }

    public DateTime ShiftDate { get; set; }

    public TimeSpan ShiftStartTime { get; set; }

    public int ShiftNumber { get; set; }

    /// <summary>
    /// gets or sets the name of the shift as a Roman number
    /// ('I', 'II', 'III') or as a verbal name ('день', 'ночь')
    /// </summary>
    public string ShiftName { get; set; }

    public PsaSummaryDto[] Summaries { get; set; }
  }
}
