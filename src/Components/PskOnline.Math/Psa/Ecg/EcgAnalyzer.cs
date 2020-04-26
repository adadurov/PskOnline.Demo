namespace PskOnline.Math.Psa.Ecg
{
  using System;

  using PskOnline.Math.Psa.Hrv;

	/// <summary>
	/// </summary>
	public class EcgAnalyzer
	{
		internal EcgAnalyzer(double SamplingRate)
		{
      this.m_SamplingRate = SamplingRate;
      this.Period = DefaultSignalParameters.MinCardioCyclePeriodInSeconds;
      this.MaxPeakWidth = DefaultSignalParameters.MaxRelativePeakWidth;
		}
   
    private double m_SamplingRate = 512;

    private int m_Period = 2;

    private double[] m_intervals = null;
    private double[] m_peaks = null;

    /// <summary>
    /// ���������� � ��������
    /// </summary>
    public double Period
    {
      get
      {
        return ((double)m_Period) / this.m_SamplingRate;
      }
      set
      {
        m_Period = (int)(value * this.m_SamplingRate);
      }
    }

    /// <summary>
    /// ���������� � ��������
    /// </summary>
    private int m_MinFrontWidth = 0;

    /// <summary>
    /// ���������� � ��������
    /// </summary>
    public double MinFrontWidth
    {
      get
      {
        return this.m_MinFrontWidth / this.m_SamplingRate;
      }
      set
      {
        this.m_MinFrontWidth = (int)(value * this.m_SamplingRate);
      }
    }

    /// <summary>
    /// ���������� � ��������
    /// </summary>
    private int m_MaxFrontWidth = 0;

    /// <summary>
    /// ���������� � ��������
    /// </summary>
    public double MaxFrontWidth
    {
      get
      {
        return this.m_MaxFrontWidth / this.m_SamplingRate;
      }
      set
      {
        this.m_MaxFrontWidth = (int)(value * this.m_SamplingRate);
      }
    }

    private int m_MaxPeakWidth = 1;

    /// <summary>
    /// ���������� � ����� �� Period
    /// </summary>
    public double MaxPeakWidth
    {
      get
      {
        return (double)m_MaxPeakWidth / (double)m_Period;
      }
      set
      {
        this.m_MaxPeakWidth = (int)( m_Period * value);
      }
    }

    /// <summary>
    /// ������������ ������.
    /// </summary>
    /// <param name="signal"></param>
    /// <returns></returns>
    public void Analyze(int[] signal)
    {
      double [] f_signal = new double[signal.Length];
      for( int i = 0; i < signal.Length; ++i )
      {
        f_signal[i] = (double)signal[i];
      }
      Analyze(f_signal);
    }

    /// <summary>
    /// ���������� ���������� ��������� -- ���������.
    /// </summary>
    /// <returns></returns>
    public double[] GetIntervals()
    {
      return this.m_intervals;
    }

    /// <summary>
    /// ���������� ���������� ��������� -- ���������� �����
    /// ����� ������������ ��� �������� � hrv::process.
    /// </summary>
    /// <returns></returns>
    public double[] GetPeaks()
    {
      return this.m_peaks;
    }

    /// <summary>
    /// ���� ��������� ��������� ���� � ������� ���������
    /// ����� � ������ ������ ����� �����,
    /// �� ��� ������������� �� �������
    /// </summary>
    double min_peak_to_average_peak_ratio = 0.6;

    /// <summary>
    /// ����������� ������ ����� ����� ���� � �������� ������� ���
    /// ���� ������ ����� ����� ���� ������� ���� ��������, 
    /// �� ��� ������������� �� �������
    /// </summary>
    int min_after_fall_magnitude = 3;

    /// <summary>
    /// ���������� ���������� ��������� ������ ����� ����� ���� �� ��������� � ������ ����
    /// ���� ��� ���������� ���� ��� ��������� ������� ���� ��������, 
    /// �� ��� ������������� �� �������
    /// </summary>
    double min_relative_after_fall_magnitude = 0.2;

    /// <summary>
    /// ���� ��������������� � ����� � ���������� ������
    /// ������������� ���������������� � �������������.
    /// 
    /// � ������ ������ ���������� ������ ������.
    /// </summary>
    /// <returns></returns>
    public void Analyze(double [] signal)
    {
      this.m_peaks = new PeakDetector().get_peaks(signal, this.m_Period, this.m_MaxPeakWidth, this.m_MinFrontWidth, this.m_MaxFrontWidth, this.min_peak_to_average_peak_ratio, this.min_after_fall_magnitude, this.min_relative_after_fall_magnitude);
    }
	}

  /// <summary>
  /// ����������� ������, � ����� ���������, ����� ������������
  /// ������ ���������� � ���������� ��� �������������.
  /// </summary>
  public class AdvancedEcgAnalyzer
  {
    /// <summary>
    /// ��� ���� ������ � ��������� ��� ������...
    /// </summary>
    private HrvAnalyzer analyzer = new HrvAnalyzer();

    internal AdvancedEcgAnalyzer(double fSamplingFrequency)
    {
      analyzer.SetDataRate(fSamplingFrequency);
      analyzer.SetMaxPulseWidth((double)DefaultSignalParameters.MaxRelativePeakWidth);
      analyzer.SetMinPulseRate((double)DefaultSignalParameters.MinHeartRate);
      analyzer.SetMaxPulseRate((double)DefaultSignalParameters.MaxHeartRate);
    }

    /// <summary>
    /// ����������� �������� ������.
    /// ���������� ����� �������� � ������� ������ ��������������� �������.
    /// </summary>
    /// <param name="signal"></param>
    public void AnalyzeSignal(int[] signal)
    {
      double[] fsignal = new double[signal.Length];
      for (int i = 0; i < signal.Length; ++i)
      {
        fsignal[i] = signal[i];
      }
      this.analyzer.ProcessSignal(fsignal);
    }

    /// <summary>
    /// ���������� ��� RR-���������� (��������� ������, ��������� ��������� �������).
    /// </summary>
    /// <param name="intervals"></param>
    public void AnalyzeIntervals(double[] intervals)
    {
      this.analyzer.ProcessRR(intervals);
    }
    
    /// <summary>
    /// ���������� ������������ �������������.
    /// </summary>
    /// <returns></returns>
    public double[] GetSpectrum()
    {
      return this.analyzer.GetSpectrum();
    }

    /// <summary>
    /// ��������� �������� �� "����� ������" �������� �������.
    /// ��. EcgAnalyzer.GetSpectrumVLF()
    /// </summary>
    /// <returns></returns>
    public double GetSpectrumVLF()
    {
      return this.analyzer.GetSpectrumVLF();
    }

    /// <summary>
    /// ��������� �������� �� "������" �������� �������.
    /// ��. EcgAnalyzer.GetSpectrumLF()
    /// </summary>
    /// <returns></returns>
    public double GetSpectrumLF()
    {
      return this.analyzer.GetSpectrumLF();
    }
    
    /// <summary>
    /// ��������� �������� �� "�������" �������� �������.
    /// ��. EcgAnalyzer.GetSpectrumHF()
    /// </summary>
    /// <returns></returns>
    public double GetSpectrumHF()
    {
      return this.analyzer.GetSpectrumHF();
    }

    public double GetSDNN()
    {
      return analyzer.GetSDNN();
    }
    
    public double GetRMSSD()
    {
      return this.analyzer.GetRMSSD();
    }
    
    public double GetSDSD()
    {
      return this.analyzer.GetSDSD();
    }

    public int   GetNN50()
    {
      return this.analyzer.GetNN50();
    }

    public double GetpNN50()
    {
      return this.analyzer.GetpNN50();
    }
  }
}
