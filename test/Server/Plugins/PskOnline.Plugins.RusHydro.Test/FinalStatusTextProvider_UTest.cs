namespace PskOnline.Server.Plugins.Rushydro.Test
{
  using NUnit.Framework;

  using PskOnline.Components.Log;
  using PskOnline.Server.Plugins.RusHydro.Logic;
  using PskOnline.Server.Plugins.RusHydro.ObjectModel;

  [TestFixture]
  public class FinalStatusTextProvider_UTest
  {
    private log4net.ILog _log = log4net.LogManager.GetLogger(typeof(FinalStatusTextProvider_UTest));

    public FinalStatusTextProvider_UTest()
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

    [Test]
    public void Text_For_All_Statuses()
    {
      PreShiftFinalConclusion pass = new PreShiftFinalConclusion();
      PreShiftFinalConclusion partial = new PreShiftFinalConclusion();
      PreShiftFinalConclusion fail = new PreShiftFinalConclusion();
      pass.Status = PsaStatus.Pass;
      partial.Status = PsaStatus.Conditional_Pass;
      fail.Status = PsaStatus.Fail;

      Assert.LessOrEqual( 4, FinalStatusTextProvider.StatusText(pass).Length );
      Assert.LessOrEqual( 4, FinalStatusTextProvider.StatusText(partial).Length );
      Assert.LessOrEqual( 4, FinalStatusTextProvider.StatusText(fail).Length );
    }
  }
}
