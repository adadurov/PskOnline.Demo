namespace PskOnline.Server.Plugins.RusHydro.Logic
{
  using PskOnline.Server.Plugins.RusHydro.ObjectModel;

  /// <summary>
  /// Формирует содержимое сводки по результатам предсменного контроля в виде
  /// plain text строки, предназначенной для демонстрации самому обледуемому.
  /// "Спасибо, ваши результаты ..."
  /// </summary>
  public class SummaryRendererPersonal
  {
    private log4net.ILog _log = log4net.LogManager.GetLogger(typeof(SummaryRendererPersonal));

    public SummaryRendererPersonal()
    {
    }

    public string RenderSummary(SummaryDocument summary)
    {
      string hrvSummary = string.Format("{0,8}" + Formatter.DefaultSpacing + "{1,-20} {2}",
        summary.HrvConclusion.TestId, summary.HrvConclusion.Text, string.Empty /*summary.HrvConclusion.Comment*/);

      string svmrSummary = string.Format("{0,8}" + Formatter.DefaultSpacing + "{1,-20} {2}",
        summary.SvmrConclusion.TestId, summary.SvmrConclusion.Text, string.Empty /*summary.SvmrConclusion.Comment*/);

      string renderedSummary = string.Format(SummaryTextFormat,
        summary.Employee.FullName,
        summary.Employee.PositionName,
        summary.CompletionTime.ToString(strings.SummaryText_DateFormat),
        summary.HostName,
        hrvSummary,
        svmrSummary,
        summary.FinalConclusion.Text);

      // add final conclusion
      _log.InfoFormat("Successfully prepared personal summary");
      return renderedSummary;
    }

    public string SummaryTextFormat
    {
      get
      {
        string NL = System.Environment.NewLine;
        return 
//          strings.SummaryText_P0_Format_Name + 
          "{0}, " + strings.ThankYouAssessmentFinished + NL +
//          strings.SummaryText_P1_1_Format_Position + "{1}" + NL +
//          strings.SummaryText_P1_Format_Date + "{2}" + NL +
          NL +
          strings.SummaryText_P5_Format_Overall_Summary + "{6}" + NL +
          strings.SummaryText_Separator + NL +
//          strings.SummaryText_P2_Format_Workstation + "{3}" + NL +
//          strings.SummaryText_Separator + NL +
          strings.SummaryText_P4_Format_SVMR + "{5}" + NL +
          strings.SummaryText_P3_Format_HRV + "{4}" + NL
          ;
      }
    }
  }
}
