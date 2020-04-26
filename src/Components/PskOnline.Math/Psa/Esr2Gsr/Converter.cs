namespace PskOnline.Math.Psa.Esr2Gsr
{
  using System;

  public class Converter
  {

    #region interface

    /// <summary>
    /// freq - частота дискретизации в Гц;
    /// averaging_time - время "усреднения" в секундах, чем оно больше, тем более гладкая получается функция.
    /// Рекомендуемое значение - 0.8 с.
    /// </summary>
    /// <param name="freq"></param>
    /// <param name="averaging_time"></param>
    public Converter(double freq, double averaging_time)
    {
      Freq = freq;

      int n = (int)(averaging_time * freq);
      if (n < 1)
      {
        throw new ArgumentException(strings.averaging_time_must_be_greater_than_single_sampling_period);
      }

      this.Filter = new LinearFitFilter(n);

      is_first = true;
      Range = 0;
      SetRangeDetectionRelaxation(30.0f);
      SetNegativeSuppressionFactor(20.0f);
    }

    /// <summary>
    /// Значение сигнала, принимаемое за максимальное, уменьшается за время t в 2 раза, если уровень сигнала меньше этого значения.
    /// </summary>
    /// <param name="t"></param>
    public void SetRangeDetectionRelaxation(double t)
    {
      RangeRelaxation = Math.Pow(0.5, 1.0 / (Freq * t));
    }

    /// <summary>
    /// Коэффициент подавления "обратного выброса", по умолчанию 20.0.
    /// (Т.е. гарантируется, что отрицательные значения по абс. величине будут в 20 раз меньше максимальных).
    /// </summary>
    /// <param name="k"></param>
    public void SetNegativeSuppressionFactor(double k)
    {
      NegativeSuppressionFactor = k;
    }

    /// <summary>
    /// Получить время задержки фильтра в секундах. Оно примерно равно averaging_time/2.
    /// </summary>
    /// <returns></returns>
    double GetLatency()
    {
      return Filter.GetLatency() / Freq;
    }

    /// <summary>
    /// Добавить точку.
    /// </summary>
    /// <param name="x"></param>
    void AddPoint(double x)
    {
      if (is_first)
      {
        is_first = false;
        Filter.Reset(x);
      }
      Filter.AddPoint(x);
    }

    /// <summary>
    /// Получить отфильтрованное значения.
    /// </summary>
    /// <returns></returns>
    double GetPoint()
    {
      double v = -Filter.GetSlope();
      AdjustRange(v);
      return Trim(v);
    }

    /// <summary>
    /// Вызывает для каждого элемента массива AddPoint()
    /// и заменяет значение элемента значением, возвращаемым
    /// функцией GetPoint()
    /// </summary>
    /// <param name="buffer"></param>
    public void FilterDestructive(double[] buffer)
    {
      for (int i = 0; i < buffer.Length; ++i)
      {
        this.AddPoint(buffer[i]);
        buffer[i] = this.GetPoint();
      }
    }
    #endregion

    #region implementation

    double Freq;
		double Range;
		double RangeRelaxation;
    double NegativeSuppressionFactor;

		LinearFitFilter Filter;

		bool is_first;

    void AdjustRange(double x)
    {
      Range *= RangeRelaxation;
      x = Math.Abs(x);
      if (x > Range)
      {
        Range = x;
      }
    }

    double Trim(double x)
    {
      if (x >= 0)
      {
        return x;
      }

      double a = Range / this.NegativeSuppressionFactor;
      return -a * (1 - a / (-x + a));
    }

#endregion
  }

  class LinearFitFilter
  {
    #region private
    double[] buffer;
    double m_sxy;
    double m_sy;
    int p;
    int n;
    #endregion

    #region	public
    public LinearFitFilter(int n)
    {
      this.n = n;
      buffer = new double[n];
      p = 0;

      double sx = n * (n - 1) / 2;
      double sxx = n * (2 * n - 1) * (n - 1) / 6.0f;

      m_sxy = n / (n * sxx - sx * sx);
      m_sy = -sx / (n * sxx - sx * sx);

      Reset(0.0f);
    }

    public void Reset(double v)
    {
      for (int i = 0; i < n; i++)
      {
        buffer[i] = v;
      }
    }

    public void AddPoint(double y)
    {
      p++;
      if( p >= n )
      {
        p = 0;
      }

      buffer[p] = y;
    }

    public double GetSlope()
    {
      double sxy = 0, sy = 0;

      int i = p;
      for (int j = 0; j < n; j++)
      {
        i++;
        if (i >= n)
          i = 0;

        sxy += buffer[i] * j;
        sy += buffer[i];

      }
      return m_sxy * sxy + m_sy * sy;
    }

    public double GetLatency()
    {
      return n * 0.5f;
    }

    #endregion
  }
}
