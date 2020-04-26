namespace PskOnline.Methods.Hrv.Processing.Logic.Pulse
{
  /// <summary>
  /// helper class for post-processing of PPG signal
  /// (allows to process all the data at once and obtain
  /// an array of heart contractions coordinates)
  /// </summary>
  public static class PpgPulseDetectorHelper
  {
    public static double[] ProcessPpgData(double SamplingRate, int[] data_buf, int BitsPerSample, string deviceTypeName)
    {
      var detector = PulseDetectorFactory.GetPpgPulseDetector(SamplingRate, BitsPerSample, deviceTypeName);

      return Math.Psa.Ppg.PpgPulseDetectorHelper.ProcessPpgData(detector, SamplingRate, data_buf, BitsPerSample);
    }

  }
}
