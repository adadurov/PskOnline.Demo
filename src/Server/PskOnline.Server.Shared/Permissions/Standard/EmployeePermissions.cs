namespace PskOnline.Server.Shared.Permissions.Standard
{
  using System.Collections.Generic;
  using System.Collections.ObjectModel;

  using PskOnline.Server.Shared.Permissions;

  /// <summary>
  /// defines permissions to view / manage organizational structure
  /// </summary>
  public class EmployeePermissions
  {
    public const string EmployeePermissionsGroupName = "Employee Permissions";

    public const string EmployeeGroupPrefix = "employee";

    public static ReadOnlyCollection<ApplicationPermission> AllPermissions;

    static EmployeePermissions()
    {
      AllPermissions = new List<ApplicationPermission>
      {
        Employee_Department_View,
      }.AsReadOnly();
    }

    public static ApplicationPermission Employee_Department_View =>
        new ApplicationPermission(
            "View employees in own department",
            EmployeeGroupPrefix, PermScope.Department, PermAction.View,
            EmployeePermissionsGroupName,
            "Permission to view all employees in own department");

  }
}
