namespace PskOnline.Server.Plugins.Rushydro.Test
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using Newtonsoft.Json.Linq;

  using NUnit.Framework;

  using PskOnline.Components.Log;
  using PskOnline.Methods.ObjectModel;
  using PskOnline.Server.Plugins.RusHydro.Logic;
  using PskOnline.Server.Plugins.RusHydro.ObjectModel;
  using PskOnline.Methods.ObjectModel.Method;

  [TestFixture]
  public class PreShiftSummaryGenerator_UTest
  {
    private readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof (PreShiftSummaryGenerator_UTest));

    private string _basePath;

    public PreShiftSummaryGenerator_UTest()
    {
    }

    [SetUp]
    public void SetUp()
    {
      LogHelper.ConfigureConsoleLogger();
      _basePath = Path.Combine(Path.GetTempPath(), "summary_generator_tests");
      Directory.CreateDirectory(_basePath);
    }

    [TearDown]
    public void TearDown()
    {
      Directory.Delete(_basePath, true);

      LogHelper.ShutdownLogSystem();
    }

    private IMethodRawData ReadMethodSpecificData(object json, string methodId)
    {
      throw new NotImplementedException();
    }

    internal class SummaryInterceptor : ISummaryRenderer
    {
      private readonly ISummaryRenderer _realRenderer;
      private SummaryDocument _lastRenderedSummary;

      public SummaryDocument LastRenderedSummary => _lastRenderedSummary;

      public SummaryInterceptor(ISummaryRenderer realRenderer)
      {
        _realRenderer = realRenderer;
      }

      public string FilenameExtension => _realRenderer.FilenameExtension;

      public void RenderSummary(SummaryDocument summary, ISummaryWriter writer,
        string baseSummaryFolderPath, out string actualFileName)
      {
        _lastRenderedSummary = summary;
        actualFileName = string.Empty;
        _realRenderer.RenderSummary(summary, writer, baseSummaryFolderPath, out actualFileName);
      }
    }

    internal class SummaryWriterInterceptor : ISummaryWriter
    {
      public SummaryWriterInterceptor(ISummaryWriter actualWriter)
      {
        _actualWriter = actualWriter;
      }
      public bool SaveSummary(SummaryWritingParameters parameters, string summaryContent, out string usedSummaryFileName)
      {
        _writeCalled = true;
        LastInterceptedContent = summaryContent;
        bool result = _actualWriter.SaveSummary(parameters, summaryContent, out usedSummaryFileName);
        _usedSummaryFileName = usedSummaryFileName;
        return result;
      }

      private ISummaryWriter _actualWriter;
      public bool _writeCalled = false;
      public string _usedSummaryFileName;
      public string LastInterceptedContent;
    }

    [Test]
    public void Inspection_1000_SVMR_Failure_Shall_Yield_a_Non_Compliant_Summary()
    {
      // Given
      // A test data set #1000
      var testData = TestDataProvider.GetTestData(1000);

      // When Rushydro rules are applied
      Expect(testData,
        // Then
        // SVMR status shall be...
        PsaStatus.Fail,
        // HRV status shall be...
        PsaStatus.Fail,
        // Overall status shall be...
        PsaStatus.Fail,
        // And summary shall be created in a folder with a path ending with...
        @"2017\07\02\день",
        // And summary RTF file name shall be...
        "2017.07.02_12.13_Азанов Андрей Васильевич",
        ".rtf");
    }

    [Test]
    public void Inspection_999()
    {
      // Given
      // A test data set #999
      var testData = TestDataProvider.GetTestData(999);

      // When Rushydro rules are applied
      Expect(testData,
        // Then 
        // SVMR status shall be...
        PsaStatus.Fail,
        // HRV status shall be...
        PsaStatus.Conditional_Pass,
        // Overall status shall be...
        PsaStatus.Fail,
        // And summary shall be created in a folder with a path ending with...
        @"2017\07\02\день",
        // And summary RTF file name shall start with...
        "2017.07.02_11.57_Азанов Андрей Васильевич",
        ".rtf");
    }

    [Test]
    public void Inspection_998()
    {
      // Given
      // A test data set #998
      var testData = TestDataProvider.GetTestData(998);

      // When Rushydro rules are applied
      Expect(testData,
        // Then 
        // SVMR status shall be...
        PsaStatus.Fail,
        // HRV status shall be...
        PsaStatus.Fail,
        // Overall status shall be...
        PsaStatus.Fail,
        // And summary shall be created in a folder with a path ending with...
        @"2017\07\02\день",
        // And summary RTF file name shall be...
        "2017.07.02_11.46_Азанов Андрей Васильевич",
        ".rtf");
    }

    [Test]
    public void Inspection_996()
    {
      // Given
      // A test data set #996
      var testData = TestDataProvider.GetTestData(996);

      // When Rushydro rules are applied
      Expect(testData,
        // Then 
        // SVMR status shall be...
        PsaStatus.Pass,
        // HRV status shall be...
        PsaStatus.Fail,
        // Overall status shall be...
        PsaStatus.Fail,
        // And summary shall be created in a folder with a path ending with...
        @"2017\07\01\ночь",
        // And summary RTF file name shall be...
        "2017.07.01_22.40_Иевлев Алексей Викторович",
        ".rtf");
    }

    [Test]
    public void Inspection_995()
    {
      // Given
      // A test data set #995
      var testData = TestDataProvider.GetTestData(995);

      // When Rushydro rules are applied
      Expect(testData,
        // Then 
        // SVMR status shall be...
        PsaStatus.Pass,
        // HRV status shall be...
        PsaStatus.Pass, 
        // Overall status shall be...
        PsaStatus.Pass,
        // And summary shall be created in a folder with a path ending with...
        @"2017\07\01\ночь",
        // And summary RTF file name shall be...
        "2017.07.01_21.32_Иевлев Алексей Викторович",
        ".rtf");
    }

    private void Expect(
      IDictionary<TestInfo, JObject> testData, 
      PsaStatus expectedSvmrStatus, 
      PsaStatus expectedHrvStatus, 
      PsaStatus expectedFinalStatus,
      string expectedPathSuffix,
      string expectedFileNamePrefix,
      string expectedFileNameSuffix
      )
    {
      //var interceptor = new SummaryInterceptor(new SummaryRendererRtf());
      //var writer = new SummaryWriterInterceptor(new SummaryWriter());
      //var renderers = new ISummaryRenderer[] { interceptor };

      var generator = new PreShiftSummaryGenerator();

      var pskOnlineTestData = testData;

      var employee = new Employee
      {
        FullName = "John Doe",
        BranchOfficeName = "Ферма №212",
        DepartmentName = "Разделочный цех",
        PositionName = "Обвальщик"
      };

      // When
      // Rushydro rules are applied for generating summary
      var summary = generator.GenerateSummary(
        "someDatabaseId", Guid.NewGuid(), employee, pskOnlineTestData);

      // Then
      // final status and individual method statuses
      // meet the specification in the method's parameters
      Assert.AreEqual(expectedSvmrStatus, summary.SvmrConclusion.Status, "SVMR status");
      Assert.AreEqual(expectedHrvStatus, summary.HrvConclusion.Status, "HRV status");
      Assert.AreEqual(expectedFinalStatus, summary.FinalConclusion.Status, "OVERALL status");

      //Assert.IsTrue(writer._writeCalled, "Write must have been called");
      //_log.InfoFormat("Summary written to the file: '{0}'", writer._usedSummaryFileName);

      //string actualSummaryFileName = Path.GetDirectoryName(writer._usedSummaryFileName);
      //Assert.IsTrue(actualSummaryFileName.EndsWith(expectedPathSuffix),
      //  $"Expected summary directory suffix is '{expectedPathSuffix}'; not found within '{actualSummaryFileName}'");

      //Assert.IsTrue(Path.GetFileName(writer._usedSummaryFileName).StartsWith(expectedFileNamePrefix),
      //  "Expected summary file name prefix must be " + expectedFileNamePrefix
      //  );

      //Assert.IsTrue(Path.GetFileName(writer._usedSummaryFileName).EndsWith(expectedFileNameSuffix), 
      //  "Expected summary file name suffix must be " + expectedPathSuffix
      //  );

      //_log.Info(summary);
      
      Assert.IsNotEmpty( summary.Employee.FullName , "Summary shall contain non-empty employee name");

      Assert.Greater(summary.CompletionTime, DateTimeOffset.MinValue, "Summary shall contain a meaningful completion date");
      Assert.Less(summary.CompletionTime, DateTimeOffset.MaxValue, "Summary shall contain a meaningful completion date");

//      Assert.IsNotEmpty(summary.Employee.DepartmentName, "Summary shall contain non-empty department name for the employee");

      Assert.IsNotEmpty(summary.Employee.PositionName, "Summary shall contain non-empty position name for the employee");

    }
  }
}
