namespace PskOnline.Methods.Hrv.Processing.Logic
{
  using PskOnline.Methods.Hrv.Processing.Logic.Personal;

  public class HrvDataProcessor_Personal : HrvBasicDataProcessor, ITwoDimConclusionDbProvider
  {
    log4net.ILog log = log4net.LogManager.GetLogger(typeof(HrvDataProcessor_Personal));

    public HrvDataProcessor_Personal()
    {
    }

    protected override CrvTwoDimConclusionDatabase GetTwoDimConslusionDatabase()
    {
      return new CrvTwoDimConclusionDatabase_Personal();
    }

  }
}
