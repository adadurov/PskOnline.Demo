using System;

namespace PskOnline.Math.Statistics
{
  /// <summary>
  /// ������������� ����� (generic class) ��� ���������� ����������.
  /// </summary>
  public class Calculator
  {
    protected Calculator()
    {
    }

    /// <summary>
    /// ������ CalcStatistics ��� �������� ������� ���� System.Single[]
    /// </summary>
    /// <param name="series"></param>
    /// <param name="sta"></param>
    public static void CalcStatistics(float[] series, StatData sta)
    {
      Calculator.CalcStatistics(Calculator.ToDoubleArray(series), sta);
    }


    private static double[] ToDoubleArray(int[] series)
    {
      double[] dseries = new double[series.Length];
      for (int i = 0; i < series.Length; ++i)
      {
        dseries[i] = series[i];
      }
      return dseries;
    }

    private static double[] ToDoubleArray(float[] series)
    {
      double[] dseries = new double[series.Length];
      for (int i = 0; i < series.Length; ++i)
      {
        dseries[i] = series[i];
      }
      return dseries;
    }

    /// <summary>
    /// ��������� ������� ��������� �������� � ������ ��������� �������������.
    /// </summary>
    /// <param name="series"></param>
    /// <param name="sta"></param>
    /// <remarks>���� � ��������� ���� ����������� ��� ������ �-��� MakeDistribution(...)</remarks>
    public static void CalcStatistics(double[] series, StatData sta)
    {
      if (series == null)
      {
        throw new ArgumentNullException("series");
      }
      if (sta == null)
      {
        throw new ArgumentNullException("sta");
      }

      int min_len = 2;
      if (series.Length < min_len)
      {
        throw new ArgumentException(string.Format(strings.series_contains_too_few_measurements, min_len), "series");
      }

      double XX, Sigma, M, T, S1, S2, S3, S4, x, As, Ex, S, Dispersion, max, min;
      int i, n;

      // ������ ������.
      S1 = S2 = S3 = S4 = As = Ex = S = Dispersion = 0;
      sta.Count = n = series.Length;

      double buf;
      max = min = series[0];
      for (i = 0; i < n; ++i)
      {
        buf = x = series[i];

        // ������ ������ �������� � ������� ����
        if (x < min)
        {
          min = x;
        }
        else if (x > max)
        {
          max = x;
        }

        // ����, �����������...
        S1 += x;          // ����� ������ ����
        S2 += (buf *= x); // ����� ��������� ������ ����
        S3 += (buf *= x); // ����� ����� ������ ����
        S4 += (buf *= x); // ����� ��������� �������� ������ ����
      }

      M = S1 / n; // v1 -- ������ ��������� ������
      T = (S2 - S1 * M) / (double)(n - 1);

      // ������� ����� ����� ���������???
      if (T < 0)
      {
        throw new ArithmeticException(strings.cannot_calc_statistics_because_dispersion_is_negative);
      }

      Dispersion = T;
      Sigma = System.Math.Sqrt(Dispersion);
      S = Sigma;
      if (S != 0)
      {
        As = (S3 - 3.0 * M * S2 + 2.0 * n * M * M * M) / (n * S * S * S);
        Ex = -3.0 + (S4 - 4.0 * M * S3 + 6.0 * M * M * S2 - 3.0 * S1 * M * M * M) / (n * S * S * S * S);
      }
      else
      {
        As = System.Single.NaN;
        Ex = System.Single.NaN;
      }

      XX = 0;
      for (i = 0; i < n; ++i)
      {
        x = (double)series[i];
        XX += System.Math.Pow(x - M, 4);
      }
      if (S != 0)
      {
        Ex = -3 + XX / (n * System.Math.Pow(S, 4));
      }
      if (S == 0)
      {
        Ex = 0;
      }

      sta.m = M;
      sta.sigma = Sigma;
      sta.dispersion = Dispersion;

      // ����������� ��� ���������� �������������
      // ��� ������ ������� MakeDistribution
//      sta.mode = 0;
//      sta.modeAmplitude = 0;

      sta.asymmetry = As;
      sta.kurtosis = Ex;

      sta.max = max;
      sta.min = min;

      sta.variation = 100 * Sigma / M;
      sta.varRange = max - min;

    }

