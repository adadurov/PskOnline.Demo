namespace PskOnline.Server.Authority.Authorization
{
  using System;
  using System.Threading.Tasks;

  using Microsoft.AspNetCore.Authorization;
  using System.Security.Claims;

  using PskOnline.Server.Authority.ObjectModel;
  using PskOnline.Server.Shared.Permissions.Standard;
  using PskOnline.Server.Authority.API.Constants;

  public class ViewUserAuthorizationRequirement : IAuthorizationRequirement
  {
  }

  /// <summary>
  /// handles authorization requests with ApplicationUser as a parameter
  /// </summary>
  public class ViewUserAuthorizationHandler : 
    AuthorizationHandler<ViewUserAuthorizationRequirement, ApplicationUser>
  {
    protected override Task HandleRequirementAsync(
      AuthorizationHandlerContext context,
      ViewUserAuthorizationRequirement requirement,
      ApplicationUser targetUser)
    {
      if (context.User == null )
      {
        return Task.CompletedTask;
      }

      if (context.User.HasClaim(CustomClaimTypes.Permission, UserPermissions.Users_GLOBAL_View) ||
          context.User.HasClaim(CustomClaimTypes.Permission, UserPermissions.Users_Tenant_View) ||
          GetIsSameUser(context.User, targetUser.Id))
      {
        context.Succeed(requirement);
      }

      return Task.CompletedTask;
    }


    internal static bool GetIsSameUser(ClaimsPrincipal user, Guid targetUserId)
    {
      return user.GetUserId() == targetUserId.ToString();
    }
  }

  /// <summary>
  /// handles authorization requests with the ID (GUID) of the target user as a parameter
  /// this is more convinient in some places
  /// </summary>
  public class ViewUserAuthorizationHandlerWithGuid :
    AuthorizationHandler<ViewUserAuthorizationRequirement, Guid>
  {
    protected override Task HandleRequirementAsync(
      AuthorizationHandlerContext context,
      ViewUserAuthorizationRequirement requirement,
      Guid targetUserId)
    {
      if (context.User == null)
      {
        return Task.CompletedTask;
      }

      if (context.User.HasClaim(CustomClaimTypes.Permission, UserPermissions.Users_GLOBAL_View) ||
          context.User.HasClaim(CustomClaimTypes.Permission, UserPermissions.Users_Tenant_View) ||
          ViewUserAuthorizationHandler.GetIsSameUser(context.User, targetUserId))
      {
        context.Succeed(requirement);
      }

      return Task.CompletedTask;
    }
  }

}