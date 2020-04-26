// peak detection method: using
// mean front's value crossing point
// or use maximum derivative point
#define USE_MEDIAN_LINE_POINT

namespace PskOnline.Math.Psa.Hrv
{
  using System;
  using System.Collections.Generic;


  /// <summary>
  /// Класс предназначен для выделения положений локальных максимумов сигнала ФПГ.
  /// 
  /// Алгоритм:
  /// 1. На каждом фрагменте сигнала длиной period ищет локальный максимум;
  ///    его положение, величина и классификация складываются в массив структур.
  ///    Классификация используется для последующей пометки принадлежности каждого
  ///    элемента к какой-либо из групп.
  /// 2. Значения пиков в этой структуре распадаются на 2 группы: одни в окрестности нуля, другие с 
  ///    величинами, соответствующими максимумам. Выявляется граница между этими группами.
  /// 3. Группа со значениями больше пороговых может содержать последовательности пиков, идущих один за другим вплотную.
  ///    Поэтому при помощи соеобразной автокорреляции выясняется характерная ширина максимума.
  /// 4. Зная ширину максимумов, можно оставить в каждой последовательности пиков по 1 штуке.
  /// 
  /// 
  /// Методы отбраковки максимумов:
  ///  1. По амплитуде фронта
  ///     (не меньше порогового значения -- должно вычисляться исходя из статистических параметров сигнала,
  ///     например, "мгновенной" дисперсии (за последние 2..3 секунды)
  ///  2. По минимальной и максимальной ширине фронта
  ///  3. По амплитуде спада (пороговое значение вычисляется аналогично -- исходя из статистических параметров сигнала,
  ///     с учетом возможного наличия выраженной дикротической волны)
  ///  4. По минимальной и максимальной ширине спада (с учетом возможного наличия выраженной дикротической волны)
  ///  5. По отношению к средней амплитуде фронта (вычисляется по 6..8 соседним пикам),
  ///     амплитуда "правильных" пиков должна быть не меньше 0.6 среднего значения
  ///  6. По отношению амплитуды спада к амплитуде фронта
  ///     (если амплитуда спада больше чем в 1.5 раза больше амплитуды фронта,
  ///     то скорее всего это так называемая "дикротическая волна", т.е. второй пик на кардиоцикле)
  /// 
  /// </summary>
  class PeakDetector
  {
    private log4net.ILog log = log4net.LogManager.GetLogger(typeof(PeakDetector));

    /// <summary>
    /// reason why given signal fragment was rejected during processing
    /// </summary>
    public enum rejection_reason
    {
      /// <summary>
      /// Undefined reason
      /// </summary>
      Undefined,

      /// <summary>
      /// Amplitude of the fragment is too high
      /// </summary>
      TooHighAmplitude,

      /// <summary>
      /// Amplitude of the fragment is too low
      /// </summary>
      TooLowAmplitude,

      /// <summary>
      /// The client has let the sensor go (depressed his finger from the sensor
      /// causing signal to momentarily reach upper and/or lower limits of the ADC),
      /// Signals inside areas between SensorDepressedArtefact and SensorPressedArtefact
      /// is not considered when calculating statistical parameters of the signal.
      /// </summary>
      SensorDepressedArtefact,

      /// <summary>
      /// The client has put his finger to the sensor after a period of not holding
      /// Signals inside areas between SensorDepressedArtefact and SensorPressedArtefact
      /// is not considered when calculating statistical parameters of the signal.
      /// </summary>
      SensorPressedArtefact
    }
    
    /// <summary>
    /// this class represents the rejected fragment of the signal
    /// </summary>
    public class rejected_fragment
    {
      public int start_index = 0;
      public int end_index = 0;
      public rejection_reason rejection_reason = 0;
    }

