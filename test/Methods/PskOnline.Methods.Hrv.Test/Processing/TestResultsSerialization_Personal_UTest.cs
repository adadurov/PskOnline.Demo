namespace PskOnline.Methods.Hrv.Test.Processing
{
  using System;
  using System.IO;

  using Newtonsoft.Json.Linq;
  using NUnit.Framework;

  using PskOnline.Components.Log;
  using PskOnline.Components.Util;

  [TestFixture]
  public class TestResultsSerialization_UTest
  {
    log4net.ILog log = log4net.LogManager.GetLogger(typeof(TestResultsSerialization_UTest));

    [SetUp]
    public void SetUp()
    {
      LogHelper.ConfigureConsoleLogger();
    }

    [TearDown]
    public void TearDown()
    {
      LogHelper.ShutdownLogSystem();
    }

    [Test]
    public void TestSerializationAndDeserialization()
    {
      string data_file_name = FileHelpers.GetPathFromExecutingAssembly(
        Path.Combine("unit_test_data", TestUtil.TestFileNames[0]));

      var result = HrvFileProcessingHelper.ProcessFile(data_file_name);

      var resultJson = JObject.FromObject(result);

      var resultString = resultJson.ToString();
//      log.Info(resultString);

      var parsedObject = JObject.Parse(resultString);
//      Console.WriteLine(parsedObject.ToString());
    }
  }	
}
