using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Methods.Hrv.ObjectModel
{
  /// <summary>
  /// Параметры кардио-цикла в сигнале ФПГ
  /// </summary>
  public class PpgCycleParameters
  {
    /// <summary>
    /// А0, мВ
    /// </summary>
    //[NeuroLab.BioMouse.common.selfdoc.ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "CycleParams_A0")]
    public double A0 = 0;

    /// <summary>
    /// А1, мВ
    /// </summary>
    //[NeuroLab.BioMouse.common.selfdoc.ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "CycleParams_A1")]
    public double A1 = 0;

    /// <summary>
    /// Т, мс
    /// </summary>
    //[NeuroLab.BioMouse.common.selfdoc.ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "CycleParams_T")]
    public double T = 0;

    /// <summary>
    /// Т0, мс
    /// </summary>
    //[NeuroLab.BioMouse.common.selfdoc.ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "CycleParams_T0")]
    public double T0 = 0;

    /// <summary>
    /// Т1, мс
    /// </summary>
    //[NeuroLab.BioMouse.common.selfdoc.ScriptComment("", "PskOnline.Methods.physio.cardio.processing.strings", "CycleParams_T1")]
    public double T1 = 0;
  }
}
