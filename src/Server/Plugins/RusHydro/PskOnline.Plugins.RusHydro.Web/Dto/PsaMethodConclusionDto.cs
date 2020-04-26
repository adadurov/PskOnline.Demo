namespace PskOnline.Server.Plugins.RusHydro.Web.Dto
{
  public class PsaMethodConclusionDto
  {
    /// <summary>
    /// test id from the database where results are stored
    /// </summary>
    public string TestId { get; set; }

    /// <summary>
    /// status
    /// </summary>
    public PsaStatus Status { get; set; }

    /// <summary>
    /// text containing verbal representation of status (always in Russian)
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// suggested color for rendering the Text
    /// </summary>
    public System.Drawing.Color Color { get; set; }

    /// <summary>
    /// comment (may contain some detailed results, for reference)
    /// </summary>
    public string Comment { get; set; }
  }
}
