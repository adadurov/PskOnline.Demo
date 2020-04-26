namespace PskOnline.Methods.Svmr.ObjectModel
{
  using PskOnline.Methods.ObjectModel;
  using PskOnline.Methods.ObjectModel.Attributes;
  using PskOnline.Methods.ObjectModel.Method;
  using PskOnline.Methods.ObjectModel.Statistics;

  /// <summary>
  /// Расширенные результаты обследования для методики ПЗМР.
  /// </summary>
  public class SvmrResults : IMethodProcessedData
  {
    public SvmrResults()
    {
      TestInfo = new TestInfo();
      SvmrStatistics = new StatData();
      SvmrErrors = new TestErrors();
      SvmrIndices = new Indices();
      SVMR_REACTIONS = new double[0];
      TestSettings = new TestSettings();
      ResultsStatisticsReliability = 1.0d;
    }

    [Exportable(1000)]
    public TestInfo TestInfo { get; set; }

    /// <summary>
    /// В случае, если значение данного показателя равно 0,
    /// показатели, вычисляемые на основе статистических показателей 
    /// ряда "время реакции" не являются достоверными
    /// (в том числе центральные моменты (матожидание, ср.кв.отл., дисперсия,
    /// гистограмма и плотность вероятности, а также показатели Зимкиной-Лоскутовой и другие).
    /// </summary>
    [Exportable(910)]
    public double ResultsStatisticsReliability { get; set; }

    /// <summary>
    /// Статистика по методике ПЗМР
    /// </summary>
    [ScriptComment("Статистика ПЗМР и распределение", "", "")]
    [Exportable(900, "t_ПР", "CorrectReactions")]
    public StatData SvmrStatistics { get; set; }

    /// <summary>
    /// Ошибки обследуемого
    /// </summary>
    [ScriptComment("Ошибки ПЗМР", "", "")]
    [Exportable(900)]
    public TestErrors SvmrErrors { get; set; }

    /// <summary>
    /// Итоговые комплексные показатели
    /// </summary>
    [ScriptComment("Итоговые комплексные показатели", "", "")]
    [Exportable(800)]
    public Indices SvmrIndices { get; set; }

    /// <summary>
    /// Интегральный показатель надежности (ИПН1), %
    /// Согласно методическому справочнику УПФТ-1/30
    /// это среднее значение коэффициента надежности для каждой реакции
    /// (см. таблицу 2.5)
    /// </summary>
    [Exportable(700)]
    public double IPN1 { get; set; }

    [Exportable(600)]
    public ulong TestDuration => (ulong)(TestInfo.FinishTime - TestInfo.StartTime).TotalSeconds;

    /// <summary>
    /// Реакции (тахограмма)
    /// </summary>
    [ScriptComment("Последовательность реакций", "", "")]
    public double[] SVMR_REACTIONS { get; set; }

    public TestSettings TestSettings { get; set; }

    /// <summary>
    /// Микропароксизм, %
    /// (отношение количества пропущенных стимулов к общему количеству стимулов)
    /// </summary>
    public double Microparoxysm => 100.0 * ((double)SvmrErrors.MissedCount) / ((double)SvmrErrors.TotalCount);

    /// <summary>
    /// % безошибочных реакций
    /// (отношение количества "безошибочных" реакций к общему количеству стимулов)
    /// </summary>
    public double NormalResponsePercent => 100.0 * ((double)SvmrErrors.NormalCount) / ((double)SvmrErrors.TotalCount);

  }
}
