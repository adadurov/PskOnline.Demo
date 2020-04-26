namespace PskOnline.Client.Api.Organization
{
  using System;
  using System.Collections.Generic;
  using System.Text;

  public class DepartmentDto
  {
    public string Id { get; set; }

    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the branch office
    /// that the department belongs to
    /// </summary>
    public string BranchOfficeId { get; set; }
  }
}
