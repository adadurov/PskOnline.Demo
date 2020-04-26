namespace PskOnline.Client.Api.Inspection
{
  public class PluginResultDescriptorDto
  {
    /// <summary>
    /// Gets or sets a unique mnemonic ID that identifies the plugin
    /// that generated the summary or conclusion.
    /// </summary>
    public string PluginType { get; set; }

    /// <summary>
    /// Contains a URL that may be used to fetch the results.
    /// The URL is relative to the API root.
    /// </summary>
    public string ResultsUrl { get; set; }
  }
}
