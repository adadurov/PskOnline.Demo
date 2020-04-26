namespace PskOnline.Server.ObjectModel
{
  using System;
  using System.ComponentModel.DataAnnotations.Schema;

  [ComplexType]
  public class TenantServiceDetails
  {
    public DateTime ServiceExpireDate { get; set; }

    public int ServiceMaxUsers { get; set; }

    public int ServiceMaxEmployees { get; set; }

    public int ServiceMaxStorageMegabytes { get; set; }
  }
}
