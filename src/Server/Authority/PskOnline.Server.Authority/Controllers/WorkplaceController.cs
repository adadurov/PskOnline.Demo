namespace PskOnline.Server.Authority.Controllers
{
  using AutoMapper;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;
  using OpenIddict.Abstractions;
  using PskOnline.Server.Authority.API;
  using PskOnline.Server.Authority.API.Dto;
  using PskOnline.Server.Authority.ObjectModel;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;

  [Authorize]
  [Route("api/[controller]")]
  public class WorkplaceController : BaseController
  {
    private readonly IOpenIddictApplicationManager _oidcAppManager;
    private readonly IWorkplaceCredentialsService _workplaceCredentialsService;
    private readonly IMapper _mapper;

    public WorkplaceController(
      IOpenIddictApplicationManager oidcAppManager,
      IWorkplaceCredentialsService workplaceCredentialsService,
      AutoMapperConfig autoMapperConfig)
    {
      _oidcAppManager = oidcAppManager;
      _workplaceCredentialsService = workplaceCredentialsService;
      _mapper = autoMapperConfig.CreateMapper();
    }

    /// <summary>
    /// Delete a workplace by ID. Not implemented for now.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{clientId}")]
    public async Task<IActionResult> DeleteWorkplace(string clientId)
    {
      return NotFound(new { error_description = "Not implemented" });

      await _workplaceCredentialsService.DeleteAsync(clientId);
      return NoContent();
    }

    /// <summary>
    /// Generate a new secret for a workplace
    /// </summary>
    /// <returns></returns>
    [HttpPost("{clientId}/new-secret")]
    [Produces(typeof(WorkplaceCredentialsDto))]
    public async Task<IActionResult> UpdateSecret(string clientId)
    {
      var newSecret = await _workplaceCredentialsService.UpdateWorkplaceSecretAsync(clientId);
      return Ok(new WorkplaceCredentialsDto
      {
        ClientId = clientId,
        ClientSecret = newSecret
      });
    }
  }
}
