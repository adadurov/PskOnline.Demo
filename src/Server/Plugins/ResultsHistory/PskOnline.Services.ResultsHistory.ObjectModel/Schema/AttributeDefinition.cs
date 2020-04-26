namespace PskOnline.Server.Plugins.ResultsHistory.ObjectModel.Schema
{
  public class AttributeDefinition
  {
    /// <summary>
    /// Gets or sets the key for the attribute
    /// (must be unique within the method)
    /// </summary>
    public string AttributeName { get; set; }

    /// <summary>
    /// Gets or sets the kind of the attribute
    /// </summary>
    public AttributeKind AttributeKind { get; set; }

    /// <summary>
    /// Gets or sets the decimal precision for the attribute.
    /// Valid only for the attributes of the Real kind.
    /// </summary>
    public int DecimalPrecision { get; set; }

    /// <summary>
    /// Gets or sets the array of attribute range definitions.
    /// Valid only for the attributes of the Real kind
    /// </summary>
    public AttributeRange[] Ranges { get; set; }
  }
}
