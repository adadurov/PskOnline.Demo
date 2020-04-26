namespace PskOnline.Server.Plugins.RusHydro.ObjectModel
{
  using PskOnline.Server.Shared.ObjectModel;
  using System;

  public class SummaryDocument : IGuidIdentity
  {
    public Guid Id { get; set; }

    public Guid InspectionId { get; set; }

    public Guid DepartmentId { get; set; }

    public Guid BranchOfficeId { get; set; }

    public Employee Employee { get; set; }

    public DateTimeOffset CompletionTime { get; set; }

    /// <summary>
    /// The date when the workins shift started
    /// </summary>
    public DateTime WorkingShiftDate { get; set; }

    /// <summary>
    /// 1 => day shift
    /// 2 => night shift
    /// </summary>
    public int WorkingShiftNumber { get; set; }

    public string HostName { get; set; }

    public PreShiftHrvConclusion HrvConclusion { get; set; }

    public PreShiftSvmrConclusion SvmrConclusion { get; set; }

    public PreShiftFinalConclusion FinalConclusion { get; set; }

    public DateTimeOffset UpdateDate { get; set; }

    public string ToolVersion { get; set; }
  }

}
