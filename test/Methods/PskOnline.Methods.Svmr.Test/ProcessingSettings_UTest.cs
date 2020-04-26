namespace PskOnline.Methods.Svmr.Test
{
  using System;

  using Newtonsoft.Json.Linq;
  using NUnit.Framework;

  using PskOnline.Components.Log;
  using PskOnline.Methods.Svmr.Processing;

  [TestFixture]
  public class ProcessingSettings_UTest
  {
    log4net.ILog log = log4net.LogManager.GetLogger(typeof(ProcessingSettings_UTest));

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
    public void Test_Serialization_And_Deserialization()
    {
      var procSettings = new ProcessingSettings();
      // serialize
      var json = JObject.FromObject(procSettings);
      var jsonString = json.ToString();
      Console.WriteLine(jsonString);
      // deserialize
      var deserialized = JObject.Parse(jsonString);
    }

    [Test]
    public void TestIdentityAndEquality()
    {
      var temp1 = new ProcessingSettings();
      temp1.Default();

      var temp2 = new ProcessingSettings();
      temp2.Default();

      ProcessingSettings temp3 = (ProcessingSettings)temp2.Clone();


      Assert.IsFalse(temp1.Equals(null));

      Assert.IsTrue(temp1.Equals(temp1));

      Assert.IsTrue(temp2.Equals(temp1));
      Assert.IsTrue(temp1.Equals(temp2));

      Assert.IsNotNull(temp3, "clone must return non-null reference!");
      Assert.AreNotSame(temp3, temp2, "clone must return new instance!");

      Assert.IsFalse(temp3 == temp2, "identy test failed");
      Assert.IsTrue(temp3 != temp2, "identy test failed");

      Assert.IsTrue(temp2.Equals(temp3));
      Assert.IsTrue(temp3.Equals(temp2));

      Assert.IsTrue(temp1.Equals(temp3));
      Assert.IsTrue(temp3.Equals(temp1));
    }
  }
}
