//#define ENABLE_DEBUG_MONITOR
#define MONITOR_IN_DEBUG_BUILDS_ONLY
#define KEEP_DATA_IN_DEBUG_MONITOR

using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Math.Psa.Ppg
{

  /// <summary>
  /// �������� �������� ������ �� ������ �����������.
  /// ��������� � ������� ���������� ���������� �.�. (��., ��������, /firmware/rb-18/proba_usb)
  /// ������ ������������� �� ���������, � ����� ����������� � ����������������.
  /// �� �������� ������ ����������� ������������� (����������) �����,
  /// ������������� �������� ������������� (���������� ��������) � �����������
  /// ������ �����������������.
  /// �������� �� ��������� ������ ������ ����������� ������ "0" ������ ����.
  /// ����������� ����� ������ -- ��� ������ ���� �� ����� 30 ��.
  /// </summary>
  /// <remarks>
  /// �� ������������� ���������� �������� � ����� ������.
  /// ������, ��� ���������� �������� ������ �� ��������������� ��������,
  /// ������ ��������� �������� ������ � ������� �������, ���������������
  /// ��� ���������� � ������� ���������������� ������� �������� ���������� �������� ������.
  /// </remarks>
  public class PpgPulseDetectorByDerivative : IPpgPulseDetector
  {
    #region ���������� ����������� ��������� ������ �� ������ �����������������
    class DetectorParameters
    {
      public override string ToString()
      {
        return string.Format(" [ BY={0}; BYD={1}; BMY={2}; BMYD={3} ]", this.BY, this.BYD, this.BMY, this.BMYD);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="dsn">sampling period, seconds</param>
      /// <param name="byd">���� ������� �����������������</param>
      /// <param name="bmyd">���� ���������� �����������</param>
      /// <param name="by">
      /// ���� ������������ ������������������ �������
      /// (�������� ������� ���� ������������������ ������� �� ��������� ��� ����������</param>
      /// <param name="bmy">���� ���������� ������������������ ������� ( �������� ������� ���� �� BMY ��������� ��������)</param>
      public DetectorParameters(double rate, int byd, int bmyd, int by, int bmy)
      {
        this.SamplingRate = rate;
        this.BYD = byd;
        this.BMYD = bmyd;
        this.BY = by;
        this.BMY = bmy;
      }

      public double SamplingRate;
      public int BYD;
      public int BMYD;
      public int BY;
      public int BMY;
    }

    static class DetectorParametersManager
    {
      public static DetectorParameters GetParameters(double samplingRate)
      {
        initParamsCache();

        System.Diagnostics.Debug.Assert(paramsMap.Count > 0);

        Dictionary<double, double> delta_dic = new Dictionary<double, double>(paramsMap.Count);
        foreach (double d in paramsMap.Keys)
        {
          delta_dic.Add(d, System.Math.Abs(d - samplingRate));
        }

        Dictionary<double, double>.KeyCollection.Enumerator first = delta_dic.Keys.GetEnumerator();
        first.MoveNext();

        double min_key = first.Current;
        double minimum = delta_dic[min_key];

        foreach (double freq in delta_dic.Keys)
        {
          if (delta_dic[freq] < minimum)
          {
            minimum = delta_dic[freq];
            min_key = freq;
          }
        }

        System.Diagnostics.Debug.Assert(
          (System.Math.Abs(min_key - samplingRate) / System.Math.Abs(min_key)) < 0.05,
          string.Format(
            "Accuracy of parameters used by PpgPulseDetectorByDerivative is worse than 5%! " + System.Environment.NewLine +
            "Please re-calculate parameters for this sampling frequency ({0} Hz) " + System.Environment.NewLine +
            "and add them to the parameters manager's cache.", samplingRate)
            );

        return paramsMap[min_key];
      }

      static Dictionary<double, DetectorParameters> paramsMap = new Dictionary<double, DetectorParameters>(5);

      static void initParamsCache()
      {
        lock (paramsMap)
        {
          if (paramsMap.Count > 0)
          {
            // already initialized
            return;
          }

          // ������� �������� (��������� ������� ��������� RB-18)
          AddParams(new DetectorParameters(52, 5, 5, 7, 7));

          // ������� �������� (������������ ��� �� RB-18 � 3 ����)
          AddParams(new DetectorParameters(156, 11, 11, 23, 23));

          // ��� ��������� ��� �.�.��������
          // ������������� ������� �������� ��� ����������� ������� 143
          AddParams(new DetectorParameters(366, 21, 21, 47, 21));

          // ��� ��������� ������� ������� �������� ����� ������������
          // � ������ ������������ �.�.���������
          AddParams(new DetectorParameters(400, 21, 21, 49, 23));

          // ��� ��������� ��� �.�.��������
          AddParams(new DetectorParameters(482, 22, 21, 25, 23));
        }
      }

      private static void AddParams(DetectorParameters detectorParameters)
      {
        paramsMap.Add(detectorParameters.SamplingRate, detectorParameters);
      }
    }
    #endregion

    private log4net.ILog log = log4net.LogManager.GetLogger(typeof(PpgPulseDetectorByDerivative));

    private PpgDispersionNormalizer m_Normalizer = null;

    /// <summary>
    /// ���������� �� ������������������ � ����������� �������
    /// </summary>
    private SeriesStatisticsCollector m_Signal_Statistics = null;

    /// <summary>
    /// ����� ������� ������������������ ������� ���
    /// ��� ���������� ����� ������������������. 
    /// ��������� ��������� ��� ��� �����������������
    /// �������� ��������� BY ��������
    /// </summary>
    int BY = 0;

    /// <summary>
    /// ����� ������� ������� �������� ������������������ �������
    /// (��� �����������������)
    /// �������� ��������� BMY ��������
    /// </summary>
    int BMY = 0;

    /// <summary>
    /// ����� ������� I-� ����������� ������������������ �������
    /// (��� �������� ���������� ������� �������� ����������� ������������������� �������)
    /// �������� ��������� BYd ��������
    /// </summary>
    int BYd = 0;

    /// <summary>
    /// ����� ������� ������� �������� I-� ����������� ������������������ �������
    /// (��� ������� �����������������)
    /// �������� ��������� BMYd ��������
    /// </summary>
    int BMYd = 0;

    /// <summary>
    /// ����� ������� �������� ������ �����������
    /// (��� �������� ����������� ������ "0")
    /// </summary>
    int BMYd2 = 0;

    double KD1 = 0;

    double KD2 = 0;

    /// <summary>
    /// ����������� ������ ����� ����������
    /// </summary>
    private double m_MinStatCollectionPeriod = 1.5;
    private double m_SamplingRate = 0;

    private ZeroCrossingDetector zero_crossing_detector = new ZeroCrossingDetector(ZeroCrossingDirection.Down);

    private SeriesStatisticsCollector m_SumY_front_history = null;
    private RingBuffer<int> m_Y = null;
    private RingBuffer<long> m_History_SumY = null;
    private RingBuffer<int> m_DY = null;
    private RingBuffer<long> m_History_SumDY = null;

    private RingBuffer<int> m_History_D2Y = null;

    private SeriesStatisticsCollector m_DY_Statistics = null;

    private double m_SumY = 0;
    private long m_SumDY = 0;

    private double m_Threshold_DY = 0;

    private int m_TotalCounter = 0;

    private bool m_bIsHistoryEmpty = true;


    public void DenyLoggerByName(string logger_name)
    {
      log.DebugFormat($"Logger '{logger_name}' will be denied for all existing appenders based on log4net.Appender.AppenderSkeleton. (But not for any appenders created in the future!)");
      var filter = new log4net.Filter.LoggerMatchFilter
      {
        LoggerToMatch = logger_name,
        AcceptOnMatch = false
      };
      filter.ActivateOptions();

      foreach( var repo in log4net.LogManager.GetAllRepositories() )
      {
        foreach( var appender in repo.GetAppenders())
        {
          var skel = appender as log4net.Appender.AppenderSkeleton;
          skel?.AddFilter(filter);
          skel?.ActivateOptions();
        }
      }
    }


    public PpgPulseDetectorByDerivative(double SamplingRate, int BitsPerSample, bool bEnableDebugMonitor)
    {
      DenyLoggerByName(typeof(PpgPulseDetectorByDerivative).ToString());

      m_SamplingRate = SamplingRate;

      DetectorParameters par = DetectorParametersManager.GetParameters(SamplingRate);

      log.Debug($"Detector parameters used for sampling rate = {SamplingRate} Hz are: {par} ");

      BY = par.BY;
      BMY = par.BMY;
      BYd = par.BYD;
      BMYd = par.BMYD;
      BMYd2 = 10;

      KD1 = BY * BMY * 64;
      KD2 = BYd * BMYd * 64;

      m_Normalizer = new PpgDispersionNormalizer(SamplingRate, BitsPerSample);

      //m_Normalizer.Debug_MonitorEnabled(true);
#if ENABLE_DEBUG_MONITOR
      this.Debug_MonitorEnabled(bEnableDebugMonitor);
#else
      // do not enable, if not enabled in this module
#endif
      // monitor always off
      //this.Debug_MonitorEnabled(false);

      // ������� ������� ���������� �� �������
      // ��� ���������� �������������� ��������� �������
      // �� ��������� m_MinStatCollectionPeriod �������������� ������ ������,
      // � ����� ������� ������� �� ��������� 2.2 �������
      m_Signal_Statistics = new SeriesStatisticsCollector((int)(SamplingRate * this.m_MinStatCollectionPeriod));

      // ������� ������� ���������� �� ����������� �������
      // ��� ���������� �������������� ��������� ����������� �������
      // �� ��������� 2.2 �������, �������������� ������,
      m_DY_Statistics = new SeriesStatisticsCollector((int)(SamplingRate * this.m_MinStatCollectionPeriod));

      // ������� ������������������ �������
      this.m_Y = new RingBuffer<int>(this.BY);

      // ������� ���� ������������������ �������
      // (����������, ����������� ����� ������������������)
      this.m_History_SumY = new RingBuffer<long>(this.BMY);

      // ������� ������������������ ����������� �������
      // ��� ������ ������������ � ������ ���������� ������
      this.m_SumY_front_history = new SeriesStatisticsCollector((int)(SamplingRate * 1.0));

      // ������� ����������� ������������������ �������
      this.m_DY = new RingBuffer<int>(this.BYd);

      // ������� ���� ����������� ������������������ �������
      // (����������, ����������� ����� ������ ������������������)
      this.m_History_SumDY = new RingBuffer<long>(this.BMYd);

      // ������� ������ ���������� ������������������ �������
      this.m_History_D2Y = new RingBuffer<int>(this.BMYd2);
    }


    private int MinThreshold
    {
      get
      {
        return 10;
      }
    }

    ~PpgPulseDetectorByDerivative()
    {
      this.Dispose();
    }


    private const int m_DumpFrequency = 8;

    private int m_DumpCounter = 0;

    [System.Diagnostics.Conditional("DEBUG")]
    internal void Dump()
    {
      if (0 == ( (m_DumpCounter++) % (((int)this.m_SamplingRate) / m_DumpFrequency)) )
      {
        log.DebugFormat( "============================================================================" );
        log.DebugFormat( "++++++++  Rez_T = {0}", this.m_DY.GetNewestValue() );
        log.DebugFormat( "++++++++  m_DY history: {0}", m_DY.ToString() );
        log.DebugFormat( "++++++++  m_SumDY = {0}", this.m_SumDY );
        log.DebugFormat( "++++++++  M_DY = {0}", this.m_SumDY / ((double)this.m_DY.GetSize()) );
      }
    }

    #region debug signal monitoring functions

#if DEBUG_MONITOR
    bool m_bDebug_MonitorEnabled = false;
    private common.debug.signalmonitor.Monitor m_Monitor = null;

    common.debug.signalmonitor.Channel channelDY_T_Brown = null;
    common.debug.signalmonitor.Channel channelT_Maroon = null;
    common.debug.signalmonitor.Channel channelY_Crimson = null;
    common.debug.signalmonitor.Channel channelMY_Red = null;
    common.debug.signalmonitor.Channel channelDY_LightGreen = null;
    common.debug.signalmonitor.Channel channelDYCut_Green = null;
    common.debug.signalmonitor.Channel channelMDY_Blue = null;
    common.debug.signalmonitor.Channel channelD2Y_Orange = null;
    common.debug.signalmonitor.Channel channelADC_Black = null;

#if MONITOR_IN_DEBUG_BUILDS_ONLY
    [System.Diagnostics.Conditional("DEBUG")]
#endif
    private void Debug_InitMonitor(double SamplingRate)
    {
      if( null == this.m_Monitor )
      {
        lock (this)
        {
          if (null == this.m_Monitor)
          {
            System.Diagnostics.Debug.Assert(null == this.m_Monitor);

            this.m_Monitor = new PskOnline.common.debug.signalmonitor.Monitor(SamplingRate, typeof(PpgPulseDetectorByDerivative).FullName, 1);

#if KEEP_DATA_IN_DEBUG_MONITOR
            this.m_Monitor.KeepData = true;
#endif

            this.m_Monitor.SetTimeSpan(10);
            this.m_Monitor.SetValueMinMax(0, 1024);

            channelADC_Black = this.m_Monitor.AddChannel(SamplingRate, "ADC", System.Drawing.Color.Black, 1);
            channelY_Crimson = this.m_Monitor.AddChannel(SamplingRate, "Y", System.Drawing.Color.Crimson, 1);
            channelMY_Red = this.m_Monitor.AddChannel(SamplingRate, "MY", System.Drawing.Color.Red, 1);
            channelDY_LightGreen = this.m_Monitor.AddChannel(SamplingRate, "DY", System.Drawing.Color.LightGreen, 1);

            channelDY_T_Brown = this.m_Monitor.AddChannel(SamplingRate, "DY_T", System.Drawing.Color.Brown, 1);
            channelT_Maroon = this.m_Monitor.AddChannel(SamplingRate, "T", System.Drawing.Color.Maroon, 2);

            channelMDY_Blue = this.m_Monitor.AddChannel(SamplingRate, "MDY", System.Drawing.Color.Blue, 1);
            channelD2Y_Orange = this.m_Monitor.AddChannel(SamplingRate, "D2Y", System.Drawing.Color.Orange, 1);
          }
        }
      }
    }

    /// <summary>
    /// enables or disables signal monitoring in debug mode
    /// </summary>
    /// <param name="bEnable"></param>
#if MONITOR_IN_DEBUG_BUILDS_ONLY
    [System.Diagnostics.Conditional("DEBUG")]
#endif
    public void Debug_MonitorEnabled(bool bEnable)
    {
      m_bDebug_MonitorEnabled = bEnable;
      if (m_bDebug_MonitorEnabled)
      {
        Debug_InitMonitor(this.m_SamplingRate);
      }
    }

#if MONITOR_IN_DEBUG_BUILDS_ONLY
    [System.Diagnostics.Conditional("DEBUG")]
#endif
    private void Debug_AddData(common.debug.signalmonitor.Channel channel, int[] val_array)
    {
      double[] double_val_array = new double[val_array.Length];

      System.Array.Copy(val_array, double_val_array, val_array.Length);

      if (m_bDebug_MonitorEnabled && (null != this.m_Monitor) )
      {
        lock (this)
        {
          if (m_bDebug_MonitorEnabled && (null != this.m_Monitor))
          {
            this.m_Monitor.AddData(channel, double_val_array);
          }
        }
      }
    }

#if MONITOR_IN_DEBUG_BUILDS_ONLY
    [System.Diagnostics.Conditional("DEBUG")]
#endif
    private void Debug_AddMark(double time, System.Drawing.Color color)
    {
      if( null != this.m_Monitor )
      {
        lock( this )
        {
          if (null != this.m_Monitor)
          {
            this.m_Monitor.AddMark(time, color);
          }
        }
      }
    }



#if MONITOR_IN_DEBUG_BUILDS_ONLY
    [System.Diagnostics.Conditional("DEBUG")]
#endif
    private void Debug_AddData(common.debug.signalmonitor.Channel channel, long[] val_array)
    {
      if (m_bDebug_MonitorEnabled && (null != this.m_Monitor) )
      {
        lock (this)
        {
          if (m_bDebug_MonitorEnabled && (null != this.m_Monitor))
          {
              double[] double_val_array = new double[val_array.Length];
              System.Array.Copy(val_array, double_val_array, val_array.Length);
              this.m_Monitor.AddData(channel, double_val_array);
          }
        }
      }
    }

    private void Debug_AddData(common.debug.signalmonitor.Channel channel, double[] val_array)
    {
      if( m_bDebug_MonitorEnabled && (null != this.m_Monitor) )
      {
        lock (this)
        {
          if (m_bDebug_MonitorEnabled && (null != this.m_Monitor))
          {
            this.m_Monitor.AddData(channel, val_array);
          }
        }
      }
    }

#if MONITOR_IN_DEBUG_BUILDS_ONLY
    [System.Diagnostics.Conditional("DEBUG")]
#endif
    private void Debug_AddData(common.debug.signalmonitor.Channel channel, long value)
    {
      this.Debug_AddData(channel, new long[] { value });
    }

#if MONITOR_IN_DEBUG_BUILDS_ONLY
    [System.Diagnostics.Conditional("DEBUG")]
#endif
    private void Debug_AddData(common.debug.signalmonitor.Channel channel, double value)
    {
      this.Debug_AddData(channel, new double[] { value });
    }

#if MONITOR_IN_DEBUG_BUILDS_ONLY
    [System.Diagnostics.Conditional("DEBUG")]
#endif
    private void Debug_DisposeMonitor()
    {
      if (null != this.m_Monitor)
      {
        lock (this)
        {
          if (null != this.m_Monitor)
          {
            m_Monitor.Dispose();
            m_Monitor = null;
          }
        }
      }
    }

#endif
    #endregion

    /// <summary>
    /// ��� ��������� � �������� �������
    /// ����������� ������ � ��� ������� ���������� ������
    /// � ���������� ������� � �������������
    /// �������� ������� HeartContractionDetected.
    /// ���������� ����������� ������ ������ �����������
    /// ������ �� �������������� ���������� �������� �������.
    /// </summary>
    /// <param name="data_buf"></param>
    public void AddData(int[] data_buf, long timestamp)
    {
      if (data_buf.Length == 0)
      {
        return;
      }

      // ����� ���������� ������ ������������� ���� ������� �������?
      long timestamp_data_count = this.m_TotalCounter + data_buf.Length;

      try
      {

#if false
        // �������: ���������� �������� ������� � ��� � ������� ������
        this.Debug_AddData(this.channelADC_Black, data_buf);
#endif

        // normalize amplitude
        m_Normalizer.NormalizeDataInPlace(data_buf);

        // analyze signal here...

        // first-point initialization handling
        if (true == this.m_bIsHistoryEmpty)
        {
          this.m_bIsHistoryEmpty = false;

          // ������� ������ -- ���������
          this.m_Y.InitBuffer(data_buf[0]);

          // ����� �������� == ������������ ������� �� ���������
          this.m_History_SumY.InitBuffer(data_buf[0] * this.m_History_SumY.GetSize());

          // ����� �������� == ������������ ������� �� ���������
          this.m_SumY_front_history.InitBuffer(data_buf[0] * this.m_History_SumY.GetSize());

          this.m_SumY = data_buf[0] * this.m_History_SumY.GetSize();

          // ������� ����������� -- "0"
          this.m_DY.InitBuffer(0);

          // ����� �������� ����������� -- "0"
          this.m_History_SumDY.InitBuffer(0);

          // ������� ������ ����������� -- "0"
          this.m_History_D2Y.InitBuffer(0);
        }

        for (int i = 0; i < data_buf.Length; ++i)
        {
          // increment data counter
          ++m_TotalCounter;

          ProcessNextSample(data_buf[i], timestamp, timestamp_data_count);
        }
      }
      catch (Exception ex)
      {
        this.log.Error(ex);
      }

    }

    double dY_Threshold_Factor
    {
      get
      {
        return 1.70;
      }
    }

    double front_amplitude_threshold_factor
    {
      get
      {
        return 0.30;
      }
    }

    /// <summary>
    /// ������������ ��������� ������ ���
    /// </summary>
    /// <param name="stab_val"></param>
    /// <param name="timestamp"></param>
    /// <param name="timestamp_data_count"></param>
    private void ProcessNextSample(int stab_val, long timestamp, long timestamp_data_count)
    {
      double RezD1 = -500;
      double RezT = -1000;
      double RezD2 = -2000;

      try
      {
        // ���������� ������� ������������������ �������
        this.m_Y.AddValue(stab_val);

        // ��������� ����� ������� �������� �������, � ������� ������
        this.m_SumY -= m_Y.GetOldestValue();
        this.m_SumY += stab_val;

        // ���������� ������� ���� (� ������, � ������� ��������)
        this.m_History_SumY.AddValue((long)this.m_SumY);

        // ���������� ������� ����������������� ��������
        this.m_SumY_front_history.AddValue(this.m_SumY);

        // ������� ����������� � ������� ������
        double Y_0 = this.m_History_SumY.GetValue(0);
        double Y_1 = this.m_History_SumY.GetValue(2);

        RezD1 = (1 * (Y_0 - Y_1) * 76800) / KD1;

        double Rez = RezD1;
        if (Rez < 0)
        {
          Rez = 0;
        }

        // �������� ���������� �� �������������� �������
        m_Signal_Statistics.AddValue(this.m_SumY / ((double)this.m_Y.GetSize()));

        // �������� ���������� �� ��������� ������ �����������, ������� 0
        m_DY_Statistics.AddValue(Rez);

        if (0 == Rez)
        {
          // ��������� ��������� �������� ��� ������ �����������
          // ��������� ��� ������ �� ��� ���, ���� �� �������� �����
          // �������� ��� ������ ��� ����������� ������������������ �������
          // � ��������� �������������� �������������
          int sigma_DY = (int)(dY_Threshold_Factor * System.Math.Sqrt(m_DY_Statistics.GetDX(0)));
          m_Threshold_DY = System.Math.Max(sigma_DY, this.MinThreshold);
        }

        // ������ ����������� � ������ ������
        RezT = Rez - m_Threshold_DY;

        if (RezT < 0)
        {
          RezT = 0;
        }

        this.m_SumDY -= this.m_DY.GetOldestValue();
        this.m_SumDY += (long)RezT;

        // ���������� ������� �������� ������ �����������, ������� 0 � ������ ������
        this.m_DY.AddValue((int)RezT);

        // ���������� ������� ����� ��������� �������� ������ �����������, ������� ��� 0 � ������ ������
        this.m_History_SumDY.AddValue((long)this.m_SumDY);

        // ��������� ������ �����������
        double MDY_0 = this.m_History_SumDY.GetValue(0);
        double MDY_1 = this.m_History_SumDY.GetValue(2);

        // ��������� 2 �����������
        RezD2 = (1 * (MDY_0 - MDY_1) * 3880) / KD2;

        // ���������� ������� �����������
        this.m_History_D2Y.AddValue((int)RezD2);

        // ��������� ����������� ���� ������ ����������� ������ ����
        double D2Y_0 = this.m_History_D2Y.GetValue(0);

        System.Diagnostics.Debug.Assert( ((int)RezD2) == ((int)D2Y_0) );

        //if( this.bFrontAlreadyDetected && ( ! zero_crossing_detector.AddValue(D2Y_0)) )
        //{
        //  this.m_SumY_front_history.GetLastRisingFrontAmplitude()
        //}

        if( zero_crossing_detector.AddValue(D2Y_0) )
        {
          // ������ ���� ����� ����� ������, � ����� ������� ��� ������...

          // � ������ ����������� ���� ������ ����������� ������ ����
          // �� ����� ������ �������� �������� ������!
          // ���� ��������������� ���� -- ��� �����.

          double sumY_front_amplitude = this.m_SumY_front_history.GetLastRisingFrontAmplitude();
          double detected_front_amplitude = sumY_front_amplitude / ((double)this.BY);
          double detected_front_duration = this.m_SumY_front_history.GetLastRisingFrontDuration();

          // ������ ������
          double front_sample_count = ((double)this.m_TotalCounter) - zero_crossing_detector.GetLastZeroCrossingPoint();

          double delta_samples = ((double)timestamp_data_count) - front_sample_count;
          double delta_time_seconds = delta_samples / this.m_SamplingRate;
          double delta_timestamp = 1.0e6 * delta_time_seconds;
          long front_timestamp = timestamp - ((long)delta_timestamp);


          log.DebugFormat(
            "front: time {0} ms, duration {1} ms, amplitude {2}, raw amplitude is {3}",
            (int)(front_sample_count / this.m_SamplingRate * 1000.0),
            (int)(detected_front_duration / this.m_SamplingRate * 1000.0),
            (int)detected_front_amplitude,
            (int)sumY_front_amplitude
            );


          if( detected_front_duration > 0 )
          {
            double front_duration_ms = ((double)detected_front_duration * 1000.0 / this.m_SamplingRate);
            double front_duration_min_threshold_ms = 2;
            double front_duration_max_threshold_ms = 1000;

            // ������ ������ ��������������� ������...
            double front_height = detected_front_amplitude;

            // ���������� ����� ������ ������
            double front_height_threshold = this.front_amplitude_threshold_factor * System.Math.Sqrt(this.m_Signal_Statistics.GetDX(1));

            // ��������� ���������� ����� ���������� ���������� ������ ������
            front_height_threshold = System.Math.Max(front_height_threshold, 10);

            if ((front_duration_ms > front_duration_min_threshold_ms))
            {
              if ((front_duration_ms < front_duration_max_threshold_ms))
              {
                if (
                    (System.Math.Abs(front_height) > front_height_threshold)
                  )
                {

                  log.DebugFormat(".NORMAL FRONT ({0} steps), threshold is {1}",
                    (int)front_height, 
                    (int)front_height_threshold
                    );
                  // ��������� ����� "��������" ������
                  // Debug_AddMark(front_sample_count / this.m_SamplingRate, System.Drawing.Color.Green);

                  // this function checks fronts and rejects those
                  // that may have been detected due to various interferences
                  this.OnFrontFound(front_sample_count, front_timestamp);
                  
                  return;
                }
                else
                {
                  this.log.DebugFormat(
                    "....front too low ({0} steps), threshold is {1}",
                    (int)front_height, 
                    (int)front_height_threshold
                    );
                }
              }
              else
              {
                this.log.DebugFormat(
                  "....front too long ({0} ms), max. allowed is {1} ms",
                  (int)front_duration_ms, 
                  (int) front_duration_max_threshold_ms
                  );
              }
            }
            else
            {
              this.log.DebugFormat(
                "....front too short ({0} ms), min. allowed is {1} ms",
                (int)front_duration_ms, 
                (int) front_duration_min_threshold_ms
                );
            }

          }
          else
          {
            this.log.Debug(
              "....front too short (1 sampling period)"
              );
          }

          //Debug_AddMark(front_sample_count / this.m_SamplingRate, System.Drawing.Color.Red);
        }

      }
      catch (Exception ex)
      {
        this.log.Error(ex);
      }
      finally
      {

        Dump();

#if ENABLE_DEBUG_MONITOR
        // �������: ���������� ��������������� ��������
        // ������������������ ������� � ������� ������
        this.Debug_AddData(this.channelY_Crimson, stab_val);

        // �������: ���������� ������� ��������
        // ������������������ ������� � ������� ������
        // (���������� ������ ����� ������������������)
        this.Debug_AddData(this.channelMY_Red, this.m_SumY / ((double)this.m_Y.GetSize()));

        // ���������� �����������, �� ���������� ���� ������ "0"
        this.Debug_AddData(this.channelDY_LightGreen, RezD1);

        // �������: ���������� �����������, ���������� ���� ������ ������
        this.Debug_AddData(this.channelDY_T_Brown, RezT);

        // �������: ���������� ������� ������
        this.Debug_AddData(this.channelT_Maroon, m_Threshold_DY);

        // �������: ���������� ���������� �������� ������ �����������
        this.Debug_AddData(this.channelMDY_Blue, this.m_SumDY / ((double)this.m_DY.GetSize()));

        // �������: ���������� ������ �����������
        this.Debug_AddData(this.channelD2Y_Orange, RezD2);
#endif
      }
    }

    private void OnFrontFound(double sample_count, long timestamp)
    {
      if (null != this.HeartContractionDetected)
      {
        this.HeartContractionDetected(sample_count, timestamp);
      }
    }

    public event HeartContractionDetectedDelegate HeartContractionDetected;


#region IDisposable Members

    public void Dispose()
    {
#if DEBUG_MONITOR
      Debug_DisposeMonitor();
#endif
    }

      #endregion
    }
}
