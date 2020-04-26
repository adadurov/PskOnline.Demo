using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Math.Psa.Ppg
{

  /// <summary>
  /// ����������� ��������� ������� ��� �� ��������� 2,2 �������
  /// 
  /// �������� ��������� � ������� ���������� �.�.����������
  /// </summary>
  public class PpgDispersionNormalizer
  {
    bool m_bIsHistoryEmpty = true;

    /// <summary>
    /// ������������ �������
    /// </summary>
    int MB;

    /// <summary>
    /// ������� ������� ������
    /// </summary>
    long[] A; 
    
    /// <summary>
    /// ������� ������� (���. ��������)
    /// </summary>
    long[] AM;

    /// <summary>
    /// ������� ����������
    /// </summary>
    long[] DD;

    /// <summary>
    /// ������� ���������
    /// </summary>
    long[] SigmaHistory;

    RingBuffer<long> stab_data_history = null;
    long stab_sum = 0;

    long AC, AC2;
    long DS1, KA, MA, MA2, IM = 0, IM2 = 0;

    /// <summary>
    /// ������������ ������� ��������� --
    /// ������ �� ��������� ��������� �������
    /// </summary>
    long DSN = 2073;

    long MSA = 0, DS = 0;

    double m_SamplingRate = 0;

    log4net.ILog log = log4net.LogManager.GetLogger(typeof(PpgDispersionNormalizer));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sampling_rate"></param>
    /// <param name="BitsPerSample">����������� ��� ���������������� ������ ���</param>
    public PpgDispersionNormalizer(double sampling_rate, int BitsPerSample)
    {
      PpgDispersionNormalizerParams para = PpgDispersionNormalizerParamsManager.GetParams(BitsPerSample);
      this.DSN = para.DSN;
      this.m_KA_min = para.KA_min;
      this.m_KA_max = para.KA_max;

      this.m_SamplingRate = sampling_rate;

      // ������������ ������� ��� ������������ ���������
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

            // adc - ��������� �������� �� ������� ������� ������
            IM++;                           // ������� ������ � �������
            IM %= MB;                       // ��������� ����� -- �������� � ������

            IM2 = IM + MB / 2;              // ������ � ������� -- ������� ������ �� �������� ����� ������.
            IM2 %= MB;                      // ��������� ����� -- �������� � ������

            AC2 = A[IM];                    // ������� �� ������ ������ �������� ������� ������
            MSA -= AC2;		                  // ��������� �� ����� ������� �������� ������� ������

            AC = adc;                       // ������������� ������ �������� ������� ������

            MSA += AC;		                  // ���������� � ����� ������ �������� ������� ������
            A[IM] = AC;		                  // ��������� � ����� ������ �������� ������� ������
            MA = (int)(MSA / ((long)MB));	  // ���������� �������� �������� ��������
            AM[IM] = MA;		                // ��������� � ������ �������� �������� ��������

            MA2 = AM[IM2];	                // ������� �� ������� ������� �������� ��������

            double w = ((double)DS) / ((double)MB); // ��������� ���������

            this.SigmaHistory[IM] = (long)(51.0 * System.Math.Sqrt(w));// ��������� ���������...

            KA = (int)(51.0 * System.Math.Sqrt(w)); // ���������� ������������ ����������

            AC = A[IM2];				            // ������� �� ������� ������� �������� ��������
            DS1 = AC - MA;   		            // �������� ����� �������� � ��������
            DS1 *= DS1;				              // ������� ����������

            DS -= (long)(DD[IM]);		        // ��������� �� ����� ������� ��������
            DS += (long)(DS1);			        // ���������� ����� ��������� ����������

            DD[IM] = DS1;				            // ��������� � ������ �������� �������� �������� ����������

            // ����������� ����������� �� �������� ���������
            // ������������� ���������� �������� ���� ��� ������ ������ � ������� ���
            //log.Debug( string.Format("KA={0}", KA) );
            KA = System.Math.Min(System.Math.Max(KA, KA_min), KA_max);

            // ��������� �������������� �������� � ���������� � �������
            // AC2 - ������ �������� ��������� �������
            // ��2 - ������ �������� ���. �������� ��������� �������
            // �� - ���������
            // DSN - ������������� �������� ���������
            long stab = (long)((DSN * (AC2 - MA2)) / KA);

            stab_data_history.AddValue(stab);
            long stab_oldest = stab_data_history.GetValue(MB - 1);
            stab_sum -= stab_oldest;
            stab_sum += stab;

            // ������� ���������� ������������ (�������� ������� ������� ������� � "0")
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
          // ����������� ������� 143
          // ���� ������������� ������������ �� ������,
          // �� ����������� �������� ��������� ������������� ���������!!!
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
