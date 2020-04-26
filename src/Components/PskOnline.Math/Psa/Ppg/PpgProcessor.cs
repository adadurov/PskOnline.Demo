//#define DEBUG_FPG_PROCESSOR

using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Math.Psa.Ppg
{

  /// <summary>
  /// Параметры одного цикла ФПГ
  /// </summary>
  public class PpgCycleDescriptor
  {
    public PpgCycleDescriptor()
    {
    }

    public PpgCycleDescriptor(float hd1, float hd2, float cd, float s, float a, float t, float _time)
    {
      HD1 = hd1;
      HD2 = hd2;
      CD = cd;
      S = s;
      A = a;
      T = t;
      time = _time;
    }

    public float HD1;
    public float HD2;
    public float CD;
    public float S;
    public float A;
    public float T;
    public float time;
  };

  public class PpgFragmentDescriptor
  {

    public PpgFragmentDescriptor(
      PpgCycleDescriptor[] cycles,
      float prm,
      float kv,
      float am,
      float sm,
      float dwf,
      float bla)
    {
      CYCLES = cycles;
      PRM = prm;
      KV = kv;
      AM = am;
      SM = sm;
      DWF = dwf;
      BLA = bla;
    }

    PpgFragmentDescriptor()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public float PRM;

    /// <summary>
    /// 
    /// </summary>
    public float KV;

    /// <summary>
    /// 
    /// </summary>
    public float AM;

    /// <summary>
    /// 
    /// </summary>
    public float SM;

    /// <summary>
    /// Dicrotic wave factor.
    /// </summary>
    public float DWF;

    /// <summary>
    /// Baseline amplitude
    /// </summary>
    public float BLA;

    public PpgCycleDescriptor[] CYCLES;
  }

  /// <summary>
  /// Проводит обработку заданного фрагмента ФПГ-сигнала.
  /// </summary>
  public class PpgProcessor
  {
    /// <summary>
    /// Helper function for automagic signal processing.
    /// </summary>
    /// <param name="signal"></param>
    /// <param name="rate"></param>
    /// <returns></returns>
    public static PpgFragmentDescriptor ProcessSignal(float[] signal, float rate)
    {
      //float t, y;
      //float t1, tm, td, t2;
      //float y1, ym, yd, y2, a;
      //float HD1, HD2, CD, S, A, T, time;
      float PRM = 0, KV = 0, AM = 0, SM = 0, DWF = 0, BLA = 0;

      System.Collections.ArrayList list = new System.Collections.ArrayList();

      PpgProcessor ppg = new PpgProcessor(rate, 0.4f, 2.0f); // initialize processor

      for (int i = 0; i < signal.Length; ++i)
      {
        ppg.AddPoint(signal[i]);      // put next point into processor
        if( ppg.HasNewData )          // check if new data is available
        {
          // get next cardio cycle description
          list.Add(ppg.get_last_cycle_description());
        }
      }

      // get statistics for the whole period
      ppg.get_statistics(ref PRM, ref KV, ref AM, ref SM, ref DWF, ref BLA);

      return new PpgFragmentDescriptor(
        (PpgCycleDescriptor[])list.ToArray(typeof(PpgCycleDescriptor)),
        PRM,
        KV,
        AM,
        SM,
        DWF,
        BLA);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="point"></param>
    public void AddPoint(float point)
    {
      this.add_point(point);
    }

    /// <summary>
    /// 
    /// </summary>
    public bool HasNewData
    {
      get
      {
        // Собственно говоря, если выполнено первое, то второе должно быть всегда верно.
        return ( ! this.find_next ) && this.description_is_valid;
      }
    }

    #region private types

    struct ppg_cycle_description
    {
      public float HD1, HD2, CD;
      public float S;
      public float A;
      public float T;

      public int i1_abs, im_abs, id_abs, i2_abs;
      public float y1, ym, yd, y2;

      public int base_i_abs;
      public float base_y;

      public float periodicity;
    };

    struct ppg_statistics
    {
      public int n;
      public float T_sum;
      public float T_sum2; // Сумма квадратов
      public float A_sum;
      public float S_sum;
      public float CD_sum;
      public float base_min, base_max;
    };



    #endregion

    #region standard limits, initial values, 'a priori' settings

    /// <summary>
    /// Частота дискретизации, Гц
    /// </summary>
    float rate;

    /// <summary>
    /// Минимальный период, известен априори
    /// </summary>
    int min_period;

    /// <summary>
    /// Максимальный период, известен априори
    /// </summary>
    int max_period;

    /// <summary>
    /// Минимальная ширина волны ФПГ.
    /// </summary>
    int min_width;

    /// <summary>
    /// Минимальная периодичность сигнала.
    /// </summary>
    float min_periodicity;

    #endregion

    #region data buffers

    /// <summary>
    /// Cycled buffer for analyzed ppg signal
    /// </summary>
    float[] buffer;

    /// <summary>
    /// Index of the last element in buffer.
    /// This index is incremented after each new sample is added.
    /// 0 &lt;= buffer_i &lt; buffer.Length
    /// </summary>
    int buffer_i;

    /// <summary>
    /// Transitional buffer for HF-part of the signal.
    /// </summary>
    float[] hf;

    /// <summary>
    /// Transitional reusable buffer for various trash.
    /// Size must be enough to hold whole period of PPG (PPG cycle).
    /// </summary>
    float[] temp;
    #endregion

    #region measured and calculated stuff + flags

    /// <summary>
    /// Total count of samles processed.
    /// </summary>
    int n;

    /// <summary>
    /// Description of recently detected cycle.
    /// </summary>
    ppg_cycle_description description;

    /// <summary>
    /// Description of recently detected cycle.
    /// </summary>
    ppg_statistics statistics;

    /// <summary>
    /// Are 'description' and 'statistics' members valid, or not.
    /// </summary>
    bool description_is_valid;

    /// <summary>
    /// If any lags have been detected by now.
    /// If they have been, then last impulse's time cannot be used to calculate period.
    /// </summary>
    bool had_lag;

    /// <summary>
    /// Indicates estimated count of samples in buffer when it makes sence to start next cycle search procedure.
    /// </summary>
    int abs_idx_of_next_check;

    /// <summary>
    /// Индекс, соответствующий последнему пику.
    /// </summary>
    int abs_idx_of_latest_peak;

    /// <summary>
    /// Индекс, соответствующий последнему пику. 
    /// </summary>
    float abs_idx_of_latest_peak_precise;  

    /// <summary>
    /// 
    /// </summary>
    int latest_period;

    /// <summary>
    /// Разрешить анализировать дальше (насчитанные данные забраны)
    /// </summary>
    bool find_next;         

    #endregion


    #region implementation

    private static readonly int FPG_PROCESSOR_KEEP_MIN_PERIODS = 3;
    private static readonly int FPG_PROCESSOR_KEEP_WINDOW_PERIODS = 2;


    public PpgProcessor(float rate, float min_period, float max_period)
    {
      this.rate = rate;
      this.min_period = (int)(rate * min_period);
      this.max_period = (int)(rate * max_period);
      this.buffer = new float[(int)(rate * max_period * FPG_PROCESSOR_KEEP_MIN_PERIODS)];
      this.hf = new float[(int)(rate * max_period * FPG_PROCESSOR_KEEP_WINDOW_PERIODS)];
      this.temp = new float[(int)(rate * max_period)];
      this.set_min_width(min_period * 0.5f);
      this.Reset();
    }

    void Reset()
    {
      this.had_lag = true;
      this.abs_idx_of_latest_peak = -1;
      this.abs_idx_of_next_check = this.max_period * 2; //Hardcoded constant 
      this.buffer_i = this.buffer.Length - 1;
      this.n = 0;
      this.find_next = true;
      this.description_is_valid = false;
      this.statistics.n = 0;
      this.statistics.T_sum = 0;
      this.statistics.T_sum2 = 0;
      this.statistics.A_sum = 0;
      this.statistics.S_sum = 0;
      this.statistics.CD_sum = 0;
      this.set_min_periodicity(0.55f);
    }

    void set_min_periodicity(float periodicity)
    {
      this.min_periodicity = periodicity;
    }

    void set_min_width(float width)
    {
      this.min_width = System.Math.Max(0, (int)(this.rate * width));
    }

    /// <summary>
    /// </summary>
    /// <param name="y">массив</param>
    /// <param name="i0">индекс, с которого надо начинать (самый последний элемент)</param>
    /// <param name="valid_n">число элементов, которые можно и нужно обработать, valid_n &lt;= n</param>
    /// <param name="minshift">минимальная величина сдвига (&lt;n)</param>
    /// <param name="maxshift">максимальная величина сдвига (&lt;n)</param>
    /// <param name="step_fold">шаг при нахождении свертки (&lt;n)</param>
    /// <param name="step_ac">шаг сдвига при расчете автокорреляции (&lt;n)</param>
    /// <param name="buffer">буфер для результата</param>
    /// <returns></returns>
    void compute_autocorrelation(float[] y, int i0, int valid_n, int minshift, int maxshift, int step_fold, int step_ac, float[] buffer)
    {
      int shift;
      float sum_xy, sum_x, sum_y, y1, y2;
      int sum_n, i1, i2;
      int breaker;

      int buf_i = 0;

      for (shift = minshift; shift <= maxshift; shift += step_ac)
      {
        sum_xy = 0; sum_x = 0; sum_y = 0;
        sum_n = 0;

        i1 = i0;
        i2 = (i0 - shift + n + n) % n;
        for (breaker = valid_n - shift; breaker > 0; breaker -= step_fold)
        {
          y1 = y[i1];
          y2 = y[i2];
          sum_xy += y1 * y2;
          sum_x += y1;
          sum_y += y2;

          i1 -= step_fold;
          i2 -= step_fold;
          if (i1 < 0)
          {
            i1 = n - 1;
          }
          if (i2 < 0)
          {
            i2 = n - 1;
          }
          sum_n++;
        }

        buffer[buf_i] = (sum_xy - sum_x * sum_y / sum_n) / sum_n;
        buf_i++;
      }
    }

    void fpg_linear_fit(float[] y, int start, int n, int i_end, int do_n, ref float a, ref float b)
    {
      int i, j;
      float sx = 0, sxx = 0, sy = 0, sxy = 0;

      i = i_end;
      for (j = do_n - 1; j >= 0; j--)
      {
        sy += y[i];
        sxy += y[i] * j;
        i--;
        if (i < 0)
        {
          i += n;
        }
      }

      sxx = (do_n - 1) * (do_n - 0.5f) * do_n / 3.0f;
      sx = (do_n - 1) * do_n / 2;
      a = (do_n * sxy - sx * sy) / (do_n * sxx - sx * sx);
      b = (sxx * sy - sx * sxy) / (do_n * sxx - sx * sx);
    }


    /// <summary>
    /// Находит период путем расчета автокорреляции.
    /// Одна из трудностей - автокорелляционная функция будет также периодической, как и входной сигнал.
    /// Сначала считается грубое приближение, которое затем итеративно уточняется.
    /// Это в несколько раз быстрее, чем полный поиск. 
    /// end_idx - индекс последнего элемента
    /// </summary>
    /// <param name="count">сколько сэмплов обработать</param>
    /// <param name="period_"></param>
    /// <param name="crap_ratio"></param>
    /// <returns></returns>
    bool estimate_period(int count, ref int period_, ref float crap_ratio)
    {
      int step_fold, step_ac;
      int minshift, maxshift;
      int i, i1, cor_n;

      int period = 0;      // It cannot be used uninitialized in this function.
      int max_cor_i = 0;

      float max_cor, threshold;

#if DEBUG_FPG_PROCESSOR 
	FILE *out;
#endif

      if (count <= this.max_period)
      {
        return false;
      }

      step_ac = 1; // Math.Max(1, this.min_width/4);
      step_fold = step_ac;

      minshift = this.min_period;
      maxshift = System.Math.Min(this.max_period, count / 2);

#if DEBUG_FPG_PROCESSOR
	out = fopen("tmp_estimate_period.dat", "w");
	if(out==NULL)
  {
		perror("");
		exit(-1);
	}
#endif

      while (step_ac != 0)
      {
        cor_n = (maxshift - minshift) / step_ac;
        compute_autocorrelation(this.hf, count - 1, count, minshift, maxshift, step_fold, step_ac, this.temp);

#if DEBUG_FPG_PROCESSOR 
		for(i=0; i<cor_n; i++)
			fprintf(out, "%i %g\n", i*step_ac + minshift, this.temp[i]);
#endif

        max_cor = this.temp[0];  // Находим максимум

        for( i = 1; i < cor_n; i++ )
        {
          if (this.temp[i] > max_cor)
          {
            max_cor = this.temp[i];
          }
        }

        threshold = max_cor * 0.7f; // Ищем самую первую точку, в которой значение будет в 2 раза меньше пикового
        for (i = 0; i < cor_n; i++)
        {
          if (this.temp[i] >= threshold)
          {
            break;
          }
        }

        period = minshift + i * step_ac; // Находим период в 1.5 раза больший, и соответствующий ему индекс i1. Ищем на интервале от i до i1 локальный максимум.
        i1 = (period + period / 2 - minshift) / step_ac;
        max_cor_i = i;
        for (; (i <= i1) && (i < cor_n); i++)
        {
          if (this.temp[max_cor_i] < this.temp[i])
          {
            max_cor_i = i;
          }
        }

        period = minshift + max_cor_i * step_ac; // Оценка периода.
        //		printf("%d\n", period);

        minshift = System.Math.Max(period - step_ac - 1, 0);
        maxshift = System.Math.Min(period + step_ac + 1, this.max_period);

        step_ac /= 2;
        step_fold = System.Math.Max(1, step_fold / 2);
      }

      period_ = period;

      float[] crap_ratio_buf = new float[1] { 0 };
      compute_autocorrelation(this.hf, count - 1, count, 0, 0, 1, 1, crap_ratio_buf);
      crap_ratio = this.temp[max_cor_i] / crap_ratio_buf[0];
#if DEBUG_FPG_PROCESSOR
	fclose(out);
#endif

      return true;
    }

    /// <summary>
    /// поиск максимума на участке массивав data, длиной n и первым элементом start.
    /// </summary>
    /// <param name="data">array to search in</param>
    /// <param name="start">index of the element to start from</param>
    /// <param name="n">count of elements to be searched</param>
    /// <param name="max">result: value of the maximum element</param>
    /// <param name="i_max">result: index of the maximum element</param>
    void find_location_of_max(float[] data, int start, int n, ref float max, ref int i_max)
    {
      if (n <= 0)
      {
        throw new ArgumentException("n");
      }

      i_max = start;
      max = data[i_max];

      for( int i = start + 1; i < start + n; ++i )
      {
        if (data[i] > max )
        {
          i_max = i;
          max = data[i];
        }
      }
    }

    bool fpg_find_location_of_min_max(float[] data, int n, ref int i_min_, ref int i_max_)
    {
      int i_max = 0, i_min = 0;

      if (n <= 0)
      {
        return false;
      }

      for (n--; n >= 0; n--)
      {
        if (data[i_min] > data[n])
        {
          i_min = n;
        }

        if (data[i_max] < data[n])
        {
          i_max = n;
        }
      }

      i_min_ = i_min;
      i_max_ = i_max;

      return true;
    }


    // Находится смещение сетки пиков, считая от самого последнего введенного сэмпла.
    bool estimate_displacement(int period, int end_idx, int periods, ref int shift, ref int l_width, ref int r_width)
    {
      int i, j, half, offset, count;
      int i_min = 0, i_max = 0;
      float dy, threshold;

#if DEBUG_FPG_PROCESSOR 
	FILE *fout;

	fout = fopen("tmp_estimate_displacement.dat", "w");
	if(fout==NULL){
		perror("");
		exit(-1);
	}
#endif

      periods = System.Math.Min(periods, this.n / period); // FIXME: redundant

      for (i = period - 1; i >= 0; i--)
      {
        this.temp[i] = 0;
      }

      j = 0;
      i = end_idx;
      count = periods * period - 1;
      while (periods > 0)
      {
        this.temp[j] += this.hf[i];
        //		printf("%d %f\n", count, this.buffer[i]);

        i--;
        if (i < 0)
        {
          i += this.hf.Length;
        }

        j++;
        if (j == period)
        {
          j = 0;
          periods--;
        }
        count--;
      }

#if DEBUG_FPG_PROCESSOR 
	for(i=period-1; i>=0; i--)
		fprintf(fout, "%d %f\n", i, this.temp[i]);
#endif

      if( ! fpg_find_location_of_min_max(this.temp, period, ref i_min, ref i_max) )
      {
        return false;
      }

      shift = i_max;
      half = period; ///2;
      offset = this.min_width / 2;

      threshold = this.temp[i_max] - 0.2f * (this.temp[i_max] - this.temp[i_min]);

      for (i = i_min + period; true; i-- )
      {
        dy = -(this.temp[(i + 1) % period] + this.temp[(i - 1) % period]);
        if ((dy < 0) && (this.temp[(i - 1) % period] > threshold))
        {
          shift = (i - 1) % period;
          break;
        }

        if (i % period == i_max)
        {
          shift = i_max;
          break;
        }
      }

      l_width = (i_min - shift + period) % period;
      r_width = period - l_width;

#if DEBUG_FPG_PROCESSOR
	fclose(fout);
#endif

      //	printf("%d %d %d\n", this.min_width, *l_width, *r_width);

      return true;
    }

    int get_absolute_position(int i)
    {
      if (i <= this.buffer_i)
      {
        return this.n - this.buffer_i + i - 1;
      }
      else
      {
        return this.n - this.buffer_i - 1 - (this.buffer.Length - i);
      }
    }


    public delegate bool CompareDelegate(float a, float b);

    bool float_less(float a, float b)
    {
      return a < b;
    }

    bool float_greater(float a, float b)
    {
      return a > b;
    }

    int refine_extremum(int i0, int halfwidth, CompareDelegate compare_delegate)
    {
      int i1, i2, i_abs, i_ext;

      i_abs = this.get_absolute_position(i0);
      i1 = System.Math.Max(this.n - this.buffer.Length, i_abs - halfwidth) % this.buffer.Length;
      i2 = (System.Math.Min(this.n, i_abs + halfwidth) + 1) % this.buffer.Length;

      i_ext = i0;
      do
      {
        if (compare_delegate(this.buffer[i1], this.buffer[i_ext]))
        {
          i_ext = i1;
        }

        i1++;
        if (i1 >= this.buffer.Length)
        {
          i1 = 0;
        }
      } while (i1 != i2);

      return i_ext;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="i0"></param>
    /// <returns></returns>
    float refine_extremum_cubic(int i0)
    {
      int i_abs;
      float y1, y2, y3, dx;

      i_abs = this.get_absolute_position(i0);
      if ((i_abs - 1 < this.n - this.buffer.Length) && (i_abs + 1 >= this.n))
        return i_abs;

      y1 = this.buffer[(this.buffer.Length + i0 - 1) % this.buffer.Length];
      y2 = this.buffer[i0];
      y3 = this.buffer[(i0 + 1) % this.buffer.Length];

      if (System.Math.Abs(y3 - 2 * y2 + y1) < float.MinValue * System.Math.Abs(y2))
      {
        return i_abs;
      }

      dx = (y3 - y1) / (y3 - 2 * y2 + y1);
      if ((dx < -1) || (dx > 1)) // Never happens.
      {
        return i_abs;
      }

      return i_abs + dx;
    }

    float average(int i_abs, int n)
    {
      int i, i1_abs, i2_abs, counter;
      float sum = 0;

      i1_abs = System.Math.Max(0, i_abs - n / 2);
      i2_abs = System.Math.Min(this.n - 1, i_abs + n - n / 2);

      n = i2_abs - i1_abs + 1;
      i = i1_abs % this.buffer.Length;
      for (counter = n; counter > 0; counter--)
      {
        sum += this.buffer[i];
        if (i >= this.buffer.Length)
          i = 0;
      }
      return sum / n;
    }

    bool measure_impulse_parameters(int i1_abs, int im_abs, float im_abs_precise, int id_abs, int i2_abs, int period, float periodicity)
    {
      int i1, im, i2, id;
      int i, j, avg_num = 1;
      float y1, ym, yd, y2;

      i1 = i1_abs % this.buffer.Length;
      im = im_abs % this.buffer.Length;
      id = id_abs % this.buffer.Length;
      i2 = i2_abs % this.buffer.Length;

      //	printf("i1i2 - %d %d\n", i1_abs, i2_abs);
      y1 = average(i1_abs, avg_num);
      ym = average(im_abs, avg_num);
      yd = average(id_abs, avg_num);
      y2 = average(i2_abs, avg_num);

      this.description.i1_abs = i1_abs;
      this.description.im_abs = im_abs;
      this.description.id_abs = id_abs;
      this.description.i2_abs = i2_abs;

      this.description.y1 = y1;
      this.description.ym = ym;
      this.description.yd = yd;
      this.description.y2 = y2;

      this.description.base_i_abs = i2_abs;             //TODO: может надо что-нибудь поинтеллектуальнее?
      this.description.base_y = y2;

      this.description.HD1 = ym - yd;
      this.description.HD2 = yd - y2;
      this.description.CD = (this.description.HD1 > 0) ? this.description.HD2 / this.description.HD1 : -1; // Ну, это на всякий случай.
      this.description.A = ym - (y2 - y1) * (im_abs_precise - i1_abs) / (i2_abs - i1_abs);
      this.description.periodicity = periodicity;

      this.description.S = 0;
      for (i = i1, j = i2_abs - i1_abs; j >= 0; j--)
      {
        this.description.S += this.buffer[i1];
        i1++;
        if( i1 >= this.buffer.Length)
        {
          i1 = 0;
        }
      }
      this.description.S -= 0.5f * (y1 + y2) * (i2_abs - i1_abs + 1.0f);
      this.description.S /= this.rate;


      if (this.had_lag)
      {
        this.description.T = (i2_abs - i1_abs) / this.rate;
      }
      else
      {
        this.description.T = (im_abs_precise - this.abs_idx_of_latest_peak_precise) / this.rate;
      }

      return true;
    }

    void yield_statistics()
    {
      this.statistics.T_sum += this.description.T;
      this.statistics.T_sum2 += this.description.T * this.description.T;
      this.statistics.A_sum += this.description.A;
      this.statistics.S_sum += this.description.S;
      this.statistics.CD_sum += this.description.CD;

      if (this.statistics.n == 0)
      {
        this.statistics.base_min = this.description.base_y;
        this.statistics.base_max = this.description.base_y;
      }
      else
      {
        if (this.statistics.base_min > this.description.base_y)
        {
          this.statistics.base_min = this.description.base_y;
        }

        if (this.statistics.base_max < this.description.base_y)
        {
          this.statistics.base_max = this.description.base_y;
        }
      }

      this.statistics.n++;
    }

    int get_statistics(ref float PRM, ref float KV, ref float AM, ref float SM, ref float DWF, ref float BLA)
    {
      if (this.statistics.n < 2)
      {
        return -1;
      }

      if (this.statistics.T_sum == 0)
      {
        return -1;
      }

      PRM = 60.0f * this.statistics.n / this.statistics.T_sum;
      KV = (float) (100.0 * System.Math.Sqrt((this.statistics.T_sum2 - this.statistics.T_sum * this.statistics.T_sum / this.statistics.n) / (this.statistics.n - 1)) * this.statistics.n / this.statistics.T_sum); //TODO: Check the formula!!!
      AM = this.statistics.A_sum / this.statistics.n;
      SM = this.statistics.S_sum / this.statistics.n;
      DWF = this.statistics.CD_sum / this.statistics.n;
      BLA = this.statistics.base_max - this.statistics.base_min;

      return 0;
    }

    bool set_frame(int center, int len, ref int i1_abs, ref int i2_abs)
    {
      int j1_abs, j2_abs, buffer_left_abs;

      buffer_left_abs = System.Math.Max(0, (this.n - this.buffer.Length)); // Индекс самого раннего элемента в буффере

      if (this.abs_idx_of_latest_peak < 0)
      {
        j1_abs = buffer_left_abs;  // двигаем окно в самое начало
        j2_abs = System.Math.Min(this.n - 1, j1_abs + len - 1);       // его конец
      }
      else
      {
        j1_abs = center - len / 2;
        j2_abs = j1_abs + len - 1;
        if (j1_abs < buffer_left_abs)
        {
          j1_abs = buffer_left_abs;
          j2_abs = System.Math.Min(this.n - 1, j1_abs + len - 1);
        }
        else if (j2_abs > this.n - 1)
        {
          j2_abs = this.n - 1;
          j1_abs = System.Math.Max(j2_abs - len + 1, buffer_left_abs);
        }
      }
      if (j2_abs - j1_abs < len - 1)
      {
        return false; // Пока что недостаточно данных
      }

      i1_abs = j1_abs;
      i2_abs = j2_abs;
      return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="array"></param>
    /// <param name="start"></param>
    /// <param name="array_n"></param>
    /// <param name="i1_abs"></param>
    /// <param name="i2_abs"></param>
    /// <param name="width"></param>
    /// <param name="_out"></param>
    void fpg_hf_pass(float[] array, int start, int array_n, int i1_abs, int i2_abs, int width, float[] _out)
    {
      int n, j, shift;
      float sum;
      int summed;

      shift = width / 2;

      sum = 0;
      summed = 0;
      n = i2_abs - i1_abs + 1;
      for (j = 0; j < n + width; j++)
      {
        if (j < n)
        {
          sum += array[(i1_abs + j) % array_n];
          summed++;
        }
        if (j >= width)
        {
          sum -= array[(i1_abs + j - width) % array_n];
          summed--;
        }

        if ((j >= shift) && (j < n + shift))
        {
          _out[j - shift] = array[(i1_abs + j - shift) % array_n] - sum / summed;
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="i1_abs"></param>
    /// <param name="i2_abs"></param>
    /// <param name="width"></param>
    /// <param name="_out"></param>
    /// <returns></returns>
    int hf_pass(int i1_abs, int i2_abs, int width, float[] _out)
    {

#if DEBUG_FPG_PROCESSOR 
	    int i;
	    FILE *fout;
    	fout = fopen("tmp_hf.dat", "w");
#endif

  fpg_hf_pass(this.buffer, 0, this.buffer.Length, i1_abs, i2_abs, width, _out);

#if DEBUG_FPG_PROCESSOR 
	    for(i=0; i <= i2_abs - i1_abs; i++)
      {
		    fprintf(fout, "%d %f\n", i, out[i]);
      }
	    fclose(fout);
#endif

      return 0;
    }


    bool find_dicrotic(int i1, int i2, ref int position)
    {
      int i, j, k;
      int i1_abs, i2_abs;
      int n;
      float[] diff = new float[] { -3, -2, -1, 0, 1, 2, 3 }; //Производная
      int diff_len = 7;
      int offset;
      float stub = 0;

#if DEBUG_FPG_PROCESSOR 
	FILE *fout;

	fout = fopen("tmp_find_dicrotic.dat", "w");
	if(fout==NULL){
		perror("");
		exit(-1);
	}
#endif
      i1_abs = get_absolute_position(i1);
      i2_abs = get_absolute_position(i2);

      offset = diff_len / 2;
      n = i2_abs - i1_abs + 1 - offset * 2; // Столько точек всего

      if (n <= 0)
      {
        return false;
      }

      fpg_hf_pass(this.buffer, 0, this.buffer.Length, i1_abs, i2_abs, (i2_abs - i1_abs) / 3, this.hf);

      i = i1;
      for (j = 0; j < n; j++)
      {
        this.temp[j] = 0;
        for (k = 0; k < diff_len; k++)
        {
          this.temp[j] += this.hf[j + k] * diff[k];
        }
        //			this.temp[j] += this.buffer[(i+j+k)%this.buffer.Length]*diff[k];

#if DEBUG_FPG_PROCESSOR 
		fprintf(fout, "%d %f\n", j, this.temp[j]);
#endif
      }

      find_location_of_max(this.temp, 0, n, ref stub, ref position);
      position = (position + offset + i1) % this.buffer.Length;

#if DEBUG_FPG_PROCESSOR
	fclose(fout);
#endif

      return true;
    }



    void add_point(float x)
    {
      int period = 0, shift = 0, l_width = 0, r_width = 0, i1, i2, im, id = 0;
      int interval, center, peak_number, im_abs_expected;
      int jump;

      int i1_abs, i2_abs, im_abs, id_abs;
      int j1_abs = 0, j2_abs = 0;
      float periodicity = 0;
      float im_abs_precise;

      this.n++;

      this.buffer_i++;
      if (this.buffer_i >= this.buffer.Length)
      {
        this.buffer_i = 0;
      }

      this.buffer[this.buffer_i] = x;

      //	fprintf(stderr, "0 0\n\n");

      if ((this.abs_idx_of_next_check <= this.n) && (this.find_next))
      {
        jump = this.min_period / 2;
        //	if( ( this.find_next ) &&
        //		((this.n % (this.min_period/3) == 0)||(this.extra_processing)) 	){ // Hardcoded constant!

        //	if(this.n == 190){
        do
        { // Dummy loop. Well, it's not a loop, actually.
          //			this.extra_processing = 0;

          interval = System.Math.Max(2 * this.max_period, this.hf.Length);   // Hardcoded constant

          if (this.abs_idx_of_latest_peak > 0)
          {
            center = this.abs_idx_of_latest_peak + this.latest_period;
          }
          else
          {
            center = 0;
          }

          if( ! set_frame(center, interval, ref j1_abs, ref j2_abs) )
          {
            break;
          }

          //		printf("xxx %d %d %d %d\n", j1_abs, j2_abs, (int)(this.max_period*0.8), this.n);
          hf_pass(j1_abs, j2_abs, (int)(this.max_period * 0.8f), this.hf);
          if ( ! estimate_period(interval, ref period, ref periodicity) )
          {
            break;
          }

          //			fprintf(stdout, "periodicity = %f\n", periodicity);
          if (periodicity < this.min_periodicity)
          {
            jump = this.min_period;
            this.had_lag = true;
            break;
          }

          //		printf("period = %d\n", period);
          if( ! estimate_displacement(period, interval - 1, interval / period, ref shift, ref l_width, ref r_width) )
          {
            break;
          }

          //		printf("lw, rw = %d %d\n", l_width, r_width);

          if (this.abs_idx_of_latest_peak < 0)
          {
            peak_number = (j2_abs - j1_abs + 1 - l_width - shift - period) / period;
          }
          else
          {
            peak_number = (j2_abs - System.Math.Max(j1_abs, this.abs_idx_of_latest_peak + period / 2) + 1 - l_width - shift) / period;
          }

          if (peak_number > 1) // FIXME: тут надо 0
          {
            jump = 1;
          }
          //				this.extra_processing = 1;

          im_abs_expected = j2_abs - peak_number * period - shift;

          //		printf("i_e = %d %d\n", im_abs_expected, peak_number);

          if (this.n - im_abs_expected - period * 1.5f <= 0) // Правый край получен не весь, надо подождать. Hardcoded constant!
          {
            break;
          }

          //		printf("%d %d %d %d %d\n", this.n - shift, period, shift, l_width, r_width);
          im = refine_extremum( im_abs_expected % this.buffer.Length, period / 8, new CompareDelegate(float_greater));
          im_abs = get_absolute_position(im);

          if ((this.abs_idx_of_latest_peak >= 0) && (System.Math.Abs(im_abs - this.abs_idx_of_latest_peak) < period / 3))
          {
            break;
          }

          if (this.n <= im_abs + r_width + period / 6 + 1) // Правый край получен не весь, надо подождать. Hardcoded constant!
          {
            break;
          }

          if (im_abs - l_width - period / 8 <= 0)
          {
            // Такое нам тоже не нужно
            this.abs_idx_of_latest_peak = im_abs;
            break;
          }

          i1 = refine_extremum(im_abs - l_width % this.buffer.Length, period / 5, float_less);
          //TODO : need something better here 
          
          i2 = refine_extremum(im_abs + r_width % this.buffer.Length, period / 5, float_less);

          i1_abs = get_absolute_position(i1);
          i2_abs = get_absolute_position(i2);

          if (! find_dicrotic(im, i2, ref id) )
          {
            break;
          }

          id_abs = get_absolute_position(id);


          im_abs_precise = refine_extremum_cubic(im); //What a mess...
          if( ! measure_impulse_parameters(i1_abs, im_abs, im_abs_precise, id_abs, i2_abs, period, periodicity) )
          {
            break;
          }

          yield_statistics();

          this.abs_idx_of_latest_peak_precise = im_abs_precise;
          this.abs_idx_of_latest_peak = im_abs;
          this.latest_period = period;
          this.description_is_valid = true;
          this.find_next = false;
          this.had_lag = false;

          if (jump != 1)
          {
            jump = period;
          }

        }
        while (false);
        this.abs_idx_of_next_check = this.n + jump;
      }
    }

    PpgCycleDescriptor get_last_cycle_description()
    {
      if( ! this.description_is_valid )
      {
        throw new Exception(strings.get_last_cycle_description_no_data_available);
      }

      PpgCycleDescriptor cycle = new PpgCycleDescriptor();

      cycle.HD1 = this.description.HD1;
      cycle.HD2 = this.description.HD2;
      cycle.CD = (this.description.HD1 > 0) ? this.description.HD2 / this.description.HD1 : -1; // just in case... to avoid division by zero
      cycle.S = this.description.S;
      cycle.A = this.description.A;
      cycle.T = this.description.T;
      cycle.time = this.description.im_abs / this.rate;

      return cycle;
    }

    int get_coordinates(ref float i1, ref float im, ref float id, ref float i2, ref float y1, ref float ym, ref float yd, ref float y2)
    {
      if (!this.description_is_valid)
      {
        return -1;
      }

      i1 = this.description.i1_abs / this.rate;
      im = this.description.im_abs / this.rate;
      id = this.description.id_abs / this.rate;
      i2 = this.description.i2_abs / this.rate;

      y1 = this.description.y1;
      ym = this.description.ym;
      yd = this.description.yd;
      y2 = this.description.y2;

      return 0;
    }

    int get_baseline_point(ref float t, ref float y)
    {
      if (!this.description_is_valid)
      {
        return -1;
      }

      t = this.description.base_i_abs / this.rate;
      y = this.description.base_y;
      return 0;
    }

    //bool get_periodicity(ref float p)
    //{
    //  if (!this.description_is_valid)
    //  {
    //    return false;
    //  }

    //  p = this.description.periodicity;
    //  return 0;
    //}

    void mark_data_as_read()
    {
      this.find_next = true;
    }

    #endregion

  }
}
