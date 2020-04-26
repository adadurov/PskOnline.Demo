namespace PskOnline.Server.Service.Middleware
{
  using System.Net.Http;
  using System.Net.Http.Headers;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Http;
  using Newtonsoft.Json;

  public static class HttpResponseExtension
  {
    public static async Task WriteAsJsonAsync<T>(
        this HttpResponse httpResponse, T data)
    {
      var dataAsString = JsonConvert.SerializeObject(data);
      httpResponse.ContentType = "application/json";
      await httpResponse.WriteAsync(dataAsString);
    }
  }
}