    /// <summary>
    /// �������� ������� ����������� � �������������� ������
    /// � ������ ����������� ������������� � ���� ��������� �����������.
    /// 
    /// ��������� ���� � ��������� ����.
    /// </summary>
    /// <param name="series">������������������ ��������� �������</param>
    /// <param name="sta">���������, ���������� ���������� ����������</param>
    /// <param name="min">���. �������� ��� ���������� �����������</param>
    /// <param name="max">����. �������� ��� ���������� �����������</param>
    /// <param name="band_width">������ ������� ���������</param>
    /// <throws>ArgumentException</throws>
    /// <throws>InvalidOperationException</throws>
    public static void MakeProbabilityDensity(double[] series, StatData sta, double min, double max, double channel_width)
    {
      if (series == null)
      {
        throw new ArgumentNullException("series");
      }
      if (sta == null)
      {
        throw new ArgumentNullException("sta");
      }
      int min_len = 1;
      if( series.Length < min_len )
      {
        throw new ArgumentException(string.Format(strings.series_contains_too_few_measurements, min_len), "series");
      }

      int density_channel_count = (int) ( (max - min) / channel_width );
      double[] probability_density = new double[density_channel_count];

      // �������������� �� 6 ������: 3 �� � 3 �����, ������� ����� �����
      int probability_channel_count = density_channel_count + 6;
      double[] probability = new double[probability_channel_count];

      double probability_min = min - 3 * channel_width;
      double probability_max = max + 3 * channel_width;

      // ������� ����������� (� �������������� ������) ����,
      // ��� ����������� ������ �� ������������������ �������� ������ X
      // (����������� �������������)
      int index;
      double cardio_duration;
      for(
          cardio_duration = probability_min, index = 0;
          cardio_duration < probability_max;
          cardio_duration += channel_width, ++index
          )
      {
        for( int i = 0; i < series.Length; ++i )
        {
          if( series[i] <= cardio_duration )
          {
            probability[index] = probability[index] + 1.0;
          }
        }
      }

      // ��������� ����������� �� "1" �������� -- ��� �������� ��������!!!
      double dSeriesLength = series.Length;
      for (int i = 0; i < probability_channel_count; ++i)
      {
        probability[i] = probability[i] / dSeriesLength;
      }

      double max_probability_density = probability_density[0];
      double max_probability_density_index = 0;

      // �������������� ����������� � �������� ��������� �����������
      for( int i = 0; i < density_channel_count; ++i )
      {
        probability_density[i] = probability[i + 3] + probability[i + 4] + probability[i + 5]
          - probability[i] - probability[i + 1] - probability[i + 2];

        if( probability_density[i] > max_probability_density )
        {
          max_probability_density = probability_density[i];
          max_probability_density_index = i;
        }
      }

      //if (false)
      //{
      //  // �����������, ��� ������� ��������� ����������� ��������� � ������������...
      //  for (int i = 0; i < density_channel_count; ++i)
      //  {
      //    probability_density[i] = probability_density[i] / max_probability_density;
      //  }
      //}

      sta.probability_density = new Distribution();

      // ��������� ����
      sta.probability_density.mode = min + channel_width * (((double)max_probability_density_index) + 0.5);
      sta.probability_density.mode_amplitude = max_probability_density;
      sta.probability_density.channel_width = channel_width;
      sta.probability_density.channel_count = density_channel_count;
      sta.probability_density.channels = probability_density;
      sta.probability_density.min = min;
      sta.probability_density.max = max;

    }

