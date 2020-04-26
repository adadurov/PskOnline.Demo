namespace PskOnline.Server.DAL.Multitenancy
{
  using System;
  using System.Threading.Tasks;
  using Microsoft.Extensions.Logging;

  using PskOnline.Server.Shared.Exceptions;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Shared.Service;
  using PskOnline.Server.Authority.API.Dto;
  using PskOnline.Server.Authority.API;

  public class TenantCreator : ITenantCreator
  {
    private readonly IService<Tenant> _tenantService;
    private readonly IRoleService _roleService;
    private readonly IAccountService _accountManager;
    private readonly ILogger<TenantCreator> _logger;
    private readonly TenantRolesCreator _tenantRolesCreator;

    public TenantCreator(
      IService<Tenant> tenantService,
      IRoleService roleService,
      IAccountService accountManager,
      ILogger<TenantCreator> logger
      )
    {
      _accountManager = accountManager;
      _roleService = roleService;
      _tenantService = tenantService;
      _logger = logger;
      _tenantRolesCreator = new TenantRolesCreator(_roleService);
    }

    public async Task<TenantCreationResult> CreateNewTenant(
      Tenant newTenant,
      UserDto newTenantAdmin,
      string password)
    {
      var newTenantId = await _tenantService.AddAsync(newTenant);
      newTenantAdmin.TenantId = newTenantId.ToString();

      try
      {
        // remember tenant admin role id
        // this information is used to automatically create group (department) accounts
        var tenantAdminRoleId = await _tenantRolesCreator.CreateDefaultRolesForTenantAsync(newTenant.Id);

        var tenantAdminRole = await _roleService.GetRoleByIdAsync(tenantAdminRoleId);
        await CreateUser(newTenantAdmin, password, tenantAdminRoleId);
        await _tenantService.UpdateAsync(newTenant);
        return new TenantCreationResult
        {
          TenantId = newTenantId,
          AdminRole = tenantAdminRole
        };
      }
      catch ( Exception ex )
      {
        _logger.LogError(ex, "Error creating default roles and admin user for new tenant.");
        // roll back the partial changes
        // remove everything related to the tenant ID
        await _roleService.DeleteAllTenantRoles(newTenantId);
        await _tenantService.RemoveAsync(newTenantId);
        throw;
      }
    }

    private async Task CreateUser(UserDto newUser, string password, Guid tenantAdminRoleId)
    {
//      newUser.EmailConfirmed = true;
      newUser.IsEnabled = true;

      var resultTuple = await _accountManager.CreateUserAsync(
        newUser, new[] { tenantAdminRoleId }, password);

      if( ! resultTuple.Item1 )
      {
        throw new BadRequestException(
          string.Join(Environment.NewLine, resultTuple.Item2));
      }
    }
  }
}
