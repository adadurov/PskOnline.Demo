namespace PskOnline.Server.Service.ViewModels
{
  using System;
  using PskOnline.Server.ObjectModel;

  public class NewInspectionDto
  {
    public DateTimeOffset StartTime { get; set; }

    public string MethodSetId { get; set; }

    public string MethodSetVersion { get; set; }

    /// <summary>
    /// Gets or sets a name of the machine
    /// where the inspection took place (a hostname)
    /// </summary>
    public string MachineName { get; set; }

    public InspectionType InspectionType { get; set; }

    public InspectionPlace InspectionPlace { get; set; }

    /// <summary>
    /// Gets or sets the ID of the patient (employee) who has completed the inspection
    /// </summary>
    public Guid ApplicationUserId { get; set; }
  }
}
