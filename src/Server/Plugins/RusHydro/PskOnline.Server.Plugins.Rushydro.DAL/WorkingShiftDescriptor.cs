﻿namespace PskOnline.Server.Plugins.RusHydro.DAL
{
  using System;

  public class WorkingShiftDescriptor
  {
    public DateTime ShiftDate { get; set; }

    public TimeSpan ShiftStartTime { get; set; }

    public TimeSpan ShiftDuration { get; set; }

    /// <summary>
    /// gets or sets the name of the shift as a Roman number
    /// ('I', 'II', 'III') or as a verbal name ('день', 'ночь')
    /// </summary>
    public string ShiftName { get; set; }

    /// <summary>
    /// Gets or sets the absolute number of the shift including 
    /// its calendar date, as generated by <see cref="WorkingShiftAbsoluteIndex"/>)
    /// </summary>
    public long ShiftAbsoluteIndex { get; set; }

    public int ShiftNumber { get; set; }
  }
}