    /// <summary>
    /// ������ ����������� �������������. ��������� ���� � ��������� ����.
    /// </summary>
    /// <param name="series">������������������ ��������� �������</param>
    /// <param name="sta">���������, ���������� ���������� ����������</param>
    /// <param name="min">���. �������� ��� ���������� �����������</param>
    /// <param name="max">����. �������� ��� ���������� �����������</param>
    /// <param name="band_width">������ ������� ���������</param>
    /// <throws>ArgumentException</throws>
    /// <throws>InvalidOperationException</throws>
    public static void MakeDistribution(double[] series, StatData sta, double min, double max, double channel_width)
    {
      int band = 0;
      int measurements = 0;
      int channel_count = (int)((max - min) / channel_width + 1); // �� ������ ������ ���� ��������...

      if (channel_count <= 0)
      {
        throw new ArgumentException(strings.unable_to_build_histogram_with_given_arguments);
      }

      Distribution distr = sta.distribution;

      distr.channels = new double[channel_count];
      distr.channel_count = channel_count;
      distr.channel_width = channel_width;
      distr.min = min;
      distr.max = max;

      // �������� �������������
      for (int d = 0; d < channel_count; ++d)
      {
        distr.channels[d] = 0;
      }

      // ������������ ���-�� �������� ��� ������� ���������
      for (int i = 0; i < series.Length; ++i)
      {
        band = (int)((series[i] - min) / channel_width);
        if ((band >= 0) && (band < channel_count))
        {
          ++measurements;
          ++distr.channels[band];
        }
      }

      int mode_channel = 0;
      double histo_max_val = distr.channels[0];

      if (measurements == 0)
      {
        string message = string.Format(strings.no_measurements_fall_into_the_requested_range, min, max);
        System.Diagnostics.Debug.Fail(message);
        throw new InvalidOperationException(message);
      }

      // ���������� ����������� �� 1 � ���������� � ������� ����.
      for (int i = 0; i < channel_count; ++i)
      {
        distr.channels[i] /= ((double)(measurements));

        // ���� �������� �� �����������.
        if (distr.channels[i] > histo_max_val)
        {
          histo_max_val = distr.channels[i];
          mode_channel = i;
        }
      }

      // ���� ���������� ��� �������� ������������� ��������� � ������������ ������������.
      // ���������, � BioMouse ���� ����������� ��� ��.
      distr.mode = min + channel_width * (((double)mode_channel) + 0.5);
      distr.mode_amplitude = distr.channels[mode_channel];
    }

    public static void GetMinMax(double[] data, ref double min, ref double max)
    {
      min = max = 0;
      if (null == data || 0 == data.Length)
      {
        return;
      }
      min = data[0];
      max = data[0];
      int count = data.Length;
      for (int i = 0; i < count; i++)
      {
        if (data[i] < min) min = data[i];
        if (data[i] > max) max = data[i];
      }

    }
    #region Deprecated functions
    /// <summary>
    /// ������� ������� �������������� ����.
    /// </summary>
    /// <param name="data">���.</param>
    /// <param name="ABS">��������� �� ���� (���� true - ���� �� �����������)</param>
    /// <returns>������� �������������� ����.</returns>
    [Obsolete("This specialized function must be moved to the module where it is used!")]
    public static double GetSimpleAverage(double[] data, bool ABS)
    {
      double summ = 0;
      for( int i = 0; i < data.Length; ++i )
      {
        if (ABS)
        {
          summ += System.Math.Abs(data[i]);
        }
        else
        {
          summ += data[i];
        }
      }
      return summ / data.Length;
    }
    #endregion

    public static float GetAverageOfAbsoluteValues(float[] array)
    {
      float[] abs = new float[array.Length];
      for( int i = 0; i < array.Length; ++i )
      {
        abs[i] = System.Math.Abs(array[i]);
      }
      return (float)GetAverage(abs);
    }

    public static float GetAverage(float[] array)
    {
      return (float)Calculator.GetAverage(Calculator.ToDoubleArray(array));
    }

    public static double GetAverage(double[] array)
    {
      int n = array.Length;
      int i;
      double s = 0;

      for (i = n - 1; i >= 0; i--)
      {
        s += array[i];
      }

      return s / ((double)n);
    }

    public static double GetStandardDeviation(double[] array)
    {
      double sum1 = 0, sum2 = 0;
      int n = array.Length;

      for (int i = n - 1; i >= 0; i--)
      {
        sum1 += array[i];
        sum2 += array[i] * array[i];
      }

      sum1 /= (double)n;
      sum2 /= (double)n;

      return System.Math.Sqrt(sum2 - sum1 * sum1);
    }
  }
}
