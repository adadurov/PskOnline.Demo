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
  using PskOnline.Server.Shared.Service;
  using PskOnline.Server.Shared.Multitenancy;
  using System.Threading.Tasks;

  [Authorize]
  [Route("api/[controller]")]
  public class PositionController : BaseController
  {
    private readonly IService<Position> _positionService;
    private readonly ITenantIdProvider _tenantIdProvider;
    private readonly ILogger _logger;

    public PositionController(
      IService<Position> positionService,
      ITenantIdProvider tenantIdProvider,
      ILogger<PositionController> logger)
    {
      _tenantIdProvider = tenantIdProvider;
      _positionService = positionService;
      _logger = logger;
    }

    [HttpGet]
    [Produces(typeof(IEnumerable<PositionDto>))]
    public async Task<IActionResult> GetAll()
    {
      var allPositions = await _positionService.GetAllAsync(null, null);
      return Ok(Mapper.Map<IEnumerable<PositionDto>>(allPositions));
    }

    [HttpGet("{id:Guid}", Name = nameof(GetPosition))]
    [Produces(typeof(PositionDto))]
    public async Task<IActionResult> GetPosition(Guid id)
    {
      var position = await _positionService.GetAsync(id);
      return Ok(Mapper.Map<PositionDto>(position));
    }

    [HttpPost]
    [Produces(typeof(CreatedWithGuidDto))]
    public async Task<IActionResult> Post([FromBody]PositionDto value)
    {
      if (value == null)
      {
        return InvalidRequestBodyJson(nameof(PositionDto));
      }
      if (! ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      var newPosition = Mapper.Map<Position>(value);
      newPosition.TenantId = _tenantIdProvider.GetTenantId();
      await _positionService.AddAsync(newPosition);
      return Created(nameof(GetPosition), new CreatedWithGuidDto { Id = newPosition.Id });
    }

    [HttpPut("{id:Guid}")]
    public async Task<IActionResult> Put(Guid id, [FromBody]PositionDto positionDto)
    {
      if (positionDto == null)
      {
        return InvalidRequestBodyJson(nameof(PositionDto));
      }
      var position = await _positionService.GetAsync(id);
      Mapper.Map(positionDto, position, typeof(PositionDto), typeof(Position));

      await _positionService.UpdateAsync(position);
      return NoContent();
    }

    [HttpDelete("{id:Guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
      await _positionService.RemoveAsync(id);
      return NoContent();
    }
  }
}
