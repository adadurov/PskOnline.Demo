namespace PskOnline.Service.Test.Integration.TestData
{
  using System;

  using PskOnline.Client.Api.Authority;
  using PskOnline.Client.Api.Organization;
  using PskOnline.Client.Api.Tenant;

  public class TenantContainer
  {
    public TenantContainer(TenantDto tenant)
    {
      AdminName = NormalizeName(tenant.Name) + "Admin";
      AdminPassword = "Qwerty1234%";
      TenantId = Guid.NewGuid();
      Tenant = tenant;
      Department_1_1 = new TestDepartmentHolder();
      Department_1_2 = new TestDepartmentHolder();
    }

    /// <summary>
    /// Quick and dirty name normalization -- skip any non-alphanumeric chars
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public string NormalizeName(string name)
    {
      if( string.IsNullOrEmpty(name) )
      {
        return name;
      }
      var normalized = string.Empty;
      for( int i = 0; i < name.Length; ++i )
      {
        if( IsValidChar(name[i]) )
        {
          normalized += name[i];
        }
      }
      return normalized;
    }

    private bool IsValidChar( char c )
    {
      return (c >= 'a' && c <= 'z') ||
             (c >= 'A' && c <= 'Z') ||
             (c >= '0' && c <= '9');
    }

    public string AdminName;

    public string AdminPassword;

    public UserEditDto AdminUser;

    public Guid TenantAdminRoleId;

    public RoleDto TenantAdminRole;

    public Guid TenantId;

    public TenantDto Tenant;

    public BranchOfficeDto BranchOffice_One;

    public TestDepartmentHolder Department_1_1;

    public TestDepartmentHolder Department_1_2;

    public PositionDto Position_Default;
  }
}
