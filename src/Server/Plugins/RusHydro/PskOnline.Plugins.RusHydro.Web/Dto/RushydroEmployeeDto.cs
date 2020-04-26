namespace PskOnline.Server.Plugins.RusHydro.Web.Dto
{
  using System;

  public class RushydroEmployeeDto
  {
    public Guid Id { get; set; }

    public string ExternalId { get; set; }

    public string FullName { get; set; }

    public string PositionName { get; set; }
  }
}
