namespace PskOnline.Server.Plugins.Rushydro.Test
{
  using System;

  using NUnit.Framework;

  using PskOnline.Components.Log;
  using PskOnline.Server.Plugins.RusHydro.Logic;
  using PskOnline.Server.Plugins.RusHydro.ObjectModel;

  [TestFixture]
  public class SummaryRenderer_UTest
  {
    private log4net.ILog _log = log4net.LogManager.GetLogger(typeof(SummaryRenderer_UTest));
    private WriterMock _writer;

    public SummaryRenderer_UTest()
    {
    }

    [SetUp]
    public void SetUp()
    {
      LogHelper.ConfigureConsoleLogger();
      _writer = new WriterMock();
    }

    [TearDown]
    public void TearDown()
    {
      LogHelper.ShutdownLogSystem();
    }

    private void PrepareData(out Employee patient, out PreShiftHrvConclusion hrvConclusion, out PreShiftSvmrConclusion svmrConclusion, out PreShiftFinalConclusion finalConclusion)
    {
      var hrvStatus = PsaStatus.Fail;
      hrvConclusion = new PreShiftHrvConclusion
      {
        Status = hrvStatus,
        Text = PsaStatusTextProvider.StatusText(hrvStatus),
        TestId = Guid.NewGuid()
      };

      var svmrStatus = PsaStatus.Pass;
      svmrConclusion = new PreShiftSvmrConclusion
      {
        Status = svmrStatus,
        Text = PsaStatusTextProvider.StatusText(svmrStatus),
        TestId = Guid.NewGuid()
      };

      var finalStatus = PsaStatus.Fail;
      finalConclusion = new PreShiftFinalConclusion
      {
        Status = finalStatus,
        InspectionId = Guid.NewGuid()
      };
      finalConclusion.Text = FinalStatusTextProvider.StatusText(finalConclusion);

      patient = new Employee()
      {
        Id = Guid.NewGuid(),
        FullName = "John Smith",
        BranchOfficeName = string.Empty,
        BranchOfficeId = Guid.NewGuid(),
        DepartmentName = string.Empty,
        DepartmentId = Guid.NewGuid(),
        PositionName = "Main Control Post",
        PositionId = Guid.NewGuid()
      };
    }

    [Test]
    public void Smoke_Test_RTF()
    {
      ISummaryRenderer renderer = new SummaryRendererRtf();
      Assert.AreEqual("rtf", renderer.FilenameExtension.ToLower());

      PreShiftHrvConclusion hrvConcl;
      PreShiftSvmrConclusion svmrConcl;
      PreShiftFinalConclusion finalConcl;
      Employee p;

      PrepareData(out p, out hrvConcl, out svmrConcl, out finalConcl);

      var summary = new SummaryDocument
      {
        Employee = p,
        CompletionTime = DateTimeOffset.Now,
        HostName = System.Net.Dns.GetHostName(),
        HrvConclusion = hrvConcl,
        SvmrConclusion = svmrConcl,
        FinalConclusion = finalConcl
      };

      string actualFileName;
      renderer.RenderSummary(summary, _writer, string.Empty, out actualFileName);

      Assert.IsNotEmpty(_writer.LastWrittenContent);
    }

    [Test]
    public void Smoke_Test_TXT()
    {
      ISummaryRenderer renderer = new SummaryRendererPlainText();
      Assert.AreEqual("txt", renderer.FilenameExtension.ToLower());

      PreShiftSvmrConclusion svmrConcl;
      PreShiftHrvConclusion hrvConcl;
      PreShiftFinalConclusion finalConcl;
      Employee p;

      PrepareData(out p, out hrvConcl, out svmrConcl, out finalConcl);

      var summary = new SummaryDocument
      {
        Employee = p,
        CompletionTime = DateTimeOffset.Now,
        HostName = System.Net.Dns.GetHostName(),
        HrvConclusion = hrvConcl,
        SvmrConclusion = svmrConcl,
        FinalConclusion = finalConcl
      };

      string actualFileName;
      renderer.RenderSummary(summary, _writer, "", out actualFileName);

      Assert.IsNotEmpty(_writer.LastWrittenContent);
    }

    /// <summary>
    /// тест для формирователя сводки для информирования самого обследуемого
    /// </summary>
    [Test]
    public void Smoke_Test_Personal()
    {
      SummaryRendererPersonal renderer = new SummaryRendererPersonal();

      PreShiftSvmrConclusion svmrConcl;
      PreShiftHrvConclusion hrvConcl;
      PreShiftFinalConclusion finalConcl;
      Employee p;

      PrepareData(out p, out hrvConcl, out svmrConcl, out finalConcl);

      var summary = new SummaryDocument
      {
        Employee = p,
        CompletionTime = DateTimeOffset.Now,
        HostName = System.Net.Dns.GetHostName(),
        HrvConclusion = hrvConcl,
        SvmrConclusion = svmrConcl,
        FinalConclusion = finalConcl
      };

      string content = renderer.RenderSummary(summary);

      _log.Info(Environment.NewLine + content);

      Assert.IsNotEmpty(content);
    }


    class WriterMock : ISummaryWriter
    {
      private string _content;

      public string LastWrittenContent { get { return _content; } }

      public bool SaveSummary(SummaryWritingParameters parameters, string summaryContent, out string usedSummaryFileName)
      {
        _content = summaryContent;
        usedSummaryFileName = "none";
        return true;
      }
    }

  }
}
