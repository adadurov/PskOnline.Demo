namespace PskOnline.Server.Service.ViewModels
{
  using System;
  using System.ComponentModel.DataAnnotations;
  using PskOnline.Server.ObjectModel;

  public class InspectionPostDto
  {
    public string Id { get; set; }

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
    /// Gets or sets the ID of the employee who completed the inspection
    /// </summary>
    [Required(ErrorMessage = "EmployeeId is required")]
    public string EmployeeId { get; set; }

  }
}
