namespace PskOnline.Server.Plugins.RusHydro.Logic
{
  using System;

  using System.Collections.Generic;
  using PskOnline.Server.Plugins.RusHydro.ObjectModel;

  /// <summary>
  /// Логика расписания (дневные и ночные смены)
  /// 
  /// Предоставляет операции для определения даты начала смены,
  /// к которой относится указанное значение даты/времени, а также
  /// вида смены (дневная или ночная).
  /// 
  /// </summary>
  public static class RusHydroScheduler
  {
    static log4net.ILog _log = log4net.LogManager.GetLogger(typeof(RusHydroScheduler));

    public static TimeSpan DayShiftStartFromMidNight => new TimeSpan(6, 30, 0);

    public static TimeSpan NightShiftStartFromMidNight => new TimeSpan(18, 30, 0);

    public static DailySchedule GetDailySchedule()
    {
      return new DailySchedule()
      {
        WorkingShifts = new List<WorkingShift>
        {
          new WorkingShift
          {
            StartTime = DayShiftStartFromMidNight,
            Duration = TimeSpan.FromHours(12),
            ShiftNumber = 1,
            ShiftName = "Day"
          },
          new WorkingShift
          {
            StartTime = NightShiftStartFromMidNight,
            Duration = TimeSpan.FromHours(12),
            ShiftNumber = 2,
            ShiftName = "Night"
          }
        }
      };
    }

    /// <summary>
    /// Given a time stamp, returns the date of the shift
    /// and sets the flag indicating whether the shift is a day or a night one
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="isDayShift"></param>
    /// <returns></returns>
    public static DateTime GetShiftStartDate(DateTime dateTime, out bool isDayShift, out int shiftNumber)
    {
      DateTime shiftStartDate;

      TimeSpan timeFromMidnight = dateTime.TimeOfDay;

      if( timeFromMidnight >= DayShiftStartFromMidNight &&
          timeFromMidnight < NightShiftStartFromMidNight)
      {
        // inspectionCompletionTime is within a day shift
        shiftStartDate = dateTime.Date;
        isDayShift = true;
      }
      else
      {
        // inspectionCompletionTime is within a night shift;
        // figure out the date that the night shift belongs to 
        // according to RusHydro business rules

        isDayShift = false;
        if( timeFromMidnight >= NightShiftStartFromMidNight )
        {
          shiftStartDate = dateTime.Date;
        }
        else
        {
          shiftStartDate = dateTime.Date.AddDays(-1);
        }
      }
      shiftNumber = isDayShift ? 1 : 2;
      return shiftStartDate;
    }

    /// <summary>
    /// Given a time stamp, returns the date and time the current shift started
    /// and sets the flag indicating whether the shift is a day or a night one
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="isDayShift"></param>
    /// <returns></returns>
    public static DateTime GetShiftStartTime(DateTime dateTime, out bool isDayShift, out int shiftNumber)
    {
      DateTime shiftStartDate = GetShiftStartDate(dateTime, out isDayShift, out shiftNumber);

      if( isDayShift )
      {
        return shiftStartDate + DayShiftStartFromMidNight;
      }
      else
      {
        return shiftStartDate + NightShiftStartFromMidNight;
      }
    }

  }
}