namespace PskOnline.Server.Plugins.RusHydro.Logic
{
  using System;

  /// <summary>
  /// Вычисляет VSR на базе показателей статистики ряда кардио-интервалов
  /// 
  ///  MEMO: state matrix cell numbers (page 8)
  ///
  /// smRow
  ///   5  |  21  22  23  24  25
  ///   4  |  16  17  18  19  20
  ///   3  |  11  12  13  14  15
  ///   2  |   6   7   8   9  10
  ///   1  |   1   2   3   4   5
  ///      +----------------------
  ///         1   2   3   4   5   smCol
  /// 
  /// </summary>
  public class Upft130VsrCalculator
  {
    private log4net.ILog _log = log4net.LogManager.GetLogger(typeof(Upft130VsrCalculator));
 
    public Upft130VsrCalculator()
    {
    }

    public static readonly double VSR_0_Critical = 0.001;

    // 638  => 5 row => 
    // 0    => 0.001
    // 782  => 
    // 1150 =>
    // 1500 => 0.001
    public double HRV_to_VSR(double MRR, double SigmaRR)
    {
      _log.InfoFormat("Classifying state with MRR={0}, SigmaRR={1}", MRR.ToString("F3"), SigmaRR.ToString("F3"));

      if (IsCriticalRR(MRR)) return VSR_0_Critical; 

      int row = Mrr2SmRow(MRR);
      System.Diagnostics.Debug.Assert(1 <= row && row <= 5);
      if( row < 1 || row > 5)
      {
        throw new ArgumentOutOfRangeException(string.Format("row={0} is not within [1; 5] -- must have been intercepted by IsCriticalRR()", row));
      }

      // we have row  in [1; 5] here

      // check if Sigma is critical for the row
      if ( IsCriticalSigmaRR(row, SigmaRR) ) return VSR_0_Critical;

      int column = SigmaRR2SmCol(row, SigmaRR);
      System.Diagnostics.Debug.Assert( 1 <= column && column <= 5 );
      if( column < 1 || column > 5 )
      {
        throw new ArgumentOutOfRangeException(string.Format("column={0} is not within [1; 5] -- must have been intercepted by IsCriticalSigmaRR()", column));
      }

      int cell = 5 * (row - 1) + column - 1;
      System.Diagnostics.Debug.Assert(0 <= cell && cell < VSR_by_cell_number.Length);

      double VSR = VSR_by_cell_number[cell];
      _log.InfoFormat("Classified state with MRR={0}, SigmaRR={1} => {2}", MRR.ToString("F3"), SigmaRR.ToString("F3"), VSR.ToString("F3"));

      return VSR;
    }

    /// <summary>
    /// </summary>
    /// <param name="smRow">a value within [1; 5]</param>
    /// <param name="SigmaRR"></param>
    /// <returns>
    /// A value within [1; 5] for SigmaRR within limits depending on the smRow parameter.
    /// 0 or 6 for SigmaRR outside of the limits for the specified smRow. 
    /// </returns>
    /// <remarks></remarks>
    public int SigmaRR2SmCol(int smRow, double SigmaRR)
    {
      if( smRow < 1 )
      {
        throw new ArgumentOutOfRangeException(string.Format("smRow={0} is less than 1", smRow));
      }
      if( smRow > 5 )
      {
        throw new ArgumentOutOfRangeException(string.Format("smRow={0} is greater than 5", smRow));
      }
      // given the row, find index of the 1st and the last column limit
      int left = (smRow - 1) * 6; // 0

      int column = 0;
      int i = 0;
      for( i = 0; i < 6; ++i )
      {
        if (SigmaRR >= SM_col_limit_by_Row[left + i])
        {
          column = i;
          break;
        }
      }
      if( i > 5 )
      {
        return 6;
      }
      return column;
    }

    private double[] SM_col_limit_by_Row = new double[]
    {
      120, 75, 60, 38, 31, 26, //  1..5
      120, 73, 60, 37, 29, 24, //  6..10
      100, 66, 53, 32, 25, 20, // 11..15
      100, 65, 50, 27, 19, 10, // 16..20
      100, 64, 41, 19, 13, 6   // 21..25
    };

    private bool IsCriticalRR(double MRR)
    {
      if (MRR < SM_row_limits_MRR[0]) return true;
      if (MRR >= SM_row_limits_MRR[SM_row_limits_MRR.Length - 1]) return true;
      return false;
    }

    public bool IsCriticalSigmaRR(int row, double SigmaRR)
    {
      // given the row, find index of the 1st and the last column limit
      int left = (row - 1) * 6;
      int right = row * 6 - 1;

      if (SM_col_limit_by_Row[left] <= SigmaRR) return true;
      if (SM_col_limit_by_Row[right] > SigmaRR) return true;
      
      return false;
    }

    /// <summary>
    /// Возвращает номер ряда в матрице сверху вниз
    /// </summary>
    /// <param name="MRR"></param>
    /// <returns></returns>
    public int Mrr2SmRow(double MRR)
    {
      int row = 1;
      for (int i = 1; i < SM_row_limits_MRR.Length; ++i)
      {
        if (MRR < SM_row_limits_MRR[i])
        {
          row = i;
          break;
        }
      }
      row = SM_row_limits_MRR.Length - row;
      return row;
    }

    /// <summary>
    /// границы указаны сверху вниз
    /// низкий пульс -- возвращаем ряд №1
    /// высокий пульс -- возвращаем ряд №5
    /// </summary>
    public static double[] SM_row_limits_MRR
    {
      get
      {
        // those values are in the inverse order compared to 
        // SigmaRR limits by rows (SM_col_limit_by_Row)
        return new double[] { 500, 667, 750, 857, 1000, 1200 };
      }
    }


    private double[] VSR_by_cell_number
    {
      get
      {
        return new double[] 
        {
          0.01, 0.15, 0.380, 0.15, 0.01,  //  1..5
          0.11, 0.50, 0.750, 0.50, 0.11,  //  6..10
          0.11, 0.75, 0.960, 0.75, 0.11,  // 11..15 
          0.11, 0.50, 0.750, 0.50, 0.11,  // 16..20
          0.01, 0.15, 0.380, 0.15, 0.01,  // 21..25
        };
      }
    }

  }

}
