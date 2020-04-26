namespace PskOnline.Methods.ObjectModel.PhysioData
{
  public class ChannelMark
  {
    public string ChannelId { get; set; }

    /// <summary>
    /// Сколько времени (в микросекундах) прошло с момента,
    /// когда в буфере канала стало Count отсчетов.
    /// </summary>
    public long Offset { get; set; }

    /// <summary>
    /// Сколько данных было в буфере канала в момент создания метки.
    /// </summary>
    public long Count { get; set; }

    public ChannelMark()
    {
    }

    public ChannelMark(ChannelMark src)
    {
      Offset = src.Offset;
      Count = src.Count;
      ChannelId = src.ChannelId;
    }
  }
}
