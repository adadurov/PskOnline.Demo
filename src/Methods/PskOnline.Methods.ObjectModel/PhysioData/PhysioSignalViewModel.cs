namespace PskOnline.Methods.ObjectModel.PhysioData
{
  /// <summary>
  /// View of a single signal curve
  /// </summary>
  public class PhysioSignalView
  {
    public PhysioSignalView(float[] data, int bitsPerSample, float samplingRate, SignalType physioSignalType)
    {
      BitsPerSample = bitsPerSample;
      SamplingRate = samplingRate;
      SignalType = physioSignalType;
      Data = data;

      if ( Data.Length > 0 )
      {
        var min = data[0];
        var max = data[0];
        for (int i = 1; i < data.Length; ++i)
        {
          if (data[i] < min)
          {
            min = data[i];
          }
          else if (data[i] > max)
          {
            max = data[i];
          }
        }
        Min = min;
        Max = max;
      }
    }

    public int BitsPerSample { get; set; }

    public SignalType SignalType { get; set; }

    public string SignalTypeName { get; set; }

    public string SensorModel { get; set; }

    public string SensorFirmwareVersion { get; set; }

    public string SensorId { get; set; }

    public string SensorLocation { get; set; }

    public float[] Data { get; set; }

    public float Min { get; set; }

    public float Max { get; set; }

    public float SamplingRate { get; set; }

  }
}
