namespace PskOnline.Methods.Hrv.Processing.Logic
{
  using System;
  using System.Collections.Generic;
  using PskOnline.Methods.Hrv.ObjectModel;
  using PskOnline.Methods.ObjectModel.PhysioData;
  using PskOnline.Methods.Processing.Contracts;

  /// <summary>
  /// 03.07.2009 adadurov
  /// ����������, ��� ��� ��������� �������� ���������� ���� ����������:
  /// ������ �������� � ������ -- ������������� 1200 ��, ����������� ����� 750 ��,
  /// ������� �������� ���������� � ����� ������� ������ �������� � ��������
  /// ����������� ��������� ���������� "����� ����", �������� ���������
  /// ������������� ����� 1500 ��.
  ///   �� ���� ���������� ���, ��� ������������ ������� ��������� (� ��������������)
  ///   ������������ � 35-���������� ������ �� ������������� ��������� ������-����������
  ///   (������ �������� ������������ �� ������� ����������!), � ������������
  ///   ������� ��������� �� ��������� � ������ -- �� ������������ � ���������� 35%
  ///   ������.
  ///   � ����������:
  ///   �) ������� �������� �� ���������� ����������� ����������
  ///   �� �������� ��������� ����� ���������� (��������� �������������� ����� 2).
  /// 
  ///   ������������ �������:
  ///      I.
  ///         1. ���� (�), �� ��������� ������ ��������� ���������� � �����������.
  ///         2. ��������� (1) (����������� ��� �� ������ ����������), ����
  ///            �������� �� ��������� (����� ����������� ���������?), �� �� ����� X ���
  ///            (���� ����� X)?
  ///     II.
  ///         1. ����������� ������ � �����? �� ����� �� �������� �������� � � ����� �����...
  ///
  ///    III. 06/07/2009 �����������.
  ///         ��� �������� �������������� ��������� �� ����������� ������� ������������ ���������
  ///         ��������� �������� ����������� ��������� ������ ���� �������� ������������������
  ///         ������-����������.
  /// 
  /// 11.11.2009 adadurov
  /// ��������� �������� ��� ��������� ����������� � ���������� ����������
  ///   �������������� ���������� ������� ���������� ����� BasicCardioDataProcessor_UTest:
  ///       Test_File_2957_With_And_Without_Peaks_Rejection()
  ///       test_unit_test_data_file_number_5_with_and_without_peaks_rejection()
  /// �������� ��������:
  ///   � ������ ���������� ��������� ����� 750 ��, ����� ����� � ����� ������������� � ������������
  ///   ��������� �����, ����� ���������� ���������� ������, ���������� �������� ����� ����������� 
  ///   ������� ����� 1200 ��. ����� �� ����� ����� ���������� ��������� ����� 750 �� -- � ����������
  ///   ����� ������������� ������ �������� ��������� �� 1500 ��, � �� �� 750.
  /// 
  /// </summary>
  class PeaksFilter
  {
    /// <summary>
    /// ��������� �� ���������� ������ ������� (�.4) �������� ������ �������.
    /// </summary>
    static bool EnableStepBackOnSuccessiveFailures = true;

