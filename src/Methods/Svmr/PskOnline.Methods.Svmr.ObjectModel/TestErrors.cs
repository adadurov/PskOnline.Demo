namespace PskOnline.Methods.Svmr.ObjectModel
{
  using PskOnline.Methods.ObjectModel.Attributes;

  public class TestErrors  
	{
		public TestErrors () : base()
		{			
		}
		
		public TestErrors (TestErrors src) 
		{
			NormalCount = src.NormalCount;
			MissedCount = src.MissedCount;
			PrematureCount = src.PrematureCount;
		  LogicErrorCount = src.LogicErrorCount;
			TotalCount = src.TotalCount;
      FilteredPrematureCount = src.FilteredPrematureCount;
      FilteredMissedCount = src.FilteredMissedCount;
		}

	  /// <summary>
	  /// ���������� �������, ������� ��� ��������� ���� �������� ��� ���������������
	  /// ��� ������, ��� ������ ��� ���������, �� ����� ������� ��������� ������ ������ (������ 150 ��)
	  /// </summary>
	  [ScriptComment("���������� \"����������\", ��������������� ��� ���������������", "", "")]
    [Exportable]
    public int FilteredPrematureCount;

    /// <summary>
    /// ���������� �������, ������� ��� ��������� ���� �������� ��� ���������������
    /// ��� ������, ��� ������ ��������� � ��� �������, �� ����� ������� ��������� ������ ������ (������ 1 �)
    /// </summary>
    [ScriptComment("���������� \"����������\" �������, ��������������� ��� �����������", "", "")]
    [Exportable]
    public int FilteredMissedCount;

    /// <summary>
		/// �����
		/// </summary>
    [ScriptComment("���������� \"����������\" �������", "", "")]
    [Exportable]
    public int NormalCount;

		/// <summary>
		/// ���������� \"�����������\" �������� (������������� �������)
		/// </summary>
    [ScriptComment("���������� \"�����������\" �������� (������������� �������)", "", "")]
    [Exportable]
    public int MissedCount;

    /// <summary>
		/// ���������� \"���������������\" (������) �������
		/// </summary>
    [ScriptComment("���������� \"���������������\" (������) �������", "", "")]
    [Exportable]
    public int PrematureCount;

    /// <summary>
    /// ��� ������� ����������� ��� ������������ ���1
    /// </summary>
    [ScriptComment("���������� \"������������\" �������", "", "")]
    [Exportable]
    public int WrongCount => PrematureCount + MissedCount;

    /// <summary>
    /// ���������� ���� ������.
    /// �� ������� ������ (2017.07.23) ��� ������ �� ����������� ��� ������� ���1.
    /// ������ ��� ����� �� �����, ��� ��� �����.
    /// </summary>
    [Exportable]
    public int LogicErrorCount;
    
		/// <summary>
		/// ����� "��������" ��������
		/// </summary>
    [ScriptComment("����� ���������� \"��������\" ��������", "", "")]
    [Exportable]
    public int TotalCount;
	}
}
