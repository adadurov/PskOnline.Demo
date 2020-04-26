namespace PskOnline.Server.Service.ViewModels
{
  public class PluginResultDescriptorDto
  {
    /// <summary>
    /// A mnemonic plugin ID (e.g. pskonline-demo)
    /// </summary>
    public string PluginType { get; set; }

    /// <summary>
    /// A URL that may be used to fetch the results
    /// </summary>
    public string ResultsUrl { get; set; }
  }
}
