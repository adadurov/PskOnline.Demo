namespace PskOnline.Server.ObjectModel
{
  using System;
  using System.ComponentModel.DataAnnotations;

  public class Position : TenantOwnedEntity
  {
    [Required]
    public string Name { get; set; }

    public Guid? BranchOfficeId { get; set; }
  }
}
