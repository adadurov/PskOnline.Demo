namespace PskOnline.Server.Authority.ObjectModel
{
  using System;

  using OpenIddict.EntityFrameworkCore.Models;

  public class PskApplication : OpenIddictApplication<string, PskAuthorization, PskToken>
  {
    public string TenantId { get; set; }

    public string DepartmentId { get; set; }

    public string BranchOfficeId { get; set; }

    public string ApplicationType { get; set; }
  }

  public class PskAuthorization : OpenIddictAuthorization<string, PskApplication, PskToken>
  {
  }

  public class PskToken : OpenIddictToken<string, PskApplication, PskAuthorization>
  {
  }

  public class PskScope : OpenIddictScope<string>
  {
  }
}
