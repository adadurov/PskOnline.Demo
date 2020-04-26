namespace PskOnline.Server.Authority
{
  using System;
  using System.Linq;
  using System.Threading.Tasks;

  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Logging;

  using PskOnline.Server.Authority.Roles;
  using PskOnline.Server.Authority.ObjectModel;
  using PskOnline.Server.Authority.Interfaces;
  using PskOnline.Server.Authority.EntityFramework;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Shared.Roles;
  using PskOnline.Server.Shared.EFCore;

  public class AuthorityDbInitializer : IDatabaseInitializer
  {
    private readonly AuthorityDbContext _context;
    private readonly IAccountManager _accountManager;
    private readonly IRoleService _roleService;
    private readonly ILogger _logger;

    public AuthorityDbInitializer(
      AuthorityDbContext context,
      IAccountManager accountManager,
      IRoleService roleService,
      ILogger<AuthorityDbInitializer> logger)
    {
      _accountManager = accountManager;
      _roleService = roleService;
      _context = context;
      _logger = logger;
    }

    public async Task SeedAsync()
    {
      _context.MigrateToCurrentVersion();

      if (!await _context.Users.AnyAsync())
      {
        _logger.LogInformation("Generating built-in accounts");

        var superAdminRoleId = DefaultSiteRoles.GetBuiltInAdminRoleDefinition().Id;

        var siteRoles = DefaultSiteRoles.GetStandardSiteRoles();
        foreach( var roleDef in siteRoles )
        {
          await EnsureRoleAsync(roleDef, TenantSpec.EntireSiteTenantId);
        }

        await CreateUserAsync(
          "admin", "tempP@ss123",
          "John A.", "Doe", 
          "admin@psk-online.ru", 
          "+1 (123) 000-0000", 
          new Guid[] { superAdminRoleId });

        _logger.LogInformation("Built-in admin account generation completed");
      }
      await _context.SaveChangesAsync();
    }

    private async Task<Guid> EnsureRoleAsync(RoleSpecification roleDef, Guid tenantId)
    {
      var existingRole = await _roleService.GetRoleInTenantByNameAsync(roleDef.Name, tenantId);
      if( existingRole == null)
      {
        var permValues = (from p in roleDef.Permissions select p.Value).ToArray();
        var applicationRole = new ApplicationRole(roleDef.Name, roleDef.Description)
        {
          Id = roleDef.Id,
          TenantId = tenantId,
          CreatedBy = nameof(AuthorityDbInitializer),
          UpdatedBy = nameof(AuthorityDbInitializer)
        };

        await _roleService.CreateRoleAsync(applicationRole, permValues);

        return applicationRole.Id;
      }
      else
      {
        if (existingRole.Id != roleDef.Id )
        {
          throw new Exception("Logic exception, the existing role should have the id specified in the RoleDef!");
        }
        return existingRole.Id;
      }
    }

    private async Task<ApplicationUser> CreateUserAsync(
      string userName, string password,
      string firstName, string lastName, string email, string phoneNumber, Guid[] roleIds)
    {
      ApplicationUser applicationUser = new ApplicationUser
      {
        UserName = userName,
        FirstName = firstName,
        LastName = lastName,
        Email = email,
        PhoneNumber = phoneNumber,
        EmailConfirmed = true,
        IsEnabled = true
      };

      var result = await _accountManager.CreateUserAsync(applicationUser, roleIds, password);

      if (!result.Item1)
        throw new Exception($"Seeding \"{userName}\" user failed. Errors: {string.Join(Environment.NewLine, result.Item2)}");

      return applicationUser;
    }
  }
}
