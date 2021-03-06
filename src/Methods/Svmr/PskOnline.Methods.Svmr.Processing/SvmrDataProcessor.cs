namespace PskOnline.Methods.Svmr.Processing
{
  using System;
  using System.Linq;
  using System.Collections.Generic;

  using AutoMapper;

  using PskOnline.Methods.ObjectModel.Settings;
  using PskOnline.Methods.ObjectModel.Statistics;
  using PskOnline.Methods.ObjectModel.Method;
  using PskOnline.Methods.ObjectModel.Test;

  using PskOnline.Methods.Processing.Contracts;
  using PskOnline.Methods.Processing.Logic;

  using PskOnline.Methods.Svmr.ObjectModel;

  /// <summary>
  /// Summary description for DataProcessor.
  /// </summary>
  public class SvmrDataProcessor : BasicDataProcessor
  {

    public override int GetProcessorVersion()
    {
      // 4 -> 5
      // updated version number so that we can re-do processing 
      // and apply corrections to reaction times
      return 5;
    }

    readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(SvmrDataProcessor));

    public SvmrDataProcessor()
      : base()
    {
      // Подставим наследника, чтобы базовый класс ничего не заметил,
      // что все хорошо и заполнил его данными (TestInfo).
      ProcessorOutputData = new SvmrResults();
      SvmrResults.SvmrErrors = new TestErrors();

      // Наши настройки по умолчанию.
      m_Settings = new ProcessingSettings();

      var mapperConfig = new MapperConfiguration(cfg => {
        cfg.AddProfile<AutoMapperStatisticsProfile>();
      });

      _mapper = mapperConfig.CreateMapper();
    } // DataProcessor

    /// <summary>
    /// Обрабатывает source_data.
    /// </summary>
    /// <param name="source_data"></param>
    /// <returns></returns>
    public override IMethodProcessedData ProcessData(IMethodRawData source_data)
    {
      if( null == base.ProcessData(source_data) )
      {
        return null;
      }

      var tr = source_data as SvmrRawData;
      if (tr == null) return null;

      var ti = source_data.TestInfo;
      if (ti == null) return null; // Нам подали на вход что-то не то.
      if (ti.MethodId != SvmrMethodId.MethodId) return null; // Нам подали на вход что-то не то.

      var allReactionTimes = new List<float>(100);
      var allReactionErrors = new List<ReactionError>(100);

//      int previousReactionTime = -1;
      double ReliabilityUpft130 = 0;
      int ReliabilityUpft130Counter = 0;
      var times_array = new List<double>(tr.Attempts.Count());
      foreach( SvmrAttempt a in tr.Attempts )
      {
        if( a.IsTraining )
        {
          // Это был тренировочный стимул.
          continue;
        }

        allReactionTimes.Add(a.ReactionTimeSeconds * 1000.0f);
        allReactionErrors.Add(a.ReactionError);

        // Ага, у нас есть "зачетный" стимул и реакция...
        // Посмотрим, что за реакция: ошибки, время и т.п.
            
        // +1 зачетный стимул
        TestErrors.TotalCount++;

        switch( a.ReactionError )
        {
          case ReactionError.NoError:
            TestErrors.NormalCount++;
            // Сюда кладем время в миллисекундах
            double reactionTime = (double) ((a.ReactionTimeSeconds - CorrectionTimeSeconds)*1000.0f);
            times_array.Add(reactionTime);
            // всем безошибочным ответам КНi присваивается по таблице 2.5"
            ReliabilityUpft130 += UPFT130Reliability.FromSingleReaction(reactionTime);
            ReliabilityUpft130Counter += 1;
            break;
          case ReactionError.Premature:
            TestErrors.PrematureCount++;
            // "всем ошибочным ответам (упреждение, запаздывание) присваивается КНi=0%"
            ReliabilityUpft130 += 0;
            ReliabilityUpft130Counter += 1;
            break;
          case ReactionError.Missed:
            TestErrors.MissedCount++;
            // "всем ошибочным ответам (упреждение, запаздывание) присваивается КНi=0%"
            ReliabilityUpft130 += 0;
            ReliabilityUpft130Counter += 1;
            break;
    		  case ReactionError.LogicError:
            TestErrors.LogicErrorCount++;
            break;
		      case ReactionError.Cancelled:
			      //Этот атом был отменён. Игнорируем его.
			      break;
          default:
            System.Diagnostics.Debug.Fail("WTF? Что это за реакция???????");
            break;
        }
      }

      // Можно пронаблюдать все времена и ошибки в ряд... для целей отладки.
      // allReactionTimes

      SvmrResults.IPN1 = ReliabilityUpft130Counter == 0 ?
        0 : (ReliabilityUpft130 / (double)ReliabilityUpft130Counter);

      // Фильтрация реакций по времени перед статистической обработкой
      // Некоторые реакции могут быть переклассифицированы в пропущенные или преждевременные
      times_array = RejectReactions(times_array);

      double[] data_for_statistics = times_array.ToArray();
 
      // Отдаем последовательность реакций.
      SvmrResults.SVMR_REACTIONS = data_for_statistics;

      //
      SvmrResults.SvmrIndices.EfficiencyOfOperation = MakeWorkabilityLevel(data_for_statistics);

      try
      {
        var mathStatData = new PskOnline.Math.Statistics.StatData();
        // Статистика по отфильтрованным реакциям.
        PskOnline.Math.Statistics.Calculator.CalcStatistics(data_for_statistics, mathStatData);

        // Распределение реакций
        PskOnline.Math.Statistics.Calculator.MakeDistribution(data_for_statistics, mathStatData, HistoMin, HistoMax, HistoStep);

        // Плотность вероятности для последовательности реакций
        PskOnline.Math.Statistics.Calculator.MakeProbabilityDensity(data_for_statistics, mathStatData, HistoMin, HistoMax, DensityStep);

        SvmrResults.SvmrStatistics = _mapper.Map<StatData>(mathStatData);
                                  
        MakeZLParams(SvmrResults.SvmrStatistics, SvmrResults.SvmrIndices);
               
        SvmrResults.ResultsStatisticsReliability = 1;
      }
      catch( System.ArgumentException )
      {
        SvmrResults.ResultsStatisticsReliability = 0;
        ZeroAllStatistics(Statistics);
//        throw new DataProcessingException(string.Format(resources.too_few_valid_reactions_in_results_format, data_for_statistics.Length));
      }

      return base.ProcessorOutputData;
    }

    double HistoMin => 100;

    double HistoMax => 2100;

    double HistoStep => 20;

    double DensityStep => 5;

    private void ZeroAllStatistics(StatData statistics)
    {
      if (statistics == null) return;

      statistics.Count = 0;
      statistics.asymmetry = 0;
      statistics.dispersion = 0;
      statistics.kurtosis = 0;
      statistics.m = 0;
      statistics.max = 0;
      statistics.min = 0;
      statistics.sigma = 0;
      statistics.varRange = 0;
      statistics.variation = 0;

      ZeroDistribution(out statistics.distribution, HistoMin, HistoMax, HistoStep);
      ZeroDistribution(out statistics.probability_density, HistoMin, HistoMax, DensityStep);
    }

    private void ZeroDistribution(out Distribution distribution, double min, double max, double channel_width)
    {
      int channel_count = (int)((max - min) / channel_width + 1); // На всякий случай один запасной...

      distribution = new Distribution();
      distribution.channels = new double[channel_count];
      distribution.channel_count = channel_count;
      distribution.channel_width = channel_width;
      distribution.min = min;
      distribution.max = max;
      distribution.mode = 0;
      distribution.mode_amplitude = 0;
    }

    private float CorrectionTimeSeconds
    {
      get
      {
        return m_Settings.CorrectionTimeSeconds;
      }
    }

    private bool IsTimeInSecondsOK(float reactionTime)
    {
      double reactionTimeMs = reactionTime * 1000.0;

      return (reactionTimeMs <= ((double) this.m_Settings.MaxReactionTimeSeconds*1000))
             && (reactionTimeMs >= ((double) this.m_Settings.MinReactionTimeSeconds*1000));
    }

    private void MakeZLParams(StatData stat, Indices indices)
    {
      // копируем распределение (так как в процессе мы его немного покорежим)
      var distrib = new Distribution(stat.distribution);

      // Нормируем интеграл распределения
      double sum = 0.0f;
      for( int i = 0; i < distrib.channel_count; ++i )
      {
        sum += distrib.channels[i];
      }

      // Нормируем элементы массива
      for( int i = 0; i < distrib.channel_count; ++i )
      {
        distrib.channels[i] /= sum;
      }

      // Также нормируем амплитуду моды... а нужно ли?
      double modeAmplitude = distrib.mode_amplitude / sum;

      // Значение моды
      double mode = distrib.mode;

      // Среднее значение
      double middle; //= statist.Middle; 

      // полумода...
      double halfMode = modeAmplitude / 2.0f;
      
//      bool start = false;
//      int intCount = 0;
      
      double integral = 0;

      int mode_channel = (int)((distrib.mode - distrib.min) / distrib.channel_width);
      int start_channel = mode_channel;
      int end_channel = mode_channel;

      for( 
           int i = mode_channel;
           i > -1;
           --i
          )
      {
        if (distrib.channels[i] > halfMode)
        {
          integral += distrib.channel_width;
        }
        else
        {
          // провалились ниже половины моды, выходим из цикла
          start_channel = i + 1;
        }
      }
      for( int i = mode_channel + 1; i < distrib.channel_count; ++i )
      {
        if (distrib.channels[i] > halfMode)
        {
          integral += distrib.channel_width;
        }
        else
        {
          // провалились ниже половины моды, выходим из цикла
          end_channel = i - 1;
        }
      }
  
      double time0_5 = integral;

      double startVal = (distrib.min + start_channel * distrib.channel_width) / 1000.0f;
      double endVal = (distrib.min + end_channel * distrib.channel_width) / 1000.0f;
      
      mode /= 1000.0f;
      time0_5 /= 1000.0f;

      middle = startVal + 0.5 * time0_5;

      if( mode == 0.0d || middle == 0.0d || time0_5 == 0.0d )
      {
        indices.ZL_SFL = double.NaN;
        indices.ZL_FCL = double.NaN;
        indices.ZL_RS = double.NaN;
        throw new DataProcessingException(resources.zl_not_enough_data);
      }

      // 1. Функциональный уровень системы
      indices.ZL_SFL = Math.Log( 1.0f / ( mode * time0_5 ) );

      // 2. Устойчивость реагирования
      indices.ZL_RS = Math.Log(modeAmplitude / time0_5);

      // 3. Уровень функциональных возможностей
      indices.ZL_FCL = Math.Log(modeAmplitude / (time0_5 * middle));
    }

    private double MakeWorkabilityLevel(double[] reactions)
    {
      double fWorkabilityLevel = 0.0d;
      double WorkabilityLevel = 0.0d;
      double Reaction = 0.0d;

      for (int i = 0; i < reactions.Length; i++)
      {
        Reaction = reactions[i];

        if (Reaction < 200.0f)
        {
          WorkabilityLevel = 110.0f;
        }
        else if( (Reaction >= 200.0f) && (Reaction < 220.0f) )
        {
          WorkabilityLevel = 100.0f;
        }
        else if( (Reaction >= 220.0f) && (Reaction < 240.0f))
        {
          WorkabilityLevel = 90.0f;
        }
        else if( (Reaction >= 240.0f) && (Reaction < 260.0f))
        {
          WorkabilityLevel = 80.0f;
        }
        else if( (Reaction >= 260.0f) && (Reaction < 280.0f))
        {
          WorkabilityLevel = 70.0f;
        }
        else if( (Reaction >= 280.0f) && (Reaction < 300.0f) )
        {
          WorkabilityLevel = 60.0f;
        }
        else if ((Reaction >= 300.0f) && (Reaction < 320.0f))
        {
          WorkabilityLevel = 50.0f;
        }
        else if( (Reaction >= 320.0f) && (Reaction < 340.0f) )
        {
          WorkabilityLevel = 40.0f;
        }
        else if( (Reaction >= 340.0f) && (Reaction < 360.0f) )
        {
          WorkabilityLevel = 30.0f;
        }
        else if( (Reaction >= 360.0f) && (Reaction < 380.0f) )
        {
          WorkabilityLevel = 20.0f;
        }
        else if ((Reaction >= 380.0f) && (Reaction < 400.0f))
        {
          WorkabilityLevel = 10.0f;
        }
        else if (Reaction >= 400.0f)
        {
          WorkabilityLevel = 0.0f;
        }

        fWorkabilityLevel += WorkabilityLevel;
      }

      fWorkabilityLevel /= ((double)(reactions.Length + this.SvmrResults.SvmrErrors.TotalCount));
      fWorkabilityLevel /= 100.0f;

      return fWorkabilityLevel;

    } // double MakeWorkabilityLevel ()

    

      
    /// <summary>
    /// Удаляет реакции, не удовлетворяющие ограничениям
    /// по минимальному и максимальному времени реакции,
    /// заданным в настройках.
    /// ("Ложные" и "Запаздывающие" реакции).
    /// Также корректирует кол-во правильных реакций и ошибок в TestErrors
    /// </summary>
    /// <param name="reaction_times"></param>
    private List<double> RejectReactions(List<double> reaction_times)
    {
      List<double> filteredTimes = new List<double>(reaction_times.Count);
      for( int i = 0; i < reaction_times.Count; ++i )
      {
        if( reaction_times[i] > ((double)this.m_Settings.MaxReactionTimeSeconds * 1000) )
        {
          log.InfoFormat(
            "Reclassifying non-erred reaction of {0} ms (> max reaction time {1}) as missed",
            reaction_times[i],
            m_Settings.MaxReactionTimeSeconds * 1000.0
            );
          ++TestErrors.MissedCount;
          --TestErrors.NormalCount;
          ++TestErrors.FilteredMissedCount;
          continue;
        }
        if( reaction_times[i] < ((double)this.m_Settings.MinReactionTimeSeconds * 1000) )
        {
          log.Info(
            $"Reclassifying non-erred reaction of {reaction_times[i]} ms " + 
            $"(<min reaction time {m_Settings.MinReactionTimeSeconds * 1000.0}) as premature"
          );
          ++TestErrors.PrematureCount;
          --TestErrors.NormalCount;
          ++TestErrors.FilteredPrematureCount;
          continue;
        }
        filteredTimes.Add(reaction_times[i]);
      }
      return filteredTimes;
    }

    public override IMethodSettings Get()
    {
      return this.m_Settings;
    }

    public override void Set(IMethodSettings settings)
    {
      m_Settings = settings as ProcessingSettings ?? 
        throw new ArgumentException(
          "The argument must be of type PskOnline.Methods.Svmr.Processing.ProcessingSettings",
          nameof(settings));
    }


    public void UTest_SetStateMatrixState(int stateMatrixRow, int stateMatrixCol)
    {
    }

    private IMapper _mapper;

    protected ProcessingSettings m_Settings = null;

    /// <summary>
    /// Содержит все наши (а также все базовые) результаты.
    /// 
    /// Такое кривое имя дано специально ,чтобы результаты теста были после всего остального.
    /// </summary>
    private SvmrResults SvmrResults => (SvmrResults)ProcessorOutputData;

    public TestErrors TestErrors => SvmrResults.SvmrErrors;
        
    public StatData Statistics => SvmrResults.SvmrStatistics;
        
  }
}
