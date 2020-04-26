namespace PskOnline.Methods.Hrv.Test.Processing
{
  using Newtonsoft.Json.Linq;
  using PskOnline.Components.Util;
  using PskOnline.Methods.Hrv.ObjectModel;
  using PskOnline.Methods.Hrv.Processing.Logic;
  using PskOnline.Methods.Hrv.Processing.Settings;
  using System;
  using System.IO;

  public static class HrvFileProcessingHelper
  {
    public static HrvResults ProcessFileWithSettings(string filename, ProcessingSettings settings)
    {
      // Open file and load JObject from the file

      var content = File.ReadAllText(filename);
      var json = JObject.Parse(content);

      TestDataJsonFormat_0_1.CheckSignature(json);

      var methodId = TestDataJsonFormat_0_1.GetMethodId(json);
      if (methodId != HrvMethodId.MethodId)
      {
        throw new NotSupportedException($"Method with id '{methodId}' is not supported!");
      }

      // extract the method-specific json
      var testDataJson = TestDataJsonFormat_0_1.GetTestData(json);

      var hrvRawData = TestDataJsonFormat_0_1.GetTestData<HrvRawData>(json);

      var dp = new HrvDataProcessor_Pro();
      if (settings != null)
      {
        dp.Set(settings);
      }
      return (HrvResults)dp.ProcessData(hrvRawData);
    }

    public static HrvResults ProcessFile(string filename)
    {
      return ProcessFileWithSettings(filename, null);
    }

  }
}
