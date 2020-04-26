namespace PskOnline.Server.Service.ViewModels
{
  using System;

  public class InspectionCompleteDto
  {
    public Guid Id { get; set; }

    public DateTimeOffset FinishTime { get; set; }

  }
}
