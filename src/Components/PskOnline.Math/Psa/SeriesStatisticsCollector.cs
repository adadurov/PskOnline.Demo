//#define ULTRA_LOW_LEVEL_LOG
using System;

namespace PskOnline.Math.Psa
{
  /// <summary>
  /// Собирает и хранит статистику ряда для
  /// указанного количества последних членов ряда
  /// 
  /// При добавлени первого значения статистика приводится в следующее состояния:
  /// история значения устанавливается в первое значение, поданное на вход
  /// мат. ожидание x
  /// дисперсия 0
  /// стандартное отклонение 0
  /// </summary>
  public class SeriesStatisticsCollector : RingBuffer<double>
  {
    int length = 0;
    int current = 0;

    bool m_bIsHistoryEmpty = true;

    double[] MX = null;
    double[] DX = null;

    double m_Min = 0;
    
    double m_Max = 0;

    // сумма всех значений истории
    double sumX = 0;


    public SeriesStatisticsCollector(int length) : base(length)
    {
      if (length < 3)
      {
        throw new ArgumentException("must be greater than or equal to 3", "length");
      }
      this.length = length;
      MX = new double[length];
      DX = new double[length];
    }

    void InitHistory(double value)
    {
      if (this.m_bIsHistoryEmpty)
      {
        base.InitBuffer(value);

        for (int i = 0; i < this.length; ++i)
        {
          this.MX[i] = value;
          this.DX[i] = 0;
        }
        sumX = value * this.length;
        this.m_Min = value;
        this.m_Max = value;
        this.m_bIsHistoryEmpty = false;
      }
    }

    double dLength
    {
      get
      {
        return (double)this.length;
      }
    }

    public override void AddValue(double value)
    {
      InitHistory(value);

      // удаляем значение, соответствующее самой старой точке истории
      double oldest_value = this.GetOldestValue();
      sumX -= oldest_value;

      // добавляем к сумме текущее значение
      sumX += value;

      base.AddValue(value);

      UpdateMax(oldest_value, value);

      UpdateMin(oldest_value, value);

      // обновляем математическое ожидание ряда
      double cur_MX = sumX / dLength;

      // вычисляем дисперсию в данный момент...
      double sum_of_dev_squared = 0;
      double dev;
      for (int i = 0; i < length; ++i)
      {
        dev = (this.GetValue(i) - cur_MX);
        sum_of_dev_squared += (dev * dev);
      }
      double cur_DX = sum_of_dev_squared / dLength;

      // сохраняем новые результаты в истории
      MX[current] = cur_MX;
      DX[current] = cur_DX;

      UpdateFrontsStatistics(value);

      // увеличиваем указатель на 1
      ++current;
      current %= length;
    }

    private void UpdateMin(double oldest_value, double new_value)
    {
      if (new_value <= this.m_Min)
      {
        this.m_Min = new_value;
      }
      else
      {
        // Удаляем старый минимум -- нужно обновить
        if (oldest_value == this.m_Min)
        {
          this.m_Min = this.GetValue(0);
          double val;
          for (int i = 1; i < this.length; ++i)
          {
            val = this.GetValue(i);
            if( val < this.m_Min )
            {
              this.m_Min = val;
            }
          }
        }
      }
    }

    private void UpdateMax(double oldest_value, double new_value)
    {
      if (new_value >= this.m_Max)
      {
        this.m_Max = new_value;
      }
      else
      {
        // Старый максимум удаляем -- нужно найти новый максимум
        if (oldest_value == this.m_Max)
        {
          this.m_Max = this.GetValue(0);
          double val;
          for (int i = 1; i < this.length; ++i)
          {
            val = this.GetValue(i);
            if (val > this.m_Max)
            {
              this.m_Max = val;
            }
          }
        }
      }
    }

    private void UpdateFrontsStatistics(double value)
    {
      double previous_value = this.GetValue(1);
      if (value >= previous_value)
      {
        if (!m_bFrontCurrentlyRising)
        {
          // Инициализируем начало фронта
          m_bFrontCurrentlyRising = true;
          m_LastRisingFrontStartValue = previous_value;
          m_LastRisingFrontStartDistance = 0;
          m_LastRisingFrontEndDistance = 0;
//#if DEBUG
//          log.DebugFormat("Start of rising front detected: previous value = {0}, current_value = {1}", previous_value, value);
//#endif
        }

        // отмечаем продолжение фронта
        ++m_LastRisingFrontStartDistance;
        m_LastRisingFrontEndValue = value;

      }
      else
      {
        // падаем...
        if (m_bFrontCurrentlyRising)
        {
          // только начинаем падать, т.е. обнаружен конец фронта...
          m_bFrontCurrentlyRising = false;
          m_LastRisingFrontEndValue = previous_value;
          m_LastRisingFrontEndDistance = 0;
        }

        // удаляемся от конца и от начала фронта
        ++m_LastRisingFrontEndDistance;
        ++m_LastRisingFrontStartDistance;

      }
    }

    private bool m_bFrontCurrentlyRising = false;
    
    private double m_LastRisingFrontStartValue = 0;
    private int m_LastRisingFrontStartDistance = 0;

    private double m_LastRisingFrontEndValue = 0;
    private int m_LastRisingFrontEndDistance = 0;

    public double GetLastRisingFrontAmplitude()
    {
      return this.m_LastRisingFrontEndValue - this.m_LastRisingFrontStartValue;
    }

    public double GetLastRisingFrontDuration()
    {
      return this.m_LastRisingFrontStartDistance - this.m_LastRisingFrontEndDistance;
    }

    public double GetMin()
    {
      return this.m_Min;
    }

    public double GetMax()
    {
      return this.m_Max;
    }

    public double GetAmplitude()
    {
      return GetMax() - GetMin();
    }

    public double GetDX(int distance)
    {
      System.Diagnostics.Debug.Assert(distance > -1);
      System.Diagnostics.Debug.Assert(distance < this.length);

      int index = this.current - (distance % this.length);
      if (index < 0)
      {
        index += this.length;
      }

      try
      {
        return this.DX[index];
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.Fail(ex.Message);
        throw;
      }
    }

    public double GetMX(int distance)
    {
      System.Diagnostics.Debug.Assert(distance > -1);
      System.Diagnostics.Debug.Assert(distance < this.length);

      int index = this.current - (distance % this.length);
      if (index < 0)
      {
        index += this.length;
      }

      try
      {
        return this.MX[index];
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.Fail(ex.Message);
        throw;
      }
    }

    public void AddSequence(double[] p)
    {
      for (int i = 0; i < p.Length; ++i)
      {
        this.AddValue(p[i]);
      }
    }
  }
}
