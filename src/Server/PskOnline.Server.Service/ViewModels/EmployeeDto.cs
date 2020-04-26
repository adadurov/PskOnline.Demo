namespace PskOnline.Server.Service.ViewModels
{
  using System;
  using System.ComponentModel.DataAnnotations;

  public class EmployeeDto
  {
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the associated user ID
    /// </summary>
    public string UserId { get; set; }

    public string ExternalId { get; set; }

    [Required(ErrorMessage = "Last name is required")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "First name is required")]
    public string FirstName { get; set; }

    public string Patronymic { get; set; }

    public DateTime BirthDate { get; set; }

    public Gender Gender { get; set; }

    /// <summary>
    /// Gets of sets the ID of the branch office within the tenant,
    /// that the user belongs to
    /// </summary>
    public string BranchOfficeId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the department that the user belongs to
    /// </summary>
    [Required(ErrorMessage = "Department is required")]
    public string DepartmentId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the position (within the tenant) that the user belongs to
    /// </summary>
    [Required(ErrorMessage = "Position is required")]
    public string PositionId { get; set; }

    public DateTime? DateOfEmployment { get; set; }
  }
}
