namespace PskOnline.Service.Test.Integration.TestData
{
  using System;
  using System.Threading.Tasks;

  using PskOnline.Client.Api;
  using PskOnline.Client.Api.Organization;

  public class TestPosition
  {
    internal static async Task SeedDefaultPosition(IApiClient client, TenantContainer tenantInfo)
    {
      var defaultPosition = new PositionDto
      {
        Id = Guid.NewGuid().ToString(),
        Name = "Electrical Engineer",
      };

      await client.SignInWithUserPasswordAsync(tenantInfo.AdminName, tenantInfo.AdminPassword);
      var result = await client.PostPositionAsync(defaultPosition);
      tenantInfo.Position_Default = defaultPosition;
    }
  }
}
