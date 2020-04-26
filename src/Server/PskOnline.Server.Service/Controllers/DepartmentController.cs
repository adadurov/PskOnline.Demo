namespace PskOnline.Server.Service.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;

  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Logging;

  using AutoMapper;

  using PskOnline.Server.Service.ViewModels;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.DAL.OrgStructure.Interfaces;
  using PskOnline.Server.Shared.Service;
  using PskOnline.Server.Authority.API.Constants;
  using PskOnline.Server.Authority.API;
  using PskOnline.Server.Authority.API.Dto;
  using PskOnline.Server.Service.Helpers;

  [Authorize]
  [Route("api/[controller]")]
  public class DepartmentController : BaseController
  {
    private readonly ITenantIdProvider _tenantIdProvider;
    private readonly IService<Department> _departmentService;
    private readonly IEmployeeService _employeeService;
    private readonly IWorkplaceCredentialsService _workplaceCredentialsService;
    private readonly ILogger _logger;

    public DepartmentController(
      IService<Department> departmentService,
      IEmployeeService employeeService,
      IWorkplaceCredentialsService workplaceCredentialsService,
      ITenantIdProvider tenantIdProvider,
      ILogger<DepartmentController> logger)
    {
      _tenantIdProvider = tenantIdProvider;
      _departmentService = departmentService;
      _employeeService = employeeService;
      _workplaceCredentialsService = workplaceCredentialsService;
      _logger = logger;
    }

    /// <summary>
    /// Retrieve all departments
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(typeof(IEnumerable<DepartmentDto>))]
    public async Task<IActionResult> GetAll()
    {
      var allDepartments = await _departmentService.GetAllAsync(null, null);
      return Ok(Mapper.Map<IEnumerable<DepartmentDto>>(allDepartments));
    }

    /// <summary>
    /// Retrieve information about a department by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:Guid}", Name = nameof(GetDepartment))]
    [Produces(typeof(DepartmentDto))]
    public async Task<IActionResult> GetDepartment(Guid id)
    {
      var department = await _departmentService.GetAsync(id);
      return Ok(Mapper.Map<DepartmentDto>(department));
    }

    /// <summary>
    /// Create an OAuth application for the specified department
    /// </summary>
    /// <param name="departmentId"></param>
    /// <param name="request"></param>
    /// <returns>Client credentials for the newly created department workplace</returns>
    /// <remarks>
    /// Each department may have multiple 'operator' or 'auditor' workplaces.
    /// Valid scopes: 
    ///   psk_dept_operator_bench -- for operator workplace
    ///   psk_dept_audit_bench -- for auditor workplace 
    /// </remarks>
    /// <response code="200">If the workplace was created successfully</response>
    /// <response code="400">If the request is not valid (e.g. an invalid scope is specified)</response>
    /// <response code="403">If the current user doesn't have permission to create workplaces in the department</response>
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [HttpPost("{departmentId:Guid}/workplace")]
    [Produces(typeof(WorkplaceCredentialsDto))]
    public async Task<IActionResult> CreateWorkplaceCredentials([FromRoute]Guid departmentId, [FromBody] WorkplaceCredentialsRequestDto request)
    {
      var areScopesOk = WorkplaceScopesValidator.CheckRequestedDepartmentWorkplaceScopes(request);
      if( ! areScopesOk.Item1 )
      {
        return BadRequest(areScopesOk.Item2);
      }

      // get the department in order to extract org structure attributes
      var department = await _departmentService.GetAsync(departmentId);

      // TODO: add authorization check here

      // build workplace descriptor
      var descriptor = new WorkplaceDescriptorDto
      {
        Scopes = request.Scopes.Split(' '),
        BranchOfficeId = department.BranchOfficeId.ToString(),
        DepartmentId = departmentId.ToString(),
        TenantId = department.TenantId.ToString(),
        WorkplaceType = "dwp",
        DisplayName = $"Department workplace ({department.Name})"
      };

      var credentials = await _workplaceCredentialsService.CreateWorkplaceAsync(descriptor);
      
      return Ok(credentials);
    }

    /// <summary>
    /// Retrieve information about workplaces in an existing department
    /// </summary>
    /// <returns></returns>
    [HttpGet("{departmentId:Guid}/workplace")]
    [Produces(typeof(IEnumerable<WorkplaceDto>))]
    public async Task<IActionResult> GetDepartmentWorkplaces(Guid departmentId)
    {
        var workplaces = await _workplaceCredentialsService.GetForDepartmentAsync(departmentId);
        return Ok(workplaces);
    }

    /// <summary>
    /// Retrieve all employees in the department
    /// </summary>
    /// <param name="departmentId"></param>
    /// <returns>An array of employees that belong to the specified department</returns>
    /// <response code="200">Success with the array of employees returned in the response body</response>
    /// <response code="403">If the current user doesn't have permission to list employees in the department</response>
    /// <response code="404">If the specified department is not found</response>
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [HttpGet("{departmentId:Guid}/employees")]
    [Produces(typeof(EmployeeDto[]))]
    public async Task<IActionResult> GetDepartmentEmployees(Guid departmentId)
    {
      // this will check access to the department
      var department = await _departmentService.GetAsync(departmentId);

      var employees = _employeeService.GetEmployeesInDepartment(department);
      return Ok(Mapper.Map<EmployeeDto[]>(employees));
    }

    /// <summary>
    /// Create a new department
    /// </summary>
    /// <param name="newDepartmentDto"></param>
    /// <returns>ID of the newly created department</returns>
    /// <response code="201">Upon success, response body contains JSON with the ID of the new department</response>
    /// <response code="400">If the request is not valid (e.g. department refers to an invalid branch office)</response>
    /// <response code="403">If the current user doesn't have permission to create departments</response>
    [HttpPost]
    [Produces(typeof(CreatedWithGuidDto))]
    public async Task<IActionResult> Post([FromBody]DepartmentDto newDepartmentDto)
    {
      if (newDepartmentDto == null)
      {
        return InvalidRequestBodyJson(nameof(DepartmentDto));
      }
      if (! ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      var newDepartment = Mapper.Map<Department>(newDepartmentDto);
      newDepartment.TenantId = _tenantIdProvider.GetTenantId();
      await _departmentService.AddAsync(newDepartment);

      var depCreatedDto = new CreatedWithGuidDto { Id = newDepartment.Id };
      _logger.LogInformation($"Successfully created department '{newDepartment.Name}' (id={newDepartment.Id}).");
      return Created(nameof(GetDepartment), depCreatedDto);
    }

    /// <summary>
    /// Update information about a department by ID
    /// </summary>
    /// <param name="id"></param>
    /// <param name="valueDto"></param>
    /// <returns></returns>
    [HttpPut("{id:Guid}")]
    public async Task<IActionResult> Put(Guid id, [FromBody]DepartmentDto valueDto)
    {
      if (valueDto == null)
      {
        return InvalidRequestBodyJson(nameof(DepartmentDto));
      }

      var updatedValue = await _departmentService.GetAsync(id);
      Mapper.Map(valueDto, updatedValue, typeof(DepartmentDto), typeof(Department));
      await _departmentService.UpdateAsync(updatedValue);
      return NoContent();
    }

    /// <summary>
    /// Delete a department by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id:Guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
//      await _departmentTokenService.RevokeAllTokens();
      await _departmentService.RemoveAsync(id);
      _logger.LogInformation($"Successfully deleted department {id}");
      return NoContent();
    }
  }
}
