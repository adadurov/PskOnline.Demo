namespace PskOnline.Methods.Hrv.ObjectModel
{
  using PskOnline.Methods.ObjectModel.Test;

  public class HrvRawData : TestRawData
  {
    /// <summary>
    /// Use -1 for 'no data' (default value)
    /// </summary>
    public long PhSyncBegin { get; set; } = -1;

    /// <summary>
    /// Use -1 for 'no data' (default value)
    /// </summary>
    public long PhSyncEnd { get; set; } = -1;

    /// <summary>
    /// The target number of intervals for recording
    /// </summary>
    public int TargetIntervals { get; set; }

    /// <summary>
    /// The number of reserved hidden intervals, that are recorded
    /// in order to have some extra data to exclude outliers etc.
    /// </summary>
    public int ReservedHiddenIntervals { get; set; }
  }
}
