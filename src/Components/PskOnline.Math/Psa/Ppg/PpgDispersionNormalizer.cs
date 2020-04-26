using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Math.Psa.Ppg
{

  /// <summary>
  /// Нормализует дисперсию сигнала ФПГ за последние 2,2 секунды
  /// 
  /// Алгоритм предложен и впервые реализован А.П.Кирилловым
  /// </summary>
  public class PpgDispersionNormalizer
  {
    bool m_bIsHistoryEmpty = true;

    /// <summary>
    /// длительность истории
    /// </summary>
    int MB;

    /// <summary>
    /// история входных данных
    /// </summary>
    long[] A; 
    
    /// <summary>
    /// история средних (мат. ожиданий)
    /// </summary>
    long[] AM;

    /// <summary>
    /// история отклонений
    /// </summary>
    long[] DD;

    /// <summary>
    /// история дисперсии
    /// </summary>
    long[] SigmaHistory;

    RingBuffer<long> stab_data_history = null;
    long stab_sum = 0;

    long AC, AC2;
    long DS1, KA, MA, MA2, IM = 0, IM2 = 0;

    /// <summary>
    /// контролирует целевую дисперсию --
    /// влияет на амплитуду выходного сигнала
    /// </summary>
    long DSN = 2073;

    long MSA = 0, DS = 0;

    double m_SamplingRate = 0;

    log4net.ILog log = log4net.LogManager.GetLogger(typeof(PpgDispersionNormalizer));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sampling_rate"></param>
    /// <param name="BitsPerSample">Разрядность АЦП стабилизируемого канала ФПГ</param>
    public PpgDispersionNormalizer(double sampling_rate, int BitsPerSample)
    {
      PpgDispersionNormalizerParams para = PpgDispersionNormalizerParamsManager.GetParams(BitsPerSample);
      this.DSN = para.DSN;
      this.m_KA_min = para.KA_min;
      this.m_KA_max = para.KA_max;

      this.m_SamplingRate = sampling_rate;

      // длительность истории для нормализации дисперсии
      this.MB = (int)( sampling_rate * 2.25 );

      this.A = new long[MB];
      this.AM = new long[MB];
      this.DD = new long[MB];

      this.SigmaHistory = new long[MB];
      this.stab_data_history = new RingBuffer<long>(MB);

    }

    ~PpgDispersionNormalizer()
    {
//      this.Debug_DisposeMonitor();
    }

    public void NormalizeDataInPlace(int[] data)
    {
      try
      {
        lock (this)
        {
          InitHistory(data);

          int adc;

          for (int i = 0; i < data.Length; ++i)
          {
            adc = data[i];

            // adc - следующее значение из массива входных данных
            IM++;                           // текущий индекс в истории
            IM %= MB;                       // кольцевой буфер -- перескок в начало

            IM2 = IM + MB / 2;              // индекс в истории -- сдвинут вперед на половину длины буфера.
            IM2 %= MB;                      // кольцевой буфер -- перескок в начало

            AC2 = A[IM];                    // достаем из буфера старое значение входных данных
            MSA -= AC2;		                  // вычитание из суммы старого значения входных данных

            AC = adc;                       // использование нового значения входных данных

            MSA += AC;		                  // добавление к сумме нового значения входных данных
            A[IM] = AC;		                  // помещение в буфер нового значения входных данных
            MA = (int)(MSA / ((long)MB));	  // вычисление текущего значения среднего
            AM[IM] = MA;		                // помещение в массив текущего значения среднего

            MA2 = AM[IM2];	                // выборка из массива старого значения среднего

            double w = ((double)DS) / ((double)MB); // вычисляем дисперсию

            this.SigmaHistory[IM] = (long)(51.0 * System.Math.Sqrt(w));// сохраняем дисперсию...

            KA = (int)(51.0 * System.Math.Sqrt(w)); // вычисление стандартного отклонения

            AC = A[IM2];				            // выборка из массива старого входного значения
            DS1 = AC - MA;   		            // разность текущ значения и среднего
            DS1 *= DS1;				              // квадрат отклонения

            DS -= (long)(DD[IM]);		        // вычитание из суммы старого значения
            DS += (long)(DS1);			        // накопление суммы квадратов отклонений

            DD[IM] = DS1;				            // помещение в массив текущего значения квадрата отклонения

            // накладываем ограничения на значение дисперсии
            // предотвращают чрезмерное усиление шума при снятии пальца с датчика ФПГ
            //log.Debug( string.Format("KA={0}", KA) );
            KA = System.Math.Min(System.Math.Max(KA, KA_min), KA_max);

            // получение нормированного значения и сохранение в истории
            // AC2 - старое значение исходного сигнала
            // МА2 - старое значение мат. ожидания исходного сигнала
            // КА - дисперсия
            // DSN - нормированное значение дисперсии
            long stab = (long)((DSN * (AC2 - MA2)) / KA);

            stab_data_history.AddValue(stab);
            long stab_oldest = stab_data_history.GetValue(MB - 1);
            stab_sum -= stab_oldest;
            stab_sum += stab;

            // убираем постоянную составляющую (приводим средний уровень сигнала к "0")
            data[i] = (int)( stab - (stab_sum / MB) );

//            Debug_AddData(this.channelADC_Black, adc);
//            Debug_AddData(this.channelStab_DarkGray, stab);
//            Debug_AddData(this.channelY_Crimson, data[i]);
////            Debug_AddData(this.channelMY_Red, stab_sum / MB);
//            Debug_AddData(this.channelSTAB_SUMM_Olive, stab_sum);
//            Debug_AddData(this.channelKA_Brown, (int)KA);
          }
        }
      }
      catch( Exception ex )
      {
        this.log.Error(ex);
        throw;
      }
    }

    private void InitHistory(int[] data)
    {
      if( this.m_bIsHistoryEmpty && (data.Length > 0) )
      {
        // reset history
        for (int i = 0; i < this.A.Length; ++i)
        {
          // Исправление дефекта 143
          // если инициализация производится не нулями,
          // то наблюдается странное поведение стабилизатора дисперсии!!!
          this.A[i] = 0;// data[0];
          this.AM[i] = 0;// data[0];
          this.DD[i] = 0;// DD_initial;
          this.SigmaHistory[i] = 0;
        }
        this.stab_data_history.InitBuffer(0);
        this.m_bIsHistoryEmpty = false;
      }
    }

    private int DD_initial
    {
      get
      {
        return 0;
      }
    }

    private int m_KA_min = 400;
    private int KA_min
    {
      get
      {
        return m_KA_min;
      }
    }

    private int m_KA_max = 13000;
    private int KA_max
    {
      get
      {
        return m_KA_max;
      }
    }


    #region debug signal monitoring functions
#if FALSE
    private common.debug.signalmonitor.Monitor m_Monitor = null;

    bool m_bDebug_MonitorEnabled = false;

    common.debug.signalmonitor.Channel channelY_Crimson = null;
    common.debug.signalmonitor.Channel channelMY_Red = null;
    common.debug.signalmonitor.Channel channelDY_LightGreen = null;
    common.debug.signalmonitor.Channel channelDYCut_Green = null;
    common.debug.signalmonitor.Channel channelMDY_Blue = null;
    common.debug.signalmonitor.Channel channelD2Y_Orange = null;
    common.debug.signalmonitor.Channel channelADC_Black = null;
    common.debug.signalmonitor.Channel channelStab_DarkGray = null;
    common.debug.signalmonitor.Channel channelSTAB_SUMM_Olive = null;

    common.debug.signalmonitor.Channel channelKA_Brown = null;

    [System.Diagnostics.Conditional("DEBUG")]
    private void Debug_InitMonitor(double SamplingRate)
    {
      System.Diagnostics.Debug.Assert(null == this.m_Monitor);

      this.m_Monitor = new PskOnline.common.debug.signalmonitor.Monitor(SamplingRate, typeof(PpgDispersionNormalizer).FullName, 1);

      this.m_Monitor.SetTimeSpan(10);
      this.m_Monitor.SetValueMinMax(0, 1024);

      channelADC_Black = this.m_Monitor.AddChannel(SamplingRate, "ADC", System.Drawing.Color.Black, 1);
      channelStab_DarkGray = this.m_Monitor.AddChannel(SamplingRate, "stab", System.Drawing.Color.DarkGray, 2);
      channelY_Crimson = this.m_Monitor.AddChannel(SamplingRate, "Y", System.Drawing.Color.Crimson, 1);
      channelSTAB_SUMM_Olive = this.m_Monitor.AddChannel(SamplingRate, "stab_sum", System.Drawing.Color.Olive, 2);
      channelMY_Red = this.m_Monitor.AddChannel(SamplingRate, "MY", System.Drawing.Color.Red, 1);
      channelKA_Brown = this.m_Monitor.AddChannel(SamplingRate, "KA", System.Drawing.Color.Brown, 1);
      channelDY_LightGreen = this.m_Monitor.AddChannel(SamplingRate, "DY", System.Drawing.Color.LightGreen, 1);
      channelDYCut_Green = this.m_Monitor.AddChannel(SamplingRate, "DY_CUT", System.Drawing.Color.Green, 1);
      channelMDY_Blue = this.m_Monitor.AddChannel(SamplingRate, "MDY", System.Drawing.Color.Blue, 1);
      channelD2Y_Orange = this.m_Monitor.AddChannel(SamplingRate, "D2Y", System.Drawing.Color.Orange, 1);
    }

    /// <summary>
    /// enables or disables signal monitoring in debug mode
    /// </summary>
    /// <param name="bEnable"></param>
    [System.Diagnostics.Conditional("DEBUG")]
    public void Debug_MonitorEnabled(bool bEnable)
    {
      m_bDebug_MonitorEnabled = bEnable;
      if (m_bDebug_MonitorEnabled)
      {
        Debug_InitMonitor(this.m_SamplingRate);
      }
    }

    [System.Diagnostics.Conditional("DEBUG")]
    private void Debug_AddData(common.debug.signalmonitor.Channel channel, long value)
    {
      if( m_bDebug_MonitorEnabled )
      {
        System.Diagnostics.Debug.Assert(null != this.m_Monitor);

        this.m_Monitor.AddData(channel, new double[] { (double)value });
      }
    }

    [System.Diagnostics.Conditional("DEBUG")]
    private void Debug_DisposeMonitor()
    {
      if (null != this.m_Monitor)
      {
        m_Monitor.Dispose();
        m_Monitor = null;
      }
    }
#endif
#endregion


  }
}
