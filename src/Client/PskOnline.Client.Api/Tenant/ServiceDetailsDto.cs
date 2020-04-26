namespace PskOnline.Client.Api.Tenant
{
  using System;

  public class ServiceDetailsDto
  {
    public DateTime ServiceExpireDate { get; set; }

    public int ServiceMaxEmployees { get; set; }

    public int ServiceMaxUsers { get; set; }

    public int ServiceMaxStorageMegabytes { get; set; }
  }
}
