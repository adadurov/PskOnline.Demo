namespace PskOnline.Client.Api
{
  using Newtonsoft.Json;
  using System;
  using System.Collections.Generic;
  using System.Net;
  using System.Net.Http;
  using System.Net.Http.Headers;
  using System.Security;
  using System.Threading.Tasks;

  public static class HttpClientJsonExtensions
  {
    [Obsolete("Use System.Net.Http.HttpClientExtensions from Microsoft.AspNet.WebApi.Client NuGet package. Have fun :)")]
    public static Task<HttpResponseMessage> PostAsJsonAsync__<T>(
        this HttpClient httpClient, string url, T data)
    {
      var dataAsString = JsonConvert.SerializeObject(data);
      var content = new StringContent(dataAsString);
      // MediaTypeNames.Application.Json (from system.net.mail)
      // is not available in .net standard
      content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
      return httpClient.PostAsync(url, content);
    }

    [Obsolete("Use System.Net.Http.HttpClientExtensions from Microsoft.AspNet.WebApi.Client NuGet package. Have fun :)")]
    public static Task<HttpResponseMessage> PutAsJsonAsync__<T>(
        this HttpClient httpClient, string url, T data)
    {
      var dataAsString = JsonConvert.SerializeObject(data);
      var content = new StringContent(dataAsString);
      // MediaTypeNames.Application.Json (from system.net.mail)
      // is not available in .net standard
      content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
      return httpClient.PutAsync(url, content);
    }

    public static async Task<T> ReadAsJsonAsync<T>(this HttpContent content)
    {
      var dataAsString = await content.ReadAsStringAsync();
      return JsonConvert.DeserializeObject<T>(dataAsString);
    }

    public static bool IsJsonContent(this HttpContent content)
    {
      if( content == null )
      {
        return false;
      }
      return content.Headers != null &&
             content.Headers.ContentType != null &&
             content.Headers.ContentType.MediaType != null &&
             content.Headers.ContentType.MediaType.Contains("application/json");
    }

  }
}
