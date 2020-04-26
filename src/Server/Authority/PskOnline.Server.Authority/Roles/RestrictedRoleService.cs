namespace PskOnline.Server.Authority.Roles
{
  using System.Linq;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.Extensions.Logging;

  using PskOnline.Server.Authority.Interfaces;
  using PskOnline.Server.Authority.ObjectModel;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Shared.Permissions;

  public class RestrictedRoleService : UnrestrictedRoleService, IRestrictedRoleService
  {
    private readonly ITenantEntityAccessChecker _tenantAccessChecker;
    private readonly IAccessScopeFilter _tenantContext;

    public RestrictedRoleService(
      RoleManager<ApplicationRole> roleManager,
      MultitenantUserManager<ApplicationUser> userManager,
      IClaimsService claimsService,
      IUserRoleRepository userRoleStore,
      ITenantEntityAccessChecker tenantAccessChecker,
      IAccessScopeFilter tenantContext,
      ILogger<RestrictedRoleService> logger)
      : base(roleManager, userManager, claimsService, userRoleStore, logger)
    {
      _tenantAccessChecker = tenantAccessChecker;
      _tenantContext = tenantContext;
    }

    protected override void CheckAccess(ApplicationRole role, EntityAction desiredAction)
    {
      _tenantAccessChecker.ValidateAccessToEntityAsync(role, desiredAction);
    }

    protected override IQueryable<ApplicationRole> AddScopeFilter(IQueryable<ApplicationRole> query)
    {
      return _tenantContext.AddScopeFilter(query);
    }

  }
}
