namespace PskOnline.Server.Plugins.RusHydro.Web.Controllers
{
  using System;
  using System.Threading.Tasks;

  using Microsoft.AspNetCore.Mvc;
  using Microsoft.AspNetCore.Authorization;
  using PskOnline.Server.Plugins.RusHydro.Web.Dto;
  using PskOnline.Server.Plugins.RusHydro.DAL;
  using AutoMapper;

  [Authorize]
  [Route("api/plugins/rushydro-psa/summary")]
  public class PsaSummaryController : Controller
  {
    private readonly IPsaSummaryService _psaSummaryService;
    private readonly IMapper _mapper;

    public PsaSummaryController(IPsaSummaryService psaSummaryService)
    {
      _mapper = AutoMapperConfig.Instance.CreateMapper();

      _psaSummaryService = psaSummaryService;
    }

    [HttpGet("inspection/{inspectionId:Guid}")]
    [Produces(typeof(PsaSummaryDto))]
    public async Task<IActionResult> GetPsaSummary(Guid inspectionId)
    {
      // проверяем права доступа
      // BUG: no access check, everything is available to everyone

      // найти сводку, вернуть.
      var summary = await _psaSummaryService.GetSummaryForInspectionAsync(inspectionId);

      var dto = _mapper.Map<PsaSummaryDto>(summary);

      // показать сводку по текущей смене
      return Ok(dto);
    }
  }
}