    /// <summary>
    /// this function returns rejected fragments of the signal,
    /// if any such fragments were detected during processing
    /// this function returns valid value only after call to get_peaks()
    /// </summary>
    /// <returns></returns>
    public List<rejected_fragment> GetRejectedFragments()
    {
      return new List<rejected_fragment>(0);
    }

    /// <summary>
    /// Данная функция принимает на вход массив array.
    /// period задает длину интервала (измеряемую в числе элементом массива), на котором производится поиск
    /// локальных максимумов. Для ЭКГ period должен быть в 2 раза меньше, чем минимально возможный
    /// период сердцебиения (соответствующий пульсу 200 уд/мин, к примеру).
    /// Для ФПГ (сигнал типа "синус") - в 3-3.5 раза. 
    /// Устанавливать period слишком маленьким не рекомендуется -
    /// это ухудшит качество работы алгоритма и уменьшит скорость.
    /// </summary>
    /// <param name="array"></param>
    /// <param name="period"></param>
    /// <param name="max_peak_width"></param>
    /// <param name="min_front_width"></param>
    /// <param name="max_front_width"></param>
    /// <param name="min_after_fall_magnitude"></param>
    /// <param name="min_peak_to_average_peak_ratio"></param>
    /// <param name="min_relative_after_fall_magnitude"></param>
    /// <returns>массив, содержащий найденные пики</returns>
    public double[] get_peaks(double[] array, int period, int max_peak_width, int min_front_width, int max_front_width, double min_peak_to_average_peak_ratio, int min_after_fall_magnitude, double min_relative_after_fall_magnitude)
    {
      peak[] peaks = get_peaks_internal(array, period, max_peak_width, min_front_width, max_front_width, min_peak_to_average_peak_ratio, min_after_fall_magnitude, min_relative_after_fall_magnitude);

      double[] positions = new double[peaks.Length];
      for (int i = 0; i < peaks.Length; ++i)
      {
        positions[i] = peaks[i].position;
      }
      return positions;
    }

