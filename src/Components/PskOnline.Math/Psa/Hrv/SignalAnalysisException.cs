namespace PskOnline.Math.Psa.Hrv
{
  using System;

  /// <summary>
  /// Represents an exception during processing of cardio signals.
  /// </summary>
  public class SignalAnalysisException : Exception
  {
    public SignalAnalysisException(string message)
      : base(message)
    {
    }
  }
}
