namespace PskOnline.Server.Plugins.ResultsHistory.ObjectModel.Schema
{
  public class SchemaDef
  {
    public string MethodId { get; set; }

    public int Version { get; set; }

    public AttributeDefinition[] Attributes { get; set; }
  }
}
