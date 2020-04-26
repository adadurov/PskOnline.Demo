using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Methods.Hrv.ObjectModel
{
  /// <summary>
  /// —пектр
  /// </summary>
  public class Spectrum
  {
    /// <summary>
    /// ќтсчеты спектра
    /// </summary>
    public List<double> SpectrumBins = new List<double>(0);

    /// <summary>
    /// –азрешение по частоте
    /// </summary>
    public double FreqResolution = 0;

    /// <summary>
    /// нижн€€ граница спектра
    /// </summary>
    public double SpectrumLow = 0;

    /// <summary>
    /// верхн€€ граница спектра
    /// </summary>
    public double SpectrumHigh = 0;
  }
}
