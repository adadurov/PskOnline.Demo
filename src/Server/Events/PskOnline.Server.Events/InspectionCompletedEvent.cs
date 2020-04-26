namespace PskOnline.Server.Shared.Contracts.Events
{
  using PskOnline.Server.ObjectModel;

  public class InspectionCompletedEvent
  {
    public Inspection Inspection { get; set; }
  }
}
