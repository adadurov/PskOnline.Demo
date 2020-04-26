namespace PskOnline.Math.Test.Psa
{
  using NUnit.Framework;

  using PskOnline.Components.Log;
  using PskOnline.Math.Psa.Ecg;

  [TestFixture]
  public class EcgAnalyzer_UTest
  {
    /// <summary>
    /// Сегмент сигнала типа ЭКГ длиной 40 точек.
    /// </summary>
    private readonly double[] signal_segment = new double[] { 1, 2, 2, 1, 2, 2, 4, 8, 16, 32, 64, 63, 32, 16, 8, 4, 2, 1, 3, 3, 4, 4, 3, 4, 5, 3, 6, 4, 4, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2 };
    private double[] signal = null;
    private readonly int count = 20;

    [SetUp]
    public void SetUp()
    {
      LogHelper.ConfigureConsoleLogger();

      System.Collections.ArrayList s = new System.Collections.ArrayList();
      for( int i = 0; i < count; ++i )
      {
        for( int j = 0; j < signal_segment.Length; ++j )
        {
          s.Add(signal_segment[j]);
        }
        // s.AddRange(signal_segment); // этот вызов добавляет сигнал задом наперед... почему?
      }
      signal = (double[])s.ToArray(typeof(double));
    }

    log4net.ILog log = log4net.LogManager.GetLogger(typeof(EcgAnalyzer_UTest));

    [TearDown]
    public void TearDown()
    {
      LogHelper.ShutdownLogSystem();
    }

    [Test]
    public void ECG_Analyzer_Smoke()
    {
      System.Collections.ArrayList s = new System.Collections.ArrayList();
      for( int i = 0; i < count; ++i )
      {
        for( int j = 0; j < signal_segment.Length; ++j )
        {
          s.Add(signal_segment[j]);
        }
        // вызов этого метода почему-то приводит
        // к добавлению сигнала задом наперед,
        // а нам этого не надо!
        // s.AddRange(signal_segment);
      }
      signal = (double[])s.ToArray(typeof(double));

      EcgAnalyzer a = EcgAnalyzerFactory.MakeSimpleAnalyzer(30);
      a.Period = 0.5; // Минимальный период в секундах
      a.MaxPeakWidth = 0.5; // Половина от периода

      a.Analyze(this.signal);

      double[] intervals = a.GetIntervals();

      if (null != intervals)
      {
        log.DebugFormat("Кол-во интервалов: {0}", intervals.Length);
        foreach (double interval in intervals)
        {
          log.Debug(interval.ToString());
        }
      }
    }
  }
}
