namespace PskOnline.Server.ObjectModel
{
  using System;
  using System.ComponentModel.DataAnnotations;

  public class Test : TenantOwnedEntity
  {
    [Required]
    public Guid InspectionId { get; set; }

    [Required]
    public string MethodId { get; set; }

    public string MethodVersion { get; set; }

    public DateTimeOffset StartTime { get; set; }

    public DateTimeOffset FinishTime { get; set; }

    /// <summary>
    /// Gets or sets the ID of the patient (employee) who has completed the inspection
    /// </summary>
    public Guid EmployeeId { get; set; }

    public Guid DepartmentId { get; set; }

    public Guid BranchOfficeId { get; set; }

    /// <summary>
    /// Raw test data recorded by the method client plugin
    /// (in a Web app, a mobile app or in a desktop app)
    /// </summary>
    public string MethodRawDataJson { get; set; }

    /// <summary>
    /// Processed test data generated by the
    /// method data processing plugin (on the server)
    /// </summary>
    public string MethodProcessedDataJson { get; set; }

    public string Comment { get; set; }
  }
}
