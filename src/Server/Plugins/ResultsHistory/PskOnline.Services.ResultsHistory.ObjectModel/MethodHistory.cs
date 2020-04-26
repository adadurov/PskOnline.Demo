namespace PskOnline.Server.Plugins.ResultsHistory.ObjectModel
{
  using PskOnline.Server.Plugins.ResultsHistory.ObjectModel.Schema;

  public class MethodHistory
  {
    public string MethodId { get; set; }

    public SchemaDef Schema { get; set; }

    public SessionResult[] Items { get; set; }
  }
}
