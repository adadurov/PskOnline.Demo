namespace PskOnline.Server.Service.ViewModels
{
  public class StatusDto
  {
    public string ServerName { get; set; }

    public string BuildNumber { get; set; }

    public string Uptime { get; set; }

    public string AppState { get; set; }

    public long Errors { get; set; }

    public bool IsStarted { get; set; }

    public SubSystemStatusDto[] SubSystems { get; set; }
  }
}
