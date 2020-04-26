namespace PskOnline.Server.Authority.API.Dto
{
  public class WorkplaceDto
  {
    /// <summary>
    /// The ID of the workplace application. Assigned by the server when the workplace is created.
    /// </summary>
    public string ClientId { get; set; }

    public string TenantId { get; set; }

    public string BranchOfficeId { get; set; }

    public string DepartmentId { get; set; }

    public string DisplayName { get; set; }

    public string Scopes { get; set; }

    /// <summary>
    /// The type of the workplace
    /// (e.g. 'dept_manager', 'branch_manager', 'branch_operator', etc.)
    /// </summary>
    public string WorkplaceType { get; set; }
  }
}
