namespace PskOnline.Service.Test.Services
{
  using System;
  using NUnit.Framework;

  using PskOnline.Server.DAL.Time;

  [TestFixture]
  public sealed class TimeService_Test
  {
    private readonly TimeService _timeService;

    public TimeService_Test()
    {
      _timeService = new TimeService();
    }

    [Test]
    public void Russian_Time_Zone_Codes_Should_Be_Valid_With_Valid_Offset()
    {
      CheckTimeZone("MSK-1", -1);
      CheckTimeZone("MSK", 0);
      CheckTimeZone("MSK+1", 1);
      CheckTimeZone("MSK+2", 2);
      CheckTimeZone("MSK+3", 3);
      CheckTimeZone("MSK+4", 4);
      CheckTimeZone("MSK+5", 5);
      CheckTimeZone("MSK+6", 6);
      CheckTimeZone("MSK+7", 7);
      CheckTimeZone("MSK+8", 8);
      CheckTimeZone("MSK+9", 9);
    }

    public void CheckTimeZone(string timeZoneId, int expectedMoscowTimeOffset)
    {
      Assert.IsTrue(_timeService.CheckTimeZoneId(timeZoneId) );

      var tz = _timeService.GetTimeZone(timeZoneId);
      Assert.That(tz.BaseUtcOffset.Hours - 3, Is.EqualTo(expectedMoscowTimeOffset));
    }
  }
}