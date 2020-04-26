namespace PskOnline.Methods.Hrv.Test.Processing
{
  using System.IO;

  using NUnit.Framework;

  using PskOnline.Components.Log;
  using PskOnline.Components.Util;
  using PskOnline.Methods.Hrv.ObjectModel;
  using PskOnline.Methods.Hrv.Processing.Settings;

  [TestFixture]
  public class Processor_UTest
  {
    log4net.ILog log = log4net.LogManager.GetLogger(typeof(Processor_UTest));

    [SetUp]
    public void SetUp()
    {
      // Необходимо для чтения старых PDS-файлов...
      LogHelper.ConfigureConsoleLogger();
    }

    [TearDown]
    public void TearDown()
    {
      LogHelper.ShutdownLogSystem();
    }

    [Test]
    public void Test_Bug_29_Regression()
    {
      string full_data_file_name = 
        FileHelpers.GetPathFromExecutingAssembly(
          Path.Combine("unit_test_data", TestUtil.TestFileNames[6]));

      var results = ProcessFile(full_data_file_name, true);

      Assert.AreEqual(73, results.CRV_INTERVALS.Length, "В сигнале должно быть обнаружено 73 кардио-интервалов!");
    }


    private int GetCountOfRejectedHrMarks(HrvResults results)
    {
      int count = 0;
      for (int i = 0; i < results.RATED_HR_MARKS.Length; ++i)
      {
        if (results.RATED_HR_MARKS[i].IntervalsCount < 2)
        {
          ++count;
        }
      }
      return count;
    }

    private HrvResults ProcessFile(string filename, bool bPerformRejection)
    {
      var ps = new ProcessingSettings {
        RejectUsingMinMaxNNTime = bPerformRejection,
        RejectUsingRelativeNNDelta = bPerformRejection
      };

      return HrvFileProcessingHelper.ProcessFileWithSettings(filename, ps);
    }

  }
}
