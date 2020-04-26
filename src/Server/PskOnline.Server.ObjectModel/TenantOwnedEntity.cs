namespace PskOnline.Server.ObjectModel
{
  using System;
  using PskOnline.Server.Shared.ObjectModel;

  /// <summary>
  /// base class for entities belonging to tenants
  /// </summary>
  public class TenantOwnedEntity : BaseEntity, ITenantEntity
  {
    /// <summary>
    /// Gets or sets the ID of the tenant that the entity belongs to.
    /// </summary>
    public Guid TenantId { get; set; }
  }
}
