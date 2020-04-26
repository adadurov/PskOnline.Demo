namespace PskOnline.Methods.ObjectModel.PhysioData
{
  using System.Linq;

  /// <summary>
  /// Маркер для синхронизации данных.
  /// </summary>
  public class Marker
  {
    /// <summary>
    /// идентификатор маркера в рамках пакета физиологических данных
    /// </summary>
    public long Id;

    /// <summary>
    /// Время создания маркера в микросекундах (от момента запуска измерения).
    /// </summary>
    public long TimestampUsec;

    /// <summary>
    /// ChannelData.ChannelId -> PhysioData.Marker.ChannelMark
    /// </summary>
    public ChannelMark[] ChannelMarks { get; set; }

    /// <summary>
    /// Конструктор без параметров нужен для сериализации.
    /// </summary>
    public Marker()
    {
    }

    public Marker(long id)
    {
      Id = id;
    }

    public Marker(Marker src)
    {
      Id = src.Id;
      TimestampUsec = src.TimestampUsec;
      ChannelMarks = src.ChannelMarks.Select(cm => new ChannelMark(cm)).ToArray();
    }

  }
}
