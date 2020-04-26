namespace PskOnline.Server.Shared.Service
{
  using System;

  public interface ITimeService
  {
    bool CheckTimeZoneId(string timeZoneId);

    TimeZoneInfo GetTimeZone(string timeZoneId);
  }
}
