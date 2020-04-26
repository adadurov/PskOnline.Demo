namespace PskOnline.Service.Test.Integration.TestData
{
  using System.Net.Http;
  using System.Threading.Tasks;

  using PskOnline.Client.Api;

  public static class ApiClientExtensions
  {
    public static async Task SignInWithUserPassword_WithSlug_Async(this IApiClient apiClient, string login, string password, HttpClient httpClient, string tenantSlug)
    {
      SimulateSlug(httpClient, tenantSlug);
      await apiClient.SignInWithUserPasswordAsync(login, password);
    }

    public static async Task AsSiteAdminAsync(this IApiClient apiClient, HttpClient httpClient)
    {
      SimulateSlug(httpClient, "");
      await apiClient.SignInWithUserPasswordAsync(
        TestUsers.DefaultSiteAdminName, TestUsers.DefaultSiteAdminPass);
    }

    public static async Task AsGryffindorAdminAsync(this IApiClient apiClient, HttpClient httpClient)
    {
      SimulateSlug(httpClient, TestTenants.GryffindorHouse.Slug);
      await apiClient.SignInWithUserPasswordAsync(
        TestUsers.Gryffindor_Admin_Username, TestUsers.Gryffindor_Admin_User_Pass);
    }

    public static async Task AsSlytherinAdminAsync(this IApiClient apiClient, HttpClient httpClient)
    {
      SimulateSlug(httpClient, TestTenants.SlytherinHouse.Slug);
      await apiClient.SignInWithUserPasswordAsync(
        TestUsers.Slytherin_Admin_Username, TestUsers.Slytherin_Admin_User_Pass);
    }

    public static void SimulateSlug(HttpClient httpClient, string slug)
    {
      httpClient.DefaultRequestHeaders.Remove("X-PSK-Slug");
      httpClient.DefaultRequestHeaders.Add("X-PSK-Slug", slug);
    }

  }
}
