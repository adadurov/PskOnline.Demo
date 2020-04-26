namespace PskOnline.Methods.Hrv.Processing.Logic.Pulse
{
  public static class PulseDetectorFactory
  {
    static log4net.ILog log = log4net.LogManager.GetLogger(typeof(PulseDetectorFactory));

    public static Math.Psa.Ppg.IPpgPulseDetector GetPpgPulseDetector(double SamplingRate, int BitsPerSample, string deviceTypeName)
    {
      return GetPpgPulseDetector(SamplingRate, BitsPerSample, deviceTypeName, true);
    }

    public static Math.Psa.Ppg.IPpgPulseDetector GetPpgPulseDetector(double SamplingRate, int BitsPerSample, string deviceTypeName, bool bEnableDebugMonitor)
    {
      log.Debug("Creating PPG pulse detector by derivatives");
      return new Math.Psa.Ppg.PpgPulseDetectorByDerivative(SamplingRate, BitsPerSample, bEnableDebugMonitor);

      //if( typeof(device.hardware.rb_18.Device).FullName == deviceTypeName)
      //{
      //  log.Debug("Created PPG pulse detector by derivatives");
      //  return new math.psa.ppg.PpgPulseDetectorByDerivative(SamplingRate, BitsPerSample, bEnableDebugMonitor);
      //}
      //else if (typeof(device.hardware.rb_16.Device).FullName == deviceTypeName)
      //{
      //  log.Debug("Created old PPG pulse detector by fronts");
      //  return new math.psa.ppg.PpgPulseDetector(SamplingRate, BitsPerSample);
      //}

      //log.Debug("Created old PPG pulse detector by fronts");
      //return new math.psa.ppg.PpgPulseDetector(SamplingRate, BitsPerSample);
    }

  }
}
