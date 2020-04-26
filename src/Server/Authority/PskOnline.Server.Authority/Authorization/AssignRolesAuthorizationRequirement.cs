namespace PskOnline.Server.Authority.Authorization
{
  using System;
  using System.Linq;
  using System.Security.Claims;
  using System.Threading.Tasks;

  using Microsoft.AspNetCore.Authorization;

  using PskOnline.Server.Authority.API.Constants;
  using PskOnline.Server.Shared.Permissions.Standard;

  public class AssignRolesAuthorizationRequirement : IAuthorizationRequirement
  {

  }

  public class UserRoleChange
  {
    public Guid[] NewRoles { get; set; }

    public Guid[] CurrentRoles { get; set; }
  }

  public class AssignRolesAuthorizationHandler : 
    AuthorizationHandler<AssignRolesAuthorizationRequirement, UserRoleChange>
  {
    protected override Task HandleRequirementAsync(
      AuthorizationHandlerContext context, 
      AssignRolesAuthorizationRequirement requirement,
      UserRoleChange newAndCurrentRoles)
    {
      if (!GetIsRolesChanged(newAndCurrentRoles.NewRoles, newAndCurrentRoles.CurrentRoles))
      {
        context.Succeed(requirement);
        return Task.CompletedTask;
      }

      if (context.User.HasClaim(CustomClaimTypes.Permission, RolePermissions.Roles_GLOBAL_Assign) ||
          context.User.HasClaim(CustomClaimTypes.Permission, RolePermissions.Roles_Tenant_Assign))
      {
        context.Succeed(requirement);
      }
      return Task.CompletedTask;
    }

    private bool GetIsRolesChanged(Guid[] newRoles, Guid[] currentRoles)
    {
      if (newRoles == null)
        newRoles = new Guid[] { };

      if (currentRoles == null)
        currentRoles = new Guid[] { };

      bool roleAdded = newRoles.Except(currentRoles).Any();
      bool roleRemoved = currentRoles.Except(newRoles).Any();

      return roleAdded || roleRemoved;
    }

    private bool GetIsUserInAllAddedRoles(ClaimsPrincipal contextUser, Guid[] newRoles, Guid[] currentRoles)
    {
      if (newRoles == null)
        newRoles = new Guid[] { };

      if (currentRoles == null)
        currentRoles = new Guid[] { };


      var addedRoles = newRoles.Except(currentRoles);

      return addedRoles.All(role => contextUser.IsInRole(role.ToString()));
    }
  }
}