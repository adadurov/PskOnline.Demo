namespace PskOnline.Methods.Hrv.Test.Processing
{
  using NUnit.Framework;
  using PskOnline.Components.Log;
  using PskOnline.Methods.Hrv.Processing.Contracts;
  using PskOnline.Methods.Hrv.Processing.Logic;

  [TestFixture]
  public class StateMatrixClassifier_UTest
  {
    readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(StateMatrixClassifier_UTest));

    [SetUp]
    public void Setup()
    {
      LogHelper.ConfigureConsoleLogger();
    }

    [TearDown]
    public void TearDown()
    {
      LogHelper.ShutdownLogSystem();
    }

    [Test]
    public void TestSinglePointBounds()
    {
      Assert.AreEqual(3.0d, DefaultStateMatrix.GetMidPoint(new double[] { 3.0d }));
    }

    [Test]
    public void TestOddBounds()
    {
      Assert.AreEqual(2.0d, DefaultStateMatrix.GetMidPoint(new double[] { 1.0d, 2.0d, 3.0d }));
    }

    [Test]
    public void TestEvenBounds()
    {
      Assert.AreEqual(2.0d, DefaultStateMatrix.GetMidPoint(new double[] { 1.0d, 3.0d }));
    }

    [Test]
    public void TestStateMatrixFactory_Default()
    {
      IStateMatrixStateClassifier default_classifier =
          StateMatrixStateClassifierFactory.GetStateMatrixClassifier();
    }

    [Test]
    public void TestStateMatrixFactory_Parametrized()
    {
      IStateMatrixStateClassifier classifier =
          StateMatrixStateClassifierFactory.GetStateMatrixClassifier(78);
    }

    [Test]
    public void Test_0_BPM_Offset()
    {
      IStateMatrixStateClassifier default_classifier =
          StateMatrixStateClassifierFactory.GetStateMatrixClassifier();

      IStateMatrixStateClassifier another_classifier =
          StateMatrixStateClassifierFactory.GetStateMatrixClassifier(
              default_classifier.GetHeartRateMidPointForSettings()
          );

      Assert.AreEqual(
          default_classifier.GetHeartRateMidPointForSettings(),
          another_classifier.GetHeartRateMidPointForSettings());
    }

    [Test]
    public void Test_10_BPM_Offset()
    {
      IStateMatrixStateClassifier default_classifier =
          StateMatrixStateClassifierFactory.GetStateMatrixClassifier();

      IStateMatrixStateClassifier another_classifier =
          StateMatrixStateClassifierFactory.GetStateMatrixClassifier(
              default_classifier.GetHeartRateMidPointForSettings() + 1
          );

      Assert.AreEqual(
          default_classifier.GetHeartRateMidPointForSettings() + 1,
          another_classifier.GetHeartRateMidPointForSettings());
    }

  }
}
