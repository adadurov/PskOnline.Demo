namespace PskOnline.Server.Authority.ObjectModel
{
  using System;
  using System.Collections.Generic;
  using Microsoft.AspNetCore.Identity;

  using PskOnline.Server.Shared.ObjectModel;
  
  public class ApplicationUser : IdentityUser<Guid>, IAuditableEntity, ITenantEntity
  {
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Patronymic { get; set; }

    public string FullName => string.Join(" ", LastName, FirstName, Patronymic);

    /// <summary>
    /// Gets or sets the ID of the tenant that the user belongs to
    /// </summary>
    public Guid TenantId { get; set; }

    public Guid? EmployeeId { get; set; }

    /// <summary>
    /// Gets of sets the ID of the branch office within the tenant,
    /// that the user belongs to
    /// </summary>
    /// <remarks>org strcture attributes are combined with permissions
    /// to define the items accessible to the user</remarks>
    public Guid? BranchOfficeId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the department that the user belongs to
    /// </summary>
    /// <remarks>org strcture attributes are combined with permissions
    /// to define the items accessible to the user</remarks>
    public Guid? DepartmentId { get; set; }

    /// <summary>
    /// Gets or sets the flag indicating whether the user
    /// is a special department-related user.
    /// </summary>
    public bool IsDepartmentSpecialUser { get; set; }

    /// <summary>
    /// Gets or sets the ID of the position (within the tenant) that the user belongs to
    /// </summary>
    /// <remarks>org strcture attributes are combined with permissions
    /// to define the items accessible to the user</remarks>
    public Guid? PositionId { get; set; }

    public string WebUiPreferences { get; set; }

    public bool IsEnabled { get; set; }

    public bool IsLockedOut => LockoutEnabled && LockoutEnd >= DateTimeOffset.UtcNow;

    public string CreatedBy { get; set; }

    public string UpdatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    /// <summary>
    /// Navigation property for the roles this user belongs to.
    /// </summary>
    public virtual ICollection<IdentityUserRole<Guid>> Roles { get; set; }

    /// <summary>
    /// Navigation property for the claims this user possesses.
    /// </summary>
    public virtual ICollection<IdentityUserClaim<Guid>> Claims { get; set; }
  }
}
