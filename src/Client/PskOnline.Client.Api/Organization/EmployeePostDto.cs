namespace PskOnline.Client.Api.Organization
{
  using System;

  using PskOnline.Client.Api.Models;

  public class EmployeePostDto
  {
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the associated user ID
    /// </summary>
    public string UserId { get; set; }

    public string ExternalId { get; set; }

    public string LastName { get; set; }

    public string FirstName { get; set; }

    public string Patronymic { get; set; }

    public DateTime BirthDate { get; set; }

    public Gender Gender { get; set; }

    /// <summary>
    /// Gets or sets the ID of the department that the user belongs to
    /// </summary>
    public string DepartmentId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the position (within the tenant) that the user belongs to
    /// </summary>
    public string PositionId { get; set; }

    public DateTime? DateOfEmployment { get; set; }
  }
}
