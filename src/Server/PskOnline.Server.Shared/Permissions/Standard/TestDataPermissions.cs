﻿namespace PskOnline.Server.Shared.Permissions.Standard
{
  using System.Collections.Generic;
  using System.Collections.ObjectModel;

  using PskOnline.Server.Shared.Permissions;

  /// <summary>
  /// Defines permissions related to Test Data objects,
  /// including Inspection, InspectionTest, Summary & TestSummary
  /// </summary>
  public class TestDataPermissions
  {
    public const string TestDataPermissionsGroupName = "Test Data Permissions";
    public const string TestPermGroup = "testdata";

    public static ReadOnlyCollection<ApplicationPermission> AllPermissions;

    static TestDataPermissions()
    {
      AllPermissions = new List<ApplicationPermission>()
        {
          Test_Self_Submit,
          Test_Self_View,
          Test_Department_Submit,
          Test_Department_View,
          Test_Branch_View,
          Test_Tenant_Submit,
          Test_Tenant_View
        }.AsReadOnly();
    }

    public static ApplicationPermission Test_Self_Submit =>
        new ApplicationPermission(
            "Submit own test data",
            TestPermGroup, PermScope.Self, PermAction.Submit,
            TestDataPermissionsGroupName,
            "Permission to submit one's own test data");

    public static ApplicationPermission Test_Self_View =>
        new ApplicationPermission(
            "View own test data",
            TestPermGroup, PermScope.Self, PermAction.View,
            TestDataPermissionsGroupName,
            "Permission to view one's own test data");

    public static ApplicationPermission Test_Department_Submit =>
        new ApplicationPermission(
            "Submit department test data",
            TestPermGroup, PermScope.Department, PermAction.Submit,
            TestDataPermissionsGroupName,
            "Permission to submit test data for own department");

    public static ApplicationPermission Test_Department_View =>
        new ApplicationPermission(
            "View department test data",
            TestPermGroup, PermScope.Department, PermAction.View,
            TestDataPermissionsGroupName,
            "Permission to view test data for own department");

    public static ApplicationPermission Test_Branch_View =>
        new ApplicationPermission(
            "View branch office test data",
            TestPermGroup, PermScope.BranchOffice, PermAction.View,
            TestDataPermissionsGroupName,
            "Permission to view test data for own branch office");

    public static ApplicationPermission Test_Tenant_Submit =>
        new ApplicationPermission(
            "Submit test data for own tenant",
            TestPermGroup, PermScope.Tenant, PermAction.Submit,
            TestDataPermissionsGroupName,
            "Permission to submit test data for own tenant");

    public static ApplicationPermission Test_Tenant_View =>
        new ApplicationPermission(
            "View test data for own tenant",
            TestPermGroup, PermScope.Tenant, PermAction.View,
            TestDataPermissionsGroupName,
            "Permission to view test data for own tenant");

  }
}
