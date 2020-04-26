namespace PskOnline.Methods.Hrv.Processing.Logic
{
  using System;
  using System.Linq;
  using System.Collections.Generic;

  using AutoMapper;

  using PskOnline.Math.Psa.Ppg;
  using PskOnline.Math.Statistics;
  using PskOnline.Math.Psa.Wavelets;

  using PskOnline.Methods.Hrv.Processing.Contracts;
  using PskOnline.Methods.Hrv.ObjectModel;
  using PskOnline.Methods.Hrv.Processing.Settings;
  
  using PskOnline.Methods.ObjectModel.Method;
  using PskOnline.Methods.ObjectModel.PhysioData;
  using PskOnline.Methods.ObjectModel.Settings;

  using PskOnline.Methods.Processing.Contracts;
  using PskOnline.Methods.Processing.Logic;

  /// <summary>
  /// Маркерный интерфейс для поиска классов-процессоров,
  /// предоставляющих матрицу заключений
  /// </summary>
  public interface ITwoDimConclusionDbProvider
  {
  }

  /// <summary>
  /// Базовый обработчик данных по методике ВСР.
  /// </summary>
  public class HrvBasicDataProcessor : BasicDataProcessor
  {
    public HrvBasicDataProcessor()
      : base()
    {
      ProcessorOutputData = new HrvResults();

      var mapperConfig = new MapperConfiguration(cfg => {
        cfg.AddProfile<AutoMapperStatisticsProfile>();
      });

      _mapper = mapperConfig.CreateMapper();
    }

    public override int GetProcessorVersion()
    {
      return 4;
    }

    /// <summary>
    /// возвращает степень двойки, ближайшую меньшую intervals_count и меньшую 256.
    /// </summary>
    /// <param name="intervals_count"></param>
    /// <returns></returns>
    private int GetMaxSpectrumBinsCount(int intervals_count)
    {
      int max_bins = intervals_count / 2;
      int bins = 256;
      while (bins > max_bins)
      {
        bins /= 2;
      }
      return bins;
    }

    private int GetMinIntervalCount(ChannelData data)
    {
      // Это время записи разделить на самый длинный кардио-интервал в секундах.
      return (int)(data.Data.Length / data.SamplingRate / (60.0 / 45.0));
    }

    /// <summary>
    /// Обрабатывает source_data по методике HRV.
    /// Ожидается, что source_data является HrvRawData
    /// </summary>
    /// <param name="source_data"></param>
    /// <returns></returns>
    public override IMethodProcessedData ProcessData(IMethodRawData source_data)
    {
      var rawHrvData = source_data as HrvRawData;
      if (rawHrvData == null)
      {
        throw new ArgumentException(
          $"Invalid input data format. Expected object of type {typeof(HrvRawData)}");
      }

      return ProcessData(rawHrvData);
    }

    public HrvResults ProcessData(HrvRawData rawHrvData)
    {
      base.ProcessData(rawHrvData);

      double[] valid_intervals_durations = null;
      List<CardioInterval> valid_intervals = null;

      // recorded data contains cardio intervals 
      // this is the case when the inspection was performed
      // using a cardio intervals sensor
      // (e.g. Bluetooth HR sensor with RR intervals)
      if( InputDataContainsCardioIntervals(rawHrvData) )
      {
        var heartRythmData = ReadCardioIntervalsFromResultSet(rawHrvData);
        valid_intervals = heartRythmData.Item1;
        HrvResults.RATED_HR_MARKS = heartRythmData.Item2.ToArray();
      }
      else
      {
        ChannelData data = null, adc_data = null;
        double[] hrv_marks = null;

        SelectAndProcessSignal(m_settings.RejectLowQualitySignalAreas, rawHrvData, out hrv_marks, out data, out adc_data);

        if ((null == data) || (null == adc_data))
        {
          throw new DataProcessingException(strings.NoSignalInSourceData);
        }

        HrvResults.CRV_CARDIO_SIGNAL = new PhysioSignalView(
          data.GetDataAsFloat(),
          (int)data.BitsPerSample,
          (float)data.SamplingRate,
          data.PhysioSignalType
          );

        HrvResults.CRV_CARDIO_SIGNAL_ADC = new PhysioSignalView(
          adc_data.GetDataAsFloat(),
          (int)adc_data.BitsPerSample,
          (float)adc_data.SamplingRate,
          adc_data.PhysioSignalType
          );

        // отметки пульса с оценкой достоверности
        var hc_marks_to_rate = new List<RatedContractionMark>(hrv_marks.Length);
        for (int i = 0; i < hrv_marks.Length; ++i)
        {
          var mark = new RatedContractionMark
          {
            Position = hrv_marks[i],
            Valid = false
          };
          hc_marks_to_rate.Add(mark);
        }

        // получаем достоверные интервалы в миллисекундах,
        // "хорошие" сокращения будут помечены как хорошие (по обоим параметрам),
        var converted_peaks =
          PeaksFilter.ConvertPeaksToIntervalsWithRejectionAndRatePeaks(
              hc_marks_to_rate,
              data.SamplingRate,
              m_settings.GetApplicableMinIntervalLength(),
              m_settings.GetApplicableMaxIntervalLength(),
              m_settings.GetApplicableMaxIntervalDeltaRelative(),
              adc_data);

        valid_intervals = converted_peaks.extracted_intervals;
        if (converted_peaks.extracted_intervals.Count < 5)
        {
          throw new DataProcessingException(string.Format(strings.too_few_intervals_detected_in_signal, valid_intervals.Count));
        }

        // это все отметки пульса
        HrvResults.CRV_HR_MARKS = hrv_marks;

        // это отметки пульса с оценками качества
        HrvResults.RATED_HR_MARKS = converted_peaks.rated_heart_contraction_marks.ToArray();

        // Спектр сигнала ФПГ
        HrvResults.SignalSpectrum = PpgSpectrumAnalyzer.CalculateSpectrum(data.Data, data.SamplingRate);
      }

      // только длительности интервалов
      valid_intervals_durations = (from interval in valid_intervals select interval.duration).ToArray();

      var peakAmoDistrib = default(MinMaxModeDescriptor);
      // кардио-интервалы подготовлены
      try
      {
        var mathStatData = new StatData();
        // считаем статистику по кардио-интервалам
        Calculator.CalcStatistics(valid_intervals_durations, mathStatData);
        Calculator.MakeProbabilityDensity(valid_intervals_durations, mathStatData, 440, 2000, 4);

        // The standard approach
        Calculator.MakeDistribution(valid_intervals_durations, mathStatData, 0, 2500, 50);

        // Find the maximized & minimized mode amplitude values
        peakAmoDistrib = MinMaxModeAmpFinder.GetPeakAmoDistrib(valid_intervals_durations, 50, 0, 2500, 1);

        HrvResults.CRV_STAT = _mapper.Map<Methods.ObjectModel.Statistics.StatData>(mathStatData);
      }
      catch( ArgumentException )
      {
        throw new DataProcessingException(
          string.Format(strings.too_few_valid_intervals_detected_in_signal, valid_intervals_durations.Length));
      }

      HrvResults.Indicators.HRV_Triangular_Index = CalcHrvTriangularIndex(valid_intervals_durations);

      double AMo = Statistics.distribution.mode_amplitude;   // Безразмерная -- в долях от 1
      double Mo = Statistics.distribution.mode / 1000;       // В секундах!!!
      //double AMo = peakAmoDistrib.ModeAmp;   // Безразмерная -- в долях от 1
      //double Mo = peakAmoDistrib.Mode / 1000;       // В секундах!!!
      double M = Statistics.m / 1000;          // В секундах!!!
      double dNN = Statistics.varRange / 1000;   // В секундах!!!
      double Sigma = Statistics.sigma / 1000;  // В секундах!!!

      var _AMo = Statistics.distribution.mode_amplitude;   // Безразмерная -- в долях от 1
      var _Mo = Statistics.distribution.mode / 1000;       // В секундах!!!

      var AmoMax = peakAmoDistrib.MaxModeDescriptor.ModeAmp; // maximized Mode Amplitude
      var MoMax = peakAmoDistrib.MaxModeDescriptor.Mode / 1000;      // Mode when the Mode amplitude is maximized

      var AmoMin = peakAmoDistrib.MinModeDescriptor.ModeAmp; // minimized Mode Amplitude
      var MoMin = peakAmoDistrib.MinModeDescriptor.Mode / 1000;      // Mode when the Mode amplitude is minimized

      var AmoMid = peakAmoDistrib.MidModeDescriptor.ModeAmp; // mean Mode Amplitude
      var MoMid = peakAmoDistrib.MidModeDescriptor.Mode / 1000;      // Mode when the Mode amplitude is close to mean

      // Вычислим индексы Баевского
      // умножаем на 100, т.к. "по Баевскому" АМо указывается в %%.
      HrvResults.Indicators.IN = AMo * 100 / (2.0 * Mo * dNN);

      HrvResults.Indicators.IN_Max = AmoMax * 100 / (2.0 * MoMax * dNN);
      HrvResults.Indicators.IN_MaxMode = MoMax;

      HrvResults.Indicators.IN_Min = AmoMin * 100 / (2.0 * MoMin * dNN);
      HrvResults.Indicators.IN_MinMode = MoMin;

      HrvResults.Indicators.IN_Mid = AmoMid * 100 / (2.0 * MoMid * dNN);
      HrvResults.Indicators.IN_MidMode = MoMid;

      // TODO: выяснить, что правильно, что нет: m_BaevskyStatistics._VPR = 1.f / (fM * fX);
      HrvResults.Indicators.VPR = 1.0 / (Mo * dNN);
      HrvResults.Indicators.PAPR = AMo * 100 / Mo;
      HrvResults.Indicators.IVR = AMo * 100 / dNN;

      // Психофизиологическая цена [адаптации]
      // АМ [%%] / ( Мат. ожидание [с] * сигма [с] )
      // умножаем на 100, т.к. АМо д.б. в %%.
      HrvResults.Indicators.PPPA = AMo * 100 / (M * Sigma);

      // Передать в результаты теста также и сами кардиоинтервалы
      // с моментами их появления в записи
      HrvResults.CRV_INTERVALS = valid_intervals.ToArray();

      // Спектр кардиоинтервалов
      HrvResults.IntervalsSpectrum = HrvSpectrumAnalyzer.MakeCardioIntervalsSpectrum(valid_intervals);

      double sum = 0.0;
      double n50 = (valid_intervals[0].duration > 50) ? 1 : 0;
      for (int i = 1; i < valid_intervals.Count; i++)
      {
        double diff = valid_intervals[i].duration - valid_intervals[i - 1].duration;
        sum += Math.Pow(diff, 2);
        if (diff > 50)
        {
          n50++;
        }
      }

      HrvResults.Indicators.pNN50 = 100.0 * n50 / (valid_intervals.Count - 1);
      HrvResults.Indicators.RMSSD = Math.Sqrt(sum);

      HrvResults.CRV_SCATTEROGRAMM_PARAMETERS = GetScatterParams(valid_intervals_durations);

      HrvResults.ResultsReliability = GetResultsReliability(HrvResults);

      return (HrvResults)ProcessorOutputData;
    }

    private bool InputDataContainsCardioIntervals(IMethodRawData rawData)
    {
      var data_for_all = rawData.PhysioData;
      System.Diagnostics.Debug.Assert(data_for_all != null);

      // in this method we always have only one patient
      var allData = rawData.PhysioData;

      // and we must have at least some physio data
      System.Diagnostics.Debug.Assert(allData != null);

      // first, we take all the data 
      var intervals = allData.GetChannelDataBySignalType(SignalType.CardioIntervals);

      return intervals != null && intervals.Any();
    }

    private Tuple<List<CardioInterval>, List<RatedContractionMark>> ReadCardioIntervalsFromResultSet(HrvRawData rawData)
    {
      var data_for_all = rawData.PhysioData;
      System.Diagnostics.Debug.Assert(data_for_all != null);

      // in this method we always have only one patient
      var all_data = rawData.PhysioData;

      // and we must have at least some physio data
      System.Diagnostics.Debug.Assert(all_data != null);

      // first, we take all the data 
      var intervals = all_data.GetChannelDataBySignalType(SignalType.CardioIntervals);

      if (intervals == null)
      {
        // there are no intervals stored in the result set in a known format...
        // use InputDataContainsCardioIntervals to check before calling Read...
        throw new InvalidOperationException(
          "The test data don't contain cardio intervals. Did you check the cardio interval data are present?");
      }

      long begin_mark = rawData.PhSyncBegin;
      long end_mark = rawData.PhSyncEnd;

      // Читаем данные из канала и заполняем массив интервалов и меток сердечных сокращений
      var signal = intervals.First();

      logger.Info($"Using pre-recorded cardio intervals from {signal.ChannelId}");

      var valid_intervals = new List<CardioInterval>(signal.Data.Length);
      var hc_marks = new List<RatedContractionMark>(signal.Data.Length);

      // запоминаем количество сэмплов для первой метки времени
      long startCount = (begin_mark == -1) ? 0 : 
        all_data.GetChannelDataCountForMarker(signal, begin_mark);
      var intervalsMSec = signal.Data;

      var lastTimestampId = signal.Timestamps.Keys.Max();
      var lastTimestamp_uSec = signal.Timestamps[lastTimestampId];
      const double oneMillion = 1000000.0;

      hc_marks.Add(new RatedContractionMark()
      {
        // здесь: секунды (канал Кардио Интервалы работает с декларированной частотой 1)
        Position = (lastTimestamp_uSec) / oneMillion,
        IntervalsCount = 1, // последняя метка всегда участвует только в 1 интервале
        Valid = true
      });
      // пока просто возьмем все сэмплы, и расположим их начиная от последней метки времени
      // считая, что все интервалы поступали непрерывно
      // это приемлемо для статистической обработки,
      // но не всегда приемлемо при использовании time-domain и frequency-domain методов
      for ( var i = intervalsMSec.Length - 1; i >= (int)startCount; --i )
      {
        var intervalDuration_uSec = intervalsMSec[i] * 1000;

        hc_marks.Insert(0, new RatedContractionMark()
        {
          // здесь: секунды (канал Кардио Интервалы работает с декларированной частотой 1)
          Position = (lastTimestamp_uSec - intervalDuration_uSec) / oneMillion,
          IntervalsCount = 2,
          Valid = true
        });

        valid_intervals.Insert(0, new CardioInterval(
          (lastTimestamp_uSec - intervalDuration_uSec) / 1000,
          lastTimestamp_uSec / 1000.0, 
          i, 
          i+1)
        );

        lastTimestamp_uSec = lastTimestamp_uSec - intervalDuration_uSec;
      }

      if( hc_marks.Count != 0 )
      {
        hc_marks.First().IntervalsCount = 1;
        hc_marks.Last().IntervalsCount = 1;
      }

      // потом сравним первую метку времени и вычисленную метку времени
      // при больших расхождениях, результат частотного анализа ритма сердца
      // может быть недостоверным

      return Tuple.Create(valid_intervals, hc_marks);
    }

    private ResultsReliabilityEstimation GetResultsReliability(HrvResults hrvResults)
    {
      var reliability =
        new ResultsReliabilityEstimation
        {
          reject_min_max_enabled = this.m_settings.RejectUsingMinMaxNNTime,
          reject_relative_enabled = this.m_settings.RejectUsingRelativeNNDelta,
          reject_nn_relative = this.m_settings.MaxIntervalDeltaRelative,
          reject_nn_min = this.m_settings.MinIntervalMilliseconds,
          reject_nn_max = this.m_settings.MaxIntervalMilliseconds,
          hc_marks_total = hrvResults.RATED_HR_MARKS.Length
        };

      // отбраковка


      for (int i = 0; i < hrvResults.RATED_HR_MARKS.Length; ++i)
      {
        RatedContractionMark mark = hrvResults.RATED_HR_MARKS[i];
        if (2 < mark.IntervalsCount)
        {
          throw new DataProcessingException(
            $"Logic error: one heart contraction was counted in more than 2 (actually, {mark.IntervalsCount}) intervals!"
          );
        }

        if (2 == mark.IntervalsCount)
        {
          ++reliability.hc_marks_2_int;
        }
        else if (1 == mark.IntervalsCount)
        {
          ++reliability.hc_marks_1_int;
        }
        else
        {
          ++reliability.hc_marks_0_int;
        }
      }

      return reliability;
    }


    #region Расчет параметров скаттерограммы
    private ScatterogrammParameters GetScatterParams(double[] valid_intervals_durations)
    {
      int i;
      double tmp;

      int scatter_count = valid_intervals_durations.Length - 1;
      double[] scatter_xs = new double[scatter_count];
      double[] scatter_ys = new double[scatter_count];

      // скаттерограмма
      for( i = 0; i < scatter_count; ++i )
      {
        scatter_xs[i] = valid_intervals_durations[i + 1];
        scatter_ys[i] = valid_intervals_durations[i];
      }

      // Расчет положения центра масс скаттерограммы 
      // координата X
      double Xc = 0.0f;
      // координата Y
      double Yc = 0.0f;

      for( i = 0; i < scatter_count; i++ )
      {
        Xc += scatter_xs[i];
        Yc += scatter_ys[i];
      }
      Xc /= ((double)scatter_count);
      Yc /= ((double)scatter_count);

      // расчет главных осей инерции
      double Ux = 0.0f;
      for( i = 0; i < scatter_count; i++ )
      {
        tmp = (scatter_xs[i] - Xc);
        Ux += tmp * tmp;
      }
      Ux /= ((double)scatter_count);
      Ux += 1.0f / 12.0f;

      double Uy = 0.0f;
      for( i = 0; i < scatter_count; i++ )
      {
        tmp = (scatter_ys[i] - Yc);
        Uy += tmp * tmp;
      }
      Uy /= ((double)scatter_count);
      Uy += 1.0f / 12.0f;

      double Uxy = 0.0f;
      for( i = 0; i < scatter_count; i++ )
      {
        Uxy += (scatter_ys[i] - Yc) * (scatter_xs[i] - Xc);
      }
      Uxy /= ((double)scatter_count);

      double C = Math.Sqrt((Ux - Uy) * (Ux - Uy) + 4 * Uxy * Uxy);

      tmp = 2.0f * Math.Sqrt(2.0f);
      double A = tmp * Math.Sqrt(Ux + Uy + C);
      double B = tmp * Math.Sqrt(Ux + Uy - C);

      double alpha = 0;

      if (Uy > Ux)
      {
        alpha = Math.Atan((Uy - Ux + C) / (2 * Uxy));
      }
      else
      {
        alpha = Math.Atan((2 * Uxy) / (Ux - Uy + C));
      }

      double beta = (Math.PI / 2.0f) - alpha;

      // Проекции осей
      double a = A * Math.Cos(alpha);
      double b = B * Math.Sin(beta);

      // Разброс по оси X
      double X_min = scatter_xs[0];
      double X_max = scatter_xs[0];

      for( i = 0; i < scatter_count; i++ )
      {
        if( scatter_xs[i] < X_min )
        {
          X_min = scatter_xs[i];
        }
        else if( scatter_xs[i] > X_max )
        {
          X_max = scatter_xs[i];
        }
      }

      ScatterogrammParameters result = new ScatterogrammParameters();
      result.AxisA = a;
      result.AxisB = b;
      result.DNN = X_max - X_min;

      double phi_alpha = GetClassForNNInterval(a);
      double phi_beta = GetClassForNNInterval(b);
      double phi_DNN = GetClassForNNInterval(X_max - X_min);
      result.FSI = phi_alpha * phi_beta * phi_DNN;

      return result;
    }

    /// <summary>
    /// возвращает класс для интервала NN
    /// </summary>
    /// <param name="NN"></param>
    /// <returns></returns>
    static double GetClassForNNInterval(double NN)
    {
      double local_rr_min = 0/*RR_MIN*/;
      double local_rr_max = 1000/*RR_MAX*/;

      NN = Math.Max(NN, local_rr_min);
      NN = Math.Min(NN, local_rr_max);

      NN -= local_rr_min;
      double _class = 0;

      if ((local_rr_max - local_rr_min) != 0)
      {
        _class = NN / (local_rr_max - local_rr_min) * 10;
      }
      else
      {
        _class = 0;
      }

      //  номера классов начинаются с 1
      //  _class += 1;

      return _class;
    }
    #endregion

  private void SelectAndProcessSignal(
      bool rejectLowQualitySignalAreas,
      HrvRawData ismid,
      out double[] out_hrv_marks,
      out ChannelData out_channel_data,
      out ChannelData out_adc_data)
    {
      long begin = 0;
      long end = 0;

      out_hrv_marks = null;
      out_channel_data = null;

      PatientPhysioData all_data = null;
      ChannelData data = GetSignal(ismid, ref all_data, ref begin, ref end);

      if( rejectLowQualitySignalAreas )
      {
        // BUG: this is for Rushydro only, until there is a solution
        // to mask lower-quality portions of the signal at the beginning
        all_data.MoveMarker(begin, 12000000L);
      }

      out_adc_data = new ChannelData(data);

      if (data == null)
      {
        return;
      }

      if (data.PhysioSignalType == SignalType.PPG )
      {
        //int[] unfiltered_data_array = data.GetData();
        int[] data_buffer = data.Data;

        // обрабатываем данные с помощью стандартного процессора
        // (фильтруем, стабилизируем амплитуду и находим фронты)
        // обрабатываем все данные, и даже нерелевантные, чтобы не повредить релевантные данные
        // (в процессе инициализации фильтра сигнал выглядит довольно страшно)
        double[] hrv_marks = Pulse.PpgPulseDetectorHelper.ProcessPpgData(
          data.SamplingRate, data_buffer, (int)data.BitsPerSample, data.DeviceTypeName);

        // избавляемся от лишних данных и лишних отметок пульса,
        // корректируя положение релевантных отметок
        ChannelData relevant_data = all_data.GetChannelDataFromLeftToRight(data, begin, end);

        long left_marker_data_count = all_data.GetChannelDataCountForMarker(data, begin);
        if (left_marker_data_count < 0)
        {
          left_marker_data_count = 0;
        }
        long right_marker_data_count = all_data.GetChannelDataCountForMarker(data, end);
        if (right_marker_data_count < 0)
        {
          right_marker_data_count = data.Data.Length;
        }

        var relevant_hrv_marks = new List<double>(hrv_marks.Length);
        for (int i = 0; i < hrv_marks.Length; ++i)
        {
          // добавляем в список только те отметки, которые попали между левым и правым маркером
          if( (hrv_marks[i] >= left_marker_data_count) && (hrv_marks[i] <= right_marker_data_count) )
          {
            relevant_hrv_marks.Add(hrv_marks[i] - ((double)left_marker_data_count));
          }
        }

        out_hrv_marks = relevant_hrv_marks.ToArray();
        out_channel_data = relevant_data;
      }
      else if (data.PhysioSignalType == SignalType.ECG)
      {
        // not implemented so far
        return;
      }
      else
      {
        // other signal types not supported
        return;
      }
      
    }

    private double CalcHrvTriangularIndex(double[] valid_intervals_durations)
    {
      try
      {
        StatData EuroStatistics = new StatData();

        // Построение гистограммы по 1/128 секундным диапазонам
        // по международному стандарту для расчета триангулярного индекса HRV
        Calculator.CalcStatistics(valid_intervals_durations, EuroStatistics);
        Calculator.MakeDistribution(valid_intervals_durations, EuroStatistics, 0, 2500, 1000.0 / 128.0);

        return 1 / EuroStatistics.distribution.mode_amplitude;
      }
      catch (System.ArgumentException)
      {
        throw new DataProcessingException(string.Format(strings.too_few_valid_intervals_detected_in_signal, valid_intervals_durations.Length));
      }
    }

    private void FilterSignal(ChannelData data)
    {
      if (data != null)
      {
        // Is it ECG signal?
        if (data.PhysioSignalType  == SignalType.ECG )
        {
          // Yes, this is ECG. Use ECG auto denoiser.
          int[] buffer = data.Data;

          EcgWaveletAutoDenoiser denoiser = new EcgWaveletAutoDenoiser();
          denoiser.FilterDestructive(buffer);
        }
        else if (data.PhysioSignalType == SignalType.PPG )
        {
          // well, this is PPG signal
          if( true )
          {
            int[] buffer = data.Data;

            var denoiser = new PpgDenoiser(data.SamplingRate);
            denoiser.FilterInPlace(buffer);
          }
        }
        else
        {
          // this is neither ECG, nor PPG, don't know how to filter it properly
        }
      }
    }

    /// <summary>
    /// </summary>
    /// <param name="intervals">List of intervals in milliseconds!!!</param>
    /// <returns></returns>
    public double[] IntervalsToCoordinates(List<CardioInterval> intervals, double data_rate)
    {
      int length = intervals.Count + 1;
      double[] data = new double[length];

      data[0] = 0;
      for (int i = 1; i < length; ++i)
      {
        data[i] = (double)(intervals[i - 1].moment * data_rate / 1000.0);
      }
      return data;
    }

    public override IMethodSettings Get()
    {
      return this.m_settings;
    }

    public override void Set(IMethodSettings settings)
    {
      this.m_settings = settings as ProcessingSettings;
    }

    /// <summary>
    /// rejects intervals according to rejection settings
    /// </summary>
    public List<CardioInterval> RejectIntervals(List<CardioInterval> intervals)
    {
      System.Diagnostics.Debug.Assert(intervals != null);
      if ((intervals.Count == 0))
      {
        return new List<CardioInterval>();
      }

      double cur_int_duration, prev_int_duration, delta, min, max, max_relative_change;
      List<CardioInterval> valid_intervals = new List<CardioInterval>(intervals.Count);

      min = m_settings.GetApplicableMinIntervalLength();
      max = m_settings.GetApplicableMaxIntervalLength();
      max_relative_change = m_settings.GetApplicableMaxIntervalDeltaRelative();

      for (int i = 0; i < intervals.Count; ++i)
      {
        var current_interval = intervals[i];
        cur_int_duration = intervals[i].duration;
        if (this.m_settings.RejectUsingMinMaxNNTime)
        {
          if (cur_int_duration > max)
          {
            continue;
          }
          if (cur_int_duration < min)
          {
            continue;
          }
        }
        if (m_settings.RejectUsingRelativeNNDelta && i > 0)
        {
          prev_int_duration = intervals[i - 1].duration;
          delta = Math.Abs(100.0 * (cur_int_duration - prev_int_duration) / prev_int_duration);

          if (delta > max_relative_change)
          {
            continue;
          }
        }

        // if we have come this far, then, this interval probably is not an artefact
        // add it to valid intervals list

        valid_intervals.Add(current_interval);
      }
      return valid_intervals;
    }

    /// <summary>
    /// Возвращает фрагмент сигнала, соответствующий периоду первого стимула;
    /// по возможности сглаженный со стабилизированной амплитудой,
    /// </summary>
    /// <param name="ismid"></param>
    /// <param name="all_data">data package to which returned channel data belongs</param>
    /// <param name="begin_mark">beginning of the relevant signal</param>
    /// <param name="end_mark">end of the relevant signal in the returned signal's buffer</param>
    /// <returns></returns>
    private ChannelData GetSignal(
      HrvRawData ismid,
      ref PatientPhysioData all_data,
      ref long begin_mark,
      ref long end_mark)
    {
      var data = ismid.PhysioData;

      // and we must have at least some physio data
      System.Diagnostics.Debug.Assert(data != null);
      
      all_data = data;

      ChannelData result = null;

      // first, we take all the data 
      var ecg_data = data.GetChannelDataBySignalType(SignalType.ECG);
      var ppg_data = data.GetChannelDataBySignalType(SignalType.PPG);

      // select appropriate signal using 'a priori' information about priority of signal types
      result = GetPreferredSignal(ecg_data, ppg_data);

      if (result == null)
      {
        return null;
      }

      begin_mark = ismid.PhSyncBegin;
      end_mark = ismid.PhSyncEnd;

      // Возвращаем результат
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private ChannelData GetPreferredSignal(IEnumerable<ChannelData> ecg_data, IEnumerable<ChannelData> ppg_data)
    {
      ChannelData physio_data = null;

      // Если есть ФПГ-сигнал, используем его. 
      if( ppg_data.Any() )
      {
        // Обрабатываем ФПГ
        physio_data = ppg_data.First();

        // Можно включить экспорт сигнала при необходимости
        ExportPhysioData(physio_data, @"c:\SourceSignal.txt");
      }

      // Если есть ЭКГ-сигнал, используем его.
      // (Переопределяем использование ФПГ, считая, что ЭКГ сигнал более важен.)
      if (ecg_data.Any())
      {
        // Обрабатываем только ЭКГ
        physio_data = ecg_data.First();
        //System.Diagnostics.Debug(ecg_data.m_channelName);
        // Можно включить экспорт сигнала при необходимости
        ExportPhysioData(physio_data, @"c:\ch.8.txt.TxtChannel.txt");
      }

      if (null != physio_data)
      {
        HrvResults.SignalType = physio_data.PhysioSignalType;
      }

      return physio_data;
    }

    /// <summary>
    /// усиливает сигнал так, чтобы он занимал не менее 50%
    /// доступного динамического диапазона при result.BitsPerSample-битном АЦП
    /// </summary>
    /// <param name="signal"></param>
    private void AmplifySignalIfNeeded(ChannelData result)
    {
      int[] signal = result.Data;

      if( signal.Length < 1 )
      {
        return;
      }

      int min = signal[0];
      int max = signal[0];
      for (int i = 1; i < signal.Length; ++i)
      {
        if (signal[i] < min)
        {
          min = signal[i];
        }
        else if (signal[i] > max)
        {
          max = signal[i];
        }
      }

      // минимум и максимум найден
      // вычисляем полную амплитуду сигнала
      long old_amplitude = max - min;

      // динамический диапазон
      long max_amplitude = (1 << (int)(Math.Ceiling(result.BitsPerSample))) - 1;

      // Новая амплитуда сигнала
      long new_amplitude = 7 * max_amplitude / 10;

      // коэффициент
      double factor = old_amplitude / new_amplitude;

      // удостоверимся, что при умножении на коэффициент
      // мы не вылезем за пределы динамического диапазона
      double dmax = max;
      // корректирующая добавка для уровня сигнала
      int delta = (int) Math.Ceiling(Math.Max(0, dmax * factor - max_amplitude));

      // пересчитываем сигнал
      for (int i = 0; i < signal.Length; ++i)
      {
        // коррекция уровня сигнала
        signal[i] -= delta;
        signal[i] = (int)( ((long)signal[i]) * new_amplitude / old_amplitude );
      }
    }

    /// <summary>
    /// сейчас поддерживается только стабилизация амплитуды сигнала ФПГ
    /// путем стабилизации дисперсии сигнала за период около 2.2 секунд
    /// </summary>
    /// <param name="signal"></param>
    private void NormalizeSignalAmplitude(ChannelData signal)
    {
      if( signal.PhysioSignalType == SignalType.PPG )
      {
        AmplifySignalIfNeeded(signal);
        var normalizer = new PpgDispersionNormalizer(signal.SamplingRate, (int)signal.BitsPerSample);
        normalizer.NormalizeDataInPlace(signal.Data);
      }
      else
      {
        logger.Warn($"Could not normalize signal of type '{signal.PhysioSignalType}'");
      }
    }

    [System.Diagnostics.Conditional("WRITE_SIGNAL_DATA")]
    void ExportPhysioData(ChannelData physio_data, string filename)
    {
      System.IO.StreamWriter sw = new System.IO.StreamWriter(filename);

      sw.WriteLine(physio_data.PhysioSignalType.ToString()); // signal type name

      sw.WriteLine(physio_data.SamplingRate); // sampling rate

      sw.WriteLine(physio_data.BitsPerSample); // bits per sample

      sw.WriteLine(5);  // ref voltage
      foreach (int i in physio_data.Data)
      {
        sw.WriteLine(i.ToString());
      }
      sw.Close();
    }

    protected virtual CrvTwoDimConclusionDatabase GetTwoDimConslusionDatabase()
    {
      throw new NotImplementedException("This method must be overriden in an ancestor marked with cardio.processing.ITwoDimConclusionDbProvider interface!");
    }

    private IMapper _mapper;

    /// <summary>
    /// Наши результаты (также содержит и базовые результаты)
    /// </summary>
    protected HrvResults HrvResults => (HrvResults)ProcessorOutputData;

    protected PskOnline.Methods.ObjectModel.Statistics.StatData Statistics => HrvResults.CRV_STAT;

    private ProcessingSettings m_settings = new ProcessingSettings();

    private log4net.ILog logger = log4net.LogManager.GetLogger(typeof(HrvBasicDataProcessor));

    #region unit test utility functions

    public void UTest_SetStateMatrixState(int row, int col)
    {
      var ps = m_settings as ProcessingSettings;
      IStateMatrixStateClassifier classifier = null;
      if( null != ps )
      {
        var normalBpm = (int) ps.PersonalNorm.NormalHeartRateAtRestBpm;
        classifier = StateMatrixStateClassifierFactory.GetStateMatrixClassifier(normalBpm);
      }
      else
      {
        classifier = StateMatrixStateClassifierFactory.GetStateMatrixClassifier();
      }

      double[] row_class_bounds = classifier.GetRowClassBoundary();
      double[] col_class_bounds = classifier.GetColClassBoundary();

      System.Diagnostics.Debug.Assert((0 <= row) && (row_class_bounds.Length > row));
      System.Diagnostics.Debug.Assert((0 <= col) && (col_class_bounds.Length > col));

      this.HrvResults.CRV_STAT.m = 0.5 * (row_class_bounds[row] + row_class_bounds[row + 1]);

      this.HrvResults.CRV_STAT.sigma = 0.5 * (col_class_bounds[col] + col_class_bounds[col + 1]);
    }
    #endregion
  }

}
