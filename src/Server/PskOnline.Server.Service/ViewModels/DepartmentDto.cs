namespace PskOnline.Server.Service.ViewModels
{
  using System.ComponentModel.DataAnnotations;

  public class DepartmentDto
  {
    public string Id { get; set; }

    [Required(ErrorMessage = "Department name is requied")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the branch office
    /// that the department belongs to
    /// </summary>
    [Required(ErrorMessage = "Branch office ID is requied")]
    public string BranchOfficeId { get; set; }
  }
}
