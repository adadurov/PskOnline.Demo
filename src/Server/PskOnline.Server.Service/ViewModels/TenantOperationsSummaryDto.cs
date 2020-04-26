namespace PskOnline.Server.Service.ViewModels
{
  using System;

  public class TenantOperationsSummaryDto
  {
    public string Id { get; set; }

    public string Name { get; set; }

    public DateTime ServiceExpireDate { get; set; }

    public long ServiceMaxUsers { get; set; }

    public long ServiceMaxEmployees { get; set; }

    public long ServiceActualUsers { get; set; }

    public long ServiceActualEmployees { get; set; }

    public long BranchOfficesCount { get; set; }

    public long DepartmentsCount { get; set; }

    public long NewInspectionsLast24Hours { get; set; }

    public long NewInspectionsLastWeek { get; set; }
  }
}
