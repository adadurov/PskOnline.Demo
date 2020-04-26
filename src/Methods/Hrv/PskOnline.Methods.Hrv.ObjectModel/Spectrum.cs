using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Methods.Hrv.ObjectModel
{
  /// <summary>
  /// ������
  /// </summary>
  public class Spectrum
  {
    /// <summary>
    /// ������� �������
    /// </summary>
    public List<double> SpectrumBins = new List<double>(0);

    /// <summary>
    /// ���������� �� �������
    /// </summary>
    public double FreqResolution = 0;

    /// <summary>
    /// ������ ������� �������
    /// </summary>
    public double SpectrumLow = 0;

    /// <summary>
    /// ������� ������� �������
    /// </summary>
    public double SpectrumHigh = 0;
  }
}
