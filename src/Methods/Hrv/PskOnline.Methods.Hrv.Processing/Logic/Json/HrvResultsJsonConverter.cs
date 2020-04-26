namespace PskOnline.Methods.Hrv.Processing.Logic.Json
{
  using System;
  using System.Collections.Generic;
  using System.Text;
  using Newtonsoft.Json;

  public class HrvResultsJsonConverter : JsonConverter
  {
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      throw new NotImplementedException();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      throw new NotImplementedException();
    }

    public override bool CanConvert(Type objectType)
    {
      throw new NotImplementedException();
    }
  }
}
