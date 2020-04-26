namespace PskOnline.Server.ObjectModel
{
  using System;

  public class Employee : TenantOwnedEntity
  {
    /// <summary>
    /// Gets or sets the associated user ID
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Gets or set the external employee ID
    /// (e.g. 'табельный номер')
    /// </summary>
    public string ExternalId { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Patronymic { get; set; }

    public string FullName => string.Join(" ", LastName, FirstName, Patronymic);

    public DateTime BirthDate { get; set; }

    public Gender Gender { get; set; }

    /// <summary>
    /// Gets of sets the ID of the branch office within the tenant,
    /// that the user belongs to
    /// </summary>
    public Guid BranchOfficeId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the department that the user belongs to
    /// </summary>
    public Guid DepartmentId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the position (within the tenant) that the user belongs to
    /// </summary>
    public Guid PositionId { get; set; }

    public DateTime DateOfEmployment { get; set; }
  }
}