    /// <summary>
    /// ������� �� ������� ��������� ���������� ������ ������-���������� � ��������� ��������� ���������
    /// � ������ � ��������.
    /// ��� ������� ����������� ��������� �������� ��������� ���������� � �������� ��������������� �������.
    /// 
    /// ����� ���������� ���� ��������� ���������� � 
    /// </summary>
    /// <param name="hr_marks_to_rate"></param>
    /// <param name="data_rate"></param>
    /// <param name="min_interval_length"></param>
    /// <param name="max_interval_length"></param>
    /// <param name="max_relative_delta">
    /// ������������ ���������� ������������ ����������� ��� ������������� ������������ ���������, %%
    /// ���� �������� ��������� ����� double.PositiveInfinity, ���������� �� �������������� ���������� �� ������������
    /// </param>
    /// <param name="signal">������, �� ������ �������� ���������� ������� ��������� ����������.
    /// ����� �������������� ��� ������ "��������" ��������� ��������� ����������</param>
    /// <returns></returns>
    /// <remarks>
    /// 
    /// 0. ����������:
    /// ����������� �����:
    /// ��� �� (��������� ����������), ���������� �� ������� �� �������� ���������� ������ 1/3
    /// ���������� ����������� ������-���������?????
    /// 
    /// ���� ������� ��������, ����� ������� ������������� � �������� ������������ ��������� ���������� (��).
    ///
    /// 1. ��������, ���� ���������� (��), ���������� �
    /// ���������� �� �������� ������������� ����� ��� � 0.65 �������� ���������
    /// � ����� ��� �� 30% ������ ����������� ���������, �� ����� ������������,
    /// ��� ������ �� ���������� � ������ �������� (������������� �����)
    /// � ��� ����� "���������" �� ���� ����������, ������������ ��� ����������
    /// ���� ����������.
    /// 1�. ���� �������� ����������� ��������� ���������� (��������, ��� ������� ��������� � ����),
    /// ���������� ��� ������ �������� �������� ��������� (�������� �� �� ���� ���������� � �������
    /// ������, ��� "������������", ���� "��������������" �� �������� ������� ���������
    /// (�.�. ��������� ������ ���������, ��������������� ������������ �� ���������� �������� ���������).
    /// 
    /// 2. ���� ��������� ��������� ���������� ���� ���������, �� �������� �����
    /// "��������" ��������, ��������� � �������� ���������� � ��������� ��
    /// ��, �������������� ������������ � ��, ��������� �� �����������, �����������
    /// �� "���������" �� � ������� ������ ���������� ������� �� ������������ ����������
    /// � ������ ������ ���������� ������-����������.
    ///
    /// 3. ���� �� ������� (2) �������� ���������� �������� �� ������� (�������� ������������
    /// ���������� �������� ��������� ����������� ����������), �������� ������� ����� ������-��������,
    /// ������ �������� ���������� �� "�����������" ��, � ��������� � ������ 1.
    /// 
    /// �����������:
    /// 4. ���� ���������� �� � ������ ����������� �� ������������� ��������� ������������
    /// ���������� ���� ������� ����� (����� T=6) ������������� �� � (T-1) ������-���������� ������, 
    /// ������������ ������ T ����� ����� (� ������� �� �� ����������� �����) � ������� �������������
    /// ��������� ������������ ������� �� ����� ����������� ���������� �� �� ��������� � ����������,
    /// � �� ��������� �� ������� ��������� �� ��� ������ ��� ����������.
    /// 
    /// ��� ��������� ���������� ����������� ������ ����������� ������������� � ���, ��� ����������
    /// ����������� ������� � �������� ������ ��������� ���������� ����� ������ ���������� ���������
    /// ������������ ��.
    /// ///////////////////////////////////////////
    /// </remarks>
    public static PeaksFilterOutput ConvertPeaksToIntervalsWithRejectionAndRatePeaks(
      List<RatedContractionMark> hr_marks_to_rate,
      double data_rate,
      double min_interval_length,
      double max_interval_length,
      double max_relative_delta,
      ChannelData signal
      )
    {
      if (hr_marks_to_rate.Count < 2)
      {
        throw new ArgumentException(strings.too_few_heart_contractions_detected_in_signal);
      }

      // �������� �� %% � ����� �� 1
      max_relative_delta /= 100.0;

      // ��������� (�����������) ����������� �������� ������������ �������� � ������ �����������
      double ave = Estimate_AverageInterval(
        hr_marks_to_rate,
        data_rate,
        min_interval_length,
        max_interval_length,
        max_relative_delta);

      List<PeaksFilterOutput> results = new List<PeaksFilterOutput>(5);

      int start = 1;
      int max_retries = 1; // 5 -- ���� ������ �������� ����������� �������� � ���������, ���� ���������

      for (int i_first = start; i_first < System.Math.Min(start + max_retries, hr_marks_to_rate.Count); ++i_first)
      {
        PeaksFilterOutput r = TryConvertAndRatePeaks(hr_marks_to_rate, data_rate, min_interval_length, max_interval_length, max_relative_delta, ave, i_first);
        results.Add(r);

        double delta_ave = System.Math.Abs(r.average_cardio_interval - ave) / ( 0.5 * (r.average_cardio_interval + ave) );
        if( (delta_ave <= 0.1) || (double.PositiveInfinity == max_relative_delta) )
        {
          Debug_DumpFilteringResults(r);
          return r;
        }
      }

      // ���� ������ ����, ������ �� ���� �� ���������� ������� ����������
      // �� ������������ � ����������� �������� � 10%. ���������� �������
      // ��������� ��������� �� ����������� ������

      PeaksFilterOutput r_min = results[0];
      double delta_r_min = System.Math.Abs(r_min.average_cardio_interval - ave) / (0.5 * (r_min.average_cardio_interval + ave));
      for (int i = 1; i < results.Count; ++i)
      {
        double delta_r_i = System.Math.Abs(results[i].average_cardio_interval - ave) / ( 0.5 * (results[i].average_cardio_interval + ave) );
        if (delta_r_i < delta_r_min )
        {
          r_min = results[i];
          delta_r_min = delta_r_i;
        }
      }

      Debug_DumpFilteringResults(r_min);

      return r_min;
    }

