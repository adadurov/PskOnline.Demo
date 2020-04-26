
export type PermissionNames =
  "View GLOBAL Users" | "Manage GLOBAL Users" |
  "View Customer Users" | "Manage Customer Users" |
    "View GLOBAL Roles" | "Manage GLOBAL Roles" | "Assign GLOBAL Roles" |
    "View Customer Roles" | "Manage Customer Roles" | "Assign Customer Roles" |
    "View Tenants" | "Manage Tenants" |
    "View Org Structure" | "Manage Org Structure";

export type PermissionValues =
    "users.GLOBAL.View" | "users.GLOBAL.Manage" |
    "users.Tenant.View" | "users.Tenant.Manage" |
    "roles.GLOBAL.View" | "roles.GLOBAL.Manage" | "roles.GLOBAL.AssignRole" |
    "roles.Tenant.View" | "roles.Tenant.Manage" | "roles.Tenant.AssignRole" |
    "tenants.GLOBAL.View" | "tenants.GLOBAL.Manage" |
    "org_struct.Tenant.View" | "org_struct.Tenant.Manage" ;

export class Permission {

    public static readonly viewGlobalUsersPermission: PermissionValues = "users.GLOBAL.View";
    public static readonly manageGlobalUsersPermission: PermissionValues = "users.GLOBAL.Manage";

    public static readonly viewTenantUsersPermission: PermissionValues = "users.Tenant.View";
    public static readonly manageTenantUsersPermission: PermissionValues = "users.Tenant.Manage";

    public static readonly viewTenantsPermission: PermissionValues = "tenants.GLOBAL.View";
    public static readonly manageTenantsPermission: PermissionValues = "tenants.GLOBAL.Manage";

    public static readonly viewTenantOrgStructurePermission: PermissionValues = "org_struct.Tenant.View";
    public static readonly manageTenantOrgStructurePermission: PermissionValues = "org_struct.Tenant.Manage";

    public static readonly viewGlobalRolesPermission: PermissionValues =   "roles.GLOBAL.View";
    public static readonly assignGlobalRolesPermission: PermissionValues = "roles.GLOBAL.AssignRole";
    public static readonly manageGlobalRolesPermission: PermissionValues = "roles.GLOBAL.Manage";

    public static readonly viewTenantRolesPermission: PermissionValues = "roles.Tenant.View";
    public static readonly assignTenantRolesPermission: PermissionValues = "roles.Tenant.AssignRole";
    public static readonly manageTenantRolesPermission: PermissionValues = "roles.Tenant.Manage";

    constructor(name?: PermissionNames, value?: PermissionValues, groupName?: string, description?: string) {
        this.name = name;
        this.value = value;
        this.groupName = groupName;
        this.description = description;
    }

    public name: PermissionNames;
    public value: PermissionValues;
    public groupName: string;
    public description: string;
}