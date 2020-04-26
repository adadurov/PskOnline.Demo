namespace PskOnline.Methods.Hrv.Test.Processing
{
  using System;
  using System.IO;

  using NUnit.Framework;

  using PskOnline.Components.Log;
  using PskOnline.Components.Util;
  using PskOnline.Methods.Hrv.ObjectModel;
  using PskOnline.Methods.Hrv.Processing.Settings;

  [TestFixture]
  public class Processor_Rejection_Effect_UTest
  {
    log4net.ILog log = log4net.LogManager.GetLogger(typeof(Processor_Rejection_Effect_UTest));

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
    public void Test_File_2957_With_And_Without_Peaks_Rejection()
    {
      Test_File_With_And_Without_Peaks_Rejection(TestUtil.TestFileNames[7]);
    }

    [Test]
    public void test_unit_test_data_file_number_5_with_and_without_peaks_rejection()
    {
      Test_File_With_And_Without_Peaks_Rejection(TestUtil.TestFileNames[5]);
    }

    private void Test_File_With_And_Without_Peaks_Rejection(string filename_in_unit_test_data_folder)
    {
      string full_data_file_name = FileHelpers.GetPathFromExecutingAssembly(
        Path.Combine("unit_test_data", filename_in_unit_test_data_folder));

      var output = Test_File_With_And_Without_Rejection(full_data_file_name);

      ProcessingSettings ps_ON = output.rejection_ON_settings;
      ProcessingSettings ps_OFF = output.rejection_OFF_settings;

      HrvResults results_rejection_ON = output.rejection_ON_results;
      HrvResults results_rejection_OFF = output.rejection_OFF_results;

      Assert.AreEqual(true, ps_ON.RejectUsingMinMaxNNTime);
      Assert.AreEqual(true, ps_ON.RejectUsingRelativeNNDelta);

      Assert.AreEqual(false, ps_OFF.RejectUsingMinMaxNNTime);
      Assert.AreEqual(false, ps_OFF.RejectUsingRelativeNNDelta);

      int count_of_rejected_hr_marks_rejection_OFF = GetCountOfRejectedHrMarks(results_rejection_OFF);
      int count_of_rejected_hr_marks_rejection_ON = GetCountOfRejectedHrMarks(results_rejection_ON);

      log.InfoFormat("Finished testing of processing results from file '{0}' with rejection ON and OFF.", filename_in_unit_test_data_folder);
      log.InfoFormat("  Rejection ON:   {0} pulse waves rejected.", count_of_rejected_hr_marks_rejection_ON);
      log.InfoFormat("  Rejection OFF:  {0} pulse waves rejected.", count_of_rejected_hr_marks_rejection_OFF);

      Assert.IsTrue(
        count_of_rejected_hr_marks_rejection_ON > count_of_rejected_hr_marks_rejection_OFF,
        "Results obtained with rejection contain less rejected heart contraction marks than results obtained with no rejection."
        );

      log.Info("Checks finished OK.");
      log.Info("----------------------------------------------------------------------------------------------------");
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

    private dynamic Test_File_With_And_Without_Rejection(string filename)
    {
      return new
      {
        rejection_OFF_results = ProcessFile(filename, false, out var procSettingsNoRejection),
        rejection_OFF_settings = procSettingsNoRejection,
        rejection_ON_results = ProcessFile(filename, true, out var processingSettingsWithRejection),
        rejection_ON_settings = processingSettingsWithRejection
      };
    }

    private HrvResults ProcessFile(string filename, bool bPerformRejection, out ProcessingSettings procSettings)
    {
      procSettings = new ProcessingSettings {
        RejectUsingMinMaxNNTime = bPerformRejection,
        RejectUsingRelativeNNDelta = bPerformRejection
      };

      return HrvFileProcessingHelper.ProcessFileWithSettings(filename, procSettings);
    }
  }
}
