namespace PskOnline.Server.Plugins.RusHydro.ObjectModel
{
  /// <summary>
  /// Показатель LSR
  /// See table 1.2 from A_2556-02_МС.pdf
  /// (Методический справочник УПФТ 1/30 )
  /// 
  /// Не меняйте формат названий, он привязан к строчкам в ресурсах!
  /// </summary>
  public enum LSR_HrvFunctionalState
  {
    /// <summary>
    /// Критическое 
    /// </summary>
    Critical_0 = 0,

    /// <summary>
    /// Негативное
    /// </summary>
    Negative_1 = 1,

    /// <summary>
    /// Предельно допустимое
    /// </summary>
    OnTheEdge_2 = 2,

    /// <summary>
    /// Допустимое
    /// </summary>
    Acceptable_3 = 3,

    /// <summary>
    /// Близкое к оптимальному
    /// </summary>
    NearOptimal_4 = 4,

    /// <summary>
    /// Оптимальное
    /// </summary>
    Optimal_5 = 5
  }

}
