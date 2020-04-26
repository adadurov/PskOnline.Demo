namespace PskOnline.Server.Plugins.Rushydro.Test
{
  using NUnit.Framework;
  using PskOnline.Components.Log;
  using PskOnline.Components.Util;
  using PskOnline.Methods.Hrv.ObjectModel;
  using PskOnline.Methods.Hrv.Processing.Logic;
  using Shouldly;
  using System;
  using System.Linq;

  [TestFixture]
  public class ModeAmplitudeStabilityTest
  {

    [SetUp]
    public void SetUp()
    {
      LogHelper.ConfigureConsoleLogger();
    }

    [TearDown]
    public void TearDown()
    {
      LogHelper.ShutdownLogSystem();
    }

    [Test]
    public void Amo_Min_Max_Change_Should_Not_Exceed_30_Percent()
    {
      // Given
      var d = TestDataProvider.GetTestData(995);
      //var hrvKey = d.Keys.FirstOrDefault(k => k.MethodId == HrvMethodId.MethodId);
      //var hrv__ = TestDataJsonFormat_0_1.GetTestData<HrvRawData>(d[hrvKey]);

      var hrv = d.Where(c => c.Key.MethodId == HrvMethodId.MethodId)
                 .Select(c => c.Value.ToObject<HrvRawData>())
                 .First();

      var processor = new HrvBasicDataProcessor();

      // When
      var data = processor.ProcessData(hrv);

      var delta = data.Indicators.IN_Max - data.Indicators.IN_Min;
      var average = 0.5 * (data.Indicators.IN_Max + data.Indicators.IN_Min);

      // Then
      delta.ShouldBeLessThan(average * 0.33);
    }
  }
}
