namespace PskOnline.Server.Authority.Authorization
{
  using System;
  using System.Threading.Tasks;

  using Microsoft.AspNetCore.Authorization;
  using System.Security.Claims;

  using PskOnline.Server.Authority.ObjectModel;
  using PskOnline.Server.Shared.Permissions.Standard;
  using PskOnline.Server.Authority.API.Constants;

  public class ManageUserAuthorizationRequirement : IAuthorizationRequirement
  {
  }

  public class ManageUserAuthorizationHandler : AuthorizationHandler<ManageUserAuthorizationRequirement, ApplicationUser>
  {
    protected override Task HandleRequirementAsync(
      AuthorizationHandlerContext context, ManageUserAuthorizationRequirement requirement, ApplicationUser targetUser)
    {
      if (context.User == null)
      {
        return Task.CompletedTask;
      }

      var canManageGlobalUsers = 
        context.User.HasClaim(CustomClaimTypes.Permission, UserPermissions.Users_GLOBAL_Manage);

      var canManageUsersInTenant = 
        context.User.HasClaim(CustomClaimTypes.Permission, UserPermissions.Users_Tenant_Manage);

      if (canManageGlobalUsers || 
          canManageUsersInTenant ||
          GetIsSameUser(context.User, targetUser.Id))
      {
        context.Succeed(requirement);
      }

      return Task.CompletedTask;
    }

    private bool GetIsSameUser(ClaimsPrincipal user, Guid targetUserId)
    {
      return user.GetUserId() == targetUserId.ToString();
    }
  }
}
