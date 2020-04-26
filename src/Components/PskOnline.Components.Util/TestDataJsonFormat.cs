namespace PskOnline.Components.Util
{
  using Newtonsoft.Json.Linq;
  using System;

  public static class TestDataJsonFormat_0_1
  {
    public static string Signature => "PskOnline-File-Based/0.1.0";

    public static void CheckSignature(JObject jsonData)
    {
      var signature = (string)jsonData["formatVersion"];
      if (signature != Signature)
      {
        throw new InvalidOperationException("Check version and format of the test data!");
      }
    }

    public static string GetMethodId(JObject jsonData)
    {
      return (string)jsonData["testData"]["TestInfo"]["MethodId"];
    }

    /// <summary>
    /// Returns a JObject containing the test data (expected to represent
    /// an object of a class derived from TestRawData class)
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static JObject GetTestData(JObject json)
    {
      return (JObject)json["testData"];
    }

    public static T GetTestData<T>(JObject json)
    {
      return json["testData"].ToObject<T>();
    }

  }
}
