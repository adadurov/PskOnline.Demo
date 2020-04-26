namespace PskOnline.Methods.Svmr.ObjectModel
{
  using PskOnline.Methods.ObjectModel.Attributes;

  public class Indices
  {
    /// <summary>
    /// Уровень эффективности деятельности в интервале [0; 1].
    /// </summary>
    [Exportable()]
    public double EfficiencyOfOperation = 0.0f;

    /// <summary>
    /// Показатели Зимкиной-Лоскутовой
    /// ФУС (функциональный уровень системы)
    /// </summary>
    [Exportable()]
    public double ZL_SFL = 0.0;

    /// <summary>
    /// Показатели Зимкиной-Лоскутовой
    /// УР (устойчивость реакций)
    /// </summary>
    [Exportable()]
    public double ZL_RS = 0.0;

    /// <summary>
    /// Показатели Зимкиной-Лоскутовой
    /// УФВ (уровень функциональных возможностей)
    /// </summary>
    [Exportable()]
    public double ZL_FCL = 0.0;
  }
}
