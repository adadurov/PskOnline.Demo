using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Math.Psa.Ppg
{

  /// <summary>
  /// ќжидаемое направление пересечени€ нулевого уровн€
  /// </summary>
  public enum ZeroCrossingDirection
  {
    /// <summary>
    /// —низу вверх
    /// </summary>
    Up,

    /// <summary>
    /// —верху вниз
    /// </summary>
    Down
  }


  /// <summary>
  /// јнализирует последовательности числовых данных с целью поиска точек пересечени€ нул€.
  /// “оча пересечени€ определ€етс€ путем линейной интерпол€ции между точками, лежащими по разные стороны нул€.
  /// </summary>
  public class ZeroCrossingDetector
  {
    public ZeroCrossingDetector(ZeroCrossingDirection direction)
    {
      m_direction = direction;
    }

    private ZeroCrossingDirection m_direction;
    private int count = 0;

    private double m_lastZeroCrossingPoint = 0;

    private double previous_nonzero_value = 0;

    private int previous_nonzero_value_distance = 0;

    private double previous_value = 0;
    private bool zero_approached = false;


    internal bool CheckValuesAreOnDifferentSidesOfZero(double val1, double val2)
    {
      return ( (val1 < 0) && (val2 > 0) ) || ((val1 > 0) && (val2 < 0));
    }

    /// <summary>
    /// ¬озвращает факт наличи€ пересечени€ нул€ после анализа еще одного значени€.
    /// ƒл€ определени€ координат точки пересечени€ следует вызывать функцию GetLastZeroCrossingPoint().
    /// </summary>
    /// <param name="cur_val">Ќовое значени€ последовательности входных данных</param>
    /// <returns>¬озвращает признак обнаружени€ факта пересечени€ нул€ + </returns>
    public bool AddValue(double cur_val)
    {
      if (UpdateWithNewPoint(cur_val))
      {
        return true;
      }
      else
      {
        // — добавлением новой точки ушли на 1 от предыдущей точки пересечени€ нул€
        this.m_lastZeroCrossingPoint -= 1.0;
        return false;
      }

    }

    private bool UpdateWithNewPoint(double cur_val)
    {
      try
      {
        ++count;
        if (count > 1)
        {

          // сразу перескочили "0"?
          if (CheckValuesAreOnDifferentSidesOfZero(previous_value, cur_val))
          {
            previous_nonzero_value = previous_value;
            zero_approached = true;
            previous_nonzero_value_distance = -1;
          }

          // приблизились к "0" раньше?
          if (zero_approached)
          {
            if (CheckValuesAreOnDifferentSidesOfZero(previous_nonzero_value, cur_val))
            {
              // да, есть пересечение
              // расчитать положение...

              double y1 = (double)previous_nonzero_value;
              double y2 = (double)cur_val;

              double x1 = (double)this.previous_nonzero_value_distance;
              double x2 = (double)0;

              System.Diagnostics.Debug.Assert(y2 != y1);

              this.m_lastZeroCrossingPoint = x1 - y1 * (x2 - x1) / (y2 - y1);


              // нужно пон€ть, вверх, или вниз случилось пересечение
              // и, в зависимости от совпадени€ направлени€ с интересующим нас
              // вернуть true или false.

              zero_approached = false;

              if ((this.previous_nonzero_value > 0) && (this.m_direction == ZeroCrossingDirection.Down))
              {
                // ѕересечение нул€ произошло сверху вниз, что нам и требуетс€
                return true;
              }
              if ((this.previous_nonzero_value < 0) && (this.m_direction == ZeroCrossingDirection.Up))
              {
                // ѕересечение нул€ произошло снизу вверх, что нам и требуетс€
                return true;
              }

              return false;
            }
            else if (cur_val == 0)
            {
              // остаемс€ в "0"
              --this.previous_nonzero_value_distance;
              return false;
            }
            else
            {
              // ушли от "0" в ту же сторону, откуда пришли
              // сбросить флаг приближени€ к "0"
              zero_approached = false;
              return false;
            }

          }

          if ((cur_val == 0) && (this.previous_value != 0))
          {
            // zero approached
            this.zero_approached = true;
            this.previous_nonzero_value = previous_value;
            this.previous_nonzero_value_distance = -1;
          }


        }
      }
      finally
      {
        previous_value = cur_val;
      }
      return false;
    }

    /// <summary>
    /// ¬озвращает рассто€ние от последнего добавленного отсчета до точки пересечени€ нул€.
    /// </summary>
    /// <returns></returns>
    public double GetLastZeroCrossingPoint()
    {
      return m_lastZeroCrossingPoint;
    }


    public bool TestSeries(double[] series)
    {
      bool bZeroCrossingDetected = false;
      for (int i = 0; i < series.Length; ++i)
      {
        bZeroCrossingDetected |= this.AddValue(series[i]);
      }
      return bZeroCrossingDetected;
    }

  }
}
