namespace PskOnline.Server.Service.Controllers
{
  using System;
  using System.Threading.Tasks;

  using AutoMapper;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using PskOnline.Server.DAL.Inspections;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Service.ViewModels;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Shared.Service;

  [Authorize]
  [Route("api/[controller]")]
  public class TestController : BaseController
  {
    private readonly ITenantIdProvider _tenantIdProvider;
    private readonly ITestService _testService;

    public TestController(
      ITestService testService,
      ITenantIdProvider tenantIdProvider
      )
    {
      _tenantIdProvider = tenantIdProvider;
      _testService = testService;
    }

    [HttpGet]
    [Route("{id:Guid}", Name = nameof(GetTest))]
    [Produces(typeof(TestDto))]
    public async Task<IActionResult> GetTest(Guid Id)
    {
      var test = await _testService.GetAsync(Id);
      return Ok(Mapper.Map<TestDto>(test));
    }

    [HttpPost]
    [Produces(typeof(CreatedWithGuidDto))]
    public async Task<IActionResult> PostTest([FromBody]TestPostDto testDto)
    {
      if (null == testDto)
      {
        return InvalidRequestBodyJson(nameof(TestDto));
      }
      if ( ! ModelState.IsValid )
      {
        return BadRequest(ModelState);
      }

      var newTest = Mapper.Map<Test>(testDto);
      newTest.TenantId = _tenantIdProvider.GetTenantId();
      await _testService.AddAsync(newTest);
      return Created(nameof(GetTest), new CreatedWithGuidDto { Id = newTest.Id });
    }

    [HttpDelete]
    [Route("{id:Guid}")]
    public async Task<IActionResult> DeleteTest(Guid id)
    {
      await _testService.RemoveAsync(id);
      return NoContent();
    }

  }
}