    /// <summary>
    /// Данная функция принимает на вход массив array.
    /// period задает длину интервала (измеряемую в числе элементом массива), на котором производится поиск
    /// локальных максимумов. Для ЭКГ, period должен быть в 2 раза меньше, чем минимально возможный
    /// период сердцебиения (соответствующий пульсу 200 уд/мин, к примеру). Для ФПГ (сигнал типа "синус") - в 3-3.5 раза. 
    /// Устанавливать period слишком маленьким не рекомендуется - это ухудшит качество работы алгоритма и уменьшит скорость.
    /// </summary>
    /// <param name="array"></param>
    /// <param name="period"></param>
    /// <param name="max_peak_width">
    /// Слишком большое значение не приводит к деградации качества, но увеличивает время выполнения</param>
    /// <param name="min_front_width"></param>
    /// <param name="max_front_width"></param>
    /// <param name=param name="min_peak_to_average_ratio"></param>
    /// <param name="min_after_fall_magnitude"></param>
    /// <param name="min_relative_after_fall_magnitude"></param>
    /// <returns>массив, содержащий найденные пики</returns>
    private peak[] get_peaks_internal(
        double[] array, 
        int period,
        int max_peak_width,
        int min_front_width,
        int max_front_width,
        double min_peak_to_average_ratio,
        int min_after_fall_magnitude,
        double min_relative_after_fall_magnitude
      )
    {
      peak[] peaks;

      // центрирует массив для устранения постоянной составляющей
      center_array(array);

      // пометим области помех
      // помехой считаются слишком крутые фронты, а также области,
      // где средний прирост за секунду меньше стандартного отклонения сигнала
      // reject_interferences(array);

      // определяем приблизительную частоту пульса (в отсчетах, а не Герцах!)
      //double rough_freq = find_main_frequency(array);

#if DEBUG
      array_print_indexed_to_file(array, array.Length, 0, "signal.dat");
#endif

      // получаем "подозрительные" пики...
      peaks = extract_peaks_using_frame(array, period, peak_status.Undefined);

      log.Info($"Raw peaks detected: {peaks.Length}");

      // объединить пики, найденные на одних и тех же фронтах
      refine_distance(peaks, peak_status.Undefined, peak_status.DistanceOK);
      peaks = repackage_peaks(peaks, peak_status.DistanceOK);

      log.Info($"Refined peaks by distance. Remaining: {peaks.Length}");

      // удалить пики, не имеющие перед собой локальных минимумов
      // по определению, после предыдущего шага такой пик может остаться только в начале записи
      peaks_with_no_preceeding_local_minimum(array, peaks, peak_status.DistanceOK, peak_status.PreceedingLocalMinimumOK);
      peaks = repackage_peaks(peaks, peak_status.PreceedingLocalMinimumOK);

      log.Info($"Refined peaks with no preceeding local minimum. Remaining: {peaks.Length}");

      // удалить пики, не имеющие перед собой фронтов нормальной длины
      refine_front_length(peaks, peak_status.PreceedingLocalMinimumOK, peak_status.FrontLengthOK, min_front_width, max_front_width);
      peaks = repackage_peaks(peaks, peak_status.FrontLengthOK);

      log.Info($"Refined peaks by preceeding front duration. Remaining: {peaks.Length}");

      // удалить пики, не имеющие перед собой достаточно высоких фронтов
      refine_front_amplitude(peaks, peak_status.FrontLengthOK, peak_status.FrontAmplitudeOK, min_peak_to_average_ratio);
      peaks = repackage_peaks(peaks, peak_status.FrontAmplitudeOK);

      log.Info($"Refined peaks by relative front amplitude. Remaining: {peaks.Length}");

      // удалить:
      //   (1) пики, не имеющие после себя достаточно мощных спадов
      //   (2) пики, у которых амплитуда фронта значительно меньше,
      //       чем амплитуда последующего спада
      // Как показала практика, в некоторых записях ФПГ
      // достаточно сильно "плавает" базовая линия сигнала.
      // В этом случае пики, находящиеся на спаде базовой линии
      // часто незаслужено "бракуются" по пункуту (2)
      refine_front_after_fall_magnitude(peaks, peak_status.FrontAmplitudeOK, peak_status.AfterFallOK, min_after_fall_magnitude, min_relative_after_fall_magnitude);
      peaks = repackage_peaks(peaks, peak_status.AfterFallOK);

      log.Info($"Refined peaks by after-fall magnitude. Remaining: {peaks.Length}");

#if DEBUG
      peaks_print(peaks);
#endif

      // package only valid peaks
      return peaks;
    }

    /// <summary>
    /// centers array to remove constant
    /// </summary>
    /// <param name="data"></param>
    public void center_array(double[] data)
    {
      double sum = 0;
      for (int i = 0; i < data.Length; ++i)
      {
        sum += data[i];
      }
      sum /= ((double)data.Length);

      for (int i = 0; i < data.Length; ++i)
      {
        data[i] -= sum;
      }
    }

    /// <summary>
    /// находим приблизительную частоту пульса
    /// (как основную частоту периодического сигнала ФПГ)
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public double find_main_frequency(double[] data)
    {
      {
        int N;
        for (N = 2; N * 2 < data.Length && N*2 <= 4096; N *= 2);


        Exocortex.DSP.Complex[] fdata = new Exocortex.DSP.Complex[N];

        for (int ii = 0; ii < N; ++ii)
        {
          fdata[ii] = new Exocortex.DSP.Complex(data[ii], 0);
        }

        Exocortex.DSP.Fourier.FFT(fdata, N, Exocortex.DSP.FourierDirection.Forward);
        double max = System.Double.MinValue;
        int i_max = -1;
        double modulus;
        
        for (int i = 0; i < N; i += 2)
        {
          modulus = fdata[i].GetModulusSquared();
          if (modulus > max)
          {
            i_max = i;
            max = modulus;
          }
        }
        return ((double)fdata.Length) / ((double)i_max);
      }
    }

