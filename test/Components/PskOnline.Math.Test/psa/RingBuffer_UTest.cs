namespace PskOnline.Math.Test.Psa
{
  using System;
  using System.Collections.Generic;

  using NUnit.Framework;

  using PskOnline.Components.Log;
  using PskOnline.Math.Psa;

  /// <summary>
  /// Набор тестов для класса RingBuffer
  /// </summary>
  [NUnit.Framework.TestFixture]
  public class RingBuffer_UTest
  {
    log4net.ILog log = log4net.LogManager.GetLogger(typeof(RingBuffer_UTest));

    [NUnit.Framework.SetUp]
    public void Setup()
    {
      LogHelper.ConfigureConsoleLogger();
    }

    [NUnit.Framework.TearDown]
    public void Teardown()
    {
      LogHelper.ShutdownLogSystem();
    }

    /// <summary>
    /// tests for correct data extraction
    /// </summary>
    [NUnit.Framework.Test]
    public void TestHistoryExtraction()
    {
      int size = 200;
      int[] array = new int[size];
      RingBuffer<int> int_buf = new RingBuffer<int>(size);

      // ring buffer initialization
      // earliest point: 19, latest value: 0
      for (int i = array.Length-1; i > -1; --i )
      {
        int_buf.AddValue(i);
        array[i] = i;
      }

      // Comparison of buffer and array content
      // (must be identical, counting )
      for (int j = 0; j < array.Length; ++j)
      {
        NUnit.Framework.Assert.AreEqual(j, array[j]);
        NUnit.Framework.Assert.AreEqual(j, int_buf.GetValue(j));
      }
    }

    [NUnit.Framework.Test]
    public void TestHistoryInitialization()
    {
      int size = 20;
      int test_val = 0x5a5a5a5a;
      RingBuffer<int> int_buf = new RingBuffer<int>(size);

      int_buf.InitBuffer(test_val);
      log.InfoFormat("test value is {0}", test_val);
      for (int i = 0; i < size; ++i)
      {
        log.InfoFormat("{0} -> {1}", i, int_buf.GetValue(i));
        int actual_value = int_buf.GetValue(i);
        NUnit.Framework.Assert.AreEqual(test_val, actual_value);
      }
    }

    [Test]
    public void TestHistorySize()
    {
      int size = 20;
      // Given
      RingBuffer<int> int_buf = new RingBuffer<int>(size);

      // When

      // Then
      Assert.Throws<ArgumentException>( () => int_buf.GetValue(size + 100) );
    }


    int[] test_sequence = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

    [NUnit.Framework.Test]
    public void TestHistory()
    {
      RingBuffer<int> theBuffer = new RingBuffer<int>(test_sequence.Length);

      this.log.Info("Adding data to the ring buffer...");

      for( int i = 0; i < (2*test_sequence.Length); ++i )
      {
        int new_value = test_sequence[i % test_sequence.Length];
        this.log.InfoFormat("{0}", new_value);
        theBuffer.AddValue(new_value);
      }

      this.log.Info("Checking history in the buffer...");
      for (int j = 0; j < test_sequence.Length; ++j)
      {
        int history_value = theBuffer.GetValue(j);
        this.log.InfoFormat("{0}", history_value);
        NUnit.Framework.Assert.AreEqual(test_sequence[test_sequence.Length - 1 - j], history_value);

      }


    }



    int[] test_sequence_zeroes = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    int[] test_sequence_hump = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };


    /// <summary>
    /// Для проверки поведения кольцевого буфера в связи с дефектом №162
    /// </summary>
    [NUnit.Framework.Test]
    public void Test_Sum_History()
    {

      // делаем десять заходов...
      int N = 10;

      List<int> real_test_sequence = new List<int>();
      real_test_sequence.AddRange(test_sequence_zeroes);
      real_test_sequence.AddRange(test_sequence_hump);
      real_test_sequence.AddRange(test_sequence_zeroes);

      int[] real_test_sequence_list = real_test_sequence.ToArray();

      int sum_val = 0;
      int value;

      RingBuffer<int> buffer = new RingBuffer<int>(5);
      buffer.InitBuffer(0);

      for (int t = 0; t < N; ++t)
      {
        for (int i = 0; i < real_test_sequence_list.Length; ++i)
        {
          value = real_test_sequence_list[i];

          sum_val -= buffer.GetOldestValue();
          sum_val += value;

          System.Console.WriteLine("sum_val = {0}", sum_val);

          // запоминаем историю значений первой производной, большей 0 с учетом порога
          buffer.AddValue(value);
        }

        // после длинной серии нулей сумма должна быть равна нулю!
        NUnit.Framework.Assert.AreEqual((int)0, sum_val);
        System.Console.WriteLine("++++++++++ final sum_val = {0}", sum_val);
      }
    }

  }
}
