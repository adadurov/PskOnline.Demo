namespace PskOnline.Server.Plugins.Rushydro.Test
{
  using NUnit.Framework;

  using PskOnline.Components.Log;
  using PskOnline.Server.Plugins.RusHydro.Logic;
  using PskOnline.Server.Plugins.RusHydro.ObjectModel;

  [TestFixture]
  public class PsaStatus_UTest
  {
    private log4net.ILog _log = log4net.LogManager.GetLogger(typeof(PsaStatus_UTest));

    public PsaStatus_UTest()
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
    public void Color_For_All_Statuses()
    {
      System.Drawing.Color c1 = PsaStatusColorProvider.StatusColor(PsaStatus.Pass);
      System.Drawing.Color c2 = PsaStatusColorProvider.StatusColor(PsaStatus.Conditional_Pass);
      System.Drawing.Color c3 = PsaStatusColorProvider.StatusColor(PsaStatus.Fail);
    }

    [Test]
    public void Text_For_All_Statuses()
    {
      Assert.LessOrEqual( 4, PsaStatusTextProvider.StatusText(PsaStatus.Pass).Length );
      Assert.LessOrEqual( 4, PsaStatusTextProvider.StatusText(PsaStatus.Conditional_Pass).Length );
      Assert.LessOrEqual( 4, PsaStatusTextProvider.StatusText(PsaStatus.Fail).Length );
    }
  }
}
