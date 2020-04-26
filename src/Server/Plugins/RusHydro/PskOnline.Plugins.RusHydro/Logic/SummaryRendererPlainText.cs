namespace PskOnline.Server.Plugins.RusHydro.Logic
{
  using PskOnline.Server.Plugins.RusHydro.ObjectModel;

  /// <summary>
  /// Формирует содержимое сводки по результатам предсменного контроля в текстовом формате.
  /// </summary>
  public class SummaryRendererPlainText : ISummaryRenderer
  {
    private log4net.ILog _log = log4net.LogManager.GetLogger(typeof(SummaryRendererPlainText));

    public SummaryRendererPlainText()
    {
    }

    public string FilenameExtension { get { return "txt"; } }

    public void RenderSummary(SummaryDocument summary, ISummaryWriter writer, string baseSummaryFolderPath, out string actualFileName)
    {
      string hrvSummary = string.Format("{0,8}" + Formatter.DefaultSpacing + "{1,-20} {2}",
        summary.HrvConclusion.TestId, summary.HrvConclusion.Text, summary.HrvConclusion.Comment);

      string svmrSummary = string.Format("{0,8}" + Formatter.DefaultSpacing + "{1,-20} {2}",
        summary.SvmrConclusion.TestId, summary.SvmrConclusion.Text, summary.SvmrConclusion.Comment);

      string renderedSummary = string.Format(SummaryTextFormat,
        summary.Employee,
        summary.CompletionTime.ToString(strings.SummaryText_DateFormat),
        summary.HostName,
        hrvSummary,
        svmrSummary,
        summary.FinalConclusion.Text);

      // write out to file

      var writingParams = new SummaryWritingParameters();
      writingParams.baseSummaryFolderPath = baseSummaryFolderPath;
      writingParams.completionTime = summary.CompletionTime;
      writingParams.filenameExtension = FilenameExtension;
      writingParams.hostName = summary.HostName;
      writingParams.employee = summary.Employee;

      writer.SaveSummary(writingParams, renderedSummary, out actualFileName);
    }

    public string SummaryTextFormat
    {
      get
      {
        string NL = System.Environment.NewLine;
        return strings.SummaryText_P0_Format_Name + "{0}" + NL +
          strings.SummaryText_P1_Format_Date + "{1}" + NL +
          strings.SummaryText_P2_Format_Workstation + "{2}" + NL +
          strings.SummaryText_Separator + NL +
          strings.SummaryText_P3_Format_HRV + "{3}" + NL +
          strings.SummaryText_P4_Format_SVMR + "{4}" + NL +
          NL +
          "{5}" + NL;
      }
    }
  }

}
