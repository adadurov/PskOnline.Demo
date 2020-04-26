namespace PskOnline.Client.Api.OpenId
{
  using System;
  using System.Collections.Generic;
  using System.Net.Http;
  using System.Threading.Tasks;

  public static class OpenIdUtils
  {
    public static async Task<TokenInfoResponse> ExecuteTokenRequest(HttpClient httpClient, Dictionary<string, string> values)
    {
      var request = new HttpRequestMessage
      {
        Method = HttpMethod.Post,
        RequestUri = new Uri(httpClient.BaseAddress.AbsoluteUri + "connect/token"),
        Content = new FormUrlEncodedContent(values)
      };

      var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
      if (!response.IsSuccessStatusCode)
      {
        var ex = await ExceptionHelper.CreateExceptionAsync(response);
        throw new AuthenticationException(ex.Message, ex);
      }

      try
      {
        var payload = await response.Content.ReadAsJsonAsync<TokenInfoResponse>();
        return payload;
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException("Could not parse a successful authentication response.", ex);
      }
    }
  }
}
