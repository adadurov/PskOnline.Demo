namespace PskOnline.Server.ObjectModel
{
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;

  using PskOnline.Server.Shared.ObjectModel;

  /// <summary>
  /// Branch office of a Tenant
  /// </summary>
  public class BranchOffice : TenantOwnedEntity, INamedEntity
  {
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the ID of the time zone
    /// that the branch office is located in.
    /// </summary>
    public string TimeZoneId { get; set; }

    /// <summary>
    /// Gets or sets the collection of child departments of the branch office
    /// </summary>
    public virtual ICollection<Department> Departments { get; set; }

    /// <summary>
    /// Gets or sets the collection of positions defined within the branch office
    /// </summary>
    public virtual ICollection<Position> Positions { get; set; }
  }
}
