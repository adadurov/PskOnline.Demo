namespace PskOnline.Service.Test.Integration.TestData
{
  using PskOnline.Client.Api.Authority;

  public static class TestUsers
  {
    public static string DefaultSiteAdminName => "admin";

    public static string DefaultSiteAdminPass => "tempP@ss123";

    public static string Gryffindor_Admin_User_Pass
    {
      get => new TenantContainer(TestTenants.GryffindorHouse).AdminPassword;
    }

    public static string Gryffindor_Admin_Username
    {
      get => new TenantContainer(TestTenants.GryffindorHouse).AdminName;
    }

    public static string Slytherin_Admin_User_Pass
    {
      get => new TenantContainer(TestTenants.SlytherinHouse).AdminPassword;
    }

    public static string Slytherin_Admin_Username
    {
      get => new TenantContainer(TestTenants.SlytherinHouse).AdminName;
    }

  }
}
