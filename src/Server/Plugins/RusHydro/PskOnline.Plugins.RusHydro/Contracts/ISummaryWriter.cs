namespace PskOnline.Server.Plugins.RusHydro.Logic
{
  public interface ISummaryWriter
  {
    bool SaveSummary(
      SummaryWritingParameters parameters,
      string summaryContent,
      out string usedSummaryFileName
      );
  }
}
