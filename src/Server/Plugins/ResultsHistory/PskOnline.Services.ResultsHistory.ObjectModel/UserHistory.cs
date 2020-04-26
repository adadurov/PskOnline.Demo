namespace PskOnline.Server.Plugins.ResultsHistory.ObjectModel
{
  using System;

  /// <summary>
  /// A document describing a per-user history of inspections
  /// </summary>
  public class UserHistory
  {
    /// <summary>
    /// Gets or sets a user ID that the history document belongs to
    /// </summary>
    public Guid UserId { get; set; }

    public DateTimeOffset StartDate { get; set; }

    public DateTimeOffset EndDate { get; set; }

    /// <summary>
    /// Gets or sets the array containing per-method history items
    /// </summary>
    public MethodHistory[] MethodHistory { get; set; }
  }
}
