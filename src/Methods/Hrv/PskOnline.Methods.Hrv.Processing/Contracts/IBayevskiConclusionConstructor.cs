namespace PskOnline.Methods.Hrv.Processing.Contracts
{
  using PskOnline.Methods.Hrv.ObjectModel;
  using PskOnline.Methods.ObjectModel.Statistics;

  public interface IBayevskiConclusionConstructor
  {
    string GetConclusion(bool bUseNewLineSeparators, StatData CRV_STAT);
  }
}
