namespace PskOnline.Methods.Hrv.Processing
{
  using System;
  using System.Collections.Generic;
  using PskOnline.Methods.Hrv.ObjectModel;

  /// <summary>
  /// ���������� ������� ������������������ ������-����������
  /// </summary>
  public class HrvSpectrumAnalyzer
  {
    
    /// <summary>
    /// return a in complex power b
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    static Exocortex.DSP.Complex pow(Exocortex.DSP.Complex a, Exocortex.DSP.Complex b)
    {
      double r = a.GetModulus();
      double teta = a.GetArgument();

      double c = b.Re;
      double d = b.Im;

      Exocortex.DSP.Complex r1 = new Exocortex.DSP.Complex( Math.Pow(r, c) * Math.Exp( - d * teta), 0);
      Exocortex.DSP.Complex r2 = new Exocortex.DSP.Complex(Math.Cos(d * Math.Log(r) + c * teta), Math.Sin(d * Math.Log(r) + c * teta));

      return r1 * r2;
    }

    /// <summary>
    /// �������� ������������ ������������������
    /// ������� � ������������ �������� �����������
    /// </summary>
    /// <param name="times">������� ��������</param>
    /// <param name="samples">�������</param>
    /// <param name="time">������ ��� ������������</param>
    /// <param name="hint">����� ����������� ������� ��� ������ �������� ��������</param>
    /// <returns></returns>
    static double interpolate_linear(List<double> times, List<double> samples, double time, ref int hint)
    {
      // return linear interpolation of values before and after the given point

      System.Diagnostics.Debug.Assert( hint < times.Count );
      System.Diagnostics.Debug.Assert( hint > -1 );

      int delta = 0;
      if (times[hint] > time)
      {
        // ���� �����
        delta = -1;
      }
      else if (times[hint] < time)
      {
        delta = 1;
        // ���� ������
      }
      else
      {
        return samples[hint];
      }

      while(
        ( ((hint + delta) > -1) &&                                       // 1. ��������, ��� �� �������� � ������
          ((hint + delta) < (times.Count)) &&
          ((hint + delta + 1) < times.Count)
          ) &&
          (!((time >= times[hint]) && (time <= times[hint + 1]))))       // 2. �������� ������� ��������� ������
      {
        // ���� ��������� ����� ���������
        hint += delta;
      }

      System.Diagnostics.Debug.Assert( times.Count > 1 );
      System.Diagnostics.Debug.Assert( time <= times[times.Count - 1] );

      System.Diagnostics.Debug.Assert((hint + 1) < times.Count);
      System.Diagnostics.Debug.Assert((hint + 1) < samples.Count);

      // ������������� �������� �������� ��������

      double left_V = samples[hint];
      double right_V = samples[hint+1];
      double left_T = times[hint];
      double right_T = times[hint+1];

      double deltaV = right_V - left_V;
      double delta_T = right_T - left_T;

      double delta_t = time - left_T;
      
      return left_V + delta_t * deltaV / delta_T;
    }

    public static HrvSpectrumResult MakeCardioIntervalsSpectrumWithNoiseReduction(List<CardioInterval> cardio_intervals)
    {
      return MakeCardioIntervalsSpectrumInternal(cardio_intervals);
    }

    public static HrvSpectrumResult MakeCardioIntervalsSpectrum(List<CardioInterval> cardio_intervals)
    {
      return MakeCardioIntervalsSpectrumInternal(cardio_intervals);
    }

