namespace PskOnline.Methods.Svmr.Test
{
  using System;
  using System.IO;

  using Newtonsoft.Json.Linq;

  using PskOnline.Components.Util;
  using PskOnline.Methods.Svmr.ObjectModel;
  using PskOnline.Methods.Svmr.Processing;

  public static class SvmrFileProcessingHelper
  {
    public static SvmrResults ProcessFileWithSettings(string filename, ProcessingSettings settings)
    {
      // Open file and load JObject from the file
      var content = File.ReadAllText(filename);
      var json = JObject.Parse(content);

      TestDataJsonFormat_0_1.CheckSignature(json);

      var methodId = TestDataJsonFormat_0_1.GetMethodId(json);
      if (methodId != SvmrMethodId.MethodId)
      {
        throw new NotSupportedException($"Method with id '{methodId}' is not supported!");
      }

      // extract the method-specific json
      var testDataJson = TestDataJsonFormat_0_1.GetTestData(json);

      var hrvRawData = TestDataJsonFormat_0_1.GetTestData<SvmrRawData>(json);

      var dp = new SvmrDataProcessor();
      if (settings != null)
      {
        dp.Set(settings);
      }
      return (SvmrResults)dp.ProcessData(hrvRawData);
    }

    public static SvmrResults ProcessFile(string filename)
    {
      return ProcessFileWithSettings(filename, null);
    }
  }
}
