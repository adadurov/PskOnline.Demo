namespace PskOnline.Math.Test.Psa.Ppg
{
  using System;
  using System.Collections.Generic;

  using NUnit.Framework;

  using PskOnline.Components.Log;
  using PskOnline.Math.Psa.Ppg;

  [TestFixture]
  public class PpgPulseDetector_UTest
  {
    log4net.ILog log = log4net.LogManager.GetLogger(typeof(PpgPulseDetector_UTest));

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

    public void RunTest(string filename)
    {
      log.Info("========================================================");
      log.InfoFormat("Opening file '{0}'", filename);
      log.Info("========================================================");

      HeartContractions.Clear();

      double SamplingRate = 100;
      List<int> data_list = new List<int>(1000);

      using( System.IO.StreamReader sr = new System.IO.StreamReader(filename) )
      {
        string FirstLine = sr.ReadLine();
        string[] samplingRateLines = FirstLine.Split('=');
        if (samplingRateLines.Length < 2)
        {
          samplingRateLines = new string[] { FirstLine, FirstLine };
        }

        log.DebugFormat("1-st line of the file is \"{0}\"", FirstLine);
        log.DebugFormat("Sampling rate string is \"{0}\"", samplingRateLines[1]);

        SamplingRate = double.Parse(samplingRateLines[1]);

        while( ! sr.EndOfStream )
        {
          string line = sr.ReadLine();
          if (string.Empty != line)
          {
//            log.DebugFormat("read from file:   {0}", line);
            data_list.Add(int.Parse(line));
          }
        }
      }

      int[] data = data_list.ToArray();

      using( var detector = new PpgPulseDetector(SamplingRate, 10) )
      {
        detector.HeartContractionDetected += detector_HeartContractionDetected;
        detector.AddData(data, (long)(((double)data.Length) / SamplingRate * 1000000));
      }

      log.Info("....Test results are:");
      log.InfoFormat("....{0} heart contraction(s) detected in the loaded signal", HeartContractions.Count);
      log.Info("=================================================================");
    }

    List<double> HeartContractions = new List<double>(200);

    void detector_HeartContractionDetected(double sample_count, long timestamp)
    {
      HeartContractions.Add(sample_count);
    }

  }
}
