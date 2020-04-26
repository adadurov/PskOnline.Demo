namespace PskOnline.Client.Api.Inspection
{

  /// <summary>
  /// Type of the inspection
  /// These values are stored in the database, thus, their names and values must not change!
  /// </summary>
  public enum InspectionType
  {
    /// <summary>
    /// unspecified
    /// </summary>
    Undefined = 0,

    /// <summary>
    /// primary, during enrollment
    /// </summary>
    Primary = 1,

    /// <summary>
    /// periodical
    /// </summary>
    Periodic = 2,

    /// <summary>
    /// unplanned
    /// </summary>
    Unplanned = 3,

    /// <summary>
    /// pre-shift assessment according to applicable local regulations
    /// </summary>
    PreShift = 4,

    /// <summary>
    /// during working shift
    /// </summary>
    DuringWorkingShift = 5,

    /// <summary>
    /// after working shift
    /// </summary>
    AfterWorkingShift = 6,

    /// <summary>
    /// during rehabilitation or medical treatment
    /// </summary>
    DuringRehabilitation = 7,

    /// <summary>
    /// after rehabilitation or medical treatment
    /// </summary>
    AfterRehabilitation = 8,

    /// <summary>
    /// the inspection is performed in ourder to train on how to use 
    /// the system &amp; equipment, develop attention, etc.
    /// </summary>
    Training = 9,

    /// <summary>
    /// other
    /// </summary>
    Other  = 10
  } 

}
