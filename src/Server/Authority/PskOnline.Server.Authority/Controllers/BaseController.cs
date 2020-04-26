namespace PskOnline.Server.Authority.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Net;

  using Swashbuckle.AspNetCore.SwaggerGen;

  using Microsoft.AspNetCore.Mvc;
  using PskOnline.Server.Shared.Multitenancy;

  [SwaggerResponse((int)HttpStatusCode.Forbidden, Description = "User doesn't have enough privileges for this operation or invalid id has been requested.")]
  [SwaggerResponse((int)HttpStatusCode.Unauthorized, Description = "Authentication required.")]
  [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "The request model is invalid.")]
  [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Internal server error.")]
  public class BaseController : Controller
  {
    private bool _userTenantIdRetrieved = false;
    private Guid? _userTenantId;

    protected Guid? CurrentUserTenantId
    {
      get
      {
        if( ! _userTenantIdRetrieved )
        { 
          _userTenantId = User.GetUserTenant();
          _userTenantIdRetrieved = true;
        }
        return _userTenantId;
      }
    }

    protected void AddErrors(IEnumerable<string> errors)
    {
      foreach (var error in errors)
      {
        ModelState.AddModelError("Error", error);
      }
    }

    protected BadRequestObjectResult InvalidRequestBodyJson(string expectedDtoType)
    {
      ModelState.AddModelError(
        "Error", 
        "Request body JSON doesn't contain a valid " + expectedDtoType);
      return BadRequest(ModelState);
    }
  }
}
