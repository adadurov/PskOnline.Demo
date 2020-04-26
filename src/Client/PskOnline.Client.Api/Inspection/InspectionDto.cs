namespace PskOnline.Client.Api.Inspection
{
  using System;

  public class InspectionDto : InspectionPostDto
  {
    public DateTimeOffset? FinishTime { get; set; }

    /// <summary>
    /// Gets or sets the ID of the department where the inspection took place
    /// </summary>
    public string DepartmentId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the branch office where the inspection took place
    /// </summary>
    public string BranchOfficeId { get; set; }
  }
}
