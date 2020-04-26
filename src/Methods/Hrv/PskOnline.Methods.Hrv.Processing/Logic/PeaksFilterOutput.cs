namespace PskOnline.Methods.Hrv.Processing.Logic
{
  using System;
  using System.Collections.Generic;
  using System.Text;
  using PskOnline.Methods.Hrv.ObjectModel;

  /// <summary>
  /// Результат одного захода преобразования последовательности 
  /// сердечных сокращений в кардио-интервалы.
  /// </summary>
  public class PeaksFilterOutput
  {
    public List<RatedContractionMark> rated_heart_contraction_marks;

    public List<CardioInterval> extracted_intervals;

    public double average_cardio_interval = 0.0;

    public PeaksFilterOutput(List<RatedContractionMark> hr_marks)
    {
      this.rated_heart_contraction_marks = new List<RatedContractionMark>();
      for (int i = 0; i < hr_marks.Count; ++i)
      {
        rated_heart_contraction_marks.Add(new RatedContractionMark(hr_marks[i]));
      }
      this.extracted_intervals = new List<CardioInterval>(hr_marks.Count - 1);
    }

    /// <summary>
    /// update aggregate values (average cardio interval) upon modification of extracted_intervals list
    /// </summary>
    public void Update()
    {
      double sum = 0;
      for (int i = 0; i < this.extracted_intervals.Count; ++i)
      {
        sum += this.extracted_intervals[i].duration;
      }
      this.average_cardio_interval = sum / ((double)this.extracted_intervals.Count);
    }
  }


}
