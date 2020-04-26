using System;

namespace PskOnline.Server.Authority.API.Dto
{
  public class WorkplaceDescriptorDto
  {
    public string DisplayName { get; set; }

    public string TenantId { get; set; }

    public string BranchOfficeId { get; set; }

    public string DepartmentId { get; set; }

    public string[] Scopes { get; set; }

    /// <summary>
    /// The type of the workplace
    /// (e.g. 'dept_manager', 'branch_manager', 'branch_operator', etc.)
    /// </summary>
    public string WorkplaceType { get; set; }
  }
}
