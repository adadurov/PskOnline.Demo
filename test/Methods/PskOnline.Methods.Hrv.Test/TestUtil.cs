namespace PskOnline.Methods.Hrv.Test
{
  internal static class TestUtil
  {
    internal static string[] TestFileNames
    {
      get
      {
        return new string[]
        {
          "inspect_1_NeuroLab.BioMouse.methods.physio.cardio_2008-01-18_15-49.psk.json",
          "inspect_31_NeuroLab.BioMouse.methods.physio.cardio_2008-05-06_19-40.pds2",
          "inspect_101_NeuroLab.BioMouse.methods.physio.cardio_2008-04-15_14-17.pds2",
          "peaks_filtering_problem_25.06.2009_12.19_NeuroLab.BioMouse.methods.physio.cardio_21.pds2",
          "test_15.06.2009_18.59_NeuroLab.BioMouse.methods.physio.cardio_5.pds2",
          "failing_rejection_when_rejection_is_off_10-Nov-2009_5.47_.psk.json",
          "HRV_545_.psk.json",
          "HRV_2957_.psk.json",
          "NeuroLab.BioMouse.methods.physio.cardio_75.pds2"
        };
      }
    }

    /// <summary>
    /// Образцы записей, на которых отбраковка фронтов сбивается и 
    /// пропускает каждый второй фронт как невалидны, из-за чего ЧСС
    /// определяется как вдвое меньшее истинного значения
    /// </summary>
    internal static string[] hr2x_samples
    {
      get
      {
        return new string[]
        {

        };
      }
    }

    /// <summary>
    /// test sample definition
    /// </summary>
    public class tsd
    {
      public tsd(string fileName, double expectedMrrLow, double expectedMrrHigh)
      {
        _fileName = fileName;
        _expectedMrrLow = expectedMrrLow;
        _expectedMrrHigh = expectedMrrHigh;
      }
      public string _fileName; // Имя файла с записью ВКР
      public double _expectedMrrLow; // Ожидаемое среднее значение
      public double _expectedMrrHigh; // Ожидаемое значение сигмы
    }
    /// <summary>
    /// Образцы записей с артефактами в начале (обследуемый нажимает
    /// "начать запись", не дожидаясь появления качественного сигнала).
    /// 
    /// </summary>
    internal static tsd[] low_quality_start_samples
    {
      get
      {
        return new tsd[]
        {
          new tsd("burges_lq_start_10_2017-06-16_7.14.psk.json", 882.5d, 883.5d),
          new tsd("burges_lq_start_11_2017-06-15_8.03.psk.json", 971.0d, 972.0d),
          new tsd("burges_lq_start_12_2017-06-16_2.42.psk.json", 704.0d, 705.0d),
          new tsd("burges_lq_start_16_2017-06-14_13.08.psk.json", 1031.0d, 1032.0d),
          new tsd("burges_lq_start_17_2017-06-16_2.55.psk.json", 936.0d, 937.0d),
          new tsd("burges_lq_start_22_2017-06-15_8.28.psk.json", 836.0d, 837.0d),
          new tsd("burges_lq_start_23_2017-06-14_14.29.psk.json", 716.0d, 717.0d),
          new tsd("burges_lq_start_36_2017-06-15_3.53.psk.json", 692.0d, 693.0d),
          new tsd("burges_lq_start_44_2017-06-16_3.01.psk.json", 730.0d, 731.0d),
          new tsd("burges_lq_start_46_2017-06-15_3.42.psk.json", 977.0d, 978.0d),
          new tsd("zeyges_lq_start_19_2017-06-20_3.49.psk.json", 1001.0d, 1002.0d),
          new tsd("zeyges_lq_start_20_2017-06-21_3.52.psk.json", 846.0d, 847.0d),
          new tsd("zeyges_lq_start_46_2017-06-19_6.23.psk.json", 835.0d, 836.0d),

          new tsd("zeyges_lq_start_17_2017-06-19_5.33.psk.json", 537.0d, 538.0d),
          new tsd("burges_lq_start_41_2017-06-15_15.20.psk.json", 598.0d, 599.0d),
        };
      }
    }

  }
}
