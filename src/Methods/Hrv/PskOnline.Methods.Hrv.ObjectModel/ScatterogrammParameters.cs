namespace PskOnline.Methods.Hrv.ObjectModel
{
  using PskOnline.Methods.ObjectModel.Attributes;

  /// <summary>
  /// Scatterogramm parameters
  /// </summary>
  public class ScatterogrammParameters
  {
    /// <summary>
    /// проекция большой оси скаттерограммы на ось координат (???)
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "ScatterParams_Alpha")]
    public double AxisA = 0;

    /// <summary>
    /// проекция малой оси скаттерограммы на ось координат (???)
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "ScatterParams_Beta")]
    public double AxisB = 0;

    /// <summary>
    /// 
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "ScatterParams_AlphaByBeta")]
    public double AxisAbyAsixB
    {
      get
      {
        if ((0 != AxisB) && (AxisB != double.NaN))
        {
          return AxisA / AxisB;
        }
        return double.NaN;
      }
    }

    #region координаты концов большой полуоси
    public double AX1 = 0;
    public double AX2 = 0;
    public double AY1 = 0;
    public double AY2 = 0;
    #endregion

    #region координаты концов малой полуоси
    public double BX1 = 0;
    public double BX2 = 0;
    public double BY1 = 0;
    public double BY2 = 0;
    #endregion

    /// <summary>
    /// координата центра масс скаттерограммы X
    /// </summary>
    double CX = 0;

    /// <summary>
    /// координата центра масс скаттерограммы Y
    /// </summary>
    double CY = 0;

    /// <summary>
    /// Вариационный размах
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "ScatterParams_dNN")]
    public double DNN = 0;

    /// <summary>
    /// Индекс функционального состояния
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "ScatterParams_FSI")]
    public double FSI = 0;
  }

}
