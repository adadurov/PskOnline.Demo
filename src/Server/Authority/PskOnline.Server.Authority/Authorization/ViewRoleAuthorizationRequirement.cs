namespace PskOnline.Server.Authority.Authorization
{
  using System;
  using System.Threading.Tasks;

  using Microsoft.AspNetCore.Authorization;
  using PskOnline.Server.Authority.API.Constants;
  using PskOnline.Server.Shared.Permissions.Standard;

  public class ViewRoleAuthorizationRequirement : IAuthorizationRequirement
  {

  }

  public class ViewRoleAuthorizationHandler : AuthorizationHandler<ViewRoleAuthorizationRequirement, Guid>
  {
    protected override Task HandleRequirementAsync(
      AuthorizationHandlerContext context, 
      ViewRoleAuthorizationRequirement requirement, 
      Guid roleId)
    {
      if (context.User == null)
      {
        return Task.CompletedTask;
      }
      if (context.User.HasClaim(CustomClaimTypes.Permission, RolePermissions.Roles_GLOBAL_View) ||
          context.User.HasClaim(CustomClaimTypes.Permission, RolePermissions.Roles_Tenant_View) || 
          context.User.IsInRole(roleId.ToString()))
      {
        context.Succeed(requirement);
      }
      return Task.CompletedTask;
    }
  }
}
