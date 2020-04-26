namespace PskOnline.Methods.Hrv.Test.Processing
{
  using System.IO;

  using NUnit.Framework;

  using PskOnline.Components.Log;
  using PskOnline.Methods.Hrv.ObjectModel;
  using PskOnline.Methods.Hrv.Processing.Settings;

  [TestFixture]
  public class Processor_RejectLowQualityFragments_UTest
  {
    log4net.ILog log = log4net.LogManager.GetLogger(typeof(Processor_RejectLowQualityFragments_UTest));

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

    private HrvResults ProcessFile(string filename, bool bPerformRejection, bool RejectLowQualitySignalAreas)
    {
      filename = PskOnline.Components.Util.FileHelpers.GetPathFromExecutingAssembly(
        Path.Combine("unit_test_data", Path.Combine("LowQualityStart", filename)));

      var ps = new ProcessingSettings
      {
        RejectUsingMinMaxNNTime = bPerformRejection,
        RejectUsingRelativeNNDelta = bPerformRejection,
        // special setting for RusHydro
        // will remove initial 12 seconds of recorded signal
        // because it is mostly of some poor quality
        RejectLowQualitySignalAreas = RejectLowQualitySignalAreas
      };

      return HrvFileProcessingHelper.ProcessFileWithSettings(filename, ps);
    }

    /// <summary>
    /// Как будет работать вырезание фрагментов низкого качества и как его контролировать:
    ///
    /// 1. Вычисляем индекс качества сигнала для каждого сэмпла или каждого фрагмента шириной X сэмплов.
    /// 
    /// 2. Выбираем пороговое значение.
    /// 
    /// 3. Если сердечное сокращение (фронт) попадает на область где SQI ниже порога, выбрасываем такой фронт.
    /// 
    /// </summary>
    [Test]
    [Explicit]
    [Category("Interactive")]
    public void Dump_Results_For_Files_With_Low_Quality_Fragments()
    {

      foreach (TestUtil.tsd testSample in TestUtil.low_quality_start_samples)
      {
        var result1 = ProcessFile(testSample._fileName, true, false);
        var resultWithMasking = ProcessFile(testSample._fileName, true, false);
      }
    }

    [Test]
    public void TestAllFilesWithLowQualityFragments_Must_Yield_Expected_Results()
    {

      foreach (TestUtil.tsd testSample in TestUtil.low_quality_start_samples)
      {
        // with masking low-quality fragments
        HrvResults results = ProcessFile(testSample._fileName, true, true);

        Assert.That(results.CRV_STAT.m, Is.AtLeast(testSample._expectedMrrLow),
          string.Format($"Expect MRR of at least {testSample._expectedMrrLow} for file {testSample._fileName}"));

        Assert.That(results.CRV_STAT.m, Is.AtMost(testSample._expectedMrrHigh),
          string.Format($"Expect MRR of at most {testSample._expectedMrrHigh} for file {testSample._fileName}"));

      }
    }

  }
}
