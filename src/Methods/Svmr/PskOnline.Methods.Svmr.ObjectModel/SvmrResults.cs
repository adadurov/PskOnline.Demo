namespace PskOnline.Methods.Svmr.ObjectModel
{
  using PskOnline.Methods.ObjectModel;
  using PskOnline.Methods.ObjectModel.Attributes;
  using PskOnline.Methods.ObjectModel.Method;
  using PskOnline.Methods.ObjectModel.Statistics;

  /// <summary>
  /// ����������� ���������� ������������ ��� �������� ����.
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
    /// � ������, ���� �������� ������� ���������� ����� 0,
    /// ����������, ����������� �� ������ �������������� ����������� 
    /// ���� "����� �������" �� �������� ������������
    /// (� ��� ����� ����������� ������� (�����������, ��.��.���., ���������,
    /// ����������� � ��������� �����������, � ����� ���������� ��������-���������� � ������).
    /// </summary>
    [Exportable(910)]
    public double ResultsStatisticsReliability { get; set; }

    /// <summary>
    /// ���������� �� �������� ����
    /// </summary>
    [ScriptComment("���������� ���� � �������������", "", "")]
    [Exportable(900, "t_��", "CorrectReactions")]
    public StatData SvmrStatistics { get; set; }

    /// <summary>
    /// ������ ������������
    /// </summary>
    [ScriptComment("������ ����", "", "")]
    [Exportable(900)]
    public TestErrors SvmrErrors { get; set; }

    /// <summary>
    /// �������� ����������� ����������
    /// </summary>
    [ScriptComment("�������� ����������� ����������", "", "")]
    [Exportable(800)]
    public Indices SvmrIndices { get; set; }

    /// <summary>
    /// ������������ ���������� ���������� (���1), %
    /// �������� ������������� ����������� ����-1/30
    /// ��� ������� �������� ������������ ���������� ��� ������ �������
    /// (��. ������� 2.5)
    /// </summary>
    [Exportable(700)]
    public double IPN1 { get; set; }

    [Exportable(600)]
    public ulong TestDuration => (ulong)(TestInfo.FinishTime - TestInfo.StartTime).TotalSeconds;

    /// <summary>
    /// ������� (����������)
    /// </summary>
    [ScriptComment("������������������ �������", "", "")]
    public double[] SVMR_REACTIONS { get; set; }

    public TestSettings TestSettings { get; set; }

    /// <summary>
    /// ��������������, %
    /// (��������� ���������� ����������� �������� � ������ ���������� ��������)
    /// </summary>
    public double Microparoxysm => 100.0 * ((double)SvmrErrors.MissedCount) / ((double)SvmrErrors.TotalCount);

    /// <summary>
    /// % ������������ �������
    /// (��������� ���������� "������������" ������� � ������ ���������� ��������)
    /// </summary>
    public double NormalResponsePercent => 100.0 * ((double)SvmrErrors.NormalCount) / ((double)SvmrErrors.TotalCount);

  }
}
