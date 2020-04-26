namespace PskOnline.Server.ObjectModel
{
  using System;
  using System.Collections.Generic;

  /// <summary>
  /// A record about inspection that has been started,
  /// and (at some point) finished.
  /// An inspection may consist of a single or multiple sequentially
  /// completed 'tests'. In case an inspection contains multiple tests
  /// and a non-empty 'MethodSetId', the tests may be interpreted together
  /// in order to generate a 'MethodSetSummary' (think 'test session'
  /// during pre-shift assessment)
  /// </summary>
  public class Inspection : TenantOwnedEntity
  {
    public DateTimeOffset StartTime { get; set; }

    public DateTimeOffset? FinishTime { get; set; }

    public string MethodSetId { get; set; }

    public string MethodSetVersion { get; set; }

    /// <summary>
    /// Gets or sets a name of the machine
    /// where the inspection took place (a hostname)
    /// </summary>
    public string MachineName { get; set; }

    public InspectionType InspectionType { get; set; } = InspectionType.Undefined;

    public InspectionPlace InspectionPlace { get; set; } = InspectionPlace.Undefined;

    /// <summary>
    /// Gets or sets the ID of the department where the inspection took place
    /// </summary>
    public Guid DepartmentId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the branch office where the inspection took place
    /// </summary>
    public Guid BranchOfficeId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the patient (employee) who has completed the inspection
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Gets or sets the collection of tests completed during the inspection session
    /// </summary>
    public virtual ICollection<Test> Tests { get; set; }

    public bool IsFinished => this.FinishTime.HasValue && this.FinishTime != DateTimeOffset.MinValue;
  }
}
