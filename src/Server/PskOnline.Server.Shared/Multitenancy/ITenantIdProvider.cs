namespace PskOnline.Server.Shared.Multitenancy
{
  using System;

  public interface ITenantIdProvider
  {
    /// <summary>
    /// this call may throw exception if the user
    /// doesn't have a valid TenantId claim
    /// </summary>
    /// <returns></returns>
    Guid GetTenantId();
  }
}
