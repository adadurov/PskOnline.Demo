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
  using PskOnline.Server.Shared.Service;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Authority.API.Dto;
  using PskOnline.Server.Service.Helpers;
  using PskOnline.Server.Authority.API;

  [Authorize]
  [Route("api/[controller]")]
  public class BranchOfficeController : BaseController
  {
    private readonly ILogger _logger;
    private readonly IService<BranchOffice> _branchOfficeService;
    private readonly ITenantIdProvider _tenantIdProvider;
    private readonly ITimeService _timeService;
    private readonly IWorkplaceCredentialsService _workplaceCredentialsService;

    public BranchOfficeController(
      IService<BranchOffice> branchOfficeService,
      ITimeService timeService,
      IWorkplaceCredentialsService workplaceCredentialsService,
      ITenantIdProvider tenantIdProvider,
      ILogger<BranchOfficeController> logger)
    {
      _timeService = timeService;
      _tenantIdProvider = tenantIdProvider;
      _branchOfficeService = branchOfficeService;
      _workplaceCredentialsService = workplaceCredentialsService;
      _logger = logger;
    }

    [HttpGet]
    [Produces(typeof(IEnumerable<BranchOfficeDto>))]
    public async Task<IActionResult> GetAll()
    {
      var allBranchOffices = await _branchOfficeService.GetAllAsync(null, null);
      var allViewModels = Mapper.Map<IEnumerable<BranchOfficeDto>>(allBranchOffices);
      return Ok(allViewModels);
    }

    [HttpGet("{id:Guid}", Name = nameof(GetBranchOffice))]
    [Produces(typeof(BranchOfficeDto))]
    public async Task<IActionResult> GetBranchOffice(Guid id)
    {
      var branchOffice = await _branchOfficeService.GetAsync(id);
      return Ok(Mapper.Map<BranchOfficeDto>(branchOffice));
    }

    [HttpPost]
    [Produces(typeof(CreatedWithGuidDto))]
    public async Task<IActionResult> Post([FromBody]BranchOfficeDto value)
    {
      if (value == null)
      {
        return InvalidRequestBodyJson(nameof(BranchOfficeDto));
      }
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      if ( ! CheckTimeZoneCode(value.TimeZoneId) )
      {
        AddErrors(new [] { $"Unknown time zone code specified: {value.TimeZoneId}" });
      }

      var newBranchOffice = Mapper.Map<BranchOffice>(value);
      newBranchOffice.TenantId = _tenantIdProvider.GetTenantId();
      await _branchOfficeService.AddAsync(newBranchOffice);
      return Created(nameof(GetBranchOffice), new CreatedWithGuidDto { Id = newBranchOffice.Id });
    }

    private bool CheckTimeZoneCode(string timeZoneId)
    {
      if( timeZoneId == null )
      {
        return false;
      }
      return _timeService.CheckTimeZoneId(timeZoneId);
    }

    [HttpPut("{id:Guid}")]
    public async Task<IActionResult> Put(Guid id, [FromBody]BranchOfficeDto value)
    {
      if (!CheckTimeZoneCode(value.TimeZoneId))
      {
        AddErrors(new[] { $"Unknown time zone code specified: {value.TimeZoneId}" });
        return BadRequest(ModelState);
      }

      var updatedBranchOffice = await _branchOfficeService.GetAsync(id);
      Mapper.Map(value, updatedBranchOffice, typeof(BranchOfficeDto), typeof(BranchOffice));

      await _branchOfficeService.UpdateAsync(updatedBranchOffice);
      return NoContent();
    }

    [HttpDelete("{id:Guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
      await _branchOfficeService.RemoveAsync(id);
      return NoContent();
    }

    /// <summary>
    /// Create an OAuth application for the specified branch office
    /// </summary>
    /// <param name="branchOfficeId"></param>
    /// <param name="request"></param>
    /// <returns>Client credentials for the newly created branch office workplace</returns>
    /// <remarks>
    /// Each branch may have multiple 'auditor' workplaces.
    /// Valid scopes: 
    ///   psk_dept_operator_bench -- for operator workplace
    ///   psk_dept_audit_bench -- for auditor workplace 
    /// </remarks>
    /// <response code="200">If the workplace was created successfully</response>
    /// <response code="400">If the request is not valid (e.g. an invalid scope is specified)</response>
    /// <response code="403">If the current user doesn't have permission to create workplaces in the branch office</response>
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [HttpPost("{branchOfficeId:Guid}/workplace")]
    [Produces(typeof(WorkplaceCredentialsDto))]
    public async Task<IActionResult> CreateWorkplaceCredentials([FromRoute]Guid branchOfficeId, [FromBody] WorkplaceCredentialsRequestDto request)
    {
      var areScopesOk = WorkplaceScopesValidator.CheckRequestedBranchWorkplaceScopes(request);
      if (!areScopesOk.Item1)
      {
        return BadRequest(areScopesOk.Item2);
      }

      var branchOffice = await _branchOfficeService.GetAsync(branchOfficeId);
      // TODO: add authorization check here

      // build workplace descriptor
      var descriptor = new WorkplaceDescriptorDto
      {
        Scopes = request.Scopes.Split(' '),
        BranchOfficeId = branchOfficeId.ToString(),
        DepartmentId = null,
        TenantId = branchOffice.TenantId.ToString(),
        WorkplaceType = "bwp",
        DisplayName = $"Branch workplace ({branchOffice.Name})"
      };

      var credentials = await _workplaceCredentialsService.CreateWorkplaceAsync(descriptor);

      return Ok(credentials);
    }

  }
}
