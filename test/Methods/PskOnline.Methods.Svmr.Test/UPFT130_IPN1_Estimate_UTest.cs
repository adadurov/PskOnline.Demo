namespace PskOnline.Methods.Svmr.Test
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Text;

  using NUnit.Framework;
  using PskOnline.Components.Log;
  using PskOnline.Methods.Svmr.Processing;

  [TestFixture]
  public class UPFT130_IPN1_Estimate_UTest
  {
    log4net.ILog _log = log4net.LogManager.GetLogger(typeof(UPFT130_IPN1_Estimate_UTest));

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

    class input_row
    {
      public input_row(string line)
      {
        string[] values = line.Split(',');
        M = double.Parse(values[0]);
        Min = double.Parse(values[1]);
        Max = double.Parse(values[2]);
        num_normal = int.Parse(values[3]);
        num_miss = int.Parse(values[4]);
        num_premature = int.Parse(values[5]);
        num_total = int.Parse(values[6]);
      }
      
      public override string  ToString()
      {
 	    return string.Format("{0}  {1}  {2}  {3}  {4}  {5}  {6}",
          M, Min, Max, num_normal, num_miss, num_premature, num_total);
      }

      public double M;
      public double Min;
      public double Max;
      public int num_normal;
      public int num_miss;
      public int num_premature;
      public int num_total;
    }

    [Test]
    [Explicit]
    [Category("Interactive")]
    public void Test_IPN1_0_approach()
    {
      var out_path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "IPN1_0-1.txt");

      try
      {
        string[] input = File.ReadAllLines("UPFT130_IPN1_Estimate.csv");

        var in_rows = new List<input_row>(input.Length);
        for (int i = 1; i < input.Length; ++i)
        {
          in_rows.Add(new input_row(input[i]));
          _log.Debug(in_rows[in_rows.Count - 1]);
        }

        // здесь у нас уже должно быть всё в памяти
        Assert.AreEqual(479, in_rows.Count);
        _log.Debug("================================================================");

        List<string> strings = new List<string>(in_rows.Count);
        foreach (input_row r in in_rows)
        {
          double IPN1 =
            (UPFT130Reliability.FromSingleReaction(r.Min) +
            UPFT130Reliability.FromSingleReaction(r.Max) +
            (r.num_normal - 2) * UPFT130Reliability.FromSingleReaction(r.M) +
            0 * r.num_miss +
            0 * r.num_premature
            ) / ((double)r.num_total);
          strings.Add(IPN1.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture));
          _log.Debug(strings[strings.Count - 1]);
          Assert.LessOrEqual(IPN1, 100.0);
          Assert.GreaterOrEqual(IPN1, 0.0);
        }
        File.WriteAllLines(out_path, strings.ToArray());
        Console.WriteLine($"converted data written to {out_path}");
      }
      finally
      {
        File.Delete(out_path);
      }
    }
  }
}
