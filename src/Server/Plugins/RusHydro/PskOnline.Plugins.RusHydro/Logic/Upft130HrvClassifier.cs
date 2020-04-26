namespace PskOnline.Server.Plugins.RusHydro.Logic
{
  using System;

  using PskOnline.Server.Plugins.RusHydro.ObjectModel;
  using PskOnline.Methods.Hrv.ObjectModel;

  /// <summary>
  /// Формирует сводку по результатам предсменного контроля.
  /// 
  /// </summary>
  public class Upft130HrvClassifier
  {
    private log4net.ILog _log = log4net.LogManager.GetLogger(typeof (Upft130HrvClassifier));

    public Upft130HrvClassifier()
    {
    }

    public static string Lsr2String(LSR_HrvFunctionalState state)
    {
      switch (state)
      {
        case LSR_HrvFunctionalState.Critical_0:
          return strings.LSR_0_Critical;
        case LSR_HrvFunctionalState.Negative_1:
          return strings.LSR_1_Negative;
        case LSR_HrvFunctionalState.OnTheEdge_2:
          return strings.LSR_2_OnTheEdge;
        case LSR_HrvFunctionalState.Acceptable_3:
          return strings.LSR_3_Acceptable;
        case LSR_HrvFunctionalState.NearOptimal_4:
          return strings.LSR_4_NearOptimal;
        case LSR_HrvFunctionalState.Optimal_5:
          return strings.LSR_5_Optimal;
        default:
          throw new NotSupportedException($"LSR functional state '{state}' not supported!");
      }
    }

    /// <summary>
    /// Convert VSR estimation to LSR estimation
    /// See table 1.2 from A_2556-02_МС.pdf
    /// (Методический справочник УПФТ 1/30 )
    /// </summary>
    /// <param name="Vsr"></param>
    /// <returns></returns>
    public LSR_HrvFunctionalState Vsr2Lsr(double Vsr)
    {
      if( Vsr > 0.8 ) return LSR_HrvFunctionalState.Optimal_5;
      if( Vsr > 0.64 ) return LSR_HrvFunctionalState.NearOptimal_4;
      if( Vsr > 0.37) return LSR_HrvFunctionalState.Acceptable_3;
      if( Vsr > 0.1 ) return LSR_HrvFunctionalState.OnTheEdge_2;
      if( Vsr > Upft130VsrCalculator.VSR_0_Critical ) return LSR_HrvFunctionalState.Negative_1;
      
      return LSR_HrvFunctionalState.Critical_0;
    }

    public PsaStatus Lsr2Status(LSR_HrvFunctionalState LSR)
    {
      return LSR >= LSR_HrvFunctionalState.NearOptimal_4 ? PsaStatus.Pass :
             LSR >= LSR_HrvFunctionalState.OnTheEdge_2 ? PsaStatus.Conditional_Pass : PsaStatus.Fail;      
    }

    public PreShiftHrvConclusion MakePreshiftConclusion(HrvResults neuroLabHrvData)
    {
      Upft130VsrCalculator calc = new Upft130VsrCalculator();
      double VSR = calc.HRV_to_VSR(neuroLabHrvData.CRV_STAT.m, neuroLabHrvData.CRV_STAT.sigma);
      PreShiftHrvConclusion conclusion = new PreShiftHrvConclusion();
      conclusion.StateMatrixRow = calc.Mrr2SmRow(neuroLabHrvData.CRV_STAT.m);
      conclusion.StateMatrixCol = calc.SigmaRR2SmCol(conclusion.StateMatrixRow, neuroLabHrvData.CRV_STAT.sigma);
      conclusion.VSR = VSR;
      conclusion.LSR = Vsr2Lsr(VSR);
      conclusion.LSR_Text = Lsr2String(conclusion.LSR);
      // refs #200
      // см. таблицу в разделе 4.6.2 в ТЗ
      conclusion.Status = Lsr2Status(conclusion.LSR);
      return conclusion;
    }

  }
}
