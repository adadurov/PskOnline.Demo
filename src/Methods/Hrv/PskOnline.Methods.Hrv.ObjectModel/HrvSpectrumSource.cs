using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Methods.Hrv.ObjectModel
{
  /// <summary>
  /// Контейнер исходных данных для вычисления спектра
  /// </summary>
  public class HrvSpectrumSource
  {
    public HrvSpectrumSource()
    {
    }

    private HrvSpectrumSource(HrvSpectrumSource src)
    {
    }

    public List<double> InterpolatedData = null;
    public double InterpolatedSamplingPeriod = 0.0;
    public List<double> SourceDataSampleTimes = null;
    public List<double> SourceDataSamples = null;
  }
}
