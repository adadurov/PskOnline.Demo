namespace PskOnline.Math.Test.Psa
{
  using System;

  using NUnit.Framework;

  using PskOnline.Components.Log;
  using PskOnline.Math.Psa;

  [TestFixture]
  public class SeriesStatisticsCollector_UTest
  {
    log4net.ILog logger = log4net.LogManager.GetLogger(typeof(SeriesStatisticsCollector_UTest));


    class FrontDetectionTestSequence
    {
      public FrontDetectionTestSequence(double[] seq, double expected_amp, int expected_dur)
      {
        this.sequence = seq;
        this.expected_amplitude = expected_amp;
        this.expected_duration = expected_dur;
      }

      public double[] sequence;
      public double expected_amplitude;
      public int expected_duration;
    }

    #region TEST SEQUENCES -- DO NOT MODIFY !!!
    private FrontDetectionTestSequence UnifinishedFrontSequence = new FrontDetectionTestSequence(
      new double[] {0, -1, -2, -1, 0, 1, 2, 3, 4},
      6,
      6
    );

    private FrontDetectionTestSequence FinishedFrontSequence = new FrontDetectionTestSequence(
      new double[] { 0, -1, -2, -1, 0, 1, 2, 3, 4, 3 },
      6,
      6
    );
    #endregion

    [NUnit.Framework.SetUp]
    public void Setup()
    {
      LogHelper.ConfigureConsoleLogger();
    }

    [NUnit.Framework.TearDown]
    public void TearDown()
    {
      LogHelper.ShutdownLogSystem();
    }

    const int size = 10;

    const double constantValue = 1111;

    [NUnit.Framework.Test]
    public void Test_Dispersion_Of_Constant()
    {
      SeriesStatisticsCollector ssc = new SeriesStatisticsCollector(size);

      for( int i = 0; i < size; ++i )
      {
        ssc.AddValue(constantValue);
      }

      ssc.AddValue(constantValue);

      NUnit.Framework.Assert.AreEqual(0, ssc.GetDX(0));
      NUnit.Framework.Assert.AreEqual(constantValue, ssc.GetMX(0));
    }

    [NUnit.Framework.Test]
    public void Test_Average_Of_Constant()
    {
      SeriesStatisticsCollector ssc = new SeriesStatisticsCollector(size);

      for (int i = 0; i < size; ++i)
      {
        ssc.AddValue(constantValue);
      }

      ssc.AddValue(constantValue);

      NUnit.Framework.Assert.AreEqual(constantValue, ssc.GetMX(0));
    }

    [NUnit.Framework.Test]
    public void TestCreationFailureWhenRequestingTooShortHistorySize()
    {
      Assert.Throws<ArgumentException>( () => { SeriesStatisticsCollector ssc = new SeriesStatisticsCollector(0); } );
    }

    [NUnit.Framework.Test]
    public void TestRisingFrontsDetectionOnEmptySeries()
    {
      SeriesStatisticsCollector ssc = new SeriesStatisticsCollector(100);
      NUnit.Framework.Assert.AreEqual(0, ssc.GetLastRisingFrontAmplitude());
      NUnit.Framework.Assert.AreEqual(0, ssc.GetLastRisingFrontDuration());
    }

    [NUnit.Framework.Test]
    public void TestRisingFrontsDetectionOnSingleValueSequence()
    {
      SeriesStatisticsCollector ssc = new SeriesStatisticsCollector(3);

      ssc.AddValue(0);

      NUnit.Framework.Assert.AreEqual(0, ssc.GetLastRisingFrontAmplitude());
      NUnit.Framework.Assert.AreEqual(1, ssc.GetLastRisingFrontDuration());
    }

    [NUnit.Framework.Test]
    public void TestUnfinishedFront()
    {
      SeriesStatisticsCollector ssc = new SeriesStatisticsCollector(3);

      FrontDetectionTestSequence ts = UnifinishedFrontSequence;

      ssc.AddSequence(ts.sequence);

      NUnit.Framework.Assert.AreEqual(ts.expected_duration, ssc.GetLastRisingFrontDuration());
      NUnit.Framework.Assert.AreEqual(ts.expected_amplitude, ssc.GetLastRisingFrontAmplitude());
    }

    [NUnit.Framework.Test]
    public void TestFinishedFront()
    {
      SeriesStatisticsCollector ssc = new SeriesStatisticsCollector(3);

      FrontDetectionTestSequence ts = FinishedFrontSequence;

      ssc.AddSequence(ts.sequence);

      NUnit.Framework.Assert.AreEqual(ts.expected_duration, ssc.GetLastRisingFrontDuration());
      NUnit.Framework.Assert.AreEqual(ts.expected_amplitude, ssc.GetLastRisingFrontAmplitude());
    }

    [NUnit.Framework.Test]
    public void TestMinMaxFinishedFront()
    {
      FrontDetectionTestSequence seq = this.FinishedFrontSequence;
      SeriesStatisticsCollector ssc = new SeriesStatisticsCollector(seq.sequence.Length);
      ssc.AddSequence(seq.sequence);

      NUnit.Framework.Assert.AreEqual(-2, ssc.GetMin());
      NUnit.Framework.Assert.AreEqual(4, ssc.GetMax());
    }

    [NUnit.Framework.Test]
    public void TestMinMaxUnFinishedFront()
    {
      FrontDetectionTestSequence seq = this.UnifinishedFrontSequence;
      SeriesStatisticsCollector ssc = new SeriesStatisticsCollector(seq.sequence.Length);
      ssc.AddSequence(seq.sequence);

      NUnit.Framework.Assert.AreEqual(-2, ssc.GetMin());
      NUnit.Framework.Assert.AreEqual(4, ssc.GetMax());
    }

    [NUnit.Framework.Test]
    public void TestMinMaxFinishedFrontShortHistory()
    {
      FrontDetectionTestSequence seq = this.FinishedFrontSequence;
      SeriesStatisticsCollector ssc = new SeriesStatisticsCollector(4);
      ssc.AddSequence(seq.sequence);

      NUnit.Framework.Assert.AreEqual(2, ssc.GetMin());
      NUnit.Framework.Assert.AreEqual(4, ssc.GetMax());
    }

  }
}
