using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Methods.Svmr.Processing
{
  /// <summary>
  /// Вычисляется по методическому справочнику УПФТ-1/30 
  /// на основе функции принадлежности Талалаева-Косачева
  /// </summary>
  public static class UPFT130Reliability
  {
    public static double FromSingleReaction(double reactionTime)
    {
      return SvmrReliabilityMembershipFunc.ValueAt(reactionTime);
    }

    public static double FromReactionsArray(double[] reactionTimes)
    {
      if( null == reactionTimes || 0 == reactionTimes.Length ) return 0;
      double ave = 0;
      for (int i = 0; i < reactionTimes.Length; ++i )
      {
        ave += SvmrReliabilityMembershipFunc.ValueAt(reactionTimes[i]);
      }
      return ave / (double)reactionTimes.Length;
    }
  }

  internal static class SvmrReliabilityMembershipFunc
  {
    static double[] points_x = new double[] { 150, 200, 220, 240, 260, 280, 300, 320, 340, 360, 380 };
    static double[] points_y = new double[] { 100, 90,   80,  70,  60,  50,  40,  20,  10,   5,   0 };

    internal static double ValueAt(double reactionTime)
    {
      // отсечка ложных реакций
      if( reactionTime < points_x[0] ) return 0;
      double max = points_x[points_x.Length - 1];
      for( int i = points_x.Length - 1; i >= 0; --i )
      {
        System.Diagnostics.Debug.Assert(points_x[i] <= max);
        max = points_x[i];
        if (reactionTime >= points_x[i])
        {
          return points_y[i];
        }
      }
      
      return 0;
    }
  }

}
