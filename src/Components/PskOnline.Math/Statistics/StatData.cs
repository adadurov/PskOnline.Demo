using System;

namespace PskOnline.Math.Statistics
{
  /// <summary>
  /// Параметры распределения величины
  /// </summary>
  public class StatData
  {
    public StatData()
    {
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
    public double m = 0;

    /// <summary>
    /// Среднеквадратическое отклонение
    /// </summary>
    public double sigma = 0;

    /// <summary>
    /// Дисперсия, квадратных единиц
    /// </summary>
    public double dispersion = 0;

    /// <summary>
    /// Минимум
    /// </summary>
    public double min = 0;

    /// <summary>
    /// Максимум
    /// </summary>
    public double max = 0;

    /// <summary>
    /// Коэффициент вариации 
    /// </summary>
    public double variation = 0;

    /// <summary>
    /// Вариационный размах
    /// </summary>
    public double varRange = 0;

    ///// <summary>
    ///// Мода
    ///// </summary>
    //  public double mode = 0;

    ///// <summary>
    ///// Амплитуда моды
    ///// </summary>
    //  public double modeAmplitude = 0;

    /// <summary>
    /// Асимметрия
    /// </summary>
    public double asymmetry = 0;

    /// <summary>
    /// Эксцесс
    /// </summary>
    public double kurtosis = 0;

    /// <summary>
    /// Распределение (гистограмма)
    /// </summary>
    public Distribution distribution = new Distribution();

    /// <summary>
    /// Плотность вероятности
    /// </summary>
    public Distribution probability_density = new Distribution();

    /// <summary>
    /// Количество измерений
    /// </summary>
    public int Count = 0;

  }
}
