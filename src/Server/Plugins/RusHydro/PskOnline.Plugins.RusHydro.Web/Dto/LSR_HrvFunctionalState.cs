namespace PskOnline.Server.Plugins.RusHydro.Web.Dto
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
    Critical_0 = 0,     // Критическое 

    Negative_1 = 1,     // Негативное

    OnTheEdge_2 = 2,    // Предельно допустимое

    Acceptable_3 = 3,   // Допустимое

    NearOptimal_4 = 4,  // Близкое к оптимальному

    Optimal_5 = 5       // Оптимальное
  }

}
