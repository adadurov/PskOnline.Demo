namespace PskOnline.Client.Api.Tenant
{
  public class TenantOperationsSummaryDto
  {
    public string Id { get; set; }

    public string Name { get; set; }

    public long ServiceActualUsers { get; set; }

    public long ServiceActualEmployees { get; set; }

    public long BranchOfficesCount { get; set; }

    public long DepartmentsCount { get; set; }

    public long NewInspectionsLast24Hours { get; set; }

    public long NewInspectionsLastWeek { get; set; }
  }

}
