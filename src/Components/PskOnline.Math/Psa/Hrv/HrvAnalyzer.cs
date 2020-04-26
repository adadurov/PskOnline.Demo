namespace PskOnline.Math.Psa.Hrv
{
  using System;

  using PskOnline.Math.Psa.Ppg;

  /// <summary>
  /// This class implements cardio signal processing (ECG and PPG) and detection of its secondary parameters.
  /// 
  /// Usage:
  /// 1. Set input signal parameters according to signal type and adjust other settings, if needed.
  /// 
  /// 2. Put data for processing.
  ///   Two types of data processing is supportd:
  ///     1. Digitized smoothed signal (ECG or PPG) (ProcessSignal() method).
  ///     2. Array of cardio cycles lengths (ProcessRR() method);
  /// 3. Get results (Get* methods).
  /// 4. The object can be used repeatedly, beginning from step 1 or 2.
  /// 
  /// Processing results can be divided into 3 categories:
  /// 
  /// 1. Time-domain statistical parameters. Average NN interval, standard deviation and so on.
  /// 2. Spectral parameters.
  ///   Of two possible types: FFT-based, or parametrical model-based processing only FFT-based processing is implemented for now.
  /// 3. Geometrical. Specifically, it was supposed to build the histogram and approximate it with a triangle.
  /// The problem is that this class of methods produces best results when time of measurement is more than 20 minutes.
  /// (and 24 hours is much much better), that renders them useless for analysis of short-time records.
  /// The code for histogramm collection is written but an the data at hand the triangle has huge hole where
  /// there must be peak. 
  /// This is, most likely, by design - probably, it is that peak which produces high-frequency part of the spectrum.
  /// Thus, so far it makes no sence to implement interpolation of what is totally missing.
  /// Moreover, ther is so-called 'logarithmic index' that can be calculated for 5-minutes records.
  /// </summary>
  /// <remarks>
  /// It is necessary to mention, that, in medical literature, it is stated,
  /// that selection of point of analysis in QRS-complex must be performed very carefully,
  /// and it is not necessarily that this point shall be local maximum.
  /// 
  /// The main condition is munimum bouncing of signal (due to noise, discrete nature of signal etc.).
  /// Because the maximum itself is relatively flat, then it seems that it's better to use middle of front before this maximum.
  /// However, the difference is very slight in reality and it's difficult to tell without doubt what is better.
  /// The function ProcessSignal() extracts middles of fronts using find_fronts(), and ProcessRR() function
  /// processes already extracted intervals, passed to it as an array.
  /// </remarks>
  public class HrvAnalyzer
  {
    #region private
    double data_rate;
    double min_pulse_rate;
    double max_pulse_rate;
    double max_relative_pulse_width;
    double min_abs_front_time;
    double max_abs_front_time;

    bool is_empty;

    double[] peaks;
    double[] rr;
    double[] drr;

    double fft_overlap;
    double max_frequency;
    double[] spectrum;

    void CheckValidResults()
    {
    	if( this.is_empty )
      {
		    throw new SignalAnalysisException(strings.enter_data_before_trying_to_get_result);
      }
    }

    double ComputePowerInSpectralRange(double f1, double f2)
    {
      CheckValidResults();
      int i1, i2, i;
      double sum, df;

      if (f1 < 0)
      {
        throw new ArgumentException(strings.f1_must_be_greater_than_or_equal_to_0);
      }
      if (f2 < 0)
      {
        throw new ArgumentException(strings.f2_must_be_greater_than_or_equal_to_0);
      }
      if (f1 > f2)
      {
        throw new ArgumentException(strings.f1_must_be_less_than_f2);
      }

      df = max_frequency / (this.spectrum.Length - 1);
      i1 = (int)(f1 / df);  // FIXME: check
      i2 = (int)(f2 / df);  // FIXME: Возможно, стоит использовать дополнительную интерполяцию.

      if (i1 < 0)
      {
        i1 = 0;
      }
      if (i2 < 0)
      {
        i2 = 0;
      }
      if (i1 >= this.spectrum.Length)
      {
        i1 = this.spectrum.Length - 1;
      }
      if (i2 >= this.spectrum.Length)
      {
        i2 = this.spectrum.Length - 1;
      }

      sum = 0;
      for (i = i1; i <= i2; i++)
      {
        sum += (double)this.spectrum[i];
      }

      return sum;
    }

    /// <summary>
    /// если отношение амплитуды пика к средней амплитуде
    /// пиков в записи меньше этого числа,
    /// то пик выбрасывается из анализа
    /// </summary>
    double min_peak_to_average_peak_ratio = 0.6;

    /// <summary>
    /// минимальная высота спада после пика в единицах отсчета АЦП
    /// если высота спада после пика меньшей этой величины, 
    /// то пик выбрасывается из анализа
    /// </summary>
    int min_after_fall_magnitude = 3;

    /// <summary>
    /// минимально допустимое отношение высота спада после пика по отношению к высоте пика
    /// если для некоторого пика это отношение меньшей этой величины, 
    /// то пик выбрасывается из анализа
    /// </summary>
    double min_relative_after_fall_magnitude = 0.2;


    void ComputePeaks(double[] data)
    {
      PeakDetector pd = new PeakDetector();

      this.peaks = pd.get_fronts(data,
                                 (int)(0.4 * data_rate * (60.0 / this.max_pulse_rate)),
                                 (int)(max_relative_pulse_width * data_rate * (60.0 / min_pulse_rate)),
                                 (int) (this.min_abs_front_time * data_rate),
                                 (int)(this.max_abs_front_time * data_rate),
                                 this.min_peak_to_average_peak_ratio,
                                 this.min_after_fall_magnitude,
                                 this.min_relative_after_fall_magnitude);

      if (this.peaks.Length < 3)
      {
        throw new SignalAnalysisException(strings.too_few_peaks_found_cannot_continue);
      }
    }

    void CopyPeaks(double[] data)
    {
      this.peaks = new double[data.Length];
      Array.Copy(data, this.peaks, this.peaks.Length);
    }

    void ComputeRR_and_DRR()
    {
      int i;
      rr = new double[this.peaks.Length - 1];
      drr = new double[this.peaks.Length - 2];

      for (i = 0; i < this.peaks.Length - 1; i++)
      {
        rr[i] = peaks[i + 1] - peaks[i];
      }

      for (i = 0; i < this.peaks.Length - 2; i++)
      {
        drr[i] = rr[i + 1] - rr[i];
      }
    }

    void ComputeSpectrum()
    {
      float x;
	    int p_n;    // количество интервалов (==количество пиков минус 1)
//	    int tmp = 0;

      float[] window;
      int in_, out_; // indices into 'window' array

	    int fft_n;
	    float x_step, x_window_step, dt;
	    double norm, avg;
	    int i;
	    int windows_summed;

      int dummy = -1;

	    //int peak_i = 1; // index in peaks array
	    //int interval_i = 0; // index in RR intervals array
	    p_n = this.peaks.Length - 1;
	
	    fft_n = this.spectrum.Length * 2;
	    x_step = (float) (this.data_rate / 2 / this.max_frequency);
	    x_window_step = (float)(x_step * fft_n * (1.0 - fft_overlap)); 
	    dt = (float)(x_step / data_rate);
	
	    norm = 1;
	    norm *= 1e6 / data_rate / data_rate;  // Приведение единиц к ms^2 
	    norm *= 1 / (dt*fft_n);               // Пересчет в мощность из расчета на одно квадратное окно
	    norm *= 8.0 / 3.0;                    // Коэф. коррекции для окна Hann'a
	    norm *= (dt) / fft_n;                 // Соотношение между dft и эквивалентным cft
	
	    spectrum = new double [this.spectrum.Length];
	
	    window = new float[fft_n + fft_n + fft_n / 2]; 
	    in_ = fft_n; // ой, как некрасиво!...
	    out_ = in_ + fft_n;
	    
      float[] buffer = new float[4 * fft_n];
//      int buffer_i = 0;

	    for( i = 0; i < this.spectrum.Length; i++ )
      {
		    spectrum[i] = 0;
      }
			
	    for( i = 0; i < fft_n; i++)
      {
//		    window[i] = 1.0; // Rectangular window 
        window[i] = (float)(0.5 * (1 - System.Math.Cos(2*System.Math.PI*i/(fft_n-1)))); // Hann window 
      }

//	      avg = average(p_y, p_n);

	    windows_summed = 0;
	    x = (float)this.peaks[0];

      // пока окно размером fft_n еще помещается в массиве пиков...
      while( x + x_step * fft_n < this.peaks[p_n - 1] )
      {
		    avg = 0;
		    for( i = 0; i < fft_n; i++ )
        {
          // так как интервал между пиками неодинаковый, требуется интерполяция
          window[in_ + i] = interpolation.interpolate_linear(this.peaks, this.rr, p_n, x + x_step * i, ref dummy);
			    avg += window[in_ + i];
		    }
		    avg /= fft_n;
		
        // убираем постоянную составляющую... (с учетом применяемого окна)
		    for(i=0; i < fft_n; i++)
        {
			    window[in_+i] = (float)( window[i]*(window[in_ + i] - avg));
        }

			  // БПФ
		    fft.rft_psd_slow(window, in_, window, out_, fft_n, buffer);

        // добавляем результаты этого окна
		    for(i=0; i<this.spectrum.Length; i++)
        {
			    spectrum[i] += window[out_ + i] * norm;
        }
		 
		    windows_summed++;
		    x += x_window_step;
	    }
	
	    if(windows_summed==0)
      {
		    throw new Exception("FFT window is larger than data set. Please decrease HrvAnalyzer.SetSpectrumBins() or increase HrvAnalyzer.SetMaxFrequency().");
      }
	
      // нормируем...
	    norm = 1.0 / windows_summed;
	    for( i = 0; i < this.spectrum.Length; i++ )
      {
  		  spectrum[i] *= norm;
      }
	
    }

    #endregion

    #region public
    public HrvAnalyzer()
    {
      SetDataRate(500);
      SetSpectrumBins(64);
      SetFFTOverlap(0.5);
      SetMaxFrequency(1.0);
      
      SetMinPulseRate(45);
      SetMaxPulseRate(140);
      
      SetMaxPulseWidth(0.1f);
      
      SetMinFrontTime(0.1f);
      SetMaxFrontTime(0.5f);

      is_empty = true;
    }

    /// <summary>
    /// Установить частоту дискретизации входного сигнала,
    /// при создании экземпляра класса устанавливается частота 500 Гц
    /// </summary>
    /// <param name="freq"></param>
    public void SetDataRate(double freq)
    {
    	if( freq <= 0 )
      {
		    throw new ArgumentException(strings.data_rate_must_be_greater_than_zero);
      }
	
      data_rate = freq;
    }

    /// <summary>
    /// Установить количество точек в спектре HRV.
    /// n обязано быть степенью двойки и меньше,
    /// чем половина длины входного сигнала
    /// (в данном случае -- предполагаемого количества
    /// кардио-интервалов в "сглаженном" сигнале, или ).
    /// </summary>
    /// <param name="n"></param>
    public void SetSpectrumBins(int n)
    {
      for (int i = n; i > 1; i /= 2)
      {
        if (i % 2 == 1)
        {
          throw new ArgumentException(strings.number_of_specturm_bins_must_be_power_of_2);
        }
      }
	    this.spectrum = new double[n];
    }

    /// <summary>
    /// FFT считается кусками. overlap задает,
    /// на сколько они перекрываются. def: 0.5
    /// </summary>
    /// <param name="overlap"></param>
    public void SetFFTOverlap(double overlap)
    {
	    if((overlap<0)||(overlap>=1))
      {
		    throw new ArgumentException(strings.overlap_must_be_in_range_from_0_to_1);
      }
	
    	fft_overlap = overlap;
    }

    /// <summary>
    /// Максимальная частота в спектре HRV. По умолчанию 1 Гц.
    /// </summary>
    /// <param name="freq"></param>
    public void SetMaxFrequency(double freq)
    {
	    if( freq <= 0 )
      {
		    throw new ArgumentException(strings.freq_must_be_greater_than_0);
      }
      max_frequency = freq;
    }

    /// <summary>
    /// Минимально возможная частота пульса в ударах в минуту, по умолчанию  30 уд./мин.
    /// </summary>
    /// <param name="rate"></param>
    public void SetMinPulseRate(double rate)
    {
	    if( rate <= 0 )
      {
		    throw new ArgumentException(strings.minimum_pulse_rate_must_be_greater_than_zero);
      }
    	min_pulse_rate = rate;
    }

    /// <summary>
    /// Максимально возможная частота пульса
    /// (в ударах в минуту, по умолчанию 160 уд./мин.).
    /// </summary>
    /// <param name="rate"></param>
    public void SetMaxPulseRate(double rate)
    {
	    if(rate <= 0)
      {
		    throw new ArgumentException(strings.maximum_pulse_rate_must_be_greater_than_zero);
      }
	    max_pulse_rate = rate;
    }

    /// <summary>
    /// minimum front length
    /// </summary>
    /// <param name="front"></param>
    public void SetMinFrontTime(double front)
    {
      this.min_abs_front_time = front;
    }

    public void SetMaxFrontTime(double time)
    {
      this.max_abs_front_time = time;
    }

    /// <summary>
    /// Максимально возможная ширина пика
    /// (по отношению к длине периода, по умолчанию 0.1).
    /// </summary>
    /// <param name="width"></param>
    public void SetMaxPulseWidth(double width)
    {
      if ((width <= 0) && (width >= 1))
      {
		    throw new ArgumentException(strings.maximum_peak_width_must_be_in_range_from_0_to_1);
      }
	    max_relative_pulse_width = width;
    }

    /// <summary>
    /// Ищет фронты во входном _сглаженном_ сигнале.
    /// </summary>
    /// <param name="data"></param>
    public void FindFronts(double[] data)
    {
	  	ComputePeaks(data);	
		  is_empty = false;
    }

    /// <summary>
    /// Ищет фронты во входном _сглаженном_ сигнале.
    /// </summary>
    /// <param name="data"></param>
    public void FindFronts(float[] data)
    {
      double[] double_data = new double[data.Length];
      for (int i = 0; i < double_data.Length; ++i)
      {
        double_data[i] = data[i];
      }
      ComputePeaks(double_data);
      is_empty = false;
    }

    /// <summary>
    /// Ищет фронты во входном _сглаженном_ сигнале.
    /// </summary>
    /// <param name="data"></param>
    public void FindFronts(int[] data)
    {
      double[] double_data = new double[data.Length];
      for (int i = 0; i < double_data.Length; ++i)
      {
        double_data[i] = (double)data[i];
      }
      ComputePeaks(double_data);
      is_empty = false;
    }

    /// <summary>
    /// Обработать входной _сглаженный_ сигнал.
    /// </summary>
    /// <param name="data"></param>
    public void ProcessSignal(double[] data)
    {
      ComputePeaks(data);
      ComputeRR_and_DRR();
      ComputeSpectrum();
      is_empty = false;
    }

    /// <summary>
    /// Провести вычисления, используя в качестве входных данных какие-нибудь интервалы (например, RR).
    /// </summary>
    /// <param name="rr"></param>
    public void ProcessRR(double[] rr)
    {
      CopyPeaks(rr);
      ComputeRR_and_DRR();
      ComputeSpectrum();
      is_empty = false;
    }

    /// <summary>
    /// Получить массив точек, делящих сигнал на NN-интервалы.
    /// В текущей реализации по вышеуказанным причинам в качестве точек берутся середины фронтов.
    /// </summary>
    /// <returns></returns>
    /// <remarks></remarks>
    public double[] GetNN()
    {
      CheckValidResults();
      return utils.copy_double_array(this.peaks);
    }

    /// <summary>
    /// Получить спектральное распределение.
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public double[] GetSpectrum()
    {
      CheckValidResults();
      return utils.copy_double_array(this.spectrum);
    }

    /// <summary>
    /// Интеграл спектральной плотности энергии по Очень Низким Частотам (0 &lt f &lt 0.04 Гц), измеряется в \f$ms^2\f$
    /// </summary>
    /// <returns></returns>
    public double GetSpectrumVLF()
    {
      return ComputePowerInSpectralRange(0.0033f, 0.04f);
    }

    public double GetSpectrumULF()
    {
      return ComputePowerInSpectralRange(0, 0.0033f);
    }

    /// <summary>
    /// Интеграл спектра по низким частотам (0.04 &lt f &lt 0.15 Гц), измеряется в \f$ms^2\f$
    /// </summary>
    /// <returns></returns>
    public double GetSpectrumLF()
    {
      return ComputePowerInSpectralRange(0.04f, 0.15f);
    }

    /// <summary>
    /// Интеграл спектра по высоким частотам (0.15 &lt f), измеряется в \f$ms^2\f$
    /// </summary>
    /// <returns></returns>
    public double GetSpectrumHF()
    {
      return ComputePowerInSpectralRange(0.15f, 0.4f);
    }

    /// <summary>
    /// Средняя длительность NN-интервалов, мс.
    /// </summary>
    /// <returns></returns>
    public double GetANN()
    {
      return Statistics.Calculator.GetAverage(rr) * 1000.0 / data_rate;
    }

    /// <summary>
    /// Стандартное отклонение NN интервалов, мс
    /// \f[SDNN = \sqrt{ \frac{1}{n} \sum_{i=0}^{n-1} (NN_i - \overline{NN})^2 } \f] */
    /// </summary>
    /// <returns></returns>
    public double GetSDNN()
    {
      return Statistics.Calculator.GetStandardDeviation(rr) * 1000.0 / data_rate;
    }

    /// <summary>
    /// Квадратный корень из среднего суммы возведенных в квадрат разностей между соседними NN интервалами, мс
    /// \f[RMSSD = \sqrt{ \frac{1}{n-1} \sum_{i=1}^{n-1} (NN_i - NN_{i-1})^2 } \f] */
    /// </summary>
    /// <returns></returns>
    public double GetRMSSD()
    {
      int i;
      double sum = 0;
      for (i = 0; i < drr.Length; i++)
      {
        sum += drr[i] * drr[i];
      }
      return Math.Sqrt(sum / ((double)drr.Length)) * (1000.0 / data_rate);
    }

    /// <summary>
    /// Стандартное отклонение разностей между соседними NN-интервалами, мс
    /// \f[SDSD = \sqrt{ \frac{1}{n-1} \sum_{i=1}^{n-1} (NN_i - NN_{i-1} - \overline{(NN_i - NN_{i-1})})^2 } \f] */
    /// </summary>
    /// <returns></returns>
    public double GetSDSD()
    {
      return Statistics.Calculator.GetStandardDeviation(drr) * 1000.0 / data_rate;
    }

    /// <summary>
    /// Количество всех пар соседних NN интервалов, разность между которыми превышает 50 мс
    /// </summary>
    /// <returns></returns>
    public int GetNN50()
    {
      int count = 0;
      for (int i = 0; i < drr.Length; i++)
      {
        if (Math.Abs(drr[i] * 1000.0 / data_rate) > 50)
        {
          count++;
        }
      }
      return count;
    }

    /// <summary>
    /// GetNN50() / (n-1), где n - количество NN-интервалов
    /// </summary>
    /// <returns></returns>
    public double GetpNN50()
    {
      return GetNN50() / ((double)drr.Length);
    }

    /// <summary>
    /// Logarithmical index, \f$ c^{-1} \f$.
    /// \f$\varphi\f$ factor is obtained interpolation of histogram,
    /// \f$\mid NN_i-NN_{i-1} \mid \f$ using following formula: \f$k \cdot e^{- \varphi t}\f$*/  
    /// </summary>
    /// <returns></returns>
    private double GetLogIndex()
    {
	    double max_t = 0.5 * 60 / min_pulse_rate; // Maximum length of exponent tail in seconds.
	    int n = 512;                              // Number of histogram bars
	    double[] a;
	
	    HistogramStatCollector sc = new HistogramStatCollector(0.0, max_t * data_rate, n);
	
	    for(int i = this.drr.Length; i>=0; i--)
      {
		    sc.AddPoint(Math.Abs(drr[i]), 1.0);
      }

	    a = sc.GetStats();

      double phi, dphi, y;
	    double phi_min=0, y_min=0;
	
	    double phi1 = 0;
	    double phi2 = 1;

	    int iters = 50;
	    dphi = 0.2 / iters;
	    y_min = utils.exponential_fit(a, phi_min);
	
	    for(int i=4; i>=0; i--)
      {
		    for( phi = phi1; phi < phi2; phi += dphi )
        {
          y = utils.exponential_fit(a, phi);
    		
			    if( y < y_min )
          {
				    y_min = y;
				    phi_min = phi;
			    } 
		    }
		    phi1 = phi_min - dphi;
		    phi2 = phi_min + dphi;
		    dphi /= iters;
	    }
	    return phi_min * ((double)n) / max_t ;
    }

    /// <summary>
    /// Utility class with service functions.
    /// </summary>
    private class utils
    {
      /// <summary>
      /// This method implements exponential fit procedure.
      /// </summary>
      /// <param name="array"></param>
      /// <param name="phi"></param>
      /// <returns></returns>
      public static double exponential_fit(double[] array, double phi)
      {
        double sye = 0, se2 = 0, sxye = 0, sxe2 = 0, e, x, y;

        for (int i = 0; i < array.Length; i++)
        {
          x = i + 0.5;
          y = array[i];
          e = Math.Exp(-phi * (i + 0.5));

          sye += y * e;
          se2 += e * e;
          sxye += x * y * e;
          sxe2 += x * e * e;
        }

        double k = sye / se2;
        double s = 0;
        for (int i = 0; i < array.Length; i++)
        {
          s += (k * Math.Exp(-phi * (i + 0.5)) - array[i]) * (k * Math.Exp(-phi * (i + 0.5)) - array[i]);
        }

        return s;
      }

      /// <summary>
      /// Creates a copy of an array of double-precision floating-point numbers.
      /// </summary>
      /// <param name="array"></param>
      /// <returns></returns>
      public static double[] copy_double_array(double[] array)
      {
        double[] clone = new double[array.Length];
        for (int i = 0; i < array.Length; ++i)
        {
          clone[i] = array[i];
        }
        return clone;
      }

    }
    #endregion
  };
}
