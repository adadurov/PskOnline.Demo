namespace PskOnline.Server.Service.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  using AutoMapper;

  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Logging;

  using PskOnline.Server.DAL.OrgStructure.Interfaces;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Service.ViewModels;
  using PskOnline.Server.Shared.Multitenancy;

  [Authorize]
  [Route("api/[controller]")]
  public class EmployeeController : BaseController
  {
    private readonly ITenantIdProvider _tenantIdProvider;
    private readonly IEmployeeService _employeeService;
    private readonly ILogger _logger;

    public EmployeeController(
      IEmployeeService employeeService,
      ITenantIdProvider tenantIdProvider,
      ILogger<EmployeeController> logger)
    {
      _tenantIdProvider = tenantIdProvider;
      _employeeService = employeeService;
      _logger = logger;
    }

    [HttpGet]
    [Produces(typeof(IEnumerable<EmployeeDto>))]
    public async Task<IActionResult> GetAll()
    {
      var allEmployees = await _employeeService.GetAllAsync(null, null);
      return Ok(Mapper.Map<IEnumerable<EmployeeDto>>(allEmployees));
    }

    [HttpGet("{id:Guid}", Name = nameof(GetEmployee))]
    [Produces(typeof(EmployeeDto))]
    public async Task<IActionResult> GetEmployee(Guid id)
    {
      var employee = await _employeeService.GetAsync(id);
      return Ok(Mapper.Map<EmployeeDto>(employee));
    }

    [HttpPost]
    [Produces(typeof(CreatedWithGuidDto))]
    public async Task<IActionResult> Post([FromBody]EmployeeDto value)
    {
      if (value == null)
      {
        return InvalidRequestBodyJson(nameof(EmployeeDto));
      }
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      var newEmployee = Mapper.Map<Employee>(value);
      newEmployee.TenantId = _tenantIdProvider.GetTenantId();
      await _employeeService.AddAsync(newEmployee);
      return Created(nameof(GetEmployee), new CreatedWithGuidDto { Id = newEmployee.Id });
    }

    [HttpPut("{id:Guid}")]
    public async Task<IActionResult> Put(Guid id, [FromBody]EmployeeDto employeeDto)
    {
      if (employeeDto == null)
      {
        return InvalidRequestBodyJson(nameof(EmployeeDto));
      }

      var updatedEmployee = await _employeeService.GetAsync(id);
      Mapper.Map(employeeDto, updatedEmployee, typeof(EmployeeDto), typeof(Employee));

      await _employeeService.UpdateAsync(updatedEmployee);
      return NoContent();
    }

    [HttpDelete("{id:Guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
      await _employeeService.RemoveAsync(id);
      return NoContent();
    }
  }
}
