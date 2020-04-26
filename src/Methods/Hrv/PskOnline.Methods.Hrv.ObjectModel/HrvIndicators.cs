using PskOnline.Methods.ObjectModel.Attributes;
using PskOnline.Methods.ObjectModel.Statistics;

namespace PskOnline.Methods.Hrv.ObjectModel
{
  /// <summary>
  /// ���������� �� ����������� ���.
  /// </summary>
  public class HrvIndicators
	{
    /// <summary>
    /// ������ ���������� ���������
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "IN")]
    [Exportable]
    public double IN;

    /// <summary>
    /// ������ ���������� ���������, �����������������
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "IN_maximized")]
    [Exportable]
    public double IN_Max;

    /// <summary>
    /// ����, ��������������� �������� ������������������ ������� ���������� ���������
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "IN_maximized_Mode")]
    [Exportable]
    public double IN_MaxMode;

    /// <summary>
    /// ������ ���������� ���������, ����������������
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "IN_minimized")]
    [Exportable]
    public double IN_Min;

    /// <summary>
    /// ����, ��������������� �������� ����������������� ������� ���������� ���������
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "IN_minimized_Mode")]
    [Exportable]
    public double IN_MinMode;

    /// <summary>
    /// ����������� ������ ���������� ���������
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "IN_mid")]
    [Exportable]
    public double IN_Mid;

    /// <summary>
    /// ����, ��������������� �������� "��������" ��
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "IN_mid_Mode")]
    [Exportable]
    public double IN_MidMode;

    /// <summary>
    /// ������������ ���������� �����
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "VPR")]
    [Exportable]
    public double VPR;

    /// <summary>
    /// ���������� ���������� ��������� ���������
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "PAPR")]
    [Exportable]
    public double PAPR;

    /// <summary>
    /// ������ ������������� ����������
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "IVR")]
	  [Exportable]
    public double IVR;

    /// <summary>
    /// �������������������� ���� ��������� (�����?)
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "PPPA")]
	  [Exportable]
    public double PPPA;

    [ScriptComment("RMSSD", "PskOnline.Methods.physio.cardio.processing.strings", "RMSSD")]
	  [Exportable]
    public double RMSSD;

    [ScriptComment("pNN50", "PskOnline.Methods.physio.cardio.processing.strings", "pNN50")]
	  [Exportable]
    public double pNN50;

    /// <summary>
    /// ������������� ������
    /// </summary>
    [ScriptComment("HRV_Triangular_Index", "PskOnline.Methods.physio.cardio.processing.strings", "HRV_Triangular_Index")]
    [Exportable]
    public double HRV_Triangular_Index;

    [ScriptComment("S0", "PskOnline.Methods.physio.cardio.processing.strings", "S0")]
	  [Exportable]
    public double S0;

    [ScriptComment("Sm", "PskOnline.Methods.physio.cardio.processing.strings", "Sm")]
	  [Exportable]
    public double Sm;

    [ScriptComment("Sd", "PskOnline.Methods.physio.cardio.processing.strings", "Sd")]
	  [Exportable]
    public double Sd;
  }
}
