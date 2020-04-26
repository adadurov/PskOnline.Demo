namespace PskOnline.Server.Plugins.RusHydro.Web.Logic
{
  using PskOnline.Server.Shared.Permissions;

  public static class PsaSummaryPermissions
  {
    public const string PsaSummaryPermissionsGroupName = "Psa Summary Permissions";

    public const string PsaSummaryPermGroup = "psasummary";

    public static IApplicationPermission[] AllPermissions()
    {
      return new[] { new ApplicationPermission(
            "View PSA summary for department",
            PsaSummaryPermGroup, PermScope.Department, PermAction.View,
            PsaSummaryPermissionsGroupName,
            "View PSA summary for department that the account belongs to") };
    }
  }
}
