namespace PskOnline.Methods.ObjectModel.PhysioData
{
  using System;
  using System.Collections.Generic;

  /// <summary>
  /// Данные одного физиологического канала.
  /// </summary>
  public class ChannelData
  {
    /// <summary>
    /// Уникальное имя канала во всем наборе данных,
    /// в который входит данный канал
    /// </summary>
    public string ChannelId { get; set; }

    /// <summary>
    /// Имя типа C#, соответствующего типу аппаратного устройства, с которого записан данный сигнал.
    /// </summary>
    public string DeviceTypeName { get; set; }

    public SignalType PhysioSignalType { get; set; }

    public double SamplingRate { get; set; }

    public double BitsPerSample { get; set; }

    /// <summary>
    /// Содержит отсчёты сигнала
    /// </summary>
    public int[] Data { get; set; }

    /// <summary>
    /// Все метки времени для этого канала.
    /// (timestamp : long) -> (data_count : long)
    /// </summary>
    public Dictionary<long, long> Timestamps = new Dictionary<long, long>();

    /// <summary>
    /// Конструктор без параметров нужен для сериализации.
    /// </summary>
    public ChannelData()
    {
    }

    /// <summary>
    /// Конструктор копий, позволяющий вырезать фрагмент данных
    /// </summary>
    /// <param name="src"></param>
    /// <param name="leftIndex"></param>
    /// <param name="rightIndex"></param>
    public ChannelData(ChannelData src, int leftIndex, int rightIndex)
    {
      lock (src)
      {
        if (leftIndex > rightIndex)
        {
          throw new ArgumentException(
            $"{nameof(leftIndex)}={leftIndex} must be less than or equal to {nameof(rightIndex)}={rightIndex}");
        }

        BitsPerSample = src.BitsPerSample;
        ChannelId = src.ChannelId;
        SamplingRate = src.SamplingRate;
        DeviceTypeName = src.DeviceTypeName;
        PhysioSignalType = src.PhysioSignalType;

        int newCount = rightIndex - leftIndex + 1;
        Data = new int[newCount];

        Array.Copy(src.Data, leftIndex, Data, 0, newCount);

        // Не добавляем метки времени, не попавшие
        // между leftIndex и rightIndex включительно
        Timestamps = new Dictionary<long, long>();
        if(src.Timestamps != null)
        {
          foreach(long timestamp in src.Timestamps.Keys)
          {
            long count = src.Timestamps[timestamp];
            if ((leftIndex < count) && (count <= (rightIndex + 1)))
            {
              Timestamps[timestamp] = (long)(count - leftIndex);
            }
          }
        }
      }
    }

    public ChannelData(ChannelData src)
    {
      BitsPerSample = src.BitsPerSample;
      ChannelId = src.ChannelId;
      DeviceTypeName = src.DeviceTypeName;
      SamplingRate = src.SamplingRate;
      PhysioSignalType = src.PhysioSignalType;

      Data = new int[src.Data.Length];
      Array.Copy(src.Data, Data, src.Data.Length);

      Timestamps = new Dictionary<long, long>(src.Timestamps);
    }

    public ChannelData(string channelName, string deviceTypeName, double samplingRate, double bitsPerSample, SignalType physioSignalType)
    {
      ChannelId = channelName ?? throw new ArgumentNullException(nameof(channelName));
      PhysioSignalType = physioSignalType;
      DeviceTypeName = deviceTypeName;
      SamplingRate = samplingRate;
      BitsPerSample = bitsPerSample;
      Timestamps = new Dictionary<long, long>();
    }
  }
}
