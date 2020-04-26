namespace PskOnline.Server.Service.Middleware
{
  using System;
  using System.Net;
  using System.Net.Mime;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Http;
  using Microsoft.Extensions.Logging;
  using PskOnline.Server.Service.ViewModels;
  using PskOnline.Server.Shared.Exceptions;

  public class ExceptionToHttpStatusConverter
  {
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionToHttpStatusConverter> _logger;

    public ExceptionToHttpStatusConverter(
      RequestDelegate next, ILoggerFactory loggerFactory)
    {
      _next = next ?? throw new ArgumentNullException(nameof(next));
      _logger = loggerFactory?.CreateLogger<ExceptionToHttpStatusConverter>() ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    public async Task Invoke(HttpContext httpContext)
    {

      try
      {
        await _next(httpContext);
      }
      catch (Exception ex)
      {
        if (httpContext.Response.HasStarted)
        {
          _logger.LogWarning("The response has already started, the http status code middleware will not be executed.");
          throw;
        }
        httpContext.Response.Clear();
        if (ex is UnauthorizedAccessException)
        {
          httpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        }
        else if (ex is ItemNotFoundException)
        {
          httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
        }
        else if (ex is ItemAlreadyExistsException)
        {
          httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        }
        else if (ex is BadRequestException)
        {
          httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        }
        else if (ex is ConflictException)
        {
          httpContext.Response.StatusCode = (int)HttpStatusCode.Conflict;
        }
        else
        {
          httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
          _logger.LogError(ex, "Caught exception");
        }
        httpContext.Response.ContentType = MediaTypeNames.Application.Json;
        await httpContext.Response.WriteAsJsonAsync(new ApiErrorDto { Error = ex.Message });
      }
    }
  }

}
