namespace PskOnline.Client.Api.Tenant
{
  public class TenantCreateDto
  {
    public TenantDto TenantDetails { get; set; }

    public TenantCreateAdminDto AdminUserDetails { get; set; }
  }
}
