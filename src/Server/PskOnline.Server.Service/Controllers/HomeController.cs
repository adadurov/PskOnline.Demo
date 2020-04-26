using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace PskOnline.Server.Service.Controllers
{
  public class HomeController : BaseController
  {
    public IActionResult Index()
    {
      return View();
    }

    public IActionResult Error()
    {
      ViewData["RequestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
      return View();
    }

    // GET api/values
    [Route("api/[controller]"), Authorize, HttpGet]
    public IEnumerable<string> Get()
    {
      return new string[] { "value1", "value2" };
    }
  }
}
