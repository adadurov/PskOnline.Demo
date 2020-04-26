namespace PskOnline.Server.Authority
{
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.Extensions.DependencyInjection;
  using PskOnline.Server.Authority.Authorization;
  using PskOnline.Server.Authority.API.Constants;
  using PskOnline.Server.Shared.Permissions.Standard;

  public static class AuthorizationExtensions
  {
    public static void AddPskOnlineAuthorization(this IServiceCollection services)
    {
      services.AddAuthorization(options =>
      {
        options.AddPolicy(Policies.ManageAllUsersPolicy,
          policy => policy.RequireAssertion(
            context => context.User.HasClaim(CustomClaimTypes.Permission, UserPermissions.Users_GLOBAL_Manage) ||
                       context.User.HasClaim(CustomClaimTypes.Permission, UserPermissions.Users_Tenant_Manage)));

        options.AddPolicy(Policies.ViewAllUsersPolicy,
          policy => policy.RequireAssertion(
            context => context.User.HasClaim(CustomClaimTypes.Permission, UserPermissions.Users_GLOBAL_View) ||
                       context.User.HasClaim(CustomClaimTypes.Permission, UserPermissions.Users_Tenant_View)));

        options.AddPolicy(Policies.ViewAllRolesPolicy,
          policy => policy.RequireAssertion(
            context => context.User.HasClaim(CustomClaimTypes.Permission, RolePermissions.Roles_GLOBAL_View) ||
                       context.User.HasClaim(CustomClaimTypes.Permission, RolePermissions.Roles_Tenant_View)));

        options.AddPolicy(Policies.ViewRoleByIdPolicy,
          policy => policy.AddRequirements(new ViewRoleAuthorizationRequirement()));

        options.AddPolicy(Policies.ManageAllRolesPolicy,
          policy => policy.RequireAssertion(
            context => context.User.HasClaim(CustomClaimTypes.Permission, RolePermissions.Roles_GLOBAL_Manage) ||
                       context.User.HasClaim(CustomClaimTypes.Permission, RolePermissions.Roles_Tenant_Manage)));

        options.AddPolicy(Policies.AssignAllowedRolesPolicy,
            policy => policy.Requirements.Add(new AssignRolesAuthorizationRequirement()));

      });

      // Authorization Requirement Handlers
      services.AddSingleton<IAuthorizationHandler, ViewUserAuthorizationHandler>();
      services.AddSingleton<IAuthorizationHandler, ViewUserAuthorizationHandlerWithGuid>();

      services.AddSingleton<IAuthorizationHandler, ManageUserAuthorizationHandler>();

      services.AddSingleton<IAuthorizationHandler, ViewRoleAuthorizationHandler>();

      services.AddSingleton<IAuthorizationHandler, AssignRolesAuthorizationHandler>();
    }
  }
}
