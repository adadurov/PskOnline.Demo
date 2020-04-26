namespace PskOnline.Client.Api.Organization
{
  public class EmployeeDto : EmployeePostDto
  {
    /// <summary>
    /// Gets of sets the ID of the branch office within the tenant,
    /// that the user belongs to
    /// </summary>
    public string BranchOfficeId { get; set; }
  }
}
