namespace PskOnline.Server.Plugins.RusHydro.Logic
{
  using PskOnline.Methods.Hrv.ObjectModel;
  using PskOnline.Methods.Svmr.ObjectModel;
  using PskOnline.Server.ObjectModel;

  public class TestData
  {
    public RusHydro.ObjectModel.Employee Employee { get; set; }

    public Inspection Inspection { get; set; }

    public HrvRawData HrvRawData { get; set; }

    public SvmrRawData SvmrRawData { get; set; }
  }
}