    [System.Diagnostics.Conditional("__DEBUG")]
    private static void Debug_DumpFilteringResults(PeaksFilterOutput r_min)
    {
      for (int ii = 0; ii < r_min.rated_heart_contraction_marks.Count; ++ii)
      {
        if (r_min.rated_heart_contraction_marks[ii].IntervalsCount < 2)
        {
          log4net.LogManager.GetLogger(typeof(PeaksFilter)).Debug("not so good heart contraction mark");
        }
      }
    }

    private static PeaksFilterOutput TryConvertAndRatePeaks(List<RatedContractionMark> src_hr_marks, double data_rate, double min_interval_length, double max_interval_length, double max_relative_delta, double ave, int i_first)
    {
      // ������ ������������ ��������� ���������� ������������ ����������
      var result = new PeaksFilterOutput(src_hr_marks);

      // ��� �������� ��������� � ����� ������� result...
      var hrMarksToRate = result.rated_heart_contraction_marks;
      var intervals = result.extracted_intervals;

      double begin = 0, end = 0, interval = 0, delta, rdelta;

      // ����� � ����� ������ ���������� �������� ����� ����� �������� ��������� �� ���� ������ ������
      double prev_interval = ave;

      bool bChecksPassed, rdelta_ok;
      int i_begin, i_end;

      // ���������� ���������������� ������������� ���������� �� ������� �������������� ��������� ������������
      int count_of_successive_failures_due_to_relative_change = 0;

      // ����������� ���������� ���������� ����� ����������
      // (��� ���������� �������� ������� � ������ ����� � ���������� ��������)
      int max_count_of_successive_failures_due_to_relative_change = 6;

      bool in_step_back_mode = false;

      var i_ends = new List<int>(10);
      var i_ends_interval_ends = new Dictionary<int, double>(10);

      // ���������� ��� ������������ ����� ��������� ����������
      for (int i = i_first; i < hrMarksToRate.Count; ++i)
      {
        i_begin = i - 1;
        i_end = i_begin + 1;

        bChecksPassed = false;

        // �� ���������� ����������, ���������� �� ��������� ���������� � i..i+k,
        // �������� ���������� �� ����������� ��������, � �� ��� �������� ��������,
        // �������� ������� � ����������� ��������
        //
        // ���� ����� �������� �� ������������� ����������� �� ��������� ������������ ����������� ���������,
        // ���������� ��� �������� �� ������� ��������� ��� ��� �� ����������� �� ������������� ���������
        rdelta_ok = true; // �����, ����� r_delta ���� true ���� �������� �� ����������� while ��-�� ����������� �������� ���������

        begin = hrMarksToRate[i_begin].Position / data_rate * 1000.0;

        i_ends.Clear();
        i_ends_interval_ends.Clear();

        if (max_relative_delta < double.PositiveInfinity)
        {
          for (; i_end < hrMarksToRate.Count; ++i_end)
          {
            end = hrMarksToRate[i_end].Position / data_rate * 1000.0;
            interval = end - begin;
            if (interval < min_interval_length)
            {
              // ���������� ��� �������, ������ ������� �������� ���������
              continue;
            }
            if (interval > max_interval_length)
            {
              // ���������� ������� ������� ���������, ����������
              break;
            }
            i_ends.Add(i_end);
            i_ends_interval_ends[i_end] = end;
          }
        }
        else
        {
          i_ends.Add(i_end);
          i_ends_interval_ends.Add(i_end, hrMarksToRate[i_end].Position / data_rate * 1000.0);
        }

        if (0 == i_ends.Count)
        {
          // ��������� ����������, ��������� �� begin �� ���������� ����������, �� ����������
          continue;
        }

        // ��� ������� �� "��������������" ���������� ������� ������� �����
        // ��� ��������� � ��������� ����������� ���������.
        // �������� � ����������� �������� � ���� �������� "������������" ��������.
        double min_delta = double.MaxValue;
        int min_index = -1;
        for (int l = 0; l < i_ends_interval_ends.Count; ++l)
        {
          interval = i_ends_interval_ends[i_ends[l]] - begin;
          delta = System.Math.Abs(prev_interval - interval);
          if (delta < min_delta)
          {
            min_delta = delta;
            min_index = l;
          }
        }

        // �������� � �������� ��������� ��������� �����������
        i_end = i_ends[min_index];
        // ��� ������������� ������� (� ��):
        end = i_ends_interval_ends[i_end];
        // � ���� �������� �������������
        interval = end - begin;

        delta = System.Math.Abs(prev_interval - interval);

        rdelta = delta / prev_interval;

        // �� ���������� �� ��������� � ����������� ���������:
        rdelta_ok = (rdelta <= max_relative_delta);

        // �� ����������� �������� ��� ������� ���������, ��� �������� �� ����� �������...
        bool abs_val_ok = (interval >= min_interval_length) && (interval <= max_interval_length);

        bChecksPassed = abs_val_ok && rdelta_ok;

        // ok, ������ �������� -- �� ���������� �������� � �� ��������� � �����������...
        if (bChecksPassed)
        {
          // �������� ����������� ��������
          // ������� ��� ���������� ����� begin � end ��� �������������!
          for (int j = i_begin + 1; j < i_end; ++j)
          {
            hrMarksToRate[i].Valid = false;
            hrMarksToRate[i].IntervalsCount = 0;
          }
          hrMarksToRate[i_begin].Valid = true;
          hrMarksToRate[i_end].Valid = true;

          ++hrMarksToRate[i_begin].IntervalsCount;
          ++hrMarksToRate[i_end].IntervalsCount;

          count_of_successive_failures_due_to_relative_change = 0;
          in_step_back_mode = false;

          prev_interval = interval;
          intervals.Add(new CardioInterval(begin, end, i_begin, i_end));

          i = i_end;
        }
        else
        {
          // �������� ����������� �������� ������������...
          // ��� ������?

          // ������� (i-1)-� ����� ���������� ���������� � �������� ������������ ��������
          // � i-� �� i+1-� ����� � �.�.

          // �� ������� ��������� �������� �� i-1-�� ���������� ����������
          // �������� ��� ��� �������������
          hrMarksToRate[i_begin].Valid = false;

          // �������� ��� ����������
          if (rdelta_ok) // ��-�� �������������� ���������?
          {
            // ���, ������������� ��������� � �������
            count_of_successive_failures_due_to_relative_change = 0;
            in_step_back_mode = false;
          }
          else
          {
            // ��
            // ��� ���� ��������� ���������� ������������� �� ������� �������������� ��������� ������������
            ++count_of_successive_failures_due_to_relative_change;
          }

          // �������� ���������� ���������� ����������� ������ ��-�� ��������� �����������
          // �� ������������� ��������� ������������ ���������
          if (PeaksFilter.EnableStepBackOnSuccessiveFailures)
          {
            if (count_of_successive_failures_due_to_relative_change > max_count_of_successive_failures_due_to_relative_change)
            {
              // ��� ����� (����� �� �����������, ������ ��� ����� ������ ���� ���)...
              if (false == in_step_back_mode)
              {
                // ����������, ��� ��� ����� ��� ������
                in_step_back_mode = true;
                // ��������� � ������ ��������� �� �������� ���������,
                // � �� �� �����������, � �������� ��� � ��������
                prev_interval = ave;
                count_of_successive_failures_due_to_relative_change = 0;
                // ������������ �� max_count_of_successive_failures_due_to_relative_change ���������� �����
                i = System.Math.Max(1, i - max_count_of_successive_failures_due_to_relative_change);
              }
              else
              {
                // ���� ��� ����� ��� �������, �� ������ ��� �� ���������, ����� �� �����������!
              }
            }
          }
        }
      } // for( i = ...

      result.Update();
      return result;
    }

