namespace PskOnline.Server.Plugins.ResultsHistory.ObjectModel
{
  using System;

  public class SessionResult
  {
    public DateTimeOffset SessionEndTime { get; set; }

    /// <summary>
    /// Gets or sets the array of attribute values for the session,
    /// according to the schema (which is known to the owner of the DTO)
    /// </summary>
    public AttributeValue[] Values;
  }
}
