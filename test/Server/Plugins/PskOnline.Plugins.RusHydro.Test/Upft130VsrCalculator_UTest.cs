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
  public class Upft130VsrCalculator_UTest
  {
    private log4net.ILog _log = log4net.LogManager.GetLogger(typeof(Upft130VsrCalculator_UTest));
    private Upft130VsrCalculator vsrCalc;

    public Upft130VsrCalculator_UTest()
    {
    }

    [SetUp]
    public void SetUp()
    {
      LogHelper.ConfigureConsoleLogger();
      vsrCalc = new Upft130VsrCalculator();
    }

    [TearDown]
    public void TearDown()
    {
      LogHelper.ShutdownLogSystem();
    }

    public void AssertCriticalVsr(double MRR, double SigmaRR)
    {
      Assert.AreEqual(Upft130VsrCalculator.VSR_0_Critical, new Upft130VsrCalculator().HRV_to_VSR(MRR, SigmaRR));
    }

    public void AssertNonCriticalVsr(double MRR, double SigmaRR)
    {
      Assert.Less(Upft130VsrCalculator.VSR_0_Critical, new Upft130VsrCalculator().HRV_to_VSR(MRR, SigmaRR));
    }

    [Test]
    public void OutOfBoundsStates_CriticalVSR_out_of_rows()
    {
      // top-right
      AssertCriticalVsr( 499, 5 );
      // top
      AssertCriticalVsr( 499, 33);
      // top-left
      AssertCriticalVsr( 499, 101);

      // bottom-right
      AssertCriticalVsr(1201, 5);
      // bottom
      AssertCriticalVsr(1201, 33);
      // bottom-left
      AssertCriticalVsr(1201, 121);
    }

    [Test]
    public void OutOfBoundsStates_CriticalVSR_row_5()
    {
      // row 5
      float mRR = 600;
      Assert.AreEqual(5, vsrCalc.Mrr2SmRow(mRR));
      AssertCriticalVsr(mRR, 3);
      AssertCriticalVsr(mRR, 100);

      Assert.AreEqual(0, vsrCalc.SigmaRR2SmCol(5, 100));
      Assert.IsTrue(vsrCalc.IsCriticalSigmaRR(5, 100));
    }

    [Test]
    public void OutOfBoundsStates_CriticalVSR_row_4()
    {
      // row 4
      float mRR = 715;
      Assert.AreEqual(4, vsrCalc.Mrr2SmRow(mRR));
      AssertCriticalVsr(mRR, 9);
      AssertCriticalVsr(mRR, 100);
    }

    [Test]
    public void OutOfBoundsStates_CriticalVSR_row_3()
    {
      // row 3
      float mRR = 803;
      Assert.AreEqual(3, vsrCalc.Mrr2SmRow(mRR));
      AssertCriticalVsr(mRR, 19);
      AssertCriticalVsr(mRR, 100);
    }

    [Test]
    public void OutOfBoundsStates_CriticalVSR_row_2()
    {
      // row 2
      float mRR = 928;
      Assert.AreEqual(2, vsrCalc.Mrr2SmRow(mRR));
      AssertCriticalVsr(mRR, 23);
      AssertCriticalVsr(mRR, 120);
    }

    [Test]
    public void OutOfBoundsStates_CriticalVSR_row_1()
    {
      // row 1
      float mRR = 1100;
      Assert.AreEqual(1, vsrCalc.Mrr2SmRow(mRR));
      Assert.AreEqual(6, vsrCalc.SigmaRR2SmCol(1, 25), "Expecting SigmaRR to be out of bounds and thus in column 6");
      Assert.IsTrue(vsrCalc.IsCriticalSigmaRR(1, 25));

      AssertCriticalVsr(mRR, 25);
      AssertCriticalVsr(mRR, 120);
    }

    [Test]
    public void WithinBoundsStates_row_5()
    {
      // row 5
      float mRR = 600;
      Assert.AreEqual(5, vsrCalc.Mrr2SmRow(mRR));
      AssertNonCriticalVsr(mRR, 12);
      AssertNonCriticalVsr(mRR, 90);
    }

    [Test]
    public void WithinBoundsStates_row_4()
    {
      // row 4
      float mRR = 715;
      Assert.AreEqual(4, vsrCalc.Mrr2SmRow(mRR));
      AssertNonCriticalVsr(mRR, 18);
      AssertNonCriticalVsr(mRR, 90);
    }

    [Test]
    public void WithinBoundsStates_row_3()
    {
      // row 3
      float mRR = 803;
      Assert.AreEqual(3, vsrCalc.Mrr2SmRow(mRR));
      AssertNonCriticalVsr(mRR, 22);
      AssertNonCriticalVsr(mRR, 90);
    }

    [Test]
    public void WithinBoundsStates_row_2()
    {
      // row 2
      float mRR = 928;
      Assert.AreEqual(2, vsrCalc.Mrr2SmRow(mRR));
      AssertNonCriticalVsr(mRR, 25);
      AssertNonCriticalVsr(mRR, 99);
    }

    [Test]
    public void WithinBoundsStates_row_1()
    {
      // row 1
      float mRR = 1100;
      Assert.AreEqual(1, vsrCalc.Mrr2SmRow(mRR));
      AssertNonCriticalVsr(mRR, 29);
      AssertNonCriticalVsr(mRR, 99);
    }

    void ExpectVSR(double ExpectedVsr, double MRR_min, double MRR_max, double SigmaRR_min, double SigmaRR_max, string comment)
    {
      double eps = 0.0001;
      Assert.AreEqual(ExpectedVsr, new Upft130VsrCalculator().HRV_to_VSR(MRR_min, SigmaRR_min), comment);
      Assert.AreEqual(ExpectedVsr, new Upft130VsrCalculator().HRV_to_VSR(MRR_min, SigmaRR_max - eps), comment);
      Assert.AreEqual(ExpectedVsr, new Upft130VsrCalculator().HRV_to_VSR(MRR_max - eps, SigmaRR_min), comment);
      Assert.AreEqual(ExpectedVsr, new Upft130VsrCalculator().HRV_to_VSR(MRR_max - eps, SigmaRR_max - eps), comment);
    }

    double GetMaxRRForSmRow(int row)
    {
      return Upft130VsrCalculator.SM_row_limits_MRR[Upft130VsrCalculator.SM_row_limits_MRR.Length - row];
    }

    double GetMinRRForSmRow(int row)
    {
      return Upft130VsrCalculator.SM_row_limits_MRR[Upft130VsrCalculator.SM_row_limits_MRR.Length - row - 1];
    }

    [Test]
    public void VSR_in_row_1()
    {
      const int row = 1;
      double row_RR_max = 1200;
      double row_RR_min = 1000;
      ExpectVSR(0.001, row_RR_min, row_RR_max, 120, 200, $"row {row}");
      ExpectVSR(0.01,  row_RR_min, row_RR_max,  75, 120, $"row {row}");
      ExpectVSR(0.15,  row_RR_min, row_RR_max,  60,  75, $"row {row}");
      ExpectVSR(0.38,  row_RR_min, row_RR_max,  38,  60, $"row {row}");
      ExpectVSR(0.15,  row_RR_min, row_RR_max,  31,  38, $"row {row}");
      ExpectVSR(0.01,  row_RR_min, row_RR_max,  26,  31, $"row {row}");
      ExpectVSR(0.001, row_RR_min, row_RR_max, -11,  26, $"row {row}");
    }

    [Test]
    public void VSR_in_row_2()
    {
      const int row = 2;
      double row_RR_max = 1000;
      double row_RR_min = 857;
      ExpectVSR(0.001, row_RR_min, row_RR_max, 120, 200, $"row {row}");
      ExpectVSR(0.11,  row_RR_min, row_RR_max,  73, 120, $"row {row}");
      ExpectVSR(0.50,  row_RR_min, row_RR_max,  60,  73, $"row {row}");
      ExpectVSR(0.75,  row_RR_min, row_RR_max,  37,  60, $"row {row}");
      ExpectVSR(0.50,  row_RR_min, row_RR_max,  29,  37, $"row {row}");
      ExpectVSR(0.11,  row_RR_min, row_RR_max,  24,  29, $"row {row}");
      ExpectVSR(0.001, row_RR_min, row_RR_max, -11,  24, $"row {row}");
    }

    [Test]
    public void VSR_in_row_3()
    {
      const int row = 3;
      double row_RR_max = 857;
      double row_RR_min = 750;
      ExpectVSR(0.001, row_RR_min, row_RR_max, 100, 121, $"row {row}");
      ExpectVSR(0.11,  row_RR_min, row_RR_max,  66, 100, $"row {row}");
      ExpectVSR(0.75,  row_RR_min, row_RR_max,  53,  66, $"row {row}");
      ExpectVSR(0.96,  row_RR_min, row_RR_max,  32,  53, $"row {row}");
      ExpectVSR(0.75,  row_RR_min, row_RR_max,  25,  32, $"row {row}");
      ExpectVSR(0.11,  row_RR_min, row_RR_max,  20,  25, $"row {row}");
      ExpectVSR(0.001, row_RR_min, row_RR_max, -11,  20, $"row {row}");
    }

    [Test]
    public void VSR_in_row_4()
    {
      const int row = 4;
      double row_RR_max = 750;
      double row_RR_min = 667;
      ExpectVSR(0.001, row_RR_min, row_RR_max, 100, 121, $"row {row}");
      ExpectVSR(0.11,  row_RR_min, row_RR_max,  65, 100, $"row {row}");
      ExpectVSR(0.50,  row_RR_min, row_RR_max,  50,  65, $"row {row}");
      ExpectVSR(0.75,  row_RR_min, row_RR_max,  27,  50, $"row {row}");
      ExpectVSR(0.50,  row_RR_min, row_RR_max,  19,  27, $"row {row}");
      ExpectVSR(0.11,  row_RR_min, row_RR_max,  10,  19, $"row {row}");
      ExpectVSR(0.001, row_RR_min, row_RR_max, -11,  10, $"row {row}");
    }

    [Test]
    public void VSR_in_row_5()
    {
      const int row = 5;
      double row_RR_max = 667;
      double row_RR_min = 500;
      ExpectVSR(0.001, row_RR_min, row_RR_max, 100, 121, $"row {row}");
      ExpectVSR(0.01,  row_RR_min, row_RR_max,  64, 100, $"row {row}");
      ExpectVSR(0.15,  row_RR_min, row_RR_max,  41,  64, $"row {row}");
      ExpectVSR(0.38,  row_RR_min, row_RR_max,  19,  41, $"row {row}");
      ExpectVSR(0.15,  row_RR_min, row_RR_max,  13,  19, $"row {row}");
      ExpectVSR(0.01,  row_RR_min, row_RR_max,   6,  13, $"row {row}");
      ExpectVSR(0.001, row_RR_min, row_RR_max, -11,   6, $"row {row}");
    }

    [Test]
    public void SigmaRR2SmCol_Regression_bug_184()
    {
      Upft130VsrCalculator calc = new Upft130VsrCalculator();
      calc.HRV_to_VSR(1200, 4);
      calc.HRV_to_VSR(1201, 4);
      calc.HRV_to_VSR(1300, 4);
      calc.HRV_to_VSR(1300, 130);

      Upft130VsrCalculator calc2 = new Upft130VsrCalculator();
      calc2.HRV_to_VSR(440, 4);
      calc2.HRV_to_VSR(499, 4);
      calc2.HRV_to_VSR(500, 4);
      calc2.HRV_to_VSR(501, 4);
      calc2.HRV_to_VSR(501, 130);
    }

    [Test]
    public void Calc_Row_1_shall_return_1_for_sigmaRR_at_100ms()
    {
      Upft130VsrCalculator calc = new Upft130VsrCalculator();
      int col1 = calc.SigmaRR2SmCol(1, 100.0);
      Assert.AreEqual(col1, 1);

      Upft130VsrCalculator calculator = new Upft130VsrCalculator();
    }

    [Test]
    public void Calc_Row_1_shall_return_5_for_sigmaRR_at_26ms()
    {
      Upft130VsrCalculator calc = new Upft130VsrCalculator();
      int col1 = calc.SigmaRR2SmCol(1, 26.0);
      Assert.AreEqual(col1, 5);

      Upft130VsrCalculator calculator = new Upft130VsrCalculator();
    }

    [Test]
    public void Calc_Row_2_shall_return_1_for_sigmaRR_at_100ms()
    {
      Upft130VsrCalculator calc = new Upft130VsrCalculator();
      int col1 = calc.SigmaRR2SmCol(2, 100.0);
      Assert.AreEqual(col1, 1);

      Upft130VsrCalculator calculator = new Upft130VsrCalculator();
    }

    [Test]
    public void Calc_Row_2_shall_return_5_for_sigmaRR_at_24ms()
    {
      Upft130VsrCalculator calc = new Upft130VsrCalculator();
      int col1 = calc.SigmaRR2SmCol(2, 24.0);
      Assert.AreEqual(col1, 5);

      Upft130VsrCalculator calculator = new Upft130VsrCalculator();
    }

    [Test]
    public void Calc_Row_3_shall_return_1_for_sigmaRR_at_99ms()
    {
      Upft130VsrCalculator calc = new Upft130VsrCalculator();
      int col1 = calc.SigmaRR2SmCol(3, 99.0);
      Assert.AreEqual(col1, 1);

      Upft130VsrCalculator calculator = new Upft130VsrCalculator();
    }

    [Test]
    public void Calc_Row_3_shall_return_5_for_sigmaRR_at_20ms()
    {
      Upft130VsrCalculator calc = new Upft130VsrCalculator();
      int col1 = calc.SigmaRR2SmCol(3, 20.0);
      Assert.AreEqual(col1, 5);

      Upft130VsrCalculator calculator = new Upft130VsrCalculator();
    }

    [Test]
    public void Calc_Row_4_shall_return_1_for_sigmaRR_at_99ms()
    {
      Upft130VsrCalculator calc = new Upft130VsrCalculator();
      int col1 = calc.SigmaRR2SmCol(4, 99.0);
      Assert.AreEqual(col1, 1);

      Upft130VsrCalculator calculator = new Upft130VsrCalculator();
    }

    [Test]
    public void Calc_Row_4_shall_return_5_for_sigmaRR_at_10ms()
    {
      Upft130VsrCalculator calc = new Upft130VsrCalculator();
      int col1 = calc.SigmaRR2SmCol(4, 10.0);
      Assert.AreEqual(col1, 5);

      Upft130VsrCalculator calculator = new Upft130VsrCalculator();
    }

    [Test]
    public void Calc_Row_5_shall_return_1_for_sigmaRR_at_99ms()
    {
      Upft130VsrCalculator calc = new Upft130VsrCalculator();
      int col1 = calc.SigmaRR2SmCol(5, 99.0);
      Assert.AreEqual(col1, 1);

      Upft130VsrCalculator calculator = new Upft130VsrCalculator();
    }

    [Test]
    public void Calc_Row_5_shall_return_5_for_sigmaRR_at_6ms()
    {
      Upft130VsrCalculator calc = new Upft130VsrCalculator();
      int col1 = calc.SigmaRR2SmCol(5, 6.0);
      Assert.AreEqual(col1, 5);

      Upft130VsrCalculator calculator = new Upft130VsrCalculator();
    }

    [Test]
    public void Mrr2SmRow_Out_Of_Bound_No_Exceptions()
    {
      Upft130VsrCalculator calc = new Upft130VsrCalculator();
      int row1 = calc.Mrr2SmRow(Upft130VsrCalculator.SM_row_limits_MRR[0] - 10);
      Assert.GreaterOrEqual(row1, 1);
      Assert.LessOrEqual(row1, 5);

      int row2 = calc.Mrr2SmRow(Upft130VsrCalculator.SM_row_limits_MRR[Upft130VsrCalculator.SM_row_limits_MRR.Length - 1] + 10);
      Assert.GreaterOrEqual(row2, 1);
      Assert.LessOrEqual(row2, 5);
    }

    [Test]
    public void SigmaRR2SmCol_Out_Of_Bound_No_Exceptions()
    {
      Upft130VsrCalculator calc = new Upft130VsrCalculator();

      int col = calc.SigmaRR2SmCol(1, 121);
      Assert.AreEqual(0, col);

      col = calc.SigmaRR2SmCol(1, 24);
      Assert.AreEqual(6, col);

      col = calc.SigmaRR2SmCol(5, 121);
      Assert.AreEqual(0, col);

      col = calc.SigmaRR2SmCol(5, 4);
      Assert.AreEqual(6, col);
    }

  }
}
