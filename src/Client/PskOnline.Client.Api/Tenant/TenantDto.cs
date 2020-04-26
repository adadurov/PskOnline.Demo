namespace PskOnline.Client.Api.Tenant
{
  using System;

  public class TenantDto
  {
    public string Id { get; set; }

    public string Name { get; set; }

    public string Slug { get; set; }

    public string Comment { get; set; }

    public ContactInfoDto PrimaryContact { get; set; }

    public ServiceDetailsDto ServiceDetails { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime DateModified { get; set; }
  }
}
