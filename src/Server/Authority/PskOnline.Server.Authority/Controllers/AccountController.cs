namespace PskOnline.Server.Authority.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;

  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.JsonPatch;
  using Microsoft.AspNetCore.Mvc;

  using AutoMapper;

  using PskOnline.Server.Authority;
  using PskOnline.Server.Authority.Authorization;
  using PskOnline.Server.Authority.API.Dto;
  using PskOnline.Server.Authority.Interfaces;
  using PskOnline.Server.Authority.ObjectModel;
  using PskOnline.Server.Shared.Permissions;

  using PskOnline.Server.Shared.Multitenancy;
  using Swashbuckle.AspNetCore.SwaggerGen;
  using System.Net;

  [Authorize]
  [Route("api/account")]
  public class AccountController : BaseController
  {
    private readonly IRestrictedAccountManager _accountManager;
    private readonly IAccountManager _unrestrictedAccountManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly ITenantEntityAccessChecker _accessChecker;
    private readonly IClaimsService _claimsService;
    private readonly IMapper _mapper;

    private const string GetUserByIdActionName = nameof(GetUserById);

    public AccountController(
      IRestrictedAccountManager accountManager,
      IAccountManager unrestrictedAccountManager,
      IAuthorizationService authorizationService,
      IClaimsService claimsService,
      ITenantEntityAccessChecker accessChecker,
      AutoMapperConfig autoMapperConfig)
    {
      _accountManager = accountManager;
      _unrestrictedAccountManager = unrestrictedAccountManager;
      _authorizationService = authorizationService;
      _accessChecker = accessChecker;
      _claimsService = claimsService;
      _mapper = autoMapperConfig.CreateMapper();
    }

    /// <summary>
    /// Sets the specified password for the user using the one-time token
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("reset-password")]
    [SwaggerResponse((int)HttpStatusCode.NoContent, description: "Successfully completed. The password has been reset.")]
    [SwaggerResponse((int)HttpStatusCode.Forbidden, description:"The security token is invalid or expired. User should request another password reset link.")]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, description: "The request model is invalid or the password doesn't meet complexity requirements.")]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized, description: "This status code is not applicable to this resource and method.")]
    public async Task<IActionResult> ResetPasswordUsingToken([FromBody]ResetPasswordDto request)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }
      var result = await _unrestrictedAccountManager.ResetPasswordUsingTokenAsync(request.UserName, request.Token, request.NewPassword);

      if (result.status == SecurePasswordResetResult.Success)
      {
        return NoContent();
      }

      if (result.status == SecurePasswordResetResult.InvalidToken)
      {
        return Forbid();
      }

      AddErrors(result.errors);
      return BadRequest();
    }

    /// <summary>
    /// Initiate a password reset for the specified user in the tenant
    /// identified by the slug in the server URL
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("reset-password-start")]
    [SwaggerResponse((int)HttpStatusCode.NoContent, description: "The system will soon send a password reset email if username/email is registered in the systems.")]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized, description: "This status code is not applicable to this resource and method.")]
    [SwaggerResponse((int)HttpStatusCode.Forbidden, description: "This status code is not applicable to this resource and method.")]
    public async Task<IActionResult> ResetPasswordStart([FromBody]ResetPasswordStartDto pwdResetRequest)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }
      await _unrestrictedAccountManager.SendPasswordResetLinkAsync(pwdResetRequest.UserNameOrEmail, Request);
      return NoContent();
    }

    /// <summary>
    /// Retrieve information about the currenent user
    /// </summary>
    /// <returns></returns>
    [HttpGet("users/me")]
    [Produces(typeof(UserDto))]
    public async Task<IActionResult> GetCurrentUser()
    {
      return await GetUserByUserName(this.User.Identity.Name);
    }

    /// <summary>
    /// Retrieve information about a user by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("users/{id}", Name = GetUserByIdActionName)]
    [Produces(typeof(UserDto))]
    public async Task<IActionResult> GetUserById(Guid id)
    {
      var policyResult = await _authorizationService.AuthorizeAsync(
        User, id, UserAccountAuthorizationRequirements.Read);
      if (!policyResult.Succeeded)
      {
        return Forbid();
      }

      UserDto userVM = await GetUserViewModelHelper(id);

      if (userVM == null)
      {
        return NotFound(id);
      }

      return Ok(userVM);
    }

    /// <summary>
    /// Retrieve information about a user by username
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    [HttpGet("users/username/{userName}")]
    [Produces(typeof(UserDto))]
    public async Task<IActionResult> GetUserByUserName(string userName)
    {
      ApplicationUser appUser = await _accountManager.GetUserByUserNameAsync(userName);

      var authResult = (await _authorizationService.AuthorizeAsync(User, appUser?.Id ?? default(Guid?), UserAccountAuthorizationRequirements.Read));
      if (!authResult.Succeeded)
      {
        return Forbid();
      }

      if (appUser == null)
      {
        return NotFound(userName);
      }

      return await GetUserById(appUser.Id);
    }

    /// <summary>
    /// Retrieve all users
    /// </summary>
    /// <remarks>Retrieve all users available to the current user (depending on permissions)</remarks>
    /// <returns></returns>
    [HttpGet("users")]
    [Produces(typeof(List<UserDto>))]
    //[Authorize(Authorization.Policies.ViewAllUsersPolicy)]
    public async Task<IActionResult> GetUsers()
    {
      return await GetUsers(-1, -1);
    }

    /// <summary>
    /// Retrieve all users with paging
    /// </summary>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <remarks>Retrieve all users available to the current user (depending on permissions) with paging options</remarks>
    /// <returns></returns>
    [HttpGet("users/{page:int}/{pageSize:int}")]
    [Produces(typeof(List<UserDto>))]
    //[Authorize(Authorization.Policies.ViewAllUsersPolicy)]
    public async Task<IActionResult> GetUsers(int page, int pageSize)
    {
      var usersAndRoles = await _accountManager.GetUsersAndRoleIdsAsync(page, pageSize);

      List<UserDto> usersVM = new List<UserDto>();

      foreach (var item in usersAndRoles)
      {
        var userVM = _mapper.Map<UserDto>(item.Item1);
        userVM.Roles = _mapper.Map<List<UserRoleInfo>>(item.Item2);

        usersVM.Add(userVM);
      }

      return Ok(usersVM);
    }

    /// <summary>
    /// Update the currently logged in user
    /// </summary>
    /// <param name="userDto"></param>
    /// <returns></returns>
    [HttpPut("users/me")]
    public async Task<IActionResult> UpdateCurrentUser([FromBody] UserEditDto userDto)
    {
      return await UpdateUser(User.GetUserId(), userDto);
    }

    /// <summary>
    /// Update information about a user by id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UserEditDto user)
    {
     if (user == null)
      {
        return InvalidRequestBodyJson(nameof(UserEditDto));
      }
      if (!string.IsNullOrWhiteSpace(user.Id) && id != user.Id)
      {
        return BadRequest("Conflicting user id in URL and model data");
      }

      ApplicationUser appUser = await _accountManager.GetUserByIdAsync(id);
      if( null == appUser )
      {
        return NotFound();
      }

      Guid[] currentRoleIds = appUser != null ? (await _accountManager.GetUserRoleIdsAsync(appUser)).ToArray() : null;

      await _accessChecker.ValidateAccessToEntityAsync(appUser, EntityAction.Update);

      var manageUsersPolicyResult = _authorizationService.AuthorizeAsync(
        User, appUser, UserAccountAuthorizationRequirements.Update);

      var assignRolePolicyResult = _authorizationService.AuthorizeAsync(
        User, 
        new UserRoleChange { NewRoles = user.Roles?.Select( r => Guid.Parse(r.Id)).ToArray(), CurrentRoles = currentRoleIds },
        Policies.AssignAllowedRolesPolicy);

      if ((await Task.WhenAll(manageUsersPolicyResult, assignRolePolicyResult)).Any(r => !r.Succeeded))
      {
        return Forbid();
      }

      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      if (appUser == null)
      {
        return NotFound(id);
      }

      var currentUserId = this.User.GetUserId();

      if (currentUserId == id && string.IsNullOrWhiteSpace(user.CurrentPassword))
      {
        if (!string.IsNullOrWhiteSpace(user.NewPassword))
        {
          return BadRequest("Current password is required when changing your own password");
        }

        if (appUser.UserName != user.UserName)
        {
          return BadRequest("Current password is required when changing your own username");
        }
      }

      if (currentUserId == id && (appUser.UserName != user.UserName || !string.IsNullOrWhiteSpace(user.NewPassword)))
      {
        if (!await _accountManager.CheckPasswordAsync(appUser, user.CurrentPassword))
        {
          AddErrors(new string[] { "The username/password couple is invalid." });
          return BadRequest(ModelState);
        }
      }

      _mapper.Map<UserDto, ApplicationUser>(user, appUser);

      await _accountManager.UpdateUserAsync(appUser, user.Roles.Select(r => Guid.Parse(r.Id)));

      if (!string.IsNullOrWhiteSpace(user.NewPassword))
      {
        if (!string.IsNullOrWhiteSpace(user.CurrentPassword))
        {
          await _accountManager.UpdatePasswordAsync(appUser, user.CurrentPassword, user.NewPassword);
        }
        else
        {
          await _accountManager.ResetPasswordAsync(appUser, user.NewPassword);
        }
      }
      return NoContent();
    }

    /// <summary>
    /// Partially update information about the current user
    /// </summary>
    /// <param name="patch"></param>
    /// <returns></returns>
    [HttpPatch("users/me")]
    public async Task<IActionResult> UpdateCurrentUser([FromBody] JsonPatchDocument<UserPatchDto> patch)
    {
      return await UpdateUser(this.User.GetUserId(), patch);
    }

    /// <summary>
    /// Partially update information about a user by id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="patch"></param>
    /// <returns></returns>
    [HttpPatch("users/{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] JsonPatchDocument<UserPatchDto> patch)
    {
      if (!(await _authorizationService.AuthorizeAsync(this.User, id, UserAccountAuthorizationRequirements.Update)).Succeeded)
      {
        return Forbid();
      }

      if (ModelState.IsValid)
      {
        if (patch == null)
        {
          return BadRequest($"{nameof(patch)} cannot be null");
        }

        ApplicationUser appUser = await _accountManager.GetUserByIdAsync(id);

        if (appUser == null)
        {
          return NotFound(id);
        }

        var userPVM = _mapper.Map<UserPatchDto>(appUser);
        patch.ApplyTo(userPVM, ModelState);

        if (ModelState.IsValid)
        {
          _mapper.Map<UserPatchDto, ApplicationUser>(userPVM, appUser);

          await _accountManager.UpdateUserAsync(appUser);
          return NoContent();
        }
      }

      return BadRequest(ModelState);
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="newUserDto"></param>
    /// <returns></returns>
    [HttpPost("users")]
    [Authorize(Policies.ManageAllUsersPolicy)]
    public async Task<IActionResult> Register([FromBody] UserEditDto newUserDto)
    {
      if (newUserDto == null)
      {
        return BadRequest($"{nameof(newUserDto)} cannot be null");
      }
      if( newUserDto.NewPassword == null )
      {
        return BadRequest("NewPassword may not be null");
      }

      Guid[] newUserRolesGuids = null;
      try
      {
        newUserRolesGuids = newUserDto.Roles.Select(r => Guid.Parse(r.Id)).ToArray();
      }
      catch
      {
        return BadRequest("Cannot parse GUIDs of the new user's roles");
      }

      var success = (await _authorizationService.AuthorizeAsync(
        User,
        new UserRoleChange { NewRoles = newUserRolesGuids, CurrentRoles = new Guid[] { } },
        Policies.AssignAllowedRolesPolicy)).Succeeded;

      if (!success)
      {
        return Forbid();
      }

      ApplicationUser appUser = _mapper.Map<ApplicationUser>(newUserDto);

      var result = await _accountManager.CreateUserAsync(appUser, newUserDto.Roles.Select(r => Guid.Parse(r.Id)), newUserDto.NewPassword);
      if (result.success)
      {
        UserDto userVM = await GetUserViewModelHelper(appUser.Id);
        return CreatedAtAction(GetUserByIdActionName, new { id = userVM.Id }, userVM);
      }

      AddErrors(result.errors);
      return BadRequest(ModelState);
    }

    /// <summary>
    /// Delete a user by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("users/{id}")]
    [Produces(typeof(UserDto))]
    public async Task<IActionResult> DeleteUser(string id)
    {
      var user = await _accountManager.GetUserByIdAsync(id);
      if( user == null )
      {
        return NotFound();
      }

      if (!(await _authorizationService.AuthorizeAsync(this.User, user, UserAccountAuthorizationRequirements.Delete)).Succeeded)
      {
        return Forbid();
      }

      if (!await _accountManager.TestCanDeleteUserAsync(id))
      {
        return BadRequest("User cannot be deleted. Delete all orders associated with this user and try again");
      }

      UserDto userVM = null;
      ApplicationUser appUser = await this._accountManager.GetUserByIdAsync(id);

      if (appUser != null)
      {
        userVM = await GetUserViewModelHelper(appUser.Id);
      }

      if (userVM == null)
      {
        return NotFound(id);
      }

      var result = await this._accountManager.DeleteUserAsync(appUser);
      if (!result.Item1)
      {
        throw new Exception("The following errors occurred whilst deleting user: " + string.Join(", ", result.Item2));
      }

      // FIXME: should be NoContent, but what if Web UI for user management depends on it returning OK?
      return Ok(userVM);
    }

    /// <summary>
    /// Unblock a user by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("users/unblock/{id}")]
    [Authorize(Policies.ManageAllUsersPolicy)]
    public async Task<IActionResult> UnblockUser(string id)
    {
      ApplicationUser appUser = await this._accountManager.GetUserByIdAsync(id);

      if (appUser == null)
      {
        return NotFound(id);
      }
      appUser.LockoutEnd = null;
      await _accountManager.UpdateUserAsync(appUser);

      return NoContent();
    }

    /// <summary>
    /// Retrieve Web UI preferences for the current user
    /// </summary>
    /// <returns></returns>
    [HttpGet("users/me/preferences")]
    [Produces(typeof(string))]
    public async Task<IActionResult> UserPreferences()
    {
      var userId = this.User.GetUserId();
      ApplicationUser appUser = await _accountManager.GetUserByIdAsync(userId);

      if (appUser != null)
      {
        return Ok(appUser.WebUiPreferences);
      }
      else
      {
        return NotFound(userId);
      }
    }

    /// <summary>
    /// Update Web UI preferences for the current user
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPut("users/me/preferences")]
    public async Task<IActionResult> UserPreferences([FromBody] string data)
    {
      var userId = this.User.GetUserId();
      ApplicationUser appUser = await _accountManager.GetUserByIdAsync(userId);

      if (appUser == null)
      {
        return NotFound(userId);
      }

      appUser.WebUiPreferences = data;
      await _accountManager.UpdateUserAsync(appUser);

      return NoContent();
    }

    /// <summary>
    /// Retrieve all permissions known to the server
    /// </summary>
    /// <returns></returns>
    [HttpGet("permissions")]
    [Produces(typeof(List<PermissionDto>))]
    [Authorize(Policies.ViewAllRolesPolicy)]
    public IActionResult GetAllPermissions()
    {
      var allPermissions = _claimsService.GetAllPermissions();
      if ( ! TenantSpec.BelongsToSite(User.GetUserTenant()) )
      {
        allPermissions = allPermissions.Where(p => p.Scope != PermScope.GLOBAL).ToList();
      }
      return Ok(_mapper.Map<List<PermissionDto>>(allPermissions));
    }

    private async Task<UserDto> GetUserViewModelHelper(Guid userId)
    {
      var userAndRoles = await _accountManager.GetUserAndRoleIdsAsync(userId);
      if (userAndRoles == null)
      {
        return null;
      }

      var userVM = _mapper.Map<UserDto>(userAndRoles.Item1);
      userVM.Roles = _mapper.Map<List<UserRoleInfo>>(userAndRoles.Item2);

      return userVM;
    }
  }
}
