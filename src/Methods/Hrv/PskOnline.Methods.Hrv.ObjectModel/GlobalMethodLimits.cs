namespace PskOnline.Methods.Hrv.ObjectModel
{
  /// <summary>
  /// defines global limits for applicability of the HRV method implementation
  /// </summary>
  public class GlobalMethodLimits
  {
    /// <summary>
    /// ����������� ������� ������������, ������ ���
    /// </summary>
    public static int MinPatientAgeInFullYears => 14;
    /// <summary>
    /// ���������� ���������� ��� � ������ � ������
    /// </summary>
    public static double MinHeartRateInPPM => 36;

    /// <summary>
    /// ����������� ���������� ��� � �����, ������ � ������
    /// </summary>
    public static double MaxHeartRateInPPM => 130.0;

    /// <summary>
    /// ���������� ���������� ������������ ������-��������� � ��������.
    /// ������������� ����������� ���������� ���.
    /// </summary>
    public static double MinCardioCycleInSeconds => 60.0 / MaxHeartRateInPPM;

    /// <summary>
    /// ����������� ���������� ������������ ������-��������� � ��������.
    /// ������������� ����������� ���������� ���.
    /// </summary>
    public static double MaxCardioCycleInSeconds => 60 / MinHeartRateInPPM;

    /// <summary>
    /// ���������� ���������� ������������ ������-��������� � �������������.
    /// ������������� ������������� ������.
    /// </summary>
    public static double MinCardioCycleInMilliseconds => MinCardioCycleInSeconds * 1000;

    /// <summary>
    /// ����������� ���������� ������������ ������-��������� � �������������.
    /// ������������� ���������� ���������� ���.
    /// </summary>
    public static double MaxCardioCycleInMilliseconds => MaxCardioCycleInSeconds * 1000;

    /// <summary>
    /// ����������� ���������� ���������� ����� �����
    /// ����������������� ������-�����������,
    /// ���������� � �������� ������� ���������
    /// (��� ���������� ������������ ������� GetRelativeDifferenceOfIntervals)
    /// !!! �� � %%, � � ������ (��������, 0.35, ��� ������������� 35%)!!!
    /// </summary>
    public static double MaxRelativeDifferenceOfSuccessiveIntervals => 0.35d;

    /// <summary>
    /// ���������� ������� ����� ������ � ������ ����������,
    /// ���������� � �������� ������� ���������.
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
