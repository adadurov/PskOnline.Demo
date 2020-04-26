namespace PskOnline.Server.Authority
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using System.Web;
  using AutoMapper;
  using Microsoft.AspNetCore.Http;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.EntityFrameworkCore;
  using PskOnline.Server.Authority.API.Dto;
  using PskOnline.Server.Authority.EntityFramework;
  using PskOnline.Server.Authority.Interfaces;
  using PskOnline.Server.Authority.ObjectModel;
  using PskOnline.Server.Shared.Exceptions;
  using PskOnline.Server.Shared.Permissions;
  using PskOnline.Server.Shared.Service;
  using PskOnline.Server.Shared.Validators;

  /// <summary>
  /// This class implements IAccountManager functionality without security restrictions
  /// To be used for database seeding. At runtime, use <see cref="RestrictedAccountManager"/>
  /// </summary>
  public class UnrestrictedAccountManager : IAccountManager
  {
    private readonly AuthorityDbContext _context;
    private readonly MultitenantUserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly IRoleService _roleService;
    private readonly IEmailService _mailService;
    private readonly IUserOrgStructureReferencesValidator _userReferencesValidator;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <param name="userManager"></param>
    /// <param name="roleService"></param>
    /// <param name="userReferencesValidator">used to validate Users being created or updated</param>
    /// <param name="mailService"></param>
    /// <param name="autoMapperConfig"></param>
    public UnrestrictedAccountManager(
      AuthorityDbContext context,
      MultitenantUserManager<ApplicationUser> userManager,
      IRoleService roleService,
      IUserOrgStructureReferencesValidator userReferencesValidator,
      IEmailService mailService,
      AutoMapperConfig autoMapperConfig
      )
    {
      _context = context;
      _userManager = userManager;
      _roleService = roleService;
      _mailService = mailService;
      _mapper = autoMapperConfig.CreateMapper();
      // these are needed for validation of user model
      _userReferencesValidator = userReferencesValidator;
    }

    public IRoleService RoleService => _roleService;

    private async Task ValidateUserReferences(ApplicationUser user)
    {
      // Если пользователь относится к филиалу, и у пользователя есть департамент
      // то департамент должен относиться к тому же филиалу
      await _userReferencesValidator.ValidateOrgStructureReferences(user.Id, user.UserName, user.BranchOfficeId, user.DepartmentId, user.PositionId);

      // validate role references
      if (null != user.Roles)
      {
        await ValidateRoleReferences(user.TenantId, user.Roles.Select(r => r.RoleId));
      }
    }

    private async Task ValidateRoleReferences(Guid userTenantId, IEnumerable<Guid> roleIds)
    {
      if (roleIds != null)
      {
        foreach (var roleId in roleIds)
        {
          try
          {
            var role = await _roleService.GetRoleByIdLoadRelatedAsync(roleId);
            if (userTenantId != role.TenantId)
            {
              throw BadRequestException.BadReference("User refers to invalid role " + roleId.ToString());
            }
          }
          catch( Exception ex ) 
            when( ex is ItemNotFoundException || ex is UnauthorizedAccessException )
          {
            throw BadRequestException.BadReference("User refers to invalid role " + roleId.ToString(), ex);
          }
        }
      }
    }

    public async Task<ApplicationUser> GetUserByIdAsync(string userId)
    {
      var user = await _userManager.FindByIdAsync(userId);
      await ValidateAccessToEntity(user, EntityAction.Read);
      return user;
    }

    public async Task<ApplicationUser> GetUserByUserNameAsync(string userName)
    {
      var user = await _userManager.FindByNameAsync(userName);
      await ValidateAccessToEntity(user, EntityAction.Read);
      return user;
    }

    public async Task<ApplicationUser> GetUserByEmailAsync(string email)
    {
      var user = await _userManager.FindByEmailAsync(email);
      await ValidateAccessToEntity(user, EntityAction.Read);
      return user;
    }

    public async Task<IList<Guid>> GetUserRoleIdsAsync(ApplicationUser user)
    {
      await ValidateAccessToEntity(user, EntityAction.Read);
      var userRolesQuery = _context.UserRoles.Where(ur => ur.UserId == user.Id);
      return await userRolesQuery.Select(r => r.RoleId).Distinct().ToListAsync();
    }

    public async Task<Tuple<ApplicationUser, ApplicationRole[]>> GetUserAndRoleIdsAsync(Guid userGuid)
    {
      var user = await _context.Users
          .Include(u => u.Roles)
          .Where(u => u.Id == userGuid)
          .FirstOrDefaultAsync();

      if (user == null)
        return null;

      await ValidateAccessToEntity(user, EntityAction.Read);

      var userRoleIds = user.Roles.Select(r => r.RoleId).ToList();

      var roles = await _context.Roles
          .Where(r => userRoleIds.Contains(r.Id))
          .ToArrayAsync();

      return Tuple.Create(user, roles);
    }

    protected virtual Task ValidateAccessToEntity(ApplicationUser user, EntityAction action)
    {
      return Task.CompletedTask;
    }

    public async Task<List<Tuple<ApplicationUser, ApplicationRole[]>>> GetUsersAndRoleIdsAsync(int page, int pageSize)
    {
      IQueryable<ApplicationUser> usersQuery = _context.Users
          .Include(u => u.Roles)
          .OrderBy(u => u.UserName);

      usersQuery = ApplyScopeFilter(usersQuery);

      if (page != -1)
      {
        usersQuery = usersQuery.Skip((page - 1) * pageSize);
      }

      if (pageSize != -1)
      {
        usersQuery = usersQuery.Take(pageSize);
      }

      var users = await usersQuery.ToListAsync();

      var userRoleIds = users.SelectMany(u => u.Roles.Select(r => r.RoleId)).ToList();

      var roles = await _context.Roles
          .Where(r => userRoleIds.Contains(r.Id))
          .ToArrayAsync();

      return users.Select(u => Tuple.Create(u, _context.Roles.Where(r => u.Roles.Any(ur => ur.RoleId == r.Id)).ToArray())).ToList();
    }

    protected virtual IQueryable<ApplicationUser> ApplyScopeFilter(IQueryable<ApplicationUser> usersQuery)
    {
      return usersQuery;
    }

    public async Task<(bool success, string[] errors)> CreateUserAsync(UserDto newUserDto, IEnumerable<Guid> roleIds, string password)
    {
      var newUser = _mapper.Map<ApplicationUser>(newUserDto);
      return await CreateUserAsync(newUser, roleIds, password);
    }

    public virtual async Task<(bool success, string[] errors)> CreateUserAsync(ApplicationUser newUser, IEnumerable<Guid> roleIds, string password)
    {
      // The below call will validate whether the current user actually has 
      // the permission to create users (in either 'site' or 'tenant' scope)
      // together with tenant access validation
      var t1 = ValidateAccessToEntity(newUser, EntityAction.Create);

      var existingUser = await _userManager.FindByEmailInTenantAsync(newUser.Email, newUser.TenantId);
      if (existingUser != null)
      {
        return (
          false, 
          new[] { "A user with the same email already exists. Please use a different email." }
          );
      }

      // validate that the user refers to proper BranchOffice, Department and Position
      var t2 = ValidateUserReferences(newUser);

      var result = await _userManager.CreateAsync(newUser, password);
      if (!result.Succeeded)
      {
        return (false, result.Errors.Select(e => e.Description).ToArray());
      }

      newUser = await _userManager.FindByNameInTenantAsync(newUser.UserName, newUser.TenantId);

      try
      {
        await AddToRolesAsync(newUser, roleIds.Distinct());
        _context.SaveChanges();
      }
      catch
      {
        await DeleteUserAsync(newUser);
        throw;
      }

      return (true, new string[] { });
    }

    public async Task UpdateUserAsync(ApplicationUser user)
    {
      await UpdateUserAsync(user, null);
    }

    public async Task UpdateUserAsync(ApplicationUser user, IEnumerable<Guid> roles)
    {
      await ValidateAccessToEntity(user, EntityAction.Update);
      await ValidateUserReferences(user);
      await ValidateRoleReferences(user.TenantId, roles);

      _context.Users.Update(user);

      if (roles != null)
      {
        var userRoles = await GetUserRoleIdsAsync(user);

        var rolesToRemove = userRoles.Except(roles);
        RemoveFromRoles(user, rolesToRemove);

        var rolesToAdd = roles.Except(userRoles).Distinct();
        await AddToRolesAsync(user, rolesToAdd);
      }

      await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <param name="rolesToAdd"></param>
    /// <remarks>You need to call _context.SaveChanges() upon successful completion of the transaction that the call is part of.</remarks>
    private async Task AddToRolesAsync(ApplicationUser user, IEnumerable<Guid> rolesToAdd)
    {
      foreach (var roleId in rolesToAdd)
      {
        var existingRole = await _roleService.GetRoleByIdAsync(roleId);
        if( existingRole == null )
        {
          throw new ItemNotFoundException(roleId.ToString(), "role");
        }
      }
      foreach (var roleId in rolesToAdd)
      {
        _context.UserRoles.Add(new IdentityUserRole<Guid>()
        {
          RoleId = roleId,
          UserId = user.Id
        });
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <param name="rolesToRemove"></param>
    /// <remarks>You need to call _context.SaveChanges() upon successful completion of the transaction that the call is part of.</remarks>
    private void RemoveFromRoles(ApplicationUser user, IEnumerable<Guid> rolesToRemove)
    {
      foreach (var roleId in rolesToRemove)
      {
        _context.UserRoles.Remove(new IdentityUserRole<Guid>()
        {
          RoleId = roleId,
          UserId = user.Id
        });
      }
    }


    public async Task<(bool success, string[] errors)> ResetPasswordAsync(ApplicationUser user, string newPassword)
    {
      await ValidateAccessToEntity(user, EntityAction.Update);

      string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

      var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
      if (!result.Succeeded)
      {
        return (false, result.Errors.Select(e => e.Description).ToArray());
      }

      return (true, new string[] { });
    }

    public async Task SendPasswordResetLinkAsync(string userNameOrEmail, HttpRequest request)
    {
      var user = await GetUserByUserNameAsync(userNameOrEmail);
      
      if (user == null)
      {
        user = await GetUserByEmailAsync(userNameOrEmail);
      }
      if (user == null)
      {
        return;
      }
      var token = await _userManager.GeneratePasswordResetTokenAsync(user);

      var pwdResetLink = request.Scheme + "://" + request.Host + "/admin/password-reset?" +
          $"token={HttpUtility.UrlEncode(token)}&username={HttpUtility.UrlEncode(user.UserName)}";
      
      // 3 parameters are required:
      //   {{UniquePwdResetLink}}
      //   {{UserName}}
      //   {{PskHostName}}
      // send email with the link to the user's email address
      await _mailService.SendEmailWithTemplateAsync(
        "psk_online_password_reset",
        user.UserName,
        user.Email,
        "PSK Online - запрос сброса пароля",
        new
        {
          UniquePwdResetLink = pwdResetLink,
          UserName = user.UserName,
          PskHostName = request.Host.Host
        });
    }


    public async Task<(bool, string[])> UpdatePasswordAsync(ApplicationUser user, string currentPassword, string newPassword)
    {
      await ValidateAccessToEntity(user, EntityAction.Update);

      var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
      if (!result.Succeeded)
      {
        return (false, result.Errors.Select(e => e.Description).ToArray());
      }
      return (true, new string[] { });
    }

    public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
    {
      if (!await _userManager.CheckPasswordAsync(user, password))
      {
        if (!_userManager.SupportsUserLockout)
        {
          await _userManager.AccessFailedAsync(user);
        }
        return false;
      }

      return true;
    }

    public Task<bool> TestCanDeleteUserAsync(string userId)
    {
      // FIXME: Test if user has any assets?
      // canDelete = !await ; // Do other tests...

      return Task.FromResult(true);
    }

    public async Task<(bool success, string[] errors)> DeleteUserAsync(string userId)
    {
      var user = await _userManager.FindByIdAsync(userId);

      await ValidateAccessToEntity(user, EntityAction.Delete);

      if (user != null)
      {
        return await DeleteUserAsync(user);
      }
      return (true, new string[] { });
    }

    public Task<long> GetUserCountInTenantAsync(Guid tenantId)
    {
      return _userManager.Users.LongCountAsync(u => u.TenantId == tenantId);
    }

    public IEnumerable<UserDto> GetSpecialUsersInDepartment(Guid departmentId)
    {
      var usersQuery = ApplyScopeFilter(_context.Users);
      return usersQuery
        .Where(u => u.DepartmentId == departmentId && u.IsDepartmentSpecialUser)
        .Select(u => _mapper.Map<UserDto>(u));
    }


    public async Task<(bool success, string[] errors)> DeleteUserAsync(ApplicationUser user)
    {
      await ValidateAccessToEntity(user, EntityAction.Delete);
      var result = await _userManager.DeleteAsync(user);
      return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<(SecurePasswordResetResult status, string[] errors)> ResetPasswordUsingTokenAsync(string userName, string token, string newPassword)
    {
      var user = await _userManager.FindByNameAsync(userName);
      if (user == null)
      {
        return (
          SecurePasswordResetResult.InvalidToken, 
          new[] { "Invalid token."}
          );
      }

      var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

      if (result.Succeeded)
      {
        return (
          SecurePasswordResetResult.Success, 
          new string[0]);
      }

      if (result.Errors.Any(e => string.Compare(e.Code, "InvalidToken", true) == 0))
      {
        return (
          SecurePasswordResetResult.InvalidToken, 
          new string[0]);
      }

      return (
        SecurePasswordResetResult.PasswordTooSimple, 
        result.Errors.Select(e => e.Description).ToArray()
        );
    }
  }
}
