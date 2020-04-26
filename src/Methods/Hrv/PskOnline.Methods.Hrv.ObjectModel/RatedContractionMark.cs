using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Methods.Hrv.ObjectModel
{
  /// <summary>
  /// Метка сердечного сокращения с показателем качества
  /// для удобства отображения меток на кривой ФПГ.
  /// фронты, обнаруженные процессором ФПГ проходят
  /// дополнительную обработку с целью отбраковки "неправильных"
  /// сокращений, как-то: помех, артефактов и т.п.
  /// </summary>
  public class RatedContractionMark
  {
    public RatedContractionMark()
    {
    }

    public RatedContractionMark(RatedContractionMark src)
    {
      this.Position = src.Position;
      this.Valid = src.Valid;
      this.IntervalsCount = src.IntervalsCount;
    }

    /// <summary>
    /// количество сэмплов с начала потока данных
    /// </summary>
    public double Position;

    /// <summary>
    /// количество достоверных интервалов,
    /// в которых участвует данная отметка сердечного сокращения
    /// </summary>
    public int IntervalsCount = 0;

    /// <summary>
    /// см. комментарии в теле функции ConvertPeaksToIntervalsWithRejectionAndRatePeaks
    /// </summary>
    public bool Valid { get; set; }
  }

}
