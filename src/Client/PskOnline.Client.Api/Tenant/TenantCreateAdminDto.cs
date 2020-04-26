namespace PskOnline.Client.Api.Tenant
{
  public class TenantCreateAdminDto
  {
    public string Email { get; set; }

    public string UserName { get; set; }

    public string NewPassword { get; set; }

    public string FirstName { get; set; }

    public string Patronymic { get; set; }

    public string LastName { get; set; }

    public string PhoneNumber { get; set; }
  }
}
