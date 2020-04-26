namespace PskOnline.Server.Plugins.RusHydro.ObjectModel
{
  using System;
  using System.Collections.Generic;

  /// <summary>
  /// </summary>
  public class PsaReportDocument
  {
    public Guid DepartmentId { get; set; }

    public string DepartmentName { get; set; }

    public Guid BranchOfficeId { get; set; }

    public string BranchOfficeName { get; set; }

    public DateTime ShiftDate { get; set; }

    public TimeSpan ShiftStartTime { get; set; }

    public int ShiftNumber { get; set; }

    /// <summary>
    /// gets or sets the number ('I', 'II', 'III') 
    /// or name ('день', 'ночь') of the shift
    /// </summary>
    public string ShiftName { get; set; }

    public IList<SummaryDocument> Summaries { get; set; }
  }
}
