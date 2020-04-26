namespace PskOnline.Methods.Hrv.Processing.Logic
{
  using System;
  using System.Collections.Generic;
  using PskOnline.Methods.Hrv.ObjectModel;
  using PskOnline.Methods.ObjectModel.PhysioData;
  using PskOnline.Methods.Processing.Contracts;

  /// <summary>
  /// 03.07.2009 adadurov
  /// Обнаружено, что при следующих условиях происходит сбой отбраковки:
  /// Первый интервал в записи -- экстрасистола 1200 мс, последующие около 750 мс,
  /// поэтому алгоритм отбраковки в своей текущей версии выбирает в качестве
  /// достоверных сердечные сокращения "через одно", формируя интервалы
  /// длительностью около 1500 мс.
  ///   То есть получилось так, что длительность первого интервала (с экстрасистолой)
  ///   укладывается в 35-процентный допуск на относительное изменение кардио-интервалов
  ///   (первый интервал сравнивается со средним интервалом!), а длительность
  ///   второго интервала по сравнению с первым -- не укладывается в допустимые 35%
  ///   допуск.
  ///   В результате:
  ///   а) средний интервал до отбраковки существенно отличается
  ///   от среднего интервала после отбраковки (отношение приблизительно равно 2).
  /// 
  ///   Предлагаемые решения:
  ///      I.
  ///         1. Если (а), то отбросить первое сердечное сокращение и пересчитать.
  ///         2. Повторять (1) (отбрасывать еще по одному сокращению), пока
  ///            ситуация не улучшится (какая погрешность допустима?), но не более X раз
  ///            (чему равен X)?
  ///     II.
  ///         1. Попробовать начать с конца? Но такая же ситуация возможна и в конце файла...
  ///
  ///    III. 06/07/2009 Реализовано.
  ///         При проверке относительного изменения до обнаружения первого достоверного интервала
  ///         принимать величину предыдущего интервала равной моде черновой последовательности
  ///         кардио-интервалов.
  /// 
  /// 11.11.2009 adadurov
  /// Следующая проблема при полностью отключенной в настройках отбраковке
  ///   обнаруживается следующими кейсами модульного теста BasicCardioDataProcessor_UTest:
  ///       Test_File_2957_With_And_Without_Peaks_Rejection()
  ///       test_unit_test_data_file_number_5_with_and_without_peaks_rejection()
  /// Описание проблемы:
  ///   В записи нормальные интервалы около 750 мс, затем ближе к концу экстрасистола с отстутствием
  ///   пульсовой волны, затем нормальное сокращение сердца, получается интервал между пульсомвыми 
  ///   волнами около 1200 мс. Затем до конца файла нормальные интервалы около 750 мс -- в результате
  ///   после экстрасистолы фильтр выбирает интервалы по 1500 мс, а не по 750.
  /// 
  /// </summary>
  class PeaksFilter
  {
    /// <summary>
    /// Разрешено ли применения нового правила (п.4) описания работы фильтра.
    /// </summary>
    static bool EnableStepBackOnSuccessiveFailures = true;

    /// <summary>
    /// Создает из массива сердечных сокращений массив кардио-интервалов с указанием положения интервала
    /// в записи в секундах.
    /// При анализе учитываются возможные пропуски сердечных сокращений в областях некачественного сигнала.
    /// 
    /// После построения ряда сердечных сокращений с 
    /// </summary>
    /// <param name="hr_marks_to_rate"></param>
    /// <param name="data_rate"></param>
    /// <param name="min_interval_length"></param>
    /// <param name="max_interval_length"></param>
    /// <param name="max_relative_delta">
    /// Максимальное приращение относительно предыдущего или приближенного усредненного интервала, %%
    /// Если значение параметра равно double.PositiveInfinity, фильтрация по относительному приращению не производится
    /// </param>
    /// <param name="signal">Сигнал, на основе которого определены моменты сердечных сокращений.
    /// Может использоваться для оценки "качества" найденных сердечных сокращений</param>
    /// <returns></returns>
    /// <remarks>
    /// 
    /// 0. Предложние:
    /// Выбрасываем сразу:
    /// Все СС (сердечные сокращения), расстояние от которых до соседних сокращений меньше 1/3
    /// минимально допустимого кардио-интервала?????
    /// 
    /// Зная средний интервал, можем строить предположения о качестве обнаруженных сердечных сокращений (СС).
    ///
    /// 1. Например, если обнаружено (СС), образующее с
    /// предыдущим СС интервал длительностью менее чем в 0.65 среднего интервала
    /// и более чем на 30% короче предыдущего интервала, то можем предполагать,
    /// что данное СС обнаружено в районе инцезуры (дикротической волны)
    /// и его нужно "выбросить" из ряда сокращений, используемых для построения
    /// ряда интервалов.
    /// 1а. Если значение предыдущего интервала неизвестно (например, для первого интервала в ряду),
    /// используем для оценки величину среднего интервала (получаем ее из всех интервалов в течение
    /// записи, как "качественных", таки "некачественных" по критерию скрости изменения
    /// (т.е. учитываем только интервалы, удовлетворяющие ограничениям на абсолютную величину интервала).
    /// 
    /// 2. Если некоторое сердечное сокращение было пропущено, то пытаемся найти
    /// "валидный" интервал, используя в качестве начального и конечного СС
    /// СС, предшествующий отброшенному и СС, следующие за отброшенным, предполагая
    /// их "валидными" СС и проводя оценку валидности каждого из получившехся интервалов
    /// с учетом правил отбраковки кардио-интервалов.
    ///
    /// 3. Если по правилу (2) валидных интервалов получить не удается (величина получающихся
    /// интервалов начинает превышать максимально допустимую), начинаем строить новый кардио-интервал,
    /// считая моментом начального СС "отброшенное" СС, и переходим к пункту 1.
    /// 
    /// Предложение:
    /// 4. Если фильтрация СС с учетом ограничений на относительное изменение длительности
    /// интервалов дает слишком много (более T=6) недостоверных СС и (T-1) кардио-интервалов подряд, 
    /// предлагается делать T шагов назад (к первому СС из отброшенной серии) и оценить относительное
    /// изменение длительности первого из серии отброшенных интервалов не по сравнению с предыдущим,
    /// а по сравнению со средним значением за всю запись без отбраковки.
    /// 
    /// Для получения корректных результатов должно выполняться предположение о том, что количество
    /// неотсеянных инцезур в исходном списке сердечных сокращений много меньше количества правильно
    /// обнаруженных СС.
    /// ///////////////////////////////////////////
    /// </remarks>
    public static PeaksFilterOutput ConvertPeaksToIntervalsWithRejectionAndRatePeaks(
      List<RatedContractionMark> hr_marks_to_rate,
      double data_rate,
      double min_interval_length,
      double max_interval_length,
      double max_relative_delta,
      ChannelData signal
      )
    {
      if (hr_marks_to_rate.Count < 2)
      {
        throw new ArgumentException(strings.too_few_heart_contractions_detected_in_signal);
      }

      // перейдем от %% к долям от 1
      max_relative_delta /= 100.0;

      // вычисляем (приближенно) усредненную величину достоверного интервал с учетом ограничений
      double ave = Estimate_AverageInterval(
        hr_marks_to_rate,
        data_rate,
        min_interval_length,
        max_interval_length,
        max_relative_delta);

      List<PeaksFilterOutput> results = new List<PeaksFilterOutput>(5);

      int start = 1;
      int max_retries = 1; // 5 -- этот подход подлежит дальнейшему изучению и уточнению, пока отключено

      for (int i_first = start; i_first < System.Math.Min(start + max_retries, hr_marks_to_rate.Count); ++i_first)
      {
        PeaksFilterOutput r = TryConvertAndRatePeaks(hr_marks_to_rate, data_rate, min_interval_length, max_interval_length, max_relative_delta, ave, i_first);
        results.Add(r);

        double delta_ave = System.Math.Abs(r.average_cardio_interval - ave) / ( 0.5 * (r.average_cardio_interval + ave) );
        if( (delta_ave <= 0.1) || (double.PositiveInfinity == max_relative_delta) )
        {
          Debug_DumpFilteringResults(r);
          return r;
        }
      }

      // Если попали сюда, значит ни один из полученных наборов интервалов
      // не укладывается в погрешность среднего в 10%. Необходимо выбрать
      // наилучший результат из полученного набора

      PeaksFilterOutput r_min = results[0];
      double delta_r_min = System.Math.Abs(r_min.average_cardio_interval - ave) / (0.5 * (r_min.average_cardio_interval + ave));
      for (int i = 1; i < results.Count; ++i)
      {
        double delta_r_i = System.Math.Abs(results[i].average_cardio_interval - ave) / ( 0.5 * (results[i].average_cardio_interval + ave) );
        if (delta_r_i < delta_r_min )
        {
          r_min = results[i];
          delta_r_min = delta_r_i;
        }
      }

      Debug_DumpFilteringResults(r_min);

      return r_min;
    }

    [System.Diagnostics.Conditional("__DEBUG")]
    private static void Debug_DumpFilteringResults(PeaksFilterOutput r_min)
    {
      for (int ii = 0; ii < r_min.rated_heart_contraction_marks.Count; ++ii)
      {
        if (r_min.rated_heart_contraction_marks[ii].IntervalsCount < 2)
        {
          log4net.LogManager.GetLogger(typeof(PeaksFilter)).Debug("not so good heart contraction mark");
        }
      }
    }

    private static PeaksFilterOutput TryConvertAndRatePeaks(List<RatedContractionMark> src_hr_marks, double data_rate, double min_interval_length, double max_interval_length, double max_relative_delta, double ave, int i_first)
    {
      // начало трехшагового алгоритма извлечения качественных интервалов
      var result = new PeaksFilterOutput(src_hr_marks);

      // для быстрого обращения к полям объекта result...
      var hrMarksToRate = result.rated_heart_contraction_marks;
      var intervals = result.extracted_intervals;

      double begin = 0, end = 0, interval = 0, delta, rdelta;

      // пусть в самом начале предыдущий интервал будет равен среднему интервалу за весь период оценки
      double prev_interval = ave;

      bool bChecksPassed, rdelta_ok;
      int i_begin, i_end;

      // количество последовательных отбракованных интервалов по правилу относительного изменения длительности
      int count_of_successive_failures_due_to_relative_change = 0;

      // максимально допустимое количество таких отбраковок
      // (при превышении делается возврат к началу серии и изменяются парметры)
      int max_count_of_successive_failures_due_to_relative_change = 6;

      bool in_step_back_mode = false;

      var i_ends = new List<int>(10);
      var i_ends_interval_ends = new Dictionary<int, double>(10);

      // перебираем все обнаруженные метки сердечных сокращений
      for (int i = i_first; i < hrMarksToRate.Count; ++i)
      {
        i_begin = i - 1;
        i_end = i_begin + 1;

        bChecksPassed = false;

        // из нескольких интервалов, получаемых из сердечных сокращений № i..i+k,
        // выбираем допустимые по абсолютному значению, а из них отбираем интервал,
        // наиболее близкий к предыдущему значению
        //
        // если такой интервал не удовлетворяет ограничению на изменение относительно предыдущего интервала,
        // сравниваем его величину со средним значением при том же ограничении на относительное изменение
        rdelta_ok = true; // важно, чтобы r_delta было true если вылетаем из внутреннего while из-за абсолютного значения интервала

        begin = hrMarksToRate[i_begin].Position / data_rate * 1000.0;

        i_ends.Clear();
        i_ends_interval_ends.Clear();

        if (max_relative_delta < double.PositiveInfinity)
        {
          for (; i_end < hrMarksToRate.Count; ++i_end)
          {
            end = hrMarksToRate[i_end].Position / data_rate * 1000.0;
            interval = end - begin;
            if (interval < min_interval_length)
            {
              // пропускаем все отметки, дающие слишком короткие интервалы
              continue;
            }
            if (interval > max_interval_length)
            {
              // получаются слишком длинные интервалы, прекращаем
              break;
            }
            i_ends.Add(i_end);
            i_ends_interval_ends[i_end] = end;
          }
        }
        else
        {
          i_ends.Add(i_end);
          i_ends_interval_ends.Add(i_end, hrMarksToRate[i_end].Position / data_rate * 1000.0);
        }

        if (0 == i_ends.Count)
        {
          // сердечных сокращений, отстоящих от begin на допустимое расстояние, не обнаружено
          continue;
        }

        // для каждого из "подозрительных" интервалов считаем разницу между
        // его величиной и величиной предыдущего интервала.
        // интервал с минимальной разницей и есть наиболее "качественный" интервал.
        double min_delta = double.MaxValue;
        int min_index = -1;
        for (int l = 0; l < i_ends_interval_ends.Count; ++l)
        {
          interval = i_ends_interval_ends[i_ends[l]] - begin;
          delta = System.Math.Abs(prev_interval - interval);
          if (delta < min_delta)
          {
            min_delta = delta;
            min_index = l;
          }
        }

        // работаем с наиболее вероятным сердечным сокращением
        i_end = i_ends[min_index];
        // оно соответствует времени (в мс):
        end = i_ends_interval_ends[i_end];
        // и дает интервал длительностью
        interval = end - begin;

        delta = System.Math.Abs(prev_interval - interval);

        rdelta = delta / prev_interval;

        // по приращению по отношению к предыдущему интервалу:
        rdelta_ok = (rdelta <= max_relative_delta);

        // по абсолютному значению оно обязано подходить, ибо получено из этого условия...
        bool abs_val_ok = (interval >= min_interval_length) && (interval <= max_interval_length);

        bChecksPassed = abs_val_ok && rdelta_ok;

        // ok, оценим интервал -- по абсолютной величине и по отношению к предыдущему...
        if (bChecksPassed)
        {
          // получили достоверный интервал
          // пометим все сокращения между begin и end как недостоверные!
          for (int j = i_begin + 1; j < i_end; ++j)
          {
            hrMarksToRate[i].Valid = false;
            hrMarksToRate[i].IntervalsCount = 0;
          }
          hrMarksToRate[i_begin].Valid = true;
          hrMarksToRate[i_end].Valid = true;

          ++hrMarksToRate[i_begin].IntervalsCount;
          ++hrMarksToRate[i_end].IntervalsCount;

          count_of_successive_failures_due_to_relative_change = 0;
          in_step_back_mode = false;

          prev_interval = interval;
          intervals.Add(new CardioInterval(begin, end, i_begin, i_end));

          i = i_end;
        }
        else
        {
          // наиболее достоверный интервал недостоверен...
          // что делать?

          // бросаем (i-1)-ю метку сердечного сокращения и пытаемся сформировать интервал
          // с i-й по i+1-ю метку и т.д.

          // не удалось построить интервал из i-1-го сердечного сокращения
          // помечаем его как недостоверное
          hrMarksToRate[i_begin].Valid = false;

          // Интервал был отбракован
          if (rdelta_ok) // Из-за относительного изменения?
          {
            // нет, относительное изменение в порядке
            count_of_successive_failures_due_to_relative_change = 0;
            in_step_back_mode = false;
          }
          else
          {
            // да
            // еще одно сердечное сокращение отфильтровано по правилу относительного изменения длительности
            ++count_of_successive_failures_due_to_relative_change;
          }

          // проверим количество интервалов отброшенных подряд из-за нарушения ограничений
          // на относительное изменение длительности интервала
          if (PeaksFilter.EnableStepBackOnSuccessiveFailures)
          {
            if (count_of_successive_failures_due_to_relative_change > max_count_of_successive_failures_due_to_relative_change)
            {
              // шаг назад (чтобы не зациклиться, делаем шаг назад только один раз)...
              if (false == in_step_back_mode)
              {
                // запоминаем, что шаг назад уже сделан
                in_step_back_mode = true;
                // переходим к оценке изменения по среднему интервалу,
                // а не по предыдущему, с которого все и началось
                prev_interval = ave;
                count_of_successive_failures_due_to_relative_change = 0;
                // возвращаемся на max_count_of_successive_failures_due_to_relative_change сокращений назад
                i = System.Math.Max(1, i - max_count_of_successive_failures_due_to_relative_change);
              }
              else
              {
                // если шаг назад уже сделали, то больше его не повторяем, чтобы не зациклиться!
              }
            }
          }
        }
      } // for( i = ...

      result.Update();
      return result;
    }

    private static double Estimate_AverageInterval(
      List<RatedContractionMark> hr_marks_to_rate,
      double data_rate,
      double min_interval_length,
      double max_interval_length,
      double max_relative_delta
      )
    {
      double[] coordinates = new double[hr_marks_to_rate.Count];
      for (int i = 0; i < coordinates.Length; ++i)
      {
        coordinates[i] = hr_marks_to_rate[i].Position;
      }

      // это черновой список интервалов, учитывающий всю последовательность
      // интервалов, получающихся из всей последовательности сокращений за исключением
      // интервалов, не удовлетворяющих ограничениям min_interval_length, max_interval_length
      // и max_relative_delta.
      // при проверке первого интервала на относительное изменение величина предыдущего интервала
      // принимается равной значению моды черновой последовательности кардио-интервалов.
      var draft_intervals = PeaksFilter.CoordinatesToIntervals(coordinates, data_rate);

      var draft_durations = new List<double>(draft_intervals.Count);
      foreach( CardioInterval draft_interval_1 in draft_intervals )
      {
        draft_durations.Add(draft_interval_1.duration);
      }
      double[] draft_durations_array = draft_durations.ToArray();
      var stat = new PskOnline.Math.Statistics.StatData();

      PskOnline.Math.Statistics.Calculator.CalcStatistics(draft_durations_array, stat);
      PskOnline.Math.Statistics.Calculator.MakeProbabilityDensity(draft_durations_array, stat, 400, 2000, 4);

      double ave = 0;
      double prev_interval_duration = stat.probability_density.mode;
      int draft_count = 0;
      bool bLengthOk = false;
      bool bRDeltaOk = false;

      foreach (CardioInterval draft_interval in draft_intervals)
      {
        bLengthOk = (draft_interval.duration >= min_interval_length) &&
            (draft_interval.duration <= max_interval_length);

        bRDeltaOk = (max_relative_delta >= (System.Math.Abs(prev_interval_duration - draft_interval.duration) / prev_interval_duration ) );

        if( bLengthOk && bRDeltaOk )
        {
          // учтем "хороший" интервал
          ave += draft_interval.duration;
          ++draft_count;
          prev_interval_duration = draft_interval.duration;
        }
      }

      if (draft_count == 0)
      {
        throw new DataProcessingException(
          string.Format(strings.too_few_valid_intervals_detected_in_signal, draft_count));
      }

      return ave / ((double)draft_count);
    }

    /// <summary>
    /// returns array of intervals in milliseconds!!!
    /// </summary>
    /// <param name="coordinates">координаты, выраженные в количестве сэмплов</param>
    /// <param name="dataRate"></param>
    /// <returns></returns>
    public static List<CardioInterval> CoordinatesToIntervals(double[] coordinates, double dataRate)
    {
      if (coordinates.Length < 2)
      {
        return new List<CardioInterval>(0);
      }

      var intervals = new List<CardioInterval>(coordinates.Length - 1);

      var previous = coordinates[0];
      //      double current = 0.0;
      for (int i = 1; i < coordinates.Length; ++i)
      {
        var begin = coordinates[i - 1] / dataRate * 1000.0d;
        var end = coordinates[i] / dataRate * 1000.0d;
        intervals.Add(new CardioInterval(begin, end, i - 1, i));
      }
      return intervals;
    }
  }
}