    private static double Estimate_AverageInterval(
      List<RatedContractionMark> hr_marks_to_rate,
      double data_rate,
      double min_interval_length,
      double max_interval_length,
      double max_relative_delta
      )
    {
      double[] coordinates = new double[hr_marks_to_rate.Count];
      for (int i = 0; i < coordinates.Length; ++i)
      {
        coordinates[i] = hr_marks_to_rate[i].Position;
      }

      // ��� �������� ������ ����������, ����������� ��� ������������������
      // ����������, ������������ �� ���� ������������������ ���������� �� �����������
      // ����������, �� ��������������� ������������ min_interval_length, max_interval_length
      // � max_relative_delta.
      // ��� �������� ������� ��������� �� ������������� ��������� �������� ����������� ���������
      // ����������� ������ �������� ���� �������� ������������������ ������-����������.
      var draft_intervals = PeaksFilter.CoordinatesToIntervals(coordinates, data_rate);

      var draft_durations = new List<double>(draft_intervals.Count);
      foreach( CardioInterval draft_interval_1 in draft_intervals )
      {
        draft_durations.Add(draft_interval_1.duration);
      }
      double[] draft_durations_array = draft_durations.ToArray();
      var stat = new PskOnline.Math.Statistics.StatData();

      PskOnline.Math.Statistics.Calculator.CalcStatistics(draft_durations_array, stat);
      PskOnline.Math.Statistics.Calculator.MakeProbabilityDensity(draft_durations_array, stat, 400, 2000, 4);

      double ave = 0;
      double prev_interval_duration = stat.probability_density.mode;
      int draft_count = 0;
      bool bLengthOk = false;
      bool bRDeltaOk = false;

      foreach (CardioInterval draft_interval in draft_intervals)
      {
        bLengthOk = (draft_interval.duration >= min_interval_length) &&
            (draft_interval.duration <= max_interval_length);

        bRDeltaOk = (max_relative_delta >= (System.Math.Abs(prev_interval_duration - draft_interval.duration) / prev_interval_duration ) );

        if( bLengthOk && bRDeltaOk )
        {
          // ����� "�������" ��������
          ave += draft_interval.duration;
          ++draft_count;
          prev_interval_duration = draft_interval.duration;
        }
      }

      if (draft_count == 0)
      {
        throw new DataProcessingException(
          string.Format(strings.too_few_valid_intervals_detected_in_signal, draft_count));
      }

      return ave / ((double)draft_count);
    }

    /// <summary>
    /// returns array of intervals in milliseconds!!!
    /// </summary>
    /// <param name="coordinates">����������, ���������� � ���������� �������</param>
    /// <param name="dataRate"></param>
    /// <returns></returns>
    public static List<CardioInterval> CoordinatesToIntervals(double[] coordinates, double dataRate)
    {
      if (coordinates.Length < 2)
      {
        return new List<CardioInterval>(0);
      }

      var intervals = new List<CardioInterval>(coordinates.Length - 1);

      var previous = coordinates[0];
      //      double current = 0.0;
      for (int i = 1; i < coordinates.Length; ++i)
      {
        var begin = coordinates[i - 1] / dataRate * 1000.0d;
        var end = coordinates[i] / dataRate * 1000.0d;
        intervals.Add(new CardioInterval(begin, end, i - 1, i));
      }
      return intervals;
    }
  }
}
