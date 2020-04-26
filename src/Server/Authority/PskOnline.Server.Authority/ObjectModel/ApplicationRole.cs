namespace PskOnline.Server.Authority.ObjectModel
{
  using System;
  using System.Collections.Generic;
  using Microsoft.AspNetCore.Identity;

  using PskOnline.Server.Shared.ObjectModel;

  public class ApplicationRole : IdentityRole<Guid>, ITenantEntity, IGuidIdentity
  {
    /// <summary>
    /// Initializes a new instance of <see cref="ApplicationRole"/>.
    /// </summary>
    /// <remarks>
    /// The Id property is initialized to from a new GUID string value.
    /// </remarks>
    public ApplicationRole() { }

    /// <summary>
    /// Initializes a new instance of <see cref="ApplicationRole"/>.
    /// </summary>
    /// <param name="roleName">The role name.</param>
    /// <remarks>
    /// The Id property is initialized to from a new GUID string value.
    /// </remarks>
    public ApplicationRole(string roleName)
      : base(roleName)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ApplicationRole"/>.
    /// </summary>
    /// <param name="roleName">The role name.</param>
    /// <param name="description">Description of the role.</param>
    /// <remarks>
    /// The Id property is initialized to from a new GUID string value.
    /// </remarks>
    public ApplicationRole(string roleName, string description)
      : base(roleName)
    {
      Description = description;
    }

    /// <summary>
    /// Gets or sets the ID of the tenant that this role belongs to
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the description for this role.
    /// </summary>
    public string Description { get; set; }

    public string CreatedBy { get; set; }

    public string UpdatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    /// <summary>
    /// Navigation property for the users in this role.
    /// </summary>
    public virtual ICollection<IdentityUserRole<Guid>> Users { get; set; }

    /// <summary>
    /// Navigation property for claims in this role.
    /// </summary>
    public virtual ICollection<IdentityRoleClaim<Guid>> Claims { get; set; }
  }
}
