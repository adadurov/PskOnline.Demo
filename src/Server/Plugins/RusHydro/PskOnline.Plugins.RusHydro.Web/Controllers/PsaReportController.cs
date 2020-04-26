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
  [Route("api/plugins/rushydro-psa/PsaReport")]
  public class PsaReportController : Controller
  {
    private readonly IDepartmentPsaReportService _psaReportService;
    private readonly IMapper _mapper;

    public PsaReportController(IDepartmentPsaReportService psaReportService)
    {
      _psaReportService = psaReportService;
      _mapper = AutoMapperConfig.Instance.CreateMapper();
    }

    [HttpGet("department/{departmentId:Guid}/current")]
    [Produces(typeof(PsaReportDto))]
    public async Task<IActionResult> GetPsaReport(Guid departmentId)
    {
      // проверяем права доступа
      // BUG: no access check, everything is available to everyone

      // показать сводку по текущей смене
      var report = await _psaReportService.GetCurrentShiftReportAsync(departmentId);
      var dto = _mapper.Map<PsaReportDto>(report);
      return Ok(dto);
    }
  }
}
