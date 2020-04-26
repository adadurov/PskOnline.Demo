namespace PskOnline.Methods.ObjectModel.Statistics
{
  using PskOnline.Methods.ObjectModel.Attributes;

  /// <summary>
  /// Параметры распределения величины
  /// </summary>
  public class StatData
  {
    public StatData()
    {
      distribution = new Distribution();
      probability_density = new Distribution();
    }

    public StatData(StatData src)
    {
      this.m = src.m;
      this.sigma = src.sigma;
      this.dispersion = src.dispersion;
      this.asymmetry = src.asymmetry;
      this.kurtosis = src.kurtosis;
      this.variation = src.variation;
      this.varRange = src.varRange;
      this.min = src.min;
      this.max = src.max;
      this.distribution = new Distribution(src.distribution);
      this.Count = src.Count;
    }

    /// <summary>
    /// Математическое ожидание
    /// </summary>
    [ScriptComment("Математическое ожидание", "", "")]
    [Exportable] public double m = 0;

    /// <summary>
    /// Среднеквадратическое отклонение
    /// </summary>
    [ScriptComment("Стандартное отклонение", "", "")]
    [Exportable] public double sigma = 0;

    /// <summary>
    /// Дисперсия, квадратных единиц
    /// </summary>
    [ScriptComment("Дисперсия", "", "")]
    [Exportable] public double dispersion = 0;

    /// <summary>
    /// Минимум
    /// </summary>
    [ScriptComment("Минимальное значение ряда", "", "")]
    [Exportable] public double min = 0;

    /// <summary>
    /// Максимум
    /// </summary>
    [ScriptComment("Максимальное значение ряда", "", "")]
    [Exportable] public double max = 0;

    /// <summary>
    /// Коэффициент вариации 
    /// </summary>
    [ScriptComment("Коэффициент вариации", "", "")]
    [Exportable] public double variation = 0;

    /// <summary>
    /// Вариационный размах
    /// </summary>
    [ScriptComment("Вариационный размах", "", "")]
    [Exportable] public double varRange = 0;

    ///// <summary>
    ///// Мода
    ///// </summary>
    //  [NeuroLab.BioMouse.script.ScriptComment("Мода", "", "") ]
    //  [NeuroLab.BioMouse.common.export.Exportable]
    //  public double mode = 0;

    ///// <summary>
    ///// Амплитуда моды
    ///// </summary>
    //  [NeuroLab.BioMouse.script.ScriptComment("Амплитуда моды", "", "") ]
    //  [NeuroLab.BioMouse.common.export.Exportable]
    //  public double modeAmplitude = 0;

    /// <summary>
    /// Асимметрия
    /// </summary>
    [ScriptComment("Асимметрия", "", "")] [Exportable]
    public double asymmetry = 0;

    /// <summary>
    /// Эксцесс
    /// </summary>
    [ScriptComment("Эксцесс", "", "")] [Exportable]
    public double kurtosis = 0;

    /// <summary>
    /// Распределение (гистограмма)
    /// </summary>
    [ScriptComment("Гистограмма распределения", "", "")]
    public Distribution distribution = new Distribution();

    /// <summary>
    /// Плотность вероятности
    /// </summary>
    [ScriptComment("Плотность вероятности", "", "")]
    public Distribution probability_density = new Distribution();

    /// <summary>
    /// Количество измерений
    /// </summary>
    [ScriptComment("Количество измерений", "", "")] [Exportable]
    public int Count = 0;
  }
}
