namespace PskOnline.Math.Test.Psa.Ppg
{
  using System;

  using NUnit.Framework;

  using PskOnline.Components.Log;
  using PskOnline.Math.Psa.Ppg;

  /// <summary>
  /// Ќабора тестов дл€ класса ZeroCrossingDetector
  /// </summary>
  [NUnit.Framework.TestFixture]
  public class ZeroCrossingDetector_UTest
  {
    log4net.ILog log = log4net.LogManager.GetLogger(typeof(PpgPulseDetectorByDerivative_UTest));

    [NUnit.Framework.SetUp]
    public void SetUp()
    {
      LogHelper.ConfigureConsoleLogger();
    }

    [NUnit.Framework.TearDown]
    public void TearDown()
    {
      LogHelper.ShutdownLogSystem();
    }

    private static double tolerance = 1.0e-15;

    #region set of test cases
    class TestCase
    {
      public TestCase(double[] series_, double zero_crossing_point_)
      {
        this.series = series_;
        this.zero_crossing_point = zero_crossing_point_;
      }

      public double[] series;
      public double zero_crossing_point;
    }

    /// <summary>
    /// ѕоследовательность, приближающа€с€ к нулю снизу, но не пересекающа€ ноль
    /// </summary>
    private static TestCase ApproachingFromBelowNonCrossingCase = 
      new TestCase(
        new double[] { -3, -2, -1, 0, 0, 0, 0, -1, -2, -3 },
        double.NaN
      );

    /// <summary>
    /// ѕоследовательность, приближающа€с€ к нулю сверху, но не пересекающа€ ноль
    /// </summary>
    private static TestCase ApproachingFromAboveNonCrossingCase =
      new TestCase(
        new double[] { 3, 2, 1, 0, 0, 0, 0, 1, 2, 3 },
        double.NaN
      );

    /// <summary>
    /// ѕоследовательность, пересекающа€ ноль сверху вниз с остановкой в нуле
    /// </summary>
    private static TestCase CrossingDownWithZeroCase = 
      new TestCase(
        new double[] { 3, 2, 1, 0, 0, 0, 0, -1, -2, -3 },
        -4
      );

    /// <summary>
    /// ѕоследовательность, пересекающа€ ноль снизу вверх с остановкой в нуле
    /// </summary>
    private static TestCase CrossingUpWithZeroCase = 
      new TestCase(
        new double[] { -3, -2, -1, 0, 0, 0, 0, 1, 2, 3 },
        - 4
      );

    /// <summary>
    /// ѕоследовательность, пересекающа€ ноль сверху вниз без остановки в нуле
    /// </summary>
    private static TestCase CrossingDownWithNoZeroCase =
      new TestCase(
        new double[] { 3, 2, 1, -1, -2, -3 },
        -2.5
      );


    /// <summary>
    /// ѕоследовательность, пересекающа€ ноль снизу вверх без остановки в нуле
    /// </summary>
    private static TestCase CrossingUpWithNoZeroCase =
      new TestCase(
        new double[] { -3, -2, -1, 1, 2, 3 },
        -2.5
      );


    /// <summary>
    /// ѕоследовательность, пересекающа€ ноль снизу вверх без остановки в нуле
    /// </summary>
    private static TestCase CrossingUpWithNoZeroCase2 =
      new TestCase(
        new double[] { -1, 2 },
        -2.0 / 3.0
      );

    /// <summary>
    /// ѕоследовательность, пересекающа€ ноль снизу вверх без остановки в нуле
    /// </summary>
    private static TestCase CrossingUpWithNoZeroCase_Simple =
      new TestCase(
        new double[] { -1, 1 },
        -0.5
      );

    #endregion

    [Test]
    public void TestImmediateCrossingUp()
    {
      ZeroCrossingDetector detector = new ZeroCrossingDetector(ZeroCrossingDirection.Up);
      TestCase theCase = CrossingUpWithNoZeroCase;

      Assert.IsTrue( detector.TestSeries(theCase.series));
      log.Info(detector.GetLastZeroCrossingPoint());
      Assert.IsTrue( EqualWithEpsilon(theCase.zero_crossing_point, detector.GetLastZeroCrossingPoint(), tolerance) );
    }

    [Test]
    public void TestImmediateCrossingUp2()
    {
      ZeroCrossingDetector detector = new ZeroCrossingDetector(ZeroCrossingDirection.Up);
      TestCase theCase = CrossingUpWithNoZeroCase2;

      Assert.IsTrue(detector.TestSeries(theCase.series));
      log.Info(detector.GetLastZeroCrossingPoint());
      Assert.IsTrue(EqualWithEpsilon(theCase.zero_crossing_point, detector.GetLastZeroCrossingPoint(), tolerance));
    }

    [Test]
    public void TestImmediateCrossingDown()
    {
      ZeroCrossingDetector detector = new ZeroCrossingDetector(ZeroCrossingDirection.Down);
      TestCase theCase = CrossingDownWithNoZeroCase;

      Assert.IsTrue(detector.TestSeries(theCase.series));
      log.Info(detector.GetLastZeroCrossingPoint());
      Assert.IsTrue(EqualWithEpsilon(theCase.zero_crossing_point, detector.GetLastZeroCrossingPoint(), tolerance));
    }

    [Test]
    public void TestCrossingUpWithZeroes()
    {
      ZeroCrossingDetector detector = new ZeroCrossingDetector(ZeroCrossingDirection.Up);
      TestCase theCase = CrossingUpWithZeroCase;

      Assert.IsTrue(detector.TestSeries(theCase.series));
      log.Info(detector.GetLastZeroCrossingPoint());
      Assert.IsTrue(EqualWithEpsilon(theCase.zero_crossing_point, detector.GetLastZeroCrossingPoint(), tolerance));
    }

    [Test]
    public void TestCrossingDownWithZeroes()
    {
      ZeroCrossingDetector detector = new ZeroCrossingDetector(ZeroCrossingDirection.Down);
      TestCase theCase = CrossingDownWithZeroCase;

      Assert.IsTrue(detector.TestSeries(theCase.series));
      log.Info(detector.GetLastZeroCrossingPoint());
      Assert.IsTrue(EqualWithEpsilon(theCase.zero_crossing_point, detector.GetLastZeroCrossingPoint(), tolerance));
    }

    [Test]
    public void TestApproachingFromAbove()
    {
      ZeroCrossingDetector detector = new ZeroCrossingDetector(ZeroCrossingDirection.Up);
      TestCase theCase = ApproachingFromAboveNonCrossingCase;

      Assert.IsFalse(detector.TestSeries(theCase.series));
    }

    [Test]
    public void TestApproachingFromBelow()
    {
      ZeroCrossingDetector detector = new ZeroCrossingDetector(ZeroCrossingDirection.Down);
      TestCase theCase = ApproachingFromBelowNonCrossingCase;

      Assert.IsFalse( detector.TestSeries(theCase.series) );
    }

    private bool EqualWithEpsilon(double expected, double actual, double tolerance)
    {
      log.Info("Comparing values...");
      log.InfoFormat("  expected: '{0,28:F22}', actual: '{0,28:F22}'", expected, actual);
      log.InfoFormat("  tolerance ==  '{0,28:F22}'", tolerance);
      log.InfoFormat("  abs. error == '{0,28:F22}'", Math.Abs(expected - actual));

      return Math.Abs(expected - actual) < tolerance;
    }

  }
}
