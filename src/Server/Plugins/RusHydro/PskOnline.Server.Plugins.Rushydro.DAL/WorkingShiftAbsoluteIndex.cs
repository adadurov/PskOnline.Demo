namespace PskOnline.Server.Plugins.RusHydro.DAL
{
  using PskOnline.Server.Plugins.RusHydro.Logic;
  using System;
  using System.Collections.Generic;
  using System.Text;

  public static class WorkingShiftAbsoluteIndex
  {
    /// <summary>
    /// Generates the absolute number of the shift that the specified localTime
    /// belongs to. The index is built according to the below formula:
    /// 
    /// YYYY*10^5 + MM*10^3 + dd*10^1 + shiftNumber
    /// 
    /// for example, RusHydro's day shift of 2018.12.03 has its ShiftAbsoluteIndex of 201.812.031
    /// (where . is the thousands separator)
    /// </summary>
    public static long GetAbsoluteIndex(DateTime localTime)
    {
      const long fY = 100000;
      const long fM = 1000;
      const long fD = 10;
      var shiftDate = RusHydroScheduler.GetShiftStartDate(localTime, out bool _, out int shiftNumber);
      return shiftDate.Year * fY + shiftDate.Month * fM + shiftDate.Day * fD + shiftNumber;
    }
  }
}
