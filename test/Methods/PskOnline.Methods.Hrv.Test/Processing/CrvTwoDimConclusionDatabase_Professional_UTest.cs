namespace PskOnline.Methods.Hrv.Test.Processing
{
  using NUnit.Framework;
  using PskOnline.Components.Log;
  using PskOnline.Methods.Hrv.Processing.Logic;
  using PskOnline.Methods.Hrv.Processing.Logic.Pro;

  [TestFixture]
  public class CrvTwoDimConclusionDatabase_Professional_UTest
  {
    log4net.ILog log = log4net.LogManager.GetLogger(typeof(CrvTwoDimConclusionDatabase_Professional_UTest));

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

    static System.Globalization.CultureInfo ru = System.Globalization.CultureInfo.InvariantCulture;
    static System.Globalization.CultureInfo en = System.Globalization.CultureInfo.GetCultureInfo("en");
    
    [Test]
    public void Test_Professional_Conclusions()
    {
      TestTwoDimConclusionDb(new CrvTwoDimConclusionDatabase_Professional(), new System.Globalization.CultureInfo[] { ru });
    }

    private void TestTwoDimConclusionDb(CrvTwoDimConclusionDatabase crvTwoDimConclusionDatabase, System.Globalization.CultureInfo[] cultures)
    {
      foreach (System.Globalization.CultureInfo ci in cultures)
      {
        for (int row = 1; row < 6; ++row)
        {
          for (int col = 1; col < 6; ++col)
          {
            string conclusion = crvTwoDimConclusionDatabase.GetConclusion(row, col, ci, true);

            string empty_conclusion_message = string.Format("empty conclusion: database '{0}', culture '{1}', row {2}, col {3}", crvTwoDimConclusionDatabase.GetType().Name, ci.Name, row, col);

            Assert.IsNotNull(conclusion, empty_conclusion_message);
            Assert.AreNotEqual(string.Empty, conclusion, empty_conclusion_message);
          }
        }
      }
    }

  }

	
}
