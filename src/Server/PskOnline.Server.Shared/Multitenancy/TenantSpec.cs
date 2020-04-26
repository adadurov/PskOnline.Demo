namespace PskOnline.Server.Shared.Multitenancy
{
  using System;

  public static class TenantSpec
  {
    public static bool BelongsToSite(Guid? guid)
    {
      return guid == null || !guid.HasValue || guid.Value == EntireSiteTenantId;
    }

    public static bool BelongsToTenant(Guid? guid)
    {
      return ! BelongsToSite(guid);
    }

    public static Guid EntireSiteTenantId => Guid.Empty;
  }
}
