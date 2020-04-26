namespace PskOnline.Server.Plugins.RusHydro.Logic
{
  using PskOnline.Server.Plugins.RusHydro.ObjectModel;

  public interface ISummaryRenderer
  {
    string FilenameExtension { get; }

    /// <summary>
    /// any renderer shall use fallback paths if
    /// unable to write to the baseSummaryFolderPath,
    /// as defined by SummaryWriter
    /// </summary>
    /// <param name="summary"></param>
    /// <param name="baseSummaryFolderPath"></param>
    /// <param name="actualFileName"></param>
    void RenderSummary(SummaryDocument summary, 
                       ISummaryWriter writer,
                       string baseSummaryFolderPath,
                       out string actualFileName);
  }

}
