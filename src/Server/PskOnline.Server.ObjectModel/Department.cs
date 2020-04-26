namespace PskOnline.Server.ObjectModel
{
  using System;
  using System.ComponentModel.DataAnnotations;

  using PskOnline.Server.Shared.ObjectModel;

  /// <summary>
  /// Этот класс представляет собой подразделение.
  /// </summary>
  public class Department : TenantOwnedEntity, INamedEntity
  {
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the branch office
    /// that the department belongs to
    /// </summary>
    public Guid BranchOfficeId { get; set; }
  }
}
