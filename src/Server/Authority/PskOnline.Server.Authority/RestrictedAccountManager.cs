namespace PskOnline.Server.Authority
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;

  using Microsoft.AspNetCore.Identity;

  using PskOnline.Server.Authority.Interfaces;
  using PskOnline.Server.Authority.EntityFramework;
  using PskOnline.Server.Authority.ObjectModel;
  using PskOnline.Server.Shared.Permissions;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Shared.Validators;
  using PskOnline.Server.Shared.Service;

  /// <summary>
  /// IAccountManager that implements security checks
  /// to be used at runtime by web server
  /// </summary>
  public class RestrictedAccountManager : UnrestrictedAccountManager, IRestrictedAccountManager
  {
    private readonly IAccessScopeFilter _accessScopeFilter;
    private readonly IAccessChecker _accessChecker;
    private readonly ITenantIdProvider _tenantIdProvider;

    public RestrictedAccountManager(
      AuthorityDbContext context,
      MultitenantUserManager<ApplicationUser> userManager,
      IRestrictedRoleService roleService,
      IUserOrgStructureReferencesValidator userReferencesValidator,
      ITenantIdProvider tenantIdProvider,
      ITenantEntityAccessChecker accessChecker,
      IAccessScopeFilter accessScopeFilter,
      IEmailService emailService,
      AutoMapperConfig autoMapperConfig)
       : base(context, userManager, roleService, userReferencesValidator, emailService, autoMapperConfig)
    {
      _accessScopeFilter = accessScopeFilter;
      _accessChecker = accessChecker;
      _tenantIdProvider = tenantIdProvider;
    }

    public override async Task<(bool success, string[] errors)> CreateUserAsync(ApplicationUser newUser, IEnumerable<Guid> roles, string password)
    {
      if (TenantSpec.BelongsToTenant(_tenantIdProvider.GetTenantId()))
      {
        // the call is from a 'Tenant User'...
        if (newUser.TenantId == TenantSpec.EntireSiteTenantId)
        {
          // ...and the new user model is not assigned a TenantId
          // 'Tenant User' can only create users in their own tenant
          newUser.TenantId = _tenantIdProvider.GetTenantId();
        }
        // Note that 'Site User' (see TenantSpec.IsSiteUser(...))
        // should be able to create new Users in any tenant
        // in order to create a 'tenant admin' user in a new tenant
      }

      return await base.CreateUserAsync(newUser, roles, password);
    }

    protected override async Task ValidateAccessToEntity(ApplicationUser user, EntityAction action)
    {
      await _accessChecker.ValidateAccessToEntityAsync(user, action);
    }

    protected override IQueryable<ApplicationUser> ApplyScopeFilter(IQueryable<ApplicationUser> usersQuery)
    {
      return _accessScopeFilter.AddScopeFilter(usersQuery);
    }
  }
}
