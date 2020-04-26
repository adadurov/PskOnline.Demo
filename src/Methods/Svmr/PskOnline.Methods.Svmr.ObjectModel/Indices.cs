namespace PskOnline.Methods.Svmr.ObjectModel
{
  using PskOnline.Methods.ObjectModel.Attributes;

  public class Indices
  {
    /// <summary>
    /// ������� ������������� ������������ � ��������� [0; 1].
    /// </summary>
    [Exportable()]
    public double EfficiencyOfOperation = 0.0f;

    /// <summary>
    /// ���������� ��������-����������
    /// ��� (�������������� ������� �������)
    /// </summary>
    [Exportable()]
    public double ZL_SFL = 0.0;

    /// <summary>
    /// ���������� ��������-����������
    /// �� (������������ �������)
    /// </summary>
    [Exportable()]
    public double ZL_RS = 0.0;

    /// <summary>
    /// ���������� ��������-����������
    /// ��� (������� �������������� ������������)
    /// </summary>
    [Exportable()]
    public double ZL_FCL = 0.0;
  }
}
