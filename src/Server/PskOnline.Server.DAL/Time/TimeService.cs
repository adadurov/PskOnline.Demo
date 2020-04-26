namespace PskOnline.Server.DAL.Time
{
  using System;
  using System.Collections.Generic;

  using PskOnline.Server.Shared.Service;

  public class TimeService : ITimeService
  {
    private readonly Dictionary<string, TimeZoneInfo> _timeZones;

    public TimeService()
    {
      _timeZones = InitTimeZoneDefinitions();
    }

    public bool CheckTimeZoneId(string timeZoneId)
    {
      if (timeZoneId == null) return false;
      try
      {
        return _timeZones.ContainsKey(timeZoneId);
      }
      catch (TimeZoneNotFoundException)
      {
        return false;
      }
    }

    public Dictionary<string, TimeZoneInfo> InitTimeZoneDefinitions()
    {
      var kaliningradTzName = "Kaliningrad time";
      var kaliningradTime = TimeZoneInfo.CreateCustomTimeZone("MSK-1", TimeSpan.FromHours(2), kaliningradTzName, kaliningradTzName);

      var moscowTzName = "Moscow time";
      var moscowTime = TimeZoneInfo.CreateCustomTimeZone("MSK", TimeSpan.FromHours(3), moscowTzName, moscowTzName);

      var samaraTzName = "Samara time";
      var samaraTime = TimeZoneInfo.CreateCustomTimeZone("MSK+1", TimeSpan.FromHours(4), samaraTzName, samaraTzName);

      var yekaterinburgTzName = "Yekaterinburg time";
      var yekaterinburgTime = TimeZoneInfo.CreateCustomTimeZone("MSK+2", TimeSpan.FromHours(5), yekaterinburgTzName, yekaterinburgTzName);

      var omskTzName = "Omsk time";
      var omskTime = TimeZoneInfo.CreateCustomTimeZone("MSK+3", TimeSpan.FromHours(6), omskTzName, omskTzName);

      var krasnoyarskTzName = "Krasnoyarsk time";
      var krasnoyarskTime = TimeZoneInfo.CreateCustomTimeZone("MSK+4", TimeSpan.FromHours(7), krasnoyarskTzName, krasnoyarskTzName);

      var irkTzName = "Irkutsk time";
      var irkutskTime = TimeZoneInfo.CreateCustomTimeZone("MSK+5", TimeSpan.FromHours(8), irkTzName, irkTzName);

      var yakTzName = "Yakutsk time";
      var yakutskTime = TimeZoneInfo.CreateCustomTimeZone("MSK+6", TimeSpan.FromHours(9), yakTzName, yakTzName);

      var vlaTzName = "Vladivostok time";
      var vladivostokTime = TimeZoneInfo.CreateCustomTimeZone("MSK+7", TimeSpan.FromHours(10), vlaTzName, vlaTzName);

      var magTzName = "Magadan time";
      var magadanTime = TimeZoneInfo.CreateCustomTimeZone("MSK+8", TimeSpan.FromHours(11), magTzName, magTzName);

      var kamchatkaTzName = "Kamchatka time";
      var kamchatkaTime = TimeZoneInfo.CreateCustomTimeZone("MSK+9", TimeSpan.FromHours(12), kamchatkaTzName, kamchatkaTzName);

      return new Dictionary<string, TimeZoneInfo> {
        { kaliningradTime.Id, kaliningradTime },
        { moscowTime.Id, moscowTime },
        { samaraTime.Id, samaraTime },
        { yekaterinburgTime.Id, yekaterinburgTime },
        { omskTime.Id, omskTime },
        { krasnoyarskTime.Id, krasnoyarskTime },
        { irkutskTime.Id, irkutskTime },
        { yakutskTime.Id, yakutskTime },
        { vladivostokTime.Id, vladivostokTime },
        { magadanTime.Id, magadanTime },
        { kamchatkaTime.Id, kamchatkaTime }
      };
    }

    public TimeZoneInfo GetTimeZone(string timeZoneId)
    {
      return _timeZones[timeZoneId];
    }
  }
}
