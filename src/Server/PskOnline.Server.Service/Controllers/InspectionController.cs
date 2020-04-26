namespace PskOnline.Server.Service.Controllers
{
  using System.Collections.Generic;
  using System;
  using System.Threading;
  using System.Threading.Tasks;

  using AutoMapper;

  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Http;
  using Microsoft.AspNetCore.Mvc;

  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Shared.Contracts.Service;
  using PskOnline.Server.Service.ViewModels;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Shared.Exceptions;
  using System.Linq;

  [Authorize]
  [Route("api/[controller]")]
  public class InspectionController : BaseController
  {
    private readonly IInspectionService _inspectionService;
    private readonly ITenantIdProvider _tenantIdProvider;

    public InspectionController(
      IInspectionService inspectionService,
      ITenantIdProvider tenantIdProvider
      )
    {
      _tenantIdProvider = tenantIdProvider;
      _inspectionService = inspectionService;
    }

    [HttpPut]
    [Route("{id:Guid}/complete")]
    [Produces(typeof(IEnumerable<InspectionCompleteResponseDto>))]
    public async Task<IActionResult> PutInspectionComplete(
      Guid id, [FromBody]InspectionCompleteDto completionDto, CancellationToken cancellationToken)
    {
      if (completionDto == null)
      {
        return InvalidRequestBodyJson(nameof(InspectionCompleteDto));
      }

      var results = await _inspectionService
            .CompleteInspectionAsync(id, completionDto.FinishTime, cancellationToken);

      var descriptors = results.Select(r => Mapper.Map<PluginResultDescriptorDto>(r));

      return Ok(new InspectionCompleteResponseDto
      {
        PluginResults = descriptors.ToArray()
      });
    }

    /// <summary>
    /// Retrieve inspections uploaded after the specified timestamp
    /// </summary>
    /// <param name="timestamp"></param>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <returns>400 -- if take is > 100 </returns>
    [HttpGet]
    [Route("after")]
    [Produces(typeof(IEnumerable<InspectionDto>))]
    public async Task<IActionResult> GetAfter([FromQuery]DateTimeOffset? timestamp, [FromQuery]int? skip, [FromQuery]int? take)
    {
      var maxTake = 100;
      if (take.HasValue && take > maxTake)
      {
        return BadRequest(new ApiErrorDto
        {
          Error = $"You should not ask for more than {maxTake} items in a single request"
        });
      }
      return BadRequest("Not implemented!");
    }

    [HttpGet]
    [Produces(typeof(IEnumerable<InspectionDto>))]
    public async Task<IActionResult> GetAll([FromQuery]int? skip, [FromQuery]int? take)
    {
      var maxTake = 100;
      if( take.HasValue && take > maxTake )
      {
        return BadRequest(new ApiErrorDto {
          Error = $"You should not ask for more than {maxTake} items in a single request"
        });
      }

      var inspections = await _inspectionService.GetAllAsync(skip, take);
      var dto = Mapper.Map<InspectionDto[]>(inspections);

      return Ok(dto);
    }

    [HttpGet]
    [Route("{id:Guid}", Name = nameof(GetInspection))]
    [Produces(typeof(InspectionDto))]
    public async Task<IActionResult> GetInspection(Guid id)
    {
      var inspection = await _inspectionService.GetAsync(id);
      var dto = Mapper.Map<InspectionDto>(inspection);
      return Ok( dto );
    }

    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    /// <returns>405 MethodNotAllowed</returns>
    [HttpPut]
    [Route("{id:Guid}")]
    public IActionResult PutInspection(Guid id)
    {
      Response.Headers.Add("Allow", "GET, HEAD");
      return StatusCode(StatusCodes.Status405MethodNotAllowed);
    }

    [HttpPost]
    [Produces(typeof(CreatedWithGuidDto))]
    public async Task<IActionResult> PostInspection([FromBody]InspectionPostDto inspectionDto)
    {
      if (inspectionDto == null)
      {
        return InvalidRequestBodyJson(nameof(InspectionDto));
      }
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      var newInspection = Mapper.Map<Inspection>(inspectionDto);
      newInspection.TenantId = _tenantIdProvider.GetTenantId();
      try
      {
        var newId = await _inspectionService.BeginInspectionAsync(newInspection);
        return Created(nameof(GetInspection), new CreatedWithGuidDto { Id = newId });
      }
      catch( ItemAlreadyExistsException ex )
      {
        if (ex.Id.HasValue)
        {
          // should yield 'found' response status code
          return RedirectToAction(nameof(GetInspection), new CreatedWithGuidDto { Id = ex.Id.Value });
        }
        throw new Exception(
          $"Exception of type {nameof(ItemAlreadyExistsException)} should have Id defined"
          );
      }
    }

    [HttpDelete]
    [Route("{id:Guid}")]
    public async Task<IActionResult> DeleteInspection(Guid id)
    {
      await _inspectionService.RemoveAsync(id);
      return NoContent();
    }

  }
}
