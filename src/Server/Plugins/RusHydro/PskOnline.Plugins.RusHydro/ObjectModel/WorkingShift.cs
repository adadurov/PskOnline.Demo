namespace PskOnline.Server.Plugins.RusHydro.ObjectModel
{
  using System;

  /// <summary>
  /// represents a working shift infromation for scheduling
  /// </summary>
  public class WorkingShift
  {
    /// <summary>
    /// Time of the shift start
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public TimeSpan Duration { get; set; }

    public int ShiftNumber { get; set; }

    public string ShiftName { get; set; }
  }
}
