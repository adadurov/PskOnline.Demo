namespace PskOnline.Server.Authority.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.AspNetCore.Authorization;
  using AutoMapper;

  using PskOnline.Server.Authority.API.Dto;
  using PskOnline.Server.Authority.ObjectModel;
  using PskOnline.Server.Authority.Interfaces;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Authority;

  [Authorize]
  [Route("api/account")]
  public class RolesController : BaseController
  {
    private readonly IRestrictedAccountManager _accountManager;
    private readonly IRestrictedRoleService _roleService;
    private readonly IClaimsService _claimsService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ITenantIdProvider _tenantIdProvider;
    private readonly IMapper _mapper;

    private const string GetRoleByIdActionName = nameof(GetRoleById);

    public RolesController(
      IRestrictedAccountManager accountManager,
      IRestrictedRoleService roleService,
      IClaimsService claimsService,
      IAuthorizationService authorizationService,
      ITenantIdProvider tenantIdProvider,
      AutoMapperConfig autoMapperConfig
      )
    {
      _roleService = roleService;
      _claimsService = claimsService;
      _accountManager = accountManager;
      _authorizationService = authorizationService;
      _tenantIdProvider = tenantIdProvider;
      _mapper = autoMapperConfig.CreateMapper();
    }

    private async Task<RoleDto> GetRoleViewModelHelper(Guid roleId)
    {
      var role = await _roleService.GetRoleByIdLoadRelatedAsync(roleId);
      if (role == null)
      {
        return null;
      }

      return _mapper.Map<RoleDto>(
        role, 
        options => options.ConstructServicesUsing(
          t => new ClaimValueToPermissionDtoConverter(_claimsService)));
    }

    [HttpGet("roles/{id:Guid}", Name = GetRoleByIdActionName)]
    [Produces(typeof(RoleDto))]
    public async Task<IActionResult> GetRoleById(Guid id)
    {
      var appRole = await _roleService.GetRoleByIdAsync(id);

      var viewRolePolicyResult = await _authorizationService.AuthorizeAsync(
        User, id, Authorization.Policies.ViewRoleByIdPolicy);
      if (!viewRolePolicyResult.Succeeded)
      {
        return Forbid();
      }

      if (appRole == null)
      {
        return NotFound(id);
      }

      RoleDto roleVM = await GetRoleViewModelHelper(id);

      if (roleVM == null)
      {
        return NotFound(id);
      }

      return Ok(roleVM);
    }

    [HttpGet("roles")]
    [Produces(typeof(IEnumerable<RoleInTenantDto>))]
    [Authorize(Authorization.Policies.ViewAllRolesPolicy)]
    public async Task<IActionResult> GetRoles()
    {
      return await GetRoles(-1, -1);
    }

    [HttpGet("roles/{page:int}/{pageSize:int}")]
    [Produces(typeof(IEnumerable<RoleInTenantDto>))]
    [Authorize(Authorization.Policies.ViewAllRolesPolicy)]
    public async Task<IActionResult> GetRoles(int page, int pageSize)
    {
      var roles = await _roleService.GetRolesLoadRelatedAsync(page, pageSize);
      return Ok(_mapper.Map<List<RoleInTenantDto>>(
        roles, opts => opts.ConstructServicesUsing(
          r => new ClaimValueToPermissionDtoConverter(_claimsService))));
    }

    [HttpPut("roles/{id:Guid}")]
    [Authorize(Authorization.Policies.ManageAllRolesPolicy)]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] RoleDto role)
    {
      if (ModelState.IsValid)
      {
        if (role == null)
        {
          return InvalidRequestBodyJson(nameof(RoleDto));
        }

        if (id.ToString() != role.Id)
        {
          return BadRequest("Conflicting or missing role id in model data versus the URL");
        }

        var appRole = await _roleService.GetRoleByIdInternalAsync(id);

        if (appRole == null)
        {
          return NotFound(id);
        }

        _mapper.Map<RoleDto, ApplicationRole>(role, appRole);

        await _roleService.UpdateRoleAsync(appRole, role.Permissions?.Select(p => p.Value).ToArray());
        
        return NoContent();
      }

      return BadRequest(ModelState);
    }

    [HttpPost("roles")]
    [Authorize(Authorization.Policies.ManageAllRolesPolicy)]
    public async Task<IActionResult> CreateRole([FromBody] RoleDto role)
    {
      if (! ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }
      if (role == null)
      {
        return InvalidRequestBodyJson(nameof(RoleDto));
      }

      ApplicationRole appRole = _mapper.Map<ApplicationRole>(role);

      // This will throw if tenant Id is not available,
      // which means there is a bug in authentication code
      appRole.TenantId = _tenantIdProvider.GetTenantId();

      await _roleService.CreateRoleAsync(appRole, role.Permissions?.Select(p => p.Value).ToArray());
      RoleDto roleVM = await GetRoleViewModelHelper(appRole.Id);
      return CreatedAtAction(GetRoleByIdActionName, new { id = roleVM.Id }, roleVM);
    }

    [HttpDelete("roles/{id:Guid}")]
    [Produces(typeof(RoleDto))]
    [Authorize(Authorization.Policies.ManageAllRolesPolicy)]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
      await _roleService.DeleteRoleByIdAsync(id);
      return NoContent();
    }
  }
}
