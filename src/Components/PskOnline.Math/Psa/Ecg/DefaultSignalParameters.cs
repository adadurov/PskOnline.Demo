using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Math.Psa.Ecg
{

  /// <summary>
  /// Default signal parameters for data processor setup.
  /// </summary>
  public class DefaultSignalParameters
  {
    /// <summary>
    /// Minimum cardio-cycle duration.
    /// Average duration, specified in seconds, of single
    /// cardio-cycle when examinee has heart rate of MaxHeartRate ppm.
    /// </summary>
    public static readonly double MinCardioCyclePeriodInSeconds = 60.0 / MaxHeartRate;

    /// <summary>
    /// Maximumj cardio-cycle duration.
    /// Average duration, specified in seconds, of single
    /// cardio-cycle when examinee has heart rate of MinHeartRate ppm.
    /// </summary>
    public static readonly double MaxCardioCyclePeriodInSeconds = 60.0 / MinHeartRate;

    /// <summary>
    /// Maximum peak width, relative to cardio-cycle period.
    /// </summary>
    public static readonly double MaxRelativePeakWidth = 0.5;

    /// <summary>
    /// Maximum heart rate (ppm (pulses per minute)).
    /// </summary>
    public static readonly double MaxHeartRate = 160;

    /// <summary>
    /// Минимальная ЧСС (ударов в минуту)
    /// </summary>
    public static readonly double MinHeartRate = 30;
  }
}