    public static HrvSpectrumResult MakeCardioIntervalsSpectrumInternal(List<CardioInterval> cardio_intervals)
    {
      int i;
      int int_count = cardio_intervals.Count;

      if ( int_count == 0)
      {
        throw new ArgumentException();
      }

      HrvSpectrumResult result = new HrvSpectrumResult();

      // ������ ������������� ����������
      List<double> intervals_durations = new List<double>(cardio_intervals.Count);
      // �������������� ������ �������������
      for (i = 0; i < cardio_intervals.Count; ++i)
      {
        intervals_durations.Add(cardio_intervals[i].duration);
      }

      // ������ � ������� ����������� �� ���������
      double T = 1.0;

      // ������������...
      // ������, �������� ������� �������� ��� ��������� ������ ������
      var local_int_times = new List<double>(int_count);
      for (i = 0; i < cardio_intervals.Count; ++i)
      {
        local_int_times.Add(cardio_intervals[i].moment - cardio_intervals[0].moment);
      }

      // ������������ ������������������ � �������������!!!
      // (���������� ����� ������ � ��������� ������-����������)
      double total_duration = cardio_intervals[cardio_intervals.Count - 1].moment - cardio_intervals[0].moment;

      // ������� ������������ ���������
      double ave = total_duration / ((double)int_count);

      // �������������� ������ � ������� ����������� ��� ����� ���������� 10 ��
      T = 0.1;
      double Tmsec = T * 1000;

      int interpolated_count = (int)Math.Floor(total_duration / Tmsec);

      // ����������������� ������������������ ������-����������
      var interpolated = new List<double>(interpolated_count);

      int hint = 0;
      double time = 0;
      for (i = 0; i < interpolated_count; ++i)
      {
        time += Tmsec;
        interpolated.Add(interpolate_linear(local_int_times, intervals_durations, time, ref hint));
      }

      // this is for debugging only
      //interpolated = GenerateTestSinusWave(interpolated, 1 / T);

      // �������� ���������� ������������ ��� ��������
      result.HrvSpectrumSource = new HrvSpectrumSource
      {
        InterpolatedData = interpolated,
        InterpolatedSamplingPeriod = Tmsec,
        SourceDataSamples = intervals_durations,
        SourceDataSampleTimes = local_int_times
      };

      // ��������� ������� ����������� ������ ������������������ �������
      double SF = 1 / T;

      ////////////////////////////////////////////////////////////////////////////////////////////

      var local_intervals = new List<double>(interpolated.Count);
      local_intervals.AddRange(interpolated);

      ////////////////////////////////////////////////////////////////////////////////////////////
      // ������� ���������� ������������,
      // ������� �� �������� ����������
      // ������� �������� ���������.

      double l_ave = 0.0;

      // ������� �������
      for( i = 0; i < local_intervals.Count; ++i )
      {
        l_ave += local_intervals[i];
      }

      l_ave /= local_intervals.Count;

      // �������� �������
      for( i = 0; i < local_intervals.Count; ++i )
      {
        local_intervals[i] -= l_ave;
      }

        
		  ////////////////////////////////////////////////////////////////////////////////////////////
      // � ������� ������� ����������� ������������� ����� �������� ������������ ��������� ��������
      int count = local_intervals.Count;

      // ����� ��������� ������ �������������� ����� �������
      var RealSpectrum = new List<double>(count / 2);
      for( i = 0; i < count / 2; ++i )
      {
        RealSpectrum.Add(0);
      }

      /////////////////////////////////////////////////////////////////////////////////////////////
      // ���������� �������
      double Re, Im, t;
      double N = count;

      for (int k = 0; k < count / 2; k++)
      {
        Re = 0;
        Im = 0;
        for (int n = 0; n < count; n++)
        {
          t = 2 * Math.PI * n * k / N;
          Re += local_intervals[n] * Math.Cos(t);
          Im += local_intervals[n] * Math.Sin(t);
        }
        RealSpectrum[k] = Math.Sqrt(Re * Re + Im * Im);
      }
      
      // ����� ���������� �������� ������ ������� ������� �� 0 �� 0.5 ��!!!
      double FreqResolution = (SF / 2.0) / ((double)(RealSpectrum.Count));

      // ������� �������� ���� ����� �� ��������������� ������� ����� �������� ������ ��� ��������
      int new_spectrum_count = (int)Math.Ceiling(0.5 / FreqResolution);

      System.Diagnostics.Debug.Assert(new_spectrum_count <= RealSpectrum.Count);

      // ������� � ��������� �������� ������� �� 0 �� 0.5 ��
      var spectrum = new List<double>(new_spectrum_count);
      for( i = 0; i < new_spectrum_count; ++i )
      {
        spectrum.Add(RealSpectrum[i]);
      }

      result.FreqResolution = FreqResolution;
      result.SpectrumLow = 0;
      result.SpectrumHigh = FreqResolution * spectrum.Count;

      // ��������� ���������
      double UltraLowStart = result.ULF_LowBound;
      double UltraLowEnd = result.ULF_HighBound;
      
      double VeryLowStart = result.VLF_LowBound;
      double VeryLowEnd = result.VLF_HighBound;
      
      double LowStart = result.LF_LowBound;
      double LowEnd = result.LF_HighBound;
      
      double HighStart = result.HF_LowBound;
      double HighEnd = result.HF_HighBound;

      
      int RRSpecHighIndexMin = (int)(HighStart / FreqResolution);    
      int RRSpecHighIndexMax = (int)(HighEnd / FreqResolution);
      int RRSpecHighCount = (int)(RRSpecHighIndexMax - RRSpecHighIndexMin);

      int RRSpecLowIndexMin = (int)(LowStart / FreqResolution);
      int RRSpecLowIndexMax = (int)(LowEnd / FreqResolution);
      int RRSpecLowCount = (int)(RRSpecLowIndexMax - RRSpecLowIndexMin);

      int RRSpecVeryLowIndexMin = (int)(VeryLowStart / FreqResolution);   
      int RRSpecVeryLowIndexMax = (int)(VeryLowEnd / FreqResolution);
      int RRSpecVeryLowCount = (int)(RRSpecVeryLowIndexMax - RRSpecVeryLowIndexMin);

      int RRSpecUltraLowIndexMin = (int)(UltraLowStart / FreqResolution);
      int RRSpecUltraLowIndexMax = (int)(UltraLowEnd / FreqResolution);
      int RRSpecUltraLowCount = (int)(RRSpecUltraLowIndexMax - RRSpecUltraLowIndexMin);

      int CardioIntervalsRawSpectrDataCount = 0;       

      CardioIntervalsRawSpectrDataCount = RRSpecHighCount + RRSpecLowCount + RRSpecVeryLowCount + 1;
      double[] CardioIntervalsRawSpectrData = new double[CardioIntervalsRawSpectrDataCount];

      for( i = 0;
           i < CardioIntervalsRawSpectrDataCount - 1;
           i++ )
      {
        CardioIntervalsRawSpectrData[i] = spectrum[i];
      }

      CardioIntervalsRawSpectrData[CardioIntervalsRawSpectrDataCount - 1] = HighEnd;

      // ���������� ���������� ���������
      // (� ������ � ������������ ��������
      // � ����������� ������� �������).
      result.ULFTP = 0;
      for (i = RRSpecUltraLowIndexMin;
           i < RRSpecUltraLowIndexMax;
           ++i)
      {
        result.ULFTP += (CardioIntervalsRawSpectrData[i] + CardioIntervalsRawSpectrData[i + 1]) * FreqResolution / 2.0;
      }

      result.BaevskiIndices.S0 = 0;
      for( i = RRSpecVeryLowIndexMin;
           i < RRSpecVeryLowIndexMax;
           ++i )
      {
        result.BaevskiIndices.S0 += 
          (CardioIntervalsRawSpectrData[i] + CardioIntervalsRawSpectrData[i + 1]) * FreqResolution / 2.0;
      }

      result.BaevskiIndices.Sm = 0;
      for( i = RRSpecLowIndexMin;
           i < RRSpecLowIndexMax;
           ++i )
      {
        result.BaevskiIndices.Sm += 
          (CardioIntervalsRawSpectrData[i] + CardioIntervalsRawSpectrData[i + 1]) * FreqResolution / 2.0;
      }

      result.BaevskiIndices.Sd = 0;
      for( i = RRSpecHighIndexMin;
           i < RRSpecHighIndexMax && i < CardioIntervalsRawSpectrDataCount - 2;
           ++i )
      {
        result.BaevskiIndices.Sd += 
          (CardioIntervalsRawSpectrData[i] + CardioIntervalsRawSpectrData[i + 1]) * FreqResolution / 2.0;
      }

      result.SpectrumBins = spectrum;
      result.HFTP = result.BaevskiIndices.Sd;
      result.LFTP = result.BaevskiIndices.Sm;
      result.VLFTP = result.BaevskiIndices.S0;
      return result;
    }

    private static List<double> GenerateTestSinusWave(List<double> source, double SamplingRate)
    {
      double frequency = 0.125;

      for (int i = 0; i < source.Count; ++i)
      {
        source[i] = 1000 + 1000.0 * Math.Sin(frequency * 2.0 * Math.PI * ((double)i) / SamplingRate);
      }

      frequency = 0.25;
      for (int i = 0; i < source.Count; ++i)
      {
        source[i] += 500.0 * Math.Sin(frequency * 2.0 * Math.PI * ((double)i) / SamplingRate);
      }

      frequency = 0.375;
      for (int i = 0; i < source.Count; ++i)
      {
        source[i] += 500.0 * Math.Sin(frequency * 2.0 * Math.PI * ((double)i) / SamplingRate);
      }

      return source;
    }
  }
}
