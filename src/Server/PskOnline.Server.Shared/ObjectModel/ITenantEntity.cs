namespace PskOnline.Server.Shared.ObjectModel
{
  using System;

  public interface ITenantEntity
  {
    /// <summary>
    /// Gets or sets the ID of the tenant that the entity belongs to.
    /// </summary>
    Guid TenantId { get; set; }
  }
}
