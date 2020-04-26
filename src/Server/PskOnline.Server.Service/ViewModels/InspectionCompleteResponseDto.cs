namespace PskOnline.Server.Service.ViewModels
{
  public class InspectionCompleteResponseDto
  {
    /// <summary>
    /// this attribute contains a collection, where each item
    /// describes a conclusion prepared by each plugin which
    /// was able to handle the inspection data (if any)
    /// </summary>
    public PluginResultDescriptorDto[] PluginResults { get; set; }
  }
}
