namespace PskOnline.Server.Service.Controllers
{
  using System;
  using System.Collections.Generic;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Logging;

  using AutoMapper;
  using PskOnline.Server.Service.ViewModels;
  using PskOnline.Server.ObjectModel;
  using System.Threading.Tasks;
  using PskOnline.Server.DAL.Multitenancy;
  using PskOnline.Server.Shared.Service;
  using PskOnline.Server.Authority.API.Dto;
  using PskOnline.Server.Shared.Contracts.Service;
  using PskOnline.Server.DAL.OrgStructure.Interfaces;
  using PskOnline.Server.Authority.API;

  [Authorize]
  [Route("api/[controller]")]
  public class TenantController : BaseController
  {
    private readonly IService<Tenant> _tenantService;
    private readonly ITenantCreator _tenantCreator;
    private readonly IAccountService _accountService;
    private readonly IInspectionService _inspectionService;
    private readonly IEmployeeService _employeeService;

    private readonly IService<BranchOffice> _branchOfficeService;
    private readonly IService<Department> _departmentService;

    private readonly ILogger _logger;

    public TenantController(
      IService<Tenant> tenantService,
      ITenantCreator tenantCreator,
      IAccountService accountService,
      IInspectionService inspectionService,
      IEmployeeService employeeService,
      IService<BranchOffice> branchOfficeService,
      IService<Department> departmentService,
    ILogger<TenantController> logger)
    {
      _tenantService = tenantService;
      _tenantCreator = tenantCreator;
      _accountService = accountService;
      _inspectionService = inspectionService;
      _employeeService = employeeService;
      _branchOfficeService = branchOfficeService;
      _departmentService = departmentService;
      _logger = logger;
    }

    [HttpGet]
    [Produces(typeof(IEnumerable<TenantDto>))]
    public async Task<IActionResult> GetAll()
    {
      var allTenants = await _tenantService.GetAllAsync(null, null);
      return Ok(Mapper.Map<IEnumerable<TenantDto>>(allTenants));
    }

    [HttpGet("{id:Guid}", Name = nameof(GetTenant))]
    [Produces(typeof(TenantDto))]
    public async Task<IActionResult> GetTenant(Guid id)
    {
      var tenant = await _tenantService.GetAsync(id);
      return Ok(Mapper.Map<TenantDto>(tenant));
    }

    [HttpGet("{tenantId:Guid}/operations-summary")]
    [Produces(typeof(TenantOperationsSummaryDto))]
    public async Task<IActionResult> GetOperationsSummary(Guid tenantId)
    {
      var tenant = await _tenantService.GetAsync(tenantId);
      var utcNow = DateTimeOffset.UtcNow;

      var opsSummaryDto = new TenantOperationsSummaryDto
      {
        Id = tenant.Id.ToString(),
        Name = tenant.Name,
        ServiceExpireDate = tenant.ServiceDetails.ServiceExpireDate,
        ServiceMaxUsers = tenant.ServiceDetails.ServiceMaxUsers,
        ServiceMaxEmployees = tenant.ServiceDetails.ServiceMaxEmployees,
        ServiceActualUsers = await _accountService.GetUserCountInTenantAsync(tenantId),
        ServiceActualEmployees = await _employeeService.GetItemCountInTenantAsync(tenantId),
        BranchOfficesCount = await _branchOfficeService.GetItemCountInTenantAsync(tenantId),
        DepartmentsCount = await _departmentService.GetItemCountInTenantAsync(tenantId),
        NewInspectionsLast24Hours = await _inspectionService.GetInspectionCountSinceAsync(tenantId, utcNow - TimeSpan.FromHours(24)),
        NewInspectionsLastWeek = await _inspectionService.GetInspectionCountSinceAsync(tenantId, utcNow - TimeSpan.FromDays(7))
      };
      return Ok(opsSummaryDto);
    }

    [HttpGet("{id:Guid}/shared-info")]
    [Produces(typeof(TenantSharedInfoDto))]
    public async Task<IActionResult> GetSharedInfo(Guid id)
    {
      var tenant = await _tenantService.GetAsync(id);

      var sharedInfoDto = new TenantSharedInfoDto
      {
        Id = tenant.Id.ToString(),
        Name = tenant.Name
      };
      return Ok(sharedInfoDto);
    }

    [HttpPost]
    [Produces(typeof(CreatedWithGuidDto))]
    public async Task<IActionResult> Post([FromBody]TenantCreateDto newTenantDto)
    {
      if( newTenantDto == null )
      {
        return InvalidRequestBodyJson(nameof(TenantCreateDto));
      }
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }
      if (newTenantDto.TenantDetails.PrimaryContact == null)
      {
        AddErrors(new[] { "TenantDetails model must have contact information defined" });
        return BadRequest(ModelState);
      }
      if (newTenantDto.TenantDetails.ServiceDetails == null)
      {
        AddErrors(new[] { "TenantDetails model must have service information defined" });
        return BadRequest(ModelState);
      }
      var newTenant = Mapper.Map<Tenant>(newTenantDto.TenantDetails);
      var newUser = Mapper.Map<UserDto>(newTenantDto.AdminUserDetails);
      var creationResult = await _tenantCreator.CreateNewTenant(
        newTenant, newUser, newTenantDto.AdminUserDetails.NewPassword);

      return Created(nameof(GetTenant), new CreatedWithGuidDto { Id = newTenant.Id } );
    }

    [HttpPut("{id:Guid}")]
    public async Task<IActionResult> Put(Guid id, [FromBody]TenantDto tenantDto)
    {
      if (tenantDto == null)
      {
        return InvalidRequestBodyJson(nameof(TenantDto));
      }
      if (id.ToString() != tenantDto.Id)
      {
        return BadRequest("Conflicting or missing tenant id in model data versus the URL");
      }
      if (tenantDto.PrimaryContact == null)
      {
        AddErrors(new[] { "Model must have contact information defined" });
        return BadRequest();
      }
      if (tenantDto.ServiceDetails == null)
      {
        AddErrors(new[] { "Model must have service information defined" });
        return BadRequest(ModelState);
      }
      var tenant = await _tenantService.GetAsync(id);
      Mapper.Map(tenantDto, tenant, typeof(TenantDto), typeof(Tenant));

      await _tenantService.UpdateAsync(tenant);
      return NoContent();
    }

    [HttpDelete("{id:Guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
      await _tenantService.RemoveAsync(id);
      return NoContent();
    }
  }
}
