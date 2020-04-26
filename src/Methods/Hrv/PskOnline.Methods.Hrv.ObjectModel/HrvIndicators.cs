using PskOnline.Methods.ObjectModel.Attributes;
using PskOnline.Methods.ObjectModel.Statistics;

namespace PskOnline.Methods.Hrv.ObjectModel
{
  /// <summary>
  /// Показатели по результатам ВСР.
  /// </summary>
  public class HrvIndicators
	{
    /// <summary>
    /// Индекс напряжения Баевского
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "IN")]
    [Exportable]
    public double IN;

    /// <summary>
    /// Индекс напряжения Баевского, максимизированный
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "IN_maximized")]
    [Exportable]
    public double IN_Max;

    /// <summary>
    /// Мода, соответствующая значению максимизированного индекса напряжения Баевского
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "IN_maximized_Mode")]
    [Exportable]
    public double IN_MaxMode;

    /// <summary>
    /// Индекс напряжения Баевского, минимизированный
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "IN_minimized")]
    [Exportable]
    public double IN_Min;

    /// <summary>
    /// Мода, соответствующая значению минимизированного индекса напряжения Баевского
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "IN_minimized_Mode")]
    [Exportable]
    public double IN_MinMode;

    /// <summary>
    /// Усредненный индекс напряжения Баевского
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "IN_mid")]
    [Exportable]
    public double IN_Mid;

    /// <summary>
    /// Мода, соответствующая значению "среднего" ИН
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "IN_mid_Mode")]
    [Exportable]
    public double IN_MidMode;

    /// <summary>
    /// Вегетативный показатель ритма
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "VPR")]
    [Exportable]
    public double VPR;

    /// <summary>
    /// Показатель активности процессов регуляции
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "PAPR")]
    [Exportable]
    public double PAPR;

    /// <summary>
    /// Индекс вегетативного равновесия
    /// </summary>
    [ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "IVR")]
	  [Exportable]
    public double IVR;

    /// <summary>
    /// Психофизиологическая цена адаптации (автор?)
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
    /// Триангулярный индекс
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
