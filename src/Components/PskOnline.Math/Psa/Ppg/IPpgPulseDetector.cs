using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Math.Psa.Ppg
{

  public delegate void HeartContractionDetectedDelegate(double sample_count, long timestamp);

  public interface IPpgPulseDetector : IDisposable
  {
    void AddData(int[] data_buf, long timestamp);
    event HeartContractionDetectedDelegate HeartContractionDetected;
  }
}