    private void peaks_with_no_preceeding_local_minimum(double[] array, peak[] peaks, peak_status source_status, peak_status target_status)
    {
      for (int i = 0; i < peaks.Length; ++i)
      {
        if( PeakHasPreceedingLocalMinimum(array, peaks[i]) )
        {
          if (peaks[i].status == source_status)
          {
            peaks[i].status = target_status;
          }
        }
      }
    }

    private bool PeakHasPreceedingLocalMinimum(double[] array, peak aPeak)
    {
      for (int i = aPeak.position - 1; i > 0; --i)
      {
        if (array[i - 1] > array[i])
        {
          return true;
        }
      }
      return false;
    }

    private void peaks_refine_afterfall_to_front_ration(peak[] peaks, double max_after_fall_to_front_ratio, peak_status source_status, peak_status target_status)
    {
      for (int i = 0; i < peaks.Length; ++i)
      {
        if ((peaks[i].after_fall_magnitude / peaks[i].front_amplitude) > max_after_fall_to_front_ratio)
        {
          if (peaks[i].status == source_status)
          {
            peaks[i].status = target_status;
          }
        }
      }
    }

    private void refine_front_length(peak[] peaks, peak_status source_status, peak_status target_status, int min_front_length, int max_front_length)
    {
      double average_front_length = 0.0;
      for (int i = 0; i < peaks.Length; ++i)
      {
        average_front_length += ((double)peaks[i].front_length);
      }
      average_front_length /= ((double)peaks.Length);


      for (int i = 0; i < peaks.Length; ++i)
      {
        if ((peaks[i].front_length > min_front_length) && (peaks[i].front_length < max_front_length))
        {
          if (peaks[i].status == source_status)
          {
            peaks[i].status = target_status;
          }
        }
      }
    }


    void refine_front_after_fall_magnitude(peak[] peaks, peak_status source_status, peak_status target_status, int min_after_fall_magnitude, double min_relative_after_fall_magnitude)
    {
      for (int i = 0; i < peaks.Length; ++i)
      {
        // не требовать мощного спада после пика -- этот метод плохо себя проявил на некоторых записях
        if( //(Math.Abs(peaks[i].after_fall_magnitude) >= min_after_fall_magnitude) &&
            //( (Math.Abs(peaks[i].after_fall_magnitude) / peaks[i].front_amplitude ) >= min_relative_after_fall_magnitude )
          // если спад в 2,5 и более раза больше пика -- значит, это точно инцезура
          // и этот фронт нельзя считать соответствующим сердечному сокращению (СС)
             Math.Abs(peaks[i].after_fall_magnitude) < (2.5 * Math.Abs(peaks[i].front_amplitude))
          )
        {
          if (peaks[i].status == source_status)
          {
            peaks[i].status = target_status;
          }
        }
      }
    }

    /// <summary>
    /// для каждого пика вычисляется средняя амплитуда ближайших пиков N пиков
    /// и пики с амплитудой, меньшей 0.3 средней величины удаляются.
    /// </summary>
    /// <param name="peaks"></param>
    /// <param name="source_status"></param>
    /// <param name="target_status"></param>
    private void refine_front_amplitude(peak[] peaks, peak_status source_status, peak_status target_status, double min_peak_to_average_ratio)
    {
      // усредняем по 'ave_count' пикам _ДО_ и 'ave_count - 1' пикам _ПОСЛЕ_ данного пика
      int ave_count = 4;

      // смещение для корректной обработки первых и последних интервалов
      int offset = 0;

      // сумма амплитуд пиков
      double sum_front_amp = 0;

      double[] average_amplitudes = new double[peaks.Length];

      for (int i = 0; i < peaks.Length; ++i)
      {
        if( i < ave_count )
        {
          offset = ave_count - i;
        }
        else if (i > peaks.Length - ave_count)
        {
          offset = peaks.Length - i - ave_count;
        }
        else
        {
          offset = 0;
        }

        sum_front_amp = 0;
        int averaged_count = 0;
        for (int j = i - ave_count + offset; (j < i + ave_count + offset) && (j < peaks.Length); ++j)
        {
          sum_front_amp += peaks[j].front_amplitude;
          ++averaged_count;
        }
        average_amplitudes[i] = sum_front_amp / averaged_count;
      }

      for (int i = 0; i < peaks.Length; ++i)
      {
        if (peaks[i].front_amplitude > (average_amplitudes[i] * min_peak_to_average_ratio ) )
        {
          if (peaks[i].status == source_status)
          {
            peaks[i].status = target_status;
          }
        }
      }
    }

