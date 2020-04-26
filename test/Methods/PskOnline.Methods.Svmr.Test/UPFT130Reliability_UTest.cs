namespace PskOnline.Methods.Svmr.Test
{
  using NUnit.Framework;
  using PskOnline.Components.Log;
  using PskOnline.Methods.Svmr.Processing;

  [TestFixture]
  public class UPFT130Reliability_UTest
  {
    log4net.ILog log = log4net.LogManager.GetLogger(typeof(UPFT130Reliability_UTest));

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
    public void Test_Empty()
    {
      Assert.AreEqual(0, UPFT130Reliability.FromReactionsArray(new double[] { }));
    }

    [Test]
    public void Test_Null()
    {
      Assert.AreEqual(0, UPFT130Reliability.FromReactionsArray(null));
    }

    [Test]
    public void Test_Bottom_Limit()
    {
      Assert.AreEqual(0, UPFT130Reliability.FromReactionsArray(new double[] {149.99} ) );
      Assert.AreEqual(100, UPFT130Reliability.FromReactionsArray(new double[] { 150 }));
      Assert.AreEqual(90, UPFT130Reliability.FromReactionsArray(new double[] { 200 }));
    }

    [Test]
    public void Test_Middle()
    {
      Assert.AreEqual(60, UPFT130Reliability.FromReactionsArray(new double[] { 270 }));
      Assert.AreEqual(50, UPFT130Reliability.FromReactionsArray(new double[] { 280 }));
      Assert.AreEqual(50, UPFT130Reliability.FromReactionsArray(new double[] { 299.99}));
      Assert.AreEqual(40, UPFT130Reliability.FromReactionsArray(new double[] { 300 }));
    }

    [Test]
    public void Test_Top_Limit()
    {
      Assert.AreEqual(5, UPFT130Reliability.FromReactionsArray(new double[] { 379 }));
      Assert.AreEqual(0, UPFT130Reliability.FromReactionsArray(new double[] { 380 }));
      Assert.AreEqual(0, UPFT130Reliability.FromReactionsArray(new double[] { 381 }));
    }
  }
}
