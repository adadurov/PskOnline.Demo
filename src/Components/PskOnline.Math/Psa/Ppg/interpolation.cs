using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Math.Psa.Ppg
{
  class interpolation
  {
    public static float interpolate_linear(double[] x, double[] y, int n, float x_, ref int idx)
    {
      // приводим в порядок начальный индекс
      if( idx <= 0)
      {
			  idx = 1;
      }
      else if ( idx >= n-1 )
      {
			  idx = n-2;
      }
	
	    while( (idx > 0)&&(idx < n-2) )
      {
        if (x[idx + 1] < x_)
        {
          idx += 1;
        }
        else if (x[idx] > x_)
        {
          idx -= 1;
        }
        else
        {
          break;
        }
	    }

	    System.Diagnostics.Debug.Assert( idx >= 0 );
      System.Diagnostics.Debug.Assert( idx < n );

      return (float)(y[idx] + (x_ - x[idx]) * (y[idx + 1] - y[idx]) / (x[idx + 1] - x[idx]));
    }
  }
}
