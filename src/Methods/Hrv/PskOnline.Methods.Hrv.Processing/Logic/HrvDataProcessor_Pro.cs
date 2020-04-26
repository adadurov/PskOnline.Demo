namespace PskOnline.Methods.Hrv.Processing.Logic
{
  using PskOnline.Methods.Hrv.Processing.Logic.Pro;

  public class HrvDataProcessor_Pro : HrvBasicDataProcessor, ITwoDimConclusionDbProvider
  {
    public HrvDataProcessor_Pro()
    {
    }

    protected override CrvTwoDimConclusionDatabase GetTwoDimConslusionDatabase()
    {
      return new CrvTwoDimConclusionDatabase_Professional();
    }

  }
}
