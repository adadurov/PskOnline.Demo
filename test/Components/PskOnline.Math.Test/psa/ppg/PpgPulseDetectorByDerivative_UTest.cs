namespace PskOnline.Math.Test.Psa.Ppg
{
  using System;
  using System.Collections.Generic;
  using System.IO;

  using NUnit.Framework;

  using PskOnline.Components.Log;
  using PskOnline.Components.Util;
  using PskOnline.Math.Psa.Ppg;

  [TestFixture]
  public class PpgPulseDetectorByDerivative_UTest
  {
    log4net.ILog log = log4net.LogManager.GetLogger(typeof(PpgPulseDetectorByDerivative_UTest));

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
    public void Test_RB_16_signal_01()
    {
      string filename = Path.Combine("unit_test_data", "UTest_rb_16_data.dat");

      RunTest(filename, 190);
    }

    [Test]
    public void Test_RB_16_signal_02()
    {
      string filename = Path.Combine("unit_test_data", "UTest_rb_16_data_02.dat");

      RunTest(filename, 64);
    }

    [Test]
    public void Test_RB_18_signal()
    {
      string filename = Path.Combine("unit_test_data", "UTest_rb_18_data.dat");

      RunTest(filename, 198);
    }

    /// <summary>
    /// обрабатывает сигнал из указанного файла целиком и без взаимодействи€ с пользователем
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="bInteractiveFinish"></param>
    public void RunTest(string filename, int ExpectedContractionsCount)
    {
      RunTest(filename, ExpectedContractionsCount, false);
    }

    /// <summary>
    /// обрабатывает сигнал из указанного файла целиком
    /// если bInteractiveFinish == true, показывает сообщение
    /// и ждет нажати€ кнопки ќ  перед закрытием результатов.
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="bInteractiveFinish"></param>
    public void RunTest(string filename, int ExpectedContractionsCount, bool bInteractiveFinish)
    {
      RunTest(filename, ExpectedContractionsCount, bInteractiveFinish, 0, int.MaxValue);
    }

    /// <summary>
    /// обрабатывает сигнал из указанного файла, начина€ со строки
    /// start_line и заканчива€ строкой end_line.
    /// если bInteractiveFinish == true, показывает сообщение
    /// и ждет нажати€ кнопки ќ  перед закрытием результатов.
    /// </summary>
    /// <param name="filename">filename relative to this assembly's folder</param>
    /// <param name="bInteractiveFinish"></param>
    /// <param name="start_line"></param>
    /// <param name="end_line"></param>
    public void RunTest(string filename, int ExpectedContractionsCount, bool bInteractiveFinish, int start_line, int end_line)
    {
      log.Info("========================================================");
      log.InfoFormat("Opening file '{0}'", filename);
      log.Info("========================================================");


      using( ThreadCultureModifier.SetFloatingPointNumberDecimalSeparator(","))
      {
        string path = FileHelpers.GetAssemblyFolderPath(System.Reflection.Assembly.GetExecutingAssembly());
        filename = System.IO.Path.Combine(path, filename);

        HeartContractions.Clear();

        double SamplingRate = 100;
        List<int> data_list = new List<int>(1000);

        using (System.IO.StreamReader sr = new System.IO.StreamReader(filename))
        {
          string FirstLine = sr.ReadLine();
          string[] samplingRateLines = FirstLine.Split('=');
          if (samplingRateLines.Length < 2)
          {
            samplingRateLines = new [] {FirstLine, FirstLine};
          }

          log.DebugFormat("1-st line of the file is \"{0}\"", FirstLine);
          log.DebugFormat("Sampling rate string is \"{0}\"", samplingRateLines[1]);

          SamplingRate = double.Parse(samplingRateLines[1]);

          log.DebugFormat("Sampling rate read from the file: {0}", SamplingRate);

          System.Diagnostics.Debug.Assert(
            SamplingRate < 1200,
            "Samping rate too high!\r\n" +
            "Please check that (decimal separator used for floating-point value is ','.");

          int data_lines_counter = 0;
          while ((!sr.EndOfStream) && (data_lines_counter < end_line))
          {
            string line = sr.ReadLine();
            ++data_lines_counter;

            if (data_lines_counter >= start_line)
            {
              if (string.Empty != line)
              {
                //  log.DebugFormat("read from file:   {0}", line);
                data_list.Add(int.Parse(line));
              }
            }
          }
        }

        int[] data = data_list.ToArray();

        using (PpgPulseDetectorByDerivative detector = new PpgPulseDetectorByDerivative(SamplingRate, 10, true))
        {
          detector.HeartContractionDetected += detector_HeartContractionDetected;
          detector.AddData(data, (long) (((double) data.Length)/SamplingRate*1000000));

          if( bInteractiveFinish )
          {
            // System.Windows.Forms.MessageBox.Show("Click OK to finish the test!");
            throw new NotImplementedException();
          }
        }

        log.Info("....Test results are:");
        log.InfoFormat("....{0} heart contraction(s) detected in the loaded signal", HeartContractions.Count);
        log.Info("=================================================================");

        //  ритерий успешности теста: количество
        // обнаруженных сердечных сокращений
        // равно ожидаемому предварительно вычисленному количеству
        NUnit.Framework.Assert.AreEqual(ExpectedContractionsCount, HeartContractions.Count);
      }
    }

    List<double> HeartContractions = new List<double>(200);

    void detector_HeartContractionDetected(double sample_count, long timestamp)
    {
      HeartContractions.Add(sample_count);
    }

  }
}
