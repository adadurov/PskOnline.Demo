namespace PskOnline.Server.Plugins.Rushydro.Test
{
  using Newtonsoft.Json.Linq;
  using PskOnline.Components.Util;
  using PskOnline.Methods.ObjectModel;
  using PskOnline.Methods.ObjectModel.Test;
  using System.Collections.Generic;
  using System.IO;

  public static class TestDataProvider
  {
    public static IDictionary<TestInfo, JObject> GetTestData(int inspectionId)
    {
      _log.InfoFormat("Getting test data for inspection {0}", inspectionId);
      var containers = new Dictionary<TestInfo, JObject>(2);

      int i = 0;
      foreach (string fileName in _testDataFileNames)
      {
        i++;
        if (fileName.Contains(inspectionId.ToString()))
        {
          var fullPath = GetTestDataPath(fileName);

          var content = File.ReadAllText(fullPath);

          // parse json & check signature
          var json = JObject.Parse(content);
          TestDataJsonFormat_0_1.CheckSignature(json);

          var methodId = TestDataJsonFormat_0_1.GetMethodId(json);

          // extract the method-specific json
          var testDataJson = TestDataJsonFormat_0_1.GetTestData(json);

          var basicTestData = TestDataJsonFormat_0_1.GetTestData<TestRawData>(json);

          // return the data (base data + the json data that the specific data can be extracted from)
          containers.Add(basicTestData.TestInfo, testDataJson);
        }
      }
      return containers;
    }

    private static string GetTestDataPath(string testDataFileName)
    {
      return FileHelpers.GetPathFromExecutingAssembly(
          Path.Combine("test_data", testDataFileName)
        );
    }

    private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(TestDataProvider));

    private static readonly string[] _testDataFileNames = {
      "PskOnline.Methods.Hrv_995.psk.json",
      "PskOnline.Methods.Hrv_996.psk.json",
      "PskOnline.Methods.Hrv_998.psk.json",
      "PskOnline.Methods.Hrv_999.psk.json",
      "PskOnline.Methods.Hrv_1000.psk.json",
      "PskOnline.Methods.SVMR_995.psk.json",
      "PskOnline.Methods.SVMR_996.psk.json",
      "PskOnline.Methods.SVMR_998.psk.json",
      "PskOnline.Methods.SVMR_999.psk.json",
      "PskOnline.Methods.SVMR_1000.psk.json"
    };

  }
}
