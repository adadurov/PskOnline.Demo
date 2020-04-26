namespace PskOnline.Client.Api
{
  using System;
  using System.Net.Http;

  public class ItemAlreadyExistsException : Exception
  {
    public ItemAlreadyExistsException(HttpResponseMessage response)
    {
      Location = response.Headers.Location;

      if (Location != null)
      {
        var url = Location.OriginalString;
        var offset = url.LastIndexOf('/');

        if (Guid.TryParse(url.Substring(offset + 1, url.Length - offset - 1), out Guid id))
        {
          Id = id;
        }
      }
    }

    public Uri Location { get; set; }

    public Guid? Id { get; set; }
  }
}
