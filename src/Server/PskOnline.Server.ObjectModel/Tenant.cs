namespace PskOnline.Server.ObjectModel
{
  using System;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

  using PskOnline.Server.Shared.ObjectModel;

  /// <summary>
  /// this class simulates ITenantEntity in the sense that
  /// it belongs to itself. This lets us use the existing
  /// service classes that already have implemented access
  /// to tenant entitites.
  /// </summary>
  public class Tenant : BaseEntity, ITenantEntity, INamedEntity
  {
    /// <summary>
    /// This is to simulate the behavior of a tenant entity,
    /// so that we can reuse the existing service implementations
    /// </summary>
    [NotMapped]
    public Guid TenantId
    {
      get => Id;
      set { }
    }

    [Required]
    public string Name { get; set; }

    public string Slug { get; set; }

    public string Comment { get; set; }

    /// <summary>
    /// Gets or sets the information about the primary contact person
    /// </summary>
    [Required(ErrorMessage = "Tenant must have contact information defined")]
    public ContactInfo PrimaryContact { get; set; } = new ContactInfo();

    /// <summary>
    /// Gets or sets the information about the alternate contact person
    /// When you need the 'alternate' contact, check this out
    /// https://msdn.microsoft.com/en-us/magazine/mt846463.aspx
    /// </summary>
//    public ContactInfo AlternateContact { get; set; }

    /// <summary>
    /// Gets or sets the value object describing the service
    /// that the tenant is entitled to
    /// </summary>
    public TenantServiceDetails ServiceDetails { get; set; } = new TenantServiceDetails();
  }
}
