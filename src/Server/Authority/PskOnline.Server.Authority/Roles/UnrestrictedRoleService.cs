namespace PskOnline.Server.Authority.Roles
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Security.Claims;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Logging;

  using PskOnline.Server.Authority.Interfaces;
  using PskOnline.Server.Authority.ObjectModel;
  using PskOnline.Server.Shared;
  using PskOnline.Server.Shared.Exceptions;
  using PskOnline.Server.Shared.Permissions;
  using PskOnline.Server.Shared.Roles;
  using PskOnline.Server.Authority.API.Dto;

  /// <summary>
  /// This class implements IRoleService functionality with security restrictions
  /// To be used for database seeding. At runtime, use <see cref="RestrictedAccountManager"/>
  /// </summary>
  public class UnrestrictedRoleService : IRoleService
  {
    static UnrestrictedRoleService()
    {
      var mapperConfig = new AutoMapper.MapperConfiguration( cfg =>
      {
        cfg.CreateMap<ApplicationRole, RoleDto>();
      });
      Mapper = mapperConfig.CreateMapper();
    }

    private static readonly AutoMapper.IMapper Mapper;

    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly MultitenantUserManager<ApplicationUser> _userManager;
    private readonly ILogger<UnrestrictedRoleService> _logger;
    private readonly IUserRoleRepository _userRoleStore;
    private readonly IClaimsService _claimsService;

    public UnrestrictedRoleService(
      RoleManager<ApplicationRole> roleManager,
      MultitenantUserManager<ApplicationUser> userManager,
      IClaimsService claimsService,
      IUserRoleRepository userRoleStore,
      ILogger<UnrestrictedRoleService> logger)
    {
      _userRoleStore = userRoleStore;
      _roleManager = roleManager;
      _claimsService = claimsService;
      _userManager = userManager;
      _logger = logger;
    }

    protected virtual void CheckAccess(ApplicationRole role, EntityAction desiredAction)
    {
    }

    protected virtual IQueryable<ApplicationRole> AddScopeFilter(IQueryable<ApplicationRole> query)
    {
      return query;
    }

    public async Task<Guid> CreateRoleAsync(RoleDto roleDef, Guid tenantId)
    {
      var permValues = roleDef.Permissions.Select( p => p.Value );
      var applicationRole = new ApplicationRole
      {
        TenantId = tenantId,
        Id = Guid.Parse(roleDef.Id),
        Name = roleDef.Name,
        Description = roleDef.Description
      };

      return await CreateRoleAsync(applicationRole, permValues);
    }

    public async Task<Guid> CreateRoleAsync(ApplicationRole role, IEnumerable<string> claims)
    {
      if (claims == null)
      {
        claims = new string[] { };
      }

      ValidateClaims(claims);

      CheckAccess(role, EntityAction.Create);

      var result = await _roleManager.CreateAsync(role);
      if (!result.Succeeded)
      {
        var errors = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));
        throw new BadRequestException(
          $"Failed to create role \"{role.Name}\". Errors: {errors}");
      }

      // WTF?
      role = await _roleManager.FindByIdAsync(role.Id.ToString());
      if (role == null)
      {
        throw new ItemNotFoundException(role.Id.ToString(), "role");
      }

      foreach (string claim in claims.Distinct())
      {
        var permission = _claimsService.GetPermissionByValue(claim);
        result = await _roleManager.AddClaimAsync(role, 
          new Claim(CustomClaimTypes.Permission, permission.ToString()));

        if (!result.Succeeded)
        {
          await DeleteRoleAsync(role);

          var errors = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));
          throw new BadRequestException(
            $"Failed to add claims to role \"{role.Name}\". Errors: {errors}");
        }
      }

      return role.Id;
    }

    /// <summary>
    /// looks up a role in the specified tenant by the specified roleName
    /// </summary>
    /// <param name="roleName"></param>
    /// <param name="tenantId"></param>
    /// <remarks>the roleName is normalized using the underlying RoleManager</remarks>
    /// <returns></returns>
    public async Task<ApplicationRole> GetRoleInTenantByNameAsync(string roleName, Guid tenantId)
    {
      var normalizedName = _roleManager.NormalizeKey(roleName);

      var role = await _roleManager.Roles
          .Include(r => r.Claims)
          .Include(r => r.Users)
          .Where(r => r.TenantId == tenantId && r.NormalizedName == normalizedName)
          .FirstOrDefaultAsync();
      CheckAccess(role, EntityAction.Read);
      return role;
    }

    public async Task DeleteRoleAsync(ApplicationRole role)
    {
      if (role != null)
      {
        CheckAccess(role, EntityAction.Delete);

        if (!await TestCanDeleteRoleAsync(role.Id))
        {
          throw new BadRequestException(
            "Role " + role.Id.ToString() + " cannot be deleted. Remove all users from this role and try again.");
        }

        await _roleManager.DeleteAsync(role);
      }
    }

    public async Task DeleteRoleByIdAsync(Guid roleId)
    {
      var role = await _roleManager.FindByIdAsync(roleId.ToString());
      if (role != null)
      {
        await DeleteRoleAsync(role);
      }
    }

    public async Task<RoleDto> GetRoleByIdAsync(Guid id)
    {
      var entity = await _roleManager.FindByIdAsync(id.ToString());
      if (entity != null)
      {
        CheckAccess(entity, EntityAction.Read);
        return Mapper.Map<RoleDto>(entity);
      }
      throw new ItemNotFoundException(id.ToString(), nameof(ApplicationRole));
    }

    public async Task<ApplicationRole> GetRoleByIdInternalAsync(Guid id)
    {
      var entity = await _roleManager.FindByIdAsync(id.ToString());
      if (entity != null)
      {
        CheckAccess(entity, EntityAction.Read);
        return entity;
      }
      throw new ItemNotFoundException(id.ToString(), nameof(ApplicationRole));
    }

    public async Task<ApplicationRole> GetRoleByIdLoadRelatedAsync(Guid roleId)
    {
      var role = await _roleManager.Roles
          .Include(r => r.Claims)
          .Include(r => r.Users)
          .Where(r => r.Id == roleId)
          .FirstOrDefaultAsync();
      if (role != null)
      {
        CheckAccess(role, EntityAction.Read);
        return role;
      }
      throw new ItemNotFoundException(roleId.ToString(), nameof(ApplicationRole));
    }

    public async Task<List<ApplicationRole>> GetRolesLoadRelatedAsync(int page, int pageSize)
    {
      IQueryable<ApplicationRole> rolesQuery = _roleManager.Roles
          .Include(r => r.Claims)
          .Include(r => r.Users)
          .OrderBy(r => r.Name);

      rolesQuery = AddScopeFilter(rolesQuery);
      if (page != -1)
      {
        rolesQuery = rolesQuery.Skip((page - 1) * pageSize);
      }
      if (pageSize != -1)
      {
        rolesQuery = rolesQuery.Take(pageSize);
      }
      return await rolesQuery.ToListAsync();
    }

    public async Task<bool> TestCanDeleteRoleAsync(Guid roleId)
    {
      return ! await _userRoleStore.Query().AnyAsync(ur => ur.RoleId == roleId);
    }

    public async Task UpdateRoleAsync(ApplicationRole role, IEnumerable<string> claims)
    {
      if (claims != null)
      {
        ValidateClaims(claims);
      }

      var result = await _roleManager.UpdateAsync(role);
      if (!result.Succeeded)
      {
        throw ExceptionFromIdentityErrors(result.Errors);
      }

      if (claims != null)
      {
        var currentRoleClaims = await _roleManager.GetClaimsAsync(role);
        var rolePermissions = currentRoleClaims.Where(c => c.Type == CustomClaimTypes.Permission);
        var rolePermissionsValues = rolePermissions.Select(c => c.Value).ToArray();

        var permissionsToRemove = rolePermissionsValues.Except(claims);
        var permissionsToAdd = claims.Except(rolePermissionsValues).Distinct();

        if (permissionsToRemove.Any())
        {
          foreach (string claim in permissionsToRemove)
          {
            var claimToRemove = rolePermissions.Where(c => c.Value == claim).FirstOrDefault();
            result = await _roleManager.RemoveClaimAsync(role, claimToRemove);
            if (!result.Succeeded)
            {
              throw ExceptionFromIdentityErrors(result.Errors);
            }
          }
        }

        if (permissionsToAdd.Any())
        {
          foreach (string permissionValue in permissionsToAdd)
          {
            var claimToAdd = new Claim(
              CustomClaimTypes.Permission, 
              _claimsService.GetPermissionByValue(permissionValue).ToString());
            result = await _roleManager.AddClaimAsync(role, claimToAdd);
            if (!result.Succeeded)
            {
              throw ExceptionFromIdentityErrors(result.Errors);
            }
          }
        }
      }
    }

    private BadRequestException ExceptionFromIdentityErrors(IEnumerable<IdentityError> errors)
    {
      var header = "At least one IdentityError occurred. Details: ";
      var errorDescriptions = errors.Select(e => e.Description + $" (code={e.Code})").ToArray();
      var message = header + string.Join("; ", errorDescriptions);
      return new BadRequestException(message);
    }

    private void ValidateClaims(IEnumerable<string> claims)
    {
      string[] invalidClaims = claims.Where(c => _claimsService.GetPermissionByValue(c) == null).ToArray();
      if (invalidClaims.Any())
      {
        throw new BadRequestException("The following claims are invalid: " + string.Join(", ", invalidClaims));
      }
    }

    public async Task DeleteAllTenantRoles(Guid tenantId)
    {
      var rolesToDelete = _roleManager.Roles.Where(r => r.TenantId == tenantId).ToList();
      foreach (var roleToDelete in rolesToDelete)
      {
        await _roleManager.DeleteAsync(roleToDelete);
      }
    }
  }
}
