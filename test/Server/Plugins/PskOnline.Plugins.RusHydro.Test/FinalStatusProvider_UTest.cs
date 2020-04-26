namespace PskOnline.Server.Plugins.Rushydro.Test
{
  using NUnit.Framework;

  using PskOnline.Components.Log;
  using PskOnline.Server.Plugins.RusHydro.Logic;
  using PskOnline.Server.Plugins.RusHydro.ObjectModel;

  [TestFixture]
  public class FinalStatusProvider_UTest
  {
    private log4net.ILog _log = log4net.LogManager.GetLogger(typeof(FinalStatusProvider_UTest));

    public FinalStatusProvider_UTest()
    {
    }

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

    private static void ValidateExpectedState(PsaStatus expectedFinalStatus, PsaStatus[] svmr_states, LSR_HrvFunctionalState[] hrv_states)
    {
      var hrv = new PreShiftHrvConclusion();
      var svmr = new PreShiftSvmrConclusion();

      for (int i = 0; i < svmr_states.Length; ++i)
      {
        for (int j = 0; j < hrv_states.Length; ++j)
        {
          hrv.LSR = hrv_states[j];
          svmr.Status = svmr_states[i];

          Assert.AreEqual(expectedFinalStatus, FinalStatusProvider.GetFinalConclusion(hrv, svmr).Status);
        }
      }
    }

    [Test]
    public void Test_Failed_States_Bottom_LSR()
    {
      // 2 bottom-most rows
      var svmr_states = new [] 
        { PsaStatus.Pass, PsaStatus.Conditional_Pass, PsaStatus.Fail };
      var hrv_states = new [] 
        { LSR_HrvFunctionalState.Critical_0, LSR_HrvFunctionalState.Negative_1 };

      ValidateExpectedState(PsaStatus.Fail, svmr_states, hrv_states);
    }

    [Test]
    public void Test_Failed_States_Low_IPN1()
    {
      // 1 right-most column, excluding 2 cells at the bottom
      var svmr_states = new [] { PsaStatus.Fail };
      
      var hrv_states = new [] { 
        LSR_HrvFunctionalState.OnTheEdge_2, LSR_HrvFunctionalState.Acceptable_3,
        LSR_HrvFunctionalState.NearOptimal_4, LSR_HrvFunctionalState.Optimal_5 };

      ValidateExpectedState(PsaStatus.Fail, svmr_states, hrv_states);
    }

    [Test]
    public void Test_All_Partial_States()
    {
      {
        // 1 cell in 'high IPN1' column (LSR=2 / on the edge) 
        var svmr_states = new [] { PsaStatus.Pass };
        var hrv_states = new [] { LSR_HrvFunctionalState.OnTheEdge_2 };

        ValidateExpectedState(PsaStatus.Conditional_Pass, svmr_states, hrv_states);
      }
      {
        // 4 cells in 'medium IPN1' column (LSR=2..5)
        var svmr_states = new [] { PsaStatus.Conditional_Pass };

        var hrv_states = new [] { 
        LSR_HrvFunctionalState.OnTheEdge_2, LSR_HrvFunctionalState.Acceptable_3,
        LSR_HrvFunctionalState.NearOptimal_4, LSR_HrvFunctionalState.Optimal_5 };

        ValidateExpectedState(PsaStatus.Conditional_Pass, svmr_states, hrv_states);
      }
    }

    [Test]
    public void Test_All_Passed_States()
    {
      // 1 right-most column, excluding 2 cells at the bottom
      var svmr_states = new [] { PsaStatus.Pass };

      var hrv_states = new [] { 
        LSR_HrvFunctionalState.Acceptable_3, LSR_HrvFunctionalState.NearOptimal_4,
        LSR_HrvFunctionalState.Optimal_5 };

      ValidateExpectedState(PsaStatus.Pass, svmr_states, hrv_states);
    }

  }
}