    /// <summary>
    /// помечает как "хорошие" все "последние" пики на каждом фронте. 
    /// </summary>
    /// <param name="peaks"></param>
    /// <param name="source_status"></param>
    /// <param name="target_status"></param>
    private void refine_distance(peak[] peaks, peak_status source_status, peak_status target_status)
    {
      for (int i = 0; i < peaks.Length - 1; ++i)
      {
        if (peaks[i].status == source_status)
        {
          for (int j = i+1; j < peaks.Length; ++j)
          {
            if (peaks[j].position - peaks[j].front_length < peaks[i].position)
            {
              // peaks #i is not so good to update its status
              break;
            }
            else
            {
              // peak #i is ok (not overlapped by the following peak's front)
              peaks[i].status = target_status;
              break;
            }
          }
        }
      }
    }

    /// <summary>
    /// См. get_peaks
    /// </summary>
    /// <param name="array"></param>
    /// <param name="period"></param>
    /// <param name="max_peak_width"></param>
    /// <returns></returns>
    public double[] get_fronts(double[] array, int period, int max_peak_width, int min_front_width, int max_front_width, double min_peak_to_average_peak_ratio, int min_after_fall_magnitude, double min_relative_after_fall_magnitude)
    {
      double x, t;
      int j, i, i_max, i_min;

      System.Collections.Generic.List<double> fronts = new List<double>();

      // координаты пиков могут быть в дробных количествах отсчетов
      // как середина участка с максимумом первой производной
      // координаты пиков не отбрасываются по близости друг к другу
      // а отбраковываются по наличию или отсутствию перед ним фронта, а также по амплитуде этого фронта
      peak[] peaks = get_peaks_internal(array, period, max_peak_width, min_front_width, max_front_width, min_peak_to_average_peak_ratio, min_after_fall_magnitude, min_relative_after_fall_magnitude);

      // ищем середину к-го фронта нарастания
      // (середина фронта соответствует моменту времени когда значение сигнала
      // пересекло уровень (max + min) / 2
      for (j = 0; j < peaks.Length; j++)
      {
        i_max = (int) peaks[j].position;
        i_min = (int) (peaks[j].position - peaks[j].front_length);

        // just in case anything went wrong during truncation to integer
        if (i_max < 0)
        {
          i_max = 0;
        }
        if (i_max >= array.Length)
        {
          i_max = array.Length - 1;
        }

#if USE_MEDIAN_LINE_POINT
        #region looking for front's median line
        // середина фронта по высоте
        t = (array[i_min] + array[i_max]) * 0.5f;

        // for safety, first assumption
        x = 0.5f * (i_min + i_max);

        // ищем середину фронта по времени:
        for (i = i_min; i < i_max; i++)
        {
          if ((array[i] <= t) && (t <= array[i + 1]))
          {
            if (Math.Abs(array[i] - array[i + 1]) < (Math.Abs(array[i]) + Math.Abs(array[i + 1])) * 2 * double.Epsilon)
            {
              x = i + 0.5f;
            }
            else
            {
              x = i + (t - array[i]) / (array[i + 1] - array[i]);
            }

            break;
          }
        }
        #endregion
#else
        #region looking for maximum of derivative

        int count = i_max - i_min;
        double[] der = new double[count];

        for (int jj = i_min; jj < i_max; ++jj)
        {
          der[jj - i_min] = array[jj+1] - array[jj];
        }
        
        int der_max_i = 0;
        double der_max = der[0];
        for (int jj = 0; jj < count; ++jj)
        {
          if (der[jj] > der_max)
          {
            der_max_i = jj;
            der_max = der[jj];
          }
        }

        x = i_min + der_max_i;

        #endregion
#endif
        fronts.Add(x);
      }
      return fronts.ToArray();
    }

