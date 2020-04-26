namespace PskOnline.Methods.Hrv.Processing.Logic
{
  using MathNet.Numerics.Statistics;
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public class MinMaxModeDescriptor
  {
    public HistogramDescriptor MaxModeDescriptor { get; set; }

    public HistogramDescriptor MidModeDescriptor { get; set; }

    public HistogramDescriptor MinModeDescriptor { get; set; }
  }

  public class HistogramDescriptor
  {
    public double Offset { get; set; }

    public double ModeAmp { get; set; }

    public double Mode { get; set; }

    public Histogram Historgram { get; set; }
  }

  /// <summary>
  /// Finds the 2 sets of parameters for building the Bayevsky histogram,
  /// that maximize and minimize the mode amplitude, respectively.
  /// </summary>
  public class MinMaxModeAmpFinder
  {
    /// <summary>
    /// finds a histogram with the maximum AMo
    /// using a brute force method -- tests all histograms that may be built
    /// by moving the lower and the upper bounds up by 1 ms steps
    /// </summary>
    /// <returns></returns>
    public static MinMaxModeDescriptor GetPeakAmoDistrib(double[] validIntervals, double bucketStep, double lowerBound, double upperBound, double searchStep)
    {
      var lower = lowerBound;
      var upper = upperBound;
      var nBuckets = (int)Math.Round((upper - lower) / bucketStep);
      var step = searchStep;
      var count = (double)validIntervals.Length;

      var optSteps = (int)((upper - lower) / nBuckets / step);
      var histograms = new List<Histogram>(optSteps);

      for (int i = 0; i < optSteps; ++i)
      {
        histograms.Add(new Histogram(validIntervals, nBuckets, lower + i * step, upper + i * step));
      }

      // find mode value for each histogram
      var allModes = new List<HistogramDescriptor>();
      foreach (var h in histograms)
      {
        var max = double.MinValue;
        var modeMax = 0.0d;

        for (int b = 0; b < nBuckets; ++b)
        {
          if (h[b].Count > max)
          {
            max = h[b].Count;
            modeMax = h.LowerBound + (bucketStep + 0.5) * (double)b;
          }
        }
        allModes.Add(new HistogramDescriptor {
          Historgram = h,
          Mode = modeMax,
          ModeAmp = max / count, // нормируем
          Offset = h.LowerBound - lower
        });
      }

      var maxDistroDesc = allModes.Aggregate((d1,d2 ) => d1.ModeAmp > d2.ModeAmp ? d1 : d2);

      var minDistroDesc = allModes.Aggregate((d1, d2) => d1.ModeAmp < d2.ModeAmp ? d1 : d2);

      var averageAmp = 0.5 * (maxDistroDesc.ModeAmp + minDistroDesc.ModeAmp);

      // choose the distribution that has the mode closest to the 'average mode'
      var midDistroDesc = allModes.Aggregate((d1, d2) => {
        var isFirstLess = Math.Abs(d1.ModeAmp - averageAmp) < Math.Abs(d2.ModeAmp - averageAmp);
        return isFirstLess ? d1 : d2;
        });

      return new MinMaxModeDescriptor
      {
        MaxModeDescriptor = maxDistroDesc,
        MinModeDescriptor = minDistroDesc,
        MidModeDescriptor = midDistroDesc
      };
    }
  }
}
