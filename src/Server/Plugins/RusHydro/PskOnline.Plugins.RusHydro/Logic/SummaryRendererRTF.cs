namespace PskOnline.Server.Plugins.RusHydro.Logic
{
  using System;
  using HooverUnlimited.DotNetRtfWriter;

  using PskOnline.Server.Plugins.RusHydro.ObjectModel;

  /// <summary>
  /// Формирует содержимое сводки по результатам предсменного контроля в формате RTF.
  /// </summary>
  public class SummaryRendererRtf : ISummaryRenderer
  {
    private log4net.ILog _log = log4net.LogManager.GetLogger(typeof(SummaryRendererRtf));

    public SummaryRendererRtf()
    {
    }

    public string FilenameExtension { get { return "rtf"; } }

    public void RenderSummary(SummaryDocument summary, ISummaryWriter writer, string baseFolderName, out string actualFileName)
    {
      try
      {
        string content = RenderSummaryInternal(summary);

        var writingParams = new SummaryWritingParameters();
        writingParams.baseSummaryFolderPath = baseFolderName;
        writingParams.completionTime = summary.CompletionTime;
        writingParams.filenameExtension = FilenameExtension;
        writingParams.hostName = summary.HostName;
        writingParams.employee = summary.Employee;

        writer.SaveSummary(writingParams, content, out actualFileName);
      }
      catch( Exception ex )
      {
        _log.Error(ex);
        _log.Error("Cannot save summary in RTF format. See exception information above. Falling back to TXT format.");

        // cannot save summary in RTF format for some reason
        // fall back to TXT format
        // if cannot save in plain text format, show message box
        // and let user copy the summary to clipboard
        ISummaryRenderer plainTextRenderer = null;

        try
        {
          plainTextRenderer = new SummaryRendererPlainText();
          plainTextRenderer.RenderSummary(summary, writer, baseFolderName, out actualFileName);
          _log.InfoFormat("Summary successfully written in TXT format to {0}", actualFileName);
          return;
        }
        catch (Exception ex2)
        {
          _log.Error("Cannot save summary in TXT format.");
          _log.Error(ex2);
        }

        throw new Exception("Cannot save summary in neither TXT, nor RTF format!");
      }
    }

    public string RenderSummaryInternal(SummaryDocument summary)
    {
      // for how-to see lib\dotnetrtfwriter\demo\program.cs & \demo_psa_summary_rtf\program.cs

      // Create document by specifying paper size and orientation, and default language.
      RtfDocument doc = new RtfDocument(PaperSize.A4, PaperOrientation.Portrait, Lcid.TraditionalChinese);
      // Create fonts and colors for later use
      FontDescriptor courier = doc.CreateFont("Courier New");

      // Don't instantiate RtfTable, RtfParagraph, and RtfImage objects by using
      // ``new'' keyword. Instead, use add* method in objects derived from 
      // RtfBlockList class. (See Demos.)
      RtfParagraph svmrParagraph, hrvParagraph, finalParagraph;
      // Don't instantiate RtfCharFormat by using ``new'' keyword, either. 
      // An addCharFormat method are provided by RtfParagraph objects.
      RtfCharFormat fmt;

      AddDefParagraph(doc, courier, courier).SetText(strings.SummaryText_P0_Format_Name + summary.Employee.ToString());
      AddDefParagraph(doc, courier, courier).SetText(strings.SummaryText_P1_Format_Date + summary.CompletionTime.ToString(strings.SummaryText_DateFormat));
      AddDefParagraph(doc, courier, courier).SetText(strings.SummaryText_P2_Format_Workstation + summary.HostName);
      AddDefParagraph(doc, courier, courier).SetText(strings.SummaryText_Separator);

      var hrvHeader = strings.SummaryText_P3_Format_HRV + $"{summary.HrvConclusion.TestId,8}" + Formatter.DefaultSpacing;
      var hrvStatus = summary.HrvConclusion.Text;
      var svmrHeader = strings.SummaryText_P4_Format_SVMR + $"{summary.SvmrConclusion.TestId,8}" + Formatter.DefaultSpacing;
      var svmrStatus = summary.SvmrConclusion.Text;

      AddDefParagraph(doc, courier, courier, out hrvParagraph).SetText(hrvHeader + hrvStatus);
      AddDefParagraph(doc, courier, courier, out svmrParagraph).SetText(svmrHeader + svmrStatus);

      AddDefParagraph(doc, courier, courier).SetText(" ");
      AddDefParagraph(doc, courier, courier, out finalParagraph).SetText(summary.FinalConclusion.Text);

      _log.Debug("HRV paragraph:  " + hrvParagraph.Text);
      _log.Debug("SVMR paragraph: " + svmrParagraph.Text);
      _log.Debug($"hrv_header.Length =  {hrvHeader.Length,4} ; hrv_status.Length =  {hrvStatus.Length,4}");
      _log.Debug($"svmr_header.Length = {svmrHeader.Length,4} ; svmr_status.Length = {svmrStatus.Length,4}");

      // format HRV conclusion
      fmt = hrvParagraph.AddCharFormat(hrvHeader.Length, hrvHeader.Length + hrvStatus.Length - 1);
      var hrvStatusColor = PsaStatusColorProvider.StatusColor(summary.HrvConclusion.Status);
      fmt.FgColor = doc.CreateColor(hrvStatusColor);
      //      fmt.FontStyle.addStyle(FontStyleFlag.Bold);

      // format SVMR conclusion
      fmt = svmrParagraph.AddCharFormat(svmrHeader.Length, svmrHeader.Length + svmrStatus.Length - 1);
      var svmrStatusColor = PsaStatusColorProvider.StatusColor(summary.SvmrConclusion.Status);
      fmt.FgColor = doc.CreateColor(svmrStatusColor);
      //      fmt.FontStyle.addStyle(FontStyleFlag.Bold);

      // format final conclusion
      fmt = finalParagraph.AddCharFormat(0, summary.FinalConclusion.Text.Length - 1);
      var finalStatusColor = PsaStatusColorProvider.StatusColor(summary.FinalConclusion.Status);

      fmt.FgColor = doc.CreateColor(finalStatusColor);

      return doc.Render();
    }

    private static RtfParagraph AddDefParagraph(RtfDocument doc, FontDescriptor font, FontDescriptor ansiFont, out RtfParagraph paragraph)
    {
      paragraph = doc.AddParagraph();
      paragraph.DefaultCharFormat.Font = font;
      paragraph.DefaultCharFormat.AnsiFont = ansiFont;
      return paragraph;
    }

    private static RtfParagraph AddDefParagraph(RtfDocument doc, FontDescriptor font, FontDescriptor ansiFont)
    {
      RtfParagraph par;
      AddDefParagraph(doc, font, ansiFont, out par);
      return par;
    }

  }
}
