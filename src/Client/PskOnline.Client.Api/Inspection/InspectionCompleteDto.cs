namespace PskOnline.Client.Api.Inspection
{
  using System;

  public class InspectionCompleteDto
  {
    public Guid Id { get; set; }

    public DateTimeOffset FinishTime { get; set; }

  }
}
