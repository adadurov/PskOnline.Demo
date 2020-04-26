namespace PskOnline.Server.Plugins.RusHydro.Logic
{
  using System;
  using PskOnline.Server.Plugins.RusHydro.ObjectModel;

  public class SummaryWritingParameters
  {
    public string baseSummaryFolderPath;

    public Employee employee;

    public DateTimeOffset completionTime;

    public string hostName;

    public string filenameExtension;
  }
}
