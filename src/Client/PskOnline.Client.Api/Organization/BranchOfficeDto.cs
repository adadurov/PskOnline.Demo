namespace PskOnline.Client.Api.Organization
{
  public class BranchOfficeDto
  {
    public string Id { get; set; }

    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the ID of the time zone
    /// that the branch office is located in.
    /// </summary>
    public string TimeZoneId { get; set; }
  }
}
