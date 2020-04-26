namespace PskOnline.Client.Api
{
  using System;
  using System.Linq;
  using System.Net;
  using System.Net.Http;
  using System.Net.Mime;
  using System.Threading.Tasks;

  using JetBrains.Annotations;

  internal static class ExceptionHelper
  {
    internal static async Task<Exception> CreateExceptionAsync([NotNull] HttpResponseMessage response)
    {
      if (response == null)
      {
        throw new ArgumentNullException(nameof(response));
      }

      if (response.StatusCode == HttpStatusCode.Found)
      {
        return CreateResourceAlreadyExistsException(response);
      }
      var message = await GetExceptionMessage(response);

      if (response.StatusCode == HttpStatusCode.NotFound)
      {
        return new ItemNotFoundException(message);
      }
      if (response.StatusCode == HttpStatusCode.Forbidden)
      {
        return new UnauthorizedAccessException(message);
      }
      if (response.StatusCode == HttpStatusCode.Unauthorized)
      {
        return new UnauthorizedAccessException(message);
      }
      if (response.StatusCode == HttpStatusCode.BadRequest)
      {
        return new BadRequestException(message);
      }
      if (response.StatusCode == HttpStatusCode.Conflict)
      {
        return new ConflictException(message);
      }
      return new Exception(message);
    }

    private static Exception CreateResourceAlreadyExistsException(HttpResponseMessage response)
    {
      return new ItemAlreadyExistsException(response);
    }

    private static async Task<string> GetExceptionMessage(HttpResponseMessage response)
    {
      string nl = Environment.NewLine;

      string errorMessage = "";
      if (response.Content != null)
      {
        try
        {
          if (response.Content.IsJsonContent())
          {
            var errorDto = await response.Content.ReadAsJsonAsync<ApiErrorDto>();
            errorMessage = errorDto.Error;
          }
        }
        catch
        {
        }
      }
      if (string.IsNullOrEmpty(errorMessage))
      {
        errorMessage = await response.Content.ReadAsStringAsync();
      }

      return 
        $"RequestUri: {response.RequestMessage.RequestUri}{nl}" +
        $"HttpStatusCode: {response.StatusCode}{nl}" +
        $"Message: {errorMessage}";
    }
  }
}