    #region implementation


#if DEBUG

    /// <summary>
    /// writes entire array to console - one element per line
    /// </summary>
    /// <param name="array"></param>
    void array_print(double[] array)
    {
      int n = array.Length;
      for (int i = 0; i < n; ++i)
      {
        System.Console.WriteLine(array[i]);
      }
    }

    /// <summary>
    /// writes n elements from start-th element to text file - one element per line
    /// </summary>
    /// <param name="array"></param>
    /// <param name="n"></param>
    /// <param name="start"></param>
    /// <param name="fname"></param>
    void array_print_indexed_to_file(double[] array, int n, int start, string fname)
    {
      System.IO.StreamWriter sw = new System.IO.StreamWriter(fname);
      for (; start < n; ++start)
      {
        sw.WriteLine(string.Format("{0} {1}", start, array[start]));
      }
      sw.Close();
    }
#endif

    #region Процедуры, связанные с нахождением и обработкой локальных максимумов

    enum peak_status
    {
      /// <summary>
      /// Статус не определен
      /// </summary>
      Undefined = 0,
      
      /// <summary>
      /// амплитуда пика достаточна
      /// </summary>
      FrontAmplitudeOK = 1,

      /// <summary>
      /// ширина фронта нормальная
      /// </summary>
      FrontLengthOK = 2,

      /// <summary>
      /// фронты нормальные (по длительности и по высоте)
      /// </summary>
      FrontOK = 3,
      
      /// <summary>
      /// расстояние до следующего пика с хорошим фронтом - нормальное
      /// (TODO: проверить процедуру проверки!!!!!)
      /// </summary>
      DistanceOK = 4,

      /// <summary>
      /// пик имеет нормальнй спад (по длительности и по высоте)
      /// </summary>
      AfterFallOK = 5,

      /// <summary>
      /// пик имеет предшествующий локальный минимум
      /// </summary>
      PreceedingLocalMinimumOK = 6
    }

    struct peak
    {
      public peak(peak src)
      {
        this.position = src.position;
        this.status = src.status;
        this.front_amplitude = src.front_amplitude;
        this.front_length = src.front_length;
        this.peak_length = src.peak_length;
        this.value = src.value;
        this.after_fall_magnitude = src.after_fall_magnitude;
      }

      public int position;
      public double value;
      public double front_amplitude;
      public double front_length;
      
      /// <summary>
      /// магнитуда "провала", следующего за пиком
      /// (без знака == со знаком "плюс"!!!)
      /// </summary>
      public double after_fall_magnitude;

      /// <summary>
      /// distance between previous and next local minimums of this peak
      /// </summary>
      public double peak_length;
      public peak_status status;
    }

#if DEBUG
    void peaks_print(peak[] peaks)
    {
      int i;
      for (i = 0; i < peaks.Length; i++)
      {
        System.Console.Error.WriteLine("{0} {1} {2}\n", peaks[i].status, peaks[i].position, peaks[i].value);
      }
    }
#endif

