namespace PskOnline.Server.Plugins.RusHydro.Web.Dto
{
  using System;

  public class PsaSummaryDto
  {
    public Guid Id { get; set; }

    public string InspectionId { get; set; }

    public string DepartmentId { get; set; }

    public string BranchOfficeId { get; set; }

    public RushydroEmployeeDto Employee { get; set; }

    public DateTimeOffset CompletionTime { get; set; }

    /// <summary>
    /// The date when the working shift started
    /// </summary>
    public DateTime WorkingShiftDate { get; set; }

    /// <summary>
    /// 1 => day shift
    /// 2 => night shift
    /// </summary>
    public int WorkingShiftNumber { get; set; }

    public string HostName { get; set; }

    public HrvPreShiftConclusionDto HrvConclusion { get; set; }

    public SvmrPreShiftConclusionDto SvmrConclusion { get; set; }

    public PsaFinalConclusionDto FinalConclusion { get; set; }

    public string ToolVersion { get; set; }
  }
}
