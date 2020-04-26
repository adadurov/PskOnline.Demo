namespace PskOnline.Client.Api.Inspection
{
  public class InspectionCompleteResponseDto
  {
    /// <summary>
    /// An array containing the information about the conclusions (summaries),
    /// prepared by summary generating plugins (if available).
    /// Notice that the specific plugins are configured on a per-tenant basis
    /// </summary>
    public PluginResultDescriptorDto[] PluginResults { get; set; }
  }
}