    /// <summary>
    /// searching peaks using frames of 'framesize' width
    /// </summary>
    /// <param name="array"></param>
    /// <param name="framesize"></param>
    /// <param name="set_status"></param>
    /// <returns></returns>
    peak[] extract_peaks_using_frame(double[] array, int framesize, peak_status set_status)
    {
      peak[] peaks = null;
      int i, j, offset, max_i, min_i, min_i2;
      int n = array.Length;
      int peaks_n = (n + framesize - 1) / framesize;
      peaks = new peak[peaks_n];

      for( i = 0; i < peaks_n; ++i )
      {
        offset = i * framesize;
        max_i = offset;
        for( j = 0; (j < framesize) && (j + offset < n); j++ )
        {
          if( array[max_i] < array[offset + j] )
          {
            max_i = offset + j;
          }
        }

        // calculate front's length and amplitude

        // find previous local minimum
        for (min_i = max_i - 1; (min_i >= 0) && (array[min_i] <= array[min_i + 1]); min_i--) { };
        if (min_i < 0)
        {
          min_i = max_i;
        }

        // find next local minimum (for measuring after_fall_magnitude)
        for (min_i2 = max_i + 1; (min_i2 < array.Length - 1) && (array[min_i2] <= array[min_i2 - 1]); min_i2++) { };

        // we must pass the real minimum value to leave the cycle, so this little fixup is required
        --min_i2;

        peaks[i].front_amplitude = array[max_i] - array[min_i];
        peaks[i].after_fall_magnitude = array[min_i2] -
                                                        array[max_i];
        peaks[i].front_length = max_i - min_i;
        peaks[i].peak_length = min_i2 - min_i;

        peaks[i].value = array[max_i];
        peaks[i].position = max_i;
        peaks[i].status = set_status;
      }

#if DEBUG
      // write peaks to file for analysis
      //System.Diagnostics.Debug.Fail("Write peaks to file for further analysis -- NOT IMPLEMENTED!");
#endif

      return peaks;
    }

    void peaks_reduce_close(peak[] peaks, int min_distance, peak_status status, peak_status set_status)
    {
      int i, j = 0;
      bool is_first = true;

      for (i = 0; i < peaks.Length; i++)
      {
        if (peaks[i].status != status)
        {
          continue;
        }

        if (is_first)
        {
          is_first = false;
          j = i;
          continue;
        }

        if (peaks[i].position - peaks[j].position < min_distance)
        {
          if (peaks[i].value > peaks[j].value)
          {
            peaks[j].status = set_status;
            j = i;
          }
          else
          {
            peaks[i].status = set_status;
          }
        }
        else
        {
          j = i;
        }
      }
    }

    /// <summary>
    /// Return number of peaks with spcecified status
    /// </summary>
    /// <param name="peaks"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    int peaks_count(peak[] peaks, peak_status status)
    {
      int c = 0;
      for (int n = peaks.Length - 1; n >= 0; --n)
      {
        if (peaks[n].status == status)
        {
          ++c;
        }
      }
      return c;
    }


    int peaks_find_threshold(peak[] peaks, ref double t, peak_status status)
    {
      int i, j;
      double[] stat;
      double sum, suml, s_max, s, p1, p2;
      int i_max, n;

      n = peaks_count(peaks, status);
      System.Diagnostics.Debug.Assert(n >= 2); // FIXME

      stat = new double[n];

      for (i = 0, j = 0; i < n; i++)
      {
        if (peaks[i].status == status)
        {
          stat[j++] = peaks[i].value;
        }
      }

      // we need to sort stat[] array...
      System.Array.Sort(stat);

#if DEBUG
      array_print_indexed_to_file(stat, stat.Length, 0, "stat.dat");
      for (i = 0; i < n; i++)
      {
        System.Console.Error.WriteLine("{0}", stat[i]);
      }
#endif

      sum = 0;
      for (i = 0; i < n; i++)
      {
        sum += stat[i];
      }
      // now we have sum of all peak values

      suml = stat[0];
      i_max = 1;
      s_max = 0;
      for (i = 1; i < n - 1; i++)
      {
        p1 = suml * suml / i;
        p2 = (sum - suml) * (sum - suml) / (n - i);
        s = p1 + p2;
#if DEBUG
        System.Console.Error.WriteLine("s = {0} {1} {2} {3}", i, s, p1, p2);
#endif
        if (s > s_max)
        {
          s_max = s;
          i_max = i;
        }
        suml += stat[i];
      }
#if DEBUG
      System.Console.Error.WriteLine("t = {0} {1}", i_max, stat[i_max]);
#endif
      t = (stat[i_max] + stat[i_max - 1]) * 0.5f;
      return 0;
    }

