namespace PskOnline.Client.Api.Inspection
{

  public class TestDto : TestPostDto
  {
    /// <summary>
    /// Gets or sets the ID of the patient (employee) who has completed the inspection
    /// </summary>
    public string EmployeeId { get; set; }

    public string DepartmentId { get; set; }

    public string BranchOfficeId { get; set; }
  }
}

