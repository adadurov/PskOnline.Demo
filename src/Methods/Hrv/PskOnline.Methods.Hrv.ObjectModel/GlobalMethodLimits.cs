namespace PskOnline.Methods.Hrv.ObjectModel
{
  /// <summary>
  /// defines global limits for applicability of the HRV method implementation
  /// </summary>
  public class GlobalMethodLimits
  {
    /// <summary>
    /// Минимальный возраст обследуемого, полных лет
    /// </summary>
    public static int MinPatientAgeInFullYears => 14;
    /// <summary>
    /// Минимально допустимая ЧСС в ударах в минуту
    /// </summary>
    public static double MinHeartRateInPPM => 36;

    /// <summary>
    /// Максимально допустимая ЧСС в покое, ударах в минуту
    /// </summary>
    public static double MaxHeartRateInPPM => 130.0;

    /// <summary>
    /// Минимально допустимая длительность кардио-интервала в секундах.
    /// Соответствует максимально допустимой ЧСС.
    /// </summary>
    public static double MinCardioCycleInSeconds => 60.0 / MaxHeartRateInPPM;

    /// <summary>
    /// Максимально допустимая длительность кардио-интервала в секундах.
    /// Соответствует максимально допустимой ЧСС.
    /// </summary>
    public static double MaxCardioCycleInSeconds => 60 / MinHeartRateInPPM;

    /// <summary>
    /// Минимально допустимая длительность кардио-интервала в миллисекундах.
    /// Соответствует максимальному пульсу.
    /// </summary>
    public static double MinCardioCycleInMilliseconds => MinCardioCycleInSeconds * 1000;

    /// <summary>
    /// Максимально допустимая длительность кардио-интервала в миллисекундах.
    /// Соответствует минимально допустимой ЧСС.
    /// </summary>
    public static double MaxCardioCycleInMilliseconds => MaxCardioCycleInSeconds * 1000;

    /// <summary>
    /// Максимально допустимое отклонение между двумя
    /// последовательными кардио-интервалами,
    /// отнесенное к величине первого интервала
    /// (для вычисления используется функция GetRelativeDifferenceOfIntervals)
    /// !!! НЕ В %%, А В ДРОБЯХ (НАПРИМЕР, 0.35, ЧТО СООТВЕТСТВУЕТ 35%)!!!
    /// </summary>
    public static double MaxRelativeDifferenceOfSuccessiveIntervals => 0.35d;

    /// <summary>
    /// Возвращает разницу между первым и вторым интервалам,
    /// отнесенную к величине первого интервала.
    /// </summary>
    /// <param name="interval1"></param>
    /// <param name="interval2"></param>
    /// <returns></returns>
    public static double GetRelativeDifferenceOfIntervals(double interval1, double interval2)
    {
      System.Diagnostics.Debug.Assert(interval1 > 0);
      System.Diagnostics.Debug.Assert(interval2 > 0);
      return System.Math.Abs(interval1 - interval2) / (interval1);
    }
  }
}