    /// <summary>
    /// Функция выясняет типичную ширину пика. downsample указывает погрешность ее определения,
    /// max_width - верхнюю границу поиска. downsample не должно быть меньше среднего расстояния
    /// между соседними точками, соответствующими одному пику.
    /// Возвращает ширину, или -1 в случае неудачи.
    /// </summary>
    /// <param name="peaks"></param>
    /// <param name="downsample">tolerance for peak width estimation</param>
    /// <param name="max_width">search upper boundary</param>
    /// <param name="status">peak status to be set</param>
    /// <returns></returns>
    int peaks_find_peak_width(peak[] peaks, int downsample, int max_width, peak_status status)
    {
      int i, j;
      double w;
      int n = peaks.Length;

      int bins_n = max_width / downsample + 1;
      if (bins_n <= 1)
      {
        // silly answer, but at least as good as the question was
        return downsample * bins_n;
      }

      double[] bins = new double[bins_n];

      for (i = bins_n - 1; i >= 0; i--)
      {
        bins[i] = 0;
      }

      for (i = 0; i < n; i++)
      {
        if (peaks[i].status != status)
        {
          continue;
        }
        for (j = i; (j < n) && (peaks[j].position - peaks[i].position <= max_width); j++)
        {
          if (peaks[j].status == status)
          {
            bins[(peaks[j].position - peaks[i].position) / downsample] += 1; //peaks[j].value*peaks[i].value;
          }
        }
      }
#if DEBUG
      array_print_indexed_to_file(bins, bins_n, 0, "width_stat.dat");
#endif


      for (i = 0; i < bins_n - 1; i++)
      {
        if (bins[i] < bins[0] * 0.2)
        {
          break;
        }
      }

      w = i * 1.2f * bins[0] / (bins[0] - bins[i]) * downsample + 0.4f * downsample + 1;
#if DEBUG
      System.Console.Error.WriteLine("w={0}", w);
#endif
      return (int)w;
    }

    void peaks_threshold(peak[] peaks, double threshold, peak_status status, peak_status set_status)
    {
      int n = peaks.Length;
      for (n--; n >= 0; n--)
      {
        if ((peaks[n].value > threshold) && (peaks[n].status == status))
        {
          peaks[n].status = set_status;
        }
      }
    }

    peak[] repackage_peaks(peak[] peaks, peak_status status)
    {
      int i, j, n;

      n = peaks_count(peaks, status);
      peak[] result = new peak[n];

      for (i = 0, j = 0; i < peaks.Length; i++)
      {
        if (peaks[i].status == status)
        {
          result[j++] = new peak(peaks[i]);
        }
      }
      return result;
    }

    double[] peaks_repackage(peak[] peaks, peak_status status)
    {
      int i, j, n;

      n = peaks_count(peaks, status);
      double[] result = new double[n];

      for (i = 0, j = 0; i < peaks.Length; i++)
      {
        if (peaks[i].status == status)
        {
          result[j++] = peaks[i].position;
        }
      }
      return result;
    }

    void peaks_change_status(peak[] peaks, peak_status status_in, peak_status status_out)
    {
      for (int i = 0; i < peaks.Length; ++i)
      {
        if (peaks[i].status == status_in)
        {
          peaks[i].status = status_out;
        }
      }
    }

    #endregion

    #endregion
  }
}
