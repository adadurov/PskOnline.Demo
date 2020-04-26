namespace PskOnline.Server.ObjectModel
{
  /// <summary>
  /// Place of inspection
  /// These values are stored in the database, thus, their names and values must not change!!!
  /// </summary>
  public enum InspectionPlace
  {
    /// <summary>
    /// unspecified
    /// </summary>
    Undefined = 0,

    /// <summary>
    /// in non-working conditions
    /// </summary>
    InNonWorkingConditions = 1,

    /// <summary>
    /// on workplace
    /// </summary>
    OnWorkplace = 2,

    /// <summary>
    /// in rehabilitation center
    /// </summary>
    InRehabilitationCenter = 4,

    /// <summary>
    /// in polyclinic
    /// </summary>
    InPolyclinic = 5,

    /// <summary>
    /// other
    /// </summary>
    Other = 6
  }
 }
