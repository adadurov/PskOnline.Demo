namespace PskOnline.Math.Psa.Ppg
{
  using System;


  /// <summary>
  /// ����� ����������� �������� �������� ������ (���������� ������ 104).
  /// � ���� ����� ���������� ��������� ���� ������������������� ������
  /// PpgRealTimePulseDetector, ����������� ��������� �������������.
  /// </summary>
  /// <remarks>
  /// �� ������������� ���������� �������� � ����� ������.
  /// ������, ��� ���������� �������� ������ �� ��������������� ��������,
  /// ������ ��������� �������� ������ � ������� �������, ���������������
  /// ��� ���������� � ������� ���������������� ������� �������� ���������� �������� ������.
  /// </remarks>
  public class PpgPulseDetector : IPpgPulseDetector
  {
    private log4net.ILog log = log4net.LogManager.GetLogger(typeof(PpgPulseDetector));

    private PpgDenoiser m_Denoiser = null;
    private PpgDispersionNormalizer m_Normalizer = null;
    private SeriesStatisticsCollector m_SignalStatCollector = null;


    /// <summary>
    /// ����������� ������ ����� ����������
    /// </summary>
    private double m_MinStatCollectionPeriod = 2.2;
    private double m_SamplingRate = 0;

    private int[] m_History = null;
    private int m_HistoryPointer = 0;
    private bool m_HistoryEmpty = true;
    private int m_TotalCounter = 0;
    private int m_PreviousValue = -1;

    private int m_Min = int.MaxValue;
    private int m_MinPos = 0;
    private int m_Max = int.MinValue;
    private int m_MaxPos = 0;

    private bool m_IsRising = false;
    private bool m_IsFalling = false;

    public PpgPulseDetector(double SamplingRate, int BitsPerSample)
    {
      m_SamplingRate = SamplingRate;

      m_Denoiser = new PpgDenoiser(SamplingRate);
      m_Normalizer = new PpgDispersionNormalizer(SamplingRate, BitsPerSample);

      // ������� ������� ����������
      // ��� ���������� �������������� ��������� �������
      // �� ��������� 2.2 �������, �������������� ����
      m_SignalStatCollector = new SeriesStatisticsCollector((int)(SamplingRate * this.m_MinStatCollectionPeriod));

      m_History = new int[(int)(m_MinStatCollectionPeriod * SamplingRate + 1)];
      m_HistoryEmpty = true;
    }

    /// <summary>
    /// ��� ��������� � �������� �������
    /// ����������� ������ � ��� ������� ���������� ������
    /// �������� ������� HeartContractionDetected.
    /// </summary>
    /// <param name="data_buf"></param>
    public void AddData(int[] data_buf, long timestamp)
    {
      // ����� ���������� ������ ������������� ���� ������� �������?
      long timestamp_data_count = this.m_TotalCounter + data_buf.Length;

      try
      {
        // filter signal and normalize amplitude
        m_Denoiser.FilterInPlace(data_buf);
        m_Normalizer.NormalizeDataInPlace(data_buf);

        // analyze signal here...

        // we shall detect rising fronts of amplitude
        // greater than sigma ( square root of dispersion )

        // so we must calculate dispersion and keep it for last 2 seconds
        // of signal and update it upon receiving new data fragments

        if( true == this.m_HistoryEmpty )
        {
          // ����� ����� m_HistoryEmpty ���������� � ������� AddValue
          this.m_PreviousValue = data_buf[0];
        }

        int cur_val = data_buf[0];

        for (int i = 0; i < data_buf.Length; ++i)
        {
          cur_val = data_buf[i];

          // ������ ����� ���������� �������� �������� ����������
          m_SignalStatCollector.AddValue(cur_val);
          // � ���������� � ����������� ������� ������
          AddHistoryPoint(cur_val);
          // increment data counter
          ++m_TotalCounter;

          if (this.m_IsRising)
          {
            // local maximum detection
            if (cur_val < this.m_PreviousValue)
            {
              // reset for detection of successive local minimum
              this.m_IsRising = false;
              this.m_Max = this.m_PreviousValue;
              this.m_MaxPos = this.m_TotalCounter;

              this.m_IsFalling = true;

              // end of front detected
              this.OnFrontAndPeakFound(timestamp, timestamp_data_count);
            }
          }
          else if (this.m_IsFalling)
          {
            // local minimum detection
            if (cur_val > this.m_PreviousValue)
            {
              // reset for detection successive local maximum
              this.m_IsFalling = false;
              this.m_Min = this.m_PreviousValue;
              this.m_MinPos = this.m_TotalCounter;

              this.m_IsRising = true;
            }
          }
          else if (cur_val > this.m_PreviousValue)
          {
            this.m_IsRising = true;
            this.m_IsFalling = false;
            this.m_Min = this.m_PreviousValue;
            this.m_MinPos = this.m_TotalCounter;
          }
          else if (cur_val < this.m_PreviousValue)
          {
            this.m_IsFalling = true;
            this.m_IsRising = false;
            this.m_Max = this.m_PreviousValue;
            this.m_MaxPos = this.m_TotalCounter;
          }

          // update stored previous value
          this.m_PreviousValue = cur_val;
        }
      }
      catch (Exception ex)
      {
        this.log.Error(ex);
      }

    }

    public event HeartContractionDetectedDelegate HeartContractionDetected;

    #region history of filtered and stabilized data management
    void AddHistoryPoint(int new_val)
    {
      if (m_HistoryEmpty)
      {
        m_HistoryEmpty = false;
        for (int i = 0; i < this.m_History.Length; ++i)
        {
          m_History[i] = new_val;
        }
        m_HistoryPointer = m_History.Length - 1;
      }

      m_HistoryPointer = (++this.m_HistoryPointer) % this.m_History.Length;

      m_History[m_HistoryPointer] = new_val;
    }


    long GetMinHistoryIndex()
    {
      return this.m_TotalCounter - this.m_History.Length + 1;
    }

    long GetMaxHistoryIndex()
    {
      return this.m_TotalCounter;
    }

    bool IsPointInHistoryBuffer(long index)
    {
      if ((index < GetMinHistoryIndex()) || (index > GetMaxHistoryIndex()))
      {
        return false;
      }
      return true;
    }

    /// <summary>
    /// returns history point
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    int GetHistoryPoint(long index)
    {
      if (!IsPointInHistoryBuffer(index))
      {
        System.Diagnostics.Debug.Fail("Requested index is out of history buffer.");

        throw new ArgumentOutOfRangeException(
          "index",
          index,
          string.Format(strings.IndexMustBeInRangeTemplate, this.GetMinHistoryIndex(), this.GetMaxHistoryIndex())
          );
      }

      // �� ������� ���� ����� ����� � �������, ����� �������� �������� � ����� ��������?
      int offset = (int)(this.m_TotalCounter - index);

      int pointer = m_HistoryPointer - offset;
      if (pointer < 0)
      {
        pointer += this.m_History.Length;
      }

      return this.m_History[pointer];
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="offset">
    /// �������� �� ����� ������� �� ����
    /// </param>
    /// <param name="timestamp">��������� ��������� ������� �������</param>
    /// <param name="timestamp_count">���������� ������, ��������������� ��������� ��������� ������� �������</param>
    void OnFrontAndPeakFound(long timestamp, long timestamp_count)
    {
      if (this.m_TotalCounter < this.m_SamplingRate * this.m_MinStatCollectionPeriod )
      {
        // statistics is not yet collected
        return;
      }
      
      // ����� ��������� ������ ����� ������������ ��������� ���������� ������ � ������ ������
      // � ����������� ������, ��������������� ����� �������.


      // fix for bug 75 http://server.neurolab.local:880/bugzilla/show_bug.cgi?id=75
      // ��������, ��� ����� ������������ � �������
      // (����� ������ �� ������ ����� �������)
      // ���� ���, �� �� ���������� ������ ����� ������
      if ((!IsPointInHistoryBuffer(this.m_MinPos)) || (!IsPointInHistoryBuffer(this.m_MaxPos)))
      {
        return;
      }

      if( null != this.HeartContractionDetected )
      {
        double sigma = Math.Sqrt(this.m_SignalStatCollector.GetDX( this.m_TotalCounter - this.m_MaxPos ));

        // ��������� ��������� ��������, � �� �������
        double front_duration_ms = ((double)(this.m_MaxPos - this.m_MinPos)) / this.m_SamplingRate * 1000;
        int front_magnitude = this.m_Max - this.m_Min;

        ////////////////////////////////////
        // ���� �������� ������ �� ������

        // ��������!!! ���� ����� ����� �������, ��������� ������� ����� �������� (bug 75)!!!
        // ������� ���� ���� �������� �� ������� ������ ����� � ������ �������
        double t = (GetHistoryPoint(this.m_MinPos) + GetHistoryPoint(this.m_MaxPos)) * 0.5f;

        // starting point of search
        double x = 0.5f * (this.m_MinPos + this.m_MaxPos);

        // ���� �������� ������ �� ������:
        for (int i = this.m_MinPos; i < this.m_MaxPos; i++)
        {
          if ((GetHistoryPoint(i) <= t) && (t <= GetHistoryPoint(i + 1)))
          {
            if (Math.Abs(GetHistoryPoint(i) - GetHistoryPoint(i + 1)) < (Math.Abs(GetHistoryPoint(i)) + Math.Abs(GetHistoryPoint(i + 1))) * 2 * double.Epsilon)
            {
              x = i + 0.5f;
            }
            else
            {
              x = i + (t - GetHistoryPoint(i)) / (GetHistoryPoint(i + 1) - GetHistoryPoint(i));
            }
            break;
          }
        }

        // ���������� ��������� �������� ������ (� ���� ���������� ������)
        //x = ((double)this.m_TotalCounter) - x;
        ///////////////////////////////////


        // recalculate front time using offset
            
        // ���-�� ������, �� ��������� �� ������� �������
        long delta_count = timestamp_count - ((long)x);

        // ������ ������� � ��������
        double delta_time = ((double)delta_count) / this.m_SamplingRate;

        // ������� �������, ��������������� ������
        long front_timestamp = timestamp - ((long)(delta_time * 1.0e6));


        this.log.Debug(
          string.Format(
            "front: time {3} ms, ampl. {0}, length {1} ms, signal std.dev. {2}",
            front_magnitude.ToString("0000"),
            front_duration_ms.ToString("0000"),
            sigma.ToString("0000"),
            front_timestamp / 1000
            )
          );

        // TODO: review the parameters
        if ((front_duration_ms > 30) &&
            (front_duration_ms < 1500) &&
            (front_magnitude > 20) &&
          // fix for bug #2 -- refine using front amplitude's relation to the signal dispersion 
          // http://server.neurolab.local:880/bugzilla/show_bug.cgi?id=2
            (front_magnitude > 0.3 * sigma)
            )
        {
          this.log.Debug("front: approved, firing the detection event");
          this.HeartContractionDetected(x, front_timestamp);
        }
        else
        {
          this.log.Debug("front: skipped");
        }
      } // if (null != this.FrontDetected )
    }


    #region IDisposable Members

    public void Dispose()
    {
    }

    #endregion
  }
}
