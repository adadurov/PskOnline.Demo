namespace PskOnline.Methods.Hrv.ObjectModel
{
  using PskOnline.Methods.ObjectModel;
  using PskOnline.Methods.ObjectModel.Attributes;
  using PskOnline.Methods.ObjectModel.Method;
  using PskOnline.Methods.ObjectModel.PhysioData;
  using PskOnline.Methods.ObjectModel.Statistics;


  /// <summary>
  /// Расширенные результаты обследования для методики ВКР.
  /// </summary>
  public class HrvResults : IMethodProcessedData
  {
    [Exportable(1000)]
    public TestInfo TestInfo { get; set; }

    /// <summary>
    /// Статистика ВКР
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "CrvStatistics")]
    [Exportable(900, "NN", "NN")]
    public StatData CRV_STAT { get; set; }

    [Exportable(850)]
    public HrvIndicators Indicators { get; set; }

    [Exportable(800, "RELIABILITY", "RELIABILITY")]
    public ResultsReliabilityEstimation ResultsReliability { get; set; }

    [ScriptComment("Параметры усредненного кардио-цикла", "PskOnline.Methods.physio.cardio.processing.strings", "AverageCycleParameters")]
    public PpgCycleParameters CrvAveragePpgCycleParameters = null;

    [ScriptComment("Параметры скаттерограммы", "PskOnline.Methods.physio.cardio.processing.strings", "ScatterogrammParameters")]
    [Exportable]
    public ScatterogrammParameters CRV_SCATTEROGRAMM_PARAMETERS = null;

    /// <summary>
    /// Все отметки сердечных сокращений
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "PulseMarks")]
    public double[] CRV_HR_MARKS { get; set; }

    /// <summary>
    /// Отметки сердечных сокращений с оценкой качества
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "PulseMarksWithReliabilityEstimation")]
    public RatedContractionMark[] RATED_HR_MARKS { get; set; }

    /// <summary>
    /// Кардио-интервалы (величина и время появления
    /// интервала по международному стандарту)
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "IntervalsArray")]
    public CardioInterval[] CRV_INTERVALS { get; set; }

    /// <summary>
    /// спектр кардио-интервалов
    /// </summary>
    [Exportable(400)]
    public HrvSpectrumResult IntervalsSpectrum { get ; set; }

    /// <summary>
    /// спектр сигнала ФПГ
    /// </summary>
    public Spectrum SignalSpectrum { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "AnalyzedSignal")]
    public PhysioSignalView CRV_CARDIO_SIGNAL { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "SignalFromADC")]
    public PhysioSignalView CRV_CARDIO_SIGNAL_ADC { get; set; }

    [Exportable(300)]
    public SignalType SignalType { get; set; }

    public HrvResults()
    {
      TestInfo = new TestInfo();
      CRV_STAT = new StatData();
      Indicators = new HrvIndicators();

      CRV_INTERVALS = new CardioInterval[0];
      RATED_HR_MARKS = new RatedContractionMark[0];

      IntervalsSpectrum = new HrvSpectrumResult();
      SignalSpectrum = new Spectrum();
      ResultsReliability = new ResultsReliabilityEstimation();

      CRV_CARDIO_SIGNAL = null;
      CRV_CARDIO_SIGNAL_ADC = null;
      CRV_HR_MARKS = new double[0];
    }
  }
}
