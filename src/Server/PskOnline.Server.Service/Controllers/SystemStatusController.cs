namespace PskOnline.Server.Service.Controllers
{
  using System;
  using System.Diagnostics;
  using System.Reflection;

  using Microsoft.AspNetCore.Mvc;
  using PskOnline.Server.Service.ViewModels;

  [Route("api/status")]
  public class StatusController : BaseController
  {
    public StatusController(
      )
    {
    }

    [HttpGet]
    [Produces(typeof(StatusDto))]
    public IActionResult GetStatus()
    {
      var version = Assembly.GetExecutingAssembly().GetName().Version;
      var uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
      return Ok(new StatusDto
      {
        BuildNumber = version.ToString(),
        Uptime = uptime.ToString(),
        IsStarted = true,
        ServerName = System.Net.Dns.GetHostName(),
        Errors = 0,
        AppState = "OK",
        SubSystems = new SubSystemStatusDto[] { }
      });
    }
  }
}
