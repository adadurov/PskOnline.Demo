using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Math.Psa.Ppg
{
  /// <summary>
  /// helper class for post-processing of PPG signal
  /// (allows to process all the data at once and obtain
  /// an array of heart contractions coordinates)
  /// </summary>
  public static class PpgPulseDetectorHelper
  {
    public static double[] ProcessPpgData(IPpgPulseDetector detector, double SamplingRate, int[] data_buf, int BitsPerSample)
    {
      return new PpgPulseDetectorHelperInternal().ProcessData(detector, SamplingRate, data_buf, BitsPerSample);
    }

    class PpgPulseDetectorHelperInternal
    {
      private List<double> m_Contractions = new List<double>(100);

      public double[] ProcessData(IPpgPulseDetector detector, double SamplingRate, int[] data, int BitsPerSample)
      {
        detector.HeartContractionDetected += new HeartContractionDetectedDelegate(detector_HeartContractionDetected);
        // генерируем фиктивную отметку времени
        long timestamp = (long)((((double)data.Length) / SamplingRate) * 1000000);

        detector.AddData(data, timestamp);
        return m_Contractions.ToArray();
      }

      void detector_HeartContractionDetected(double sample_count, long timestamp)
      {
        m_Contractions.Add(sample_count);
      }
    }

  }
}
