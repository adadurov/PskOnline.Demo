namespace PskOnline.Server.Plugins.Rushydro.Test
{
  using NUnit.Framework;

  using PskOnline.Components.Log;
  using PskOnline.Server.Plugins.RusHydro.Logic;
  using PskOnline.Server.Plugins.RusHydro.ObjectModel;

  /// <summary>
  /// Вычисляет VSR на базе показателей статистики ряд кардио-интервалов
  /// 
  /// </summary>
  [TestFixture]
  public class Upft130HrvClassifier_UTest
  {
    private log4net.ILog _log = log4net.LogManager.GetLogger(typeof(Upft130HrvClassifier_UTest));
    private Upft130HrvClassifier hrvClassifier;

    public Upft130HrvClassifier_UTest()
    {
    }

    [SetUp]
    public void SetUp()
    {
      LogHelper.ConfigureConsoleLogger();
      hrvClassifier = new Upft130HrvClassifier();
    }

    [TearDown]
    public void TearDown()
    {
      LogHelper.ShutdownLogSystem();
    }


    [Test]
    public void Optimal_State_Shall_Yield_Pass_Result()
    {
      Assert.AreEqual(PsaStatus.Pass, hrvClassifier.Lsr2Status(LSR_HrvFunctionalState.Optimal_5));
      Assert.AreEqual(PsaStatus.Pass, hrvClassifier.Lsr2Status(hrvClassifier.Vsr2Lsr(0.81)), "Via VSR");
    }

    [Test]
    public void Near_Optimal_State_Shall_Yield_Pass_Result()
    {
      Assert.AreEqual(PsaStatus.Pass, hrvClassifier.Lsr2Status(LSR_HrvFunctionalState.NearOptimal_4));
      Assert.AreEqual(PsaStatus.Pass, hrvClassifier.Lsr2Status(hrvClassifier.Vsr2Lsr(0.65)), "Via VSR");
    }

    [Test]
    public void Acceptable_State_Shall_Yield_Conditional_Pass_Result()
    {
      Assert.AreEqual(PsaStatus.Conditional_Pass, hrvClassifier.Lsr2Status(LSR_HrvFunctionalState.Acceptable_3));
      Assert.AreEqual(PsaStatus.Conditional_Pass, hrvClassifier.Lsr2Status(hrvClassifier.Vsr2Lsr(0.371)), "Via VSR");
    }

    [Test]
    public void OnTheEdge_State_Shall_Yield_Conditional_Pass_Result()
    {
      Assert.AreEqual(PsaStatus.Conditional_Pass, hrvClassifier.Lsr2Status(LSR_HrvFunctionalState.OnTheEdge_2));
      Assert.AreEqual(PsaStatus.Conditional_Pass, hrvClassifier.Lsr2Status(hrvClassifier.Vsr2Lsr(0.11)), "Via VSR");
    }

    [Test]
    public void Negative_State_Shall_Yield_Fail_Result()
    {
      Assert.AreEqual(PsaStatus.Fail, hrvClassifier.Lsr2Status(LSR_HrvFunctionalState.Negative_1));
      Assert.AreEqual(PsaStatus.Fail, hrvClassifier.Lsr2Status(hrvClassifier.Vsr2Lsr(0.0011)), "Via VSR");
    }

    [Test]
    public void Critical_State_Shall_Yield_Fail_Result()
    {
      Assert.AreEqual(PsaStatus.Fail, hrvClassifier.Lsr2Status(LSR_HrvFunctionalState.Critical_0));
      Assert.AreEqual(PsaStatus.Fail, hrvClassifier.Lsr2Status(hrvClassifier.Vsr2Lsr(0.001)), "Via VSR");
    }

  }
}
