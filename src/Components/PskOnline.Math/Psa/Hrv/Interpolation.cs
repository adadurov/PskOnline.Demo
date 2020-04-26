namespace PskOnline.Math.Psa.Hrv
{
  internal class Interpolation
  {
    public static float interpolate_linear(float[] x, float[] y, float x_, ref int idx)
    {
      int n = x.Length;
      if( idx <= 0 )
      {
        idx = 1;
      }
      else if ( idx >= (n - 1) )
      {
        idx = n - 2;
      }

      while ((idx > 0) && (idx < n - 2))
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

      System.Diagnostics.Debug.Assert( idx >= 0);
      System.Diagnostics.Debug.Assert( idx < n );
      return y[idx] + (x_ - x[idx]) * (y[idx + 1] - y[idx]) / (x[idx + 1] - x[idx]);
    }
  }
}
