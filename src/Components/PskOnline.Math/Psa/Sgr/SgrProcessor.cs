//#define DEBUG_ANALYZE_SGR

using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Math.Psa.Sgr
{

  /// <summary>
  /// Описывает параметры фрагмента сигнала КГР
  /// </summary>
  public class SgrDescriptor
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="l"></param>
    /// <param name="a"></param>
    /// <param name="v"></param>
    /// <param name="s"></param>
    /// <param name="d"></param>
    /// <param name="gs"></param>
    public SgrDescriptor(float l, float a, float v, float s, float d, float gs)
    {
      this.L = l;
      this.A = a;
      this.V = v;
      this.S = s;
      this.D = d;
      this.GS = gs;
    }

    public float L;
    public float A;
    public float V;
    public float S;
    public float D;
    public float GS;
  }

  /// <summary>
  /// 
  /// </summary>
  public class SgrProcessor
  {
    public static SgrDescriptor ProcessSignal(float[] signal, float rate)
    {
      float L = 0, A = 0, V = 0, S = 0, D = 0, GS = 0;
      SgrProcessor.process_sgr(signal, rate, ref L, ref A, ref V, ref S, ref D, ref GS);
      return new SgrDescriptor(L, A, V, S, D, GS);
    }

    #region implementation

    /// <summary>
    /// hidden constructor to prevent creation of class instances
    /// </summary>
    private SgrProcessor()
    {
    }

    private static readonly int SGR_MIN_POINTS_COUNT_FOR_TAN_ESTIMATION = 9;

    /// <summary>
    /// Returns x where f0 comes to  абсциссу, в которой зануляется f0 (вернее, выходит на постоянный уровень)
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="y1"></param>
    /// <param name="h"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    private static float f0_cutoff(float a, float b, float y1, float h, float x)
    {
      float x0 = (y1 - b + h) / a;
      float w = 2 * h / a;
      return x0 - w;
    }

    /// <summary>
    /// Calculates value of smooth (differentiable at any point) joint function, consisting of square and linear function
    /// linear part is of the form a*x+b, y1 is a constant level, h is height of square part.
    /// 'a' may be any floating-point number, 'h' must be positive floating-point number.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="y1"></param>
    /// <param name="h"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    private static float f0(float a, float b, float y1, float h, float x)
    {
      float x0, w;

      x0 = (y1 - b + h) / a;
      w = 2 * h / a;

      if (a > 0)
      {
        if (x >= (x0 - w))
        {
          if (x <= x0)
          {
            return a * a / 4 / h * (x - x0 + w) * (x - x0 + w) + y1;
          }
          else
          {
            return a * x + b;
          }
        }
        else
        {
          return y1;
        }
      }
      else if (a < 0)
      {
        if (x < (x0 - w))
        {
          if (x <= x0)
          {
            return a * x + b;
          }
          else
          {
            return a * a / 4 / h * (x - x0 + w) * (x - x0 + w) + y1;
          }
        }
        else
        {
          return y1;
        }
      }

      // this case is exceptional: if a=0, then h=0, also.
      return y1;
    }

    /// <summary>
    /// Finds first point where 'y' crosses 'threshold', going from 'start' along 'direction'.
    /// </summary>
    /// <param name="y"></param>
    /// <param name="threshold"></param>
    /// <param name="start"></param>
    /// <param name="direction"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    private static int find_first_crossing_of_threshold(float[] y, float threshold, int start, int direction, ref float result)
    {
  	  int i;
      bool found = false;
  	  float diff0, diff;

	    diff0 = y[start] - threshold; 
	    if(System.Math.Abs(diff0) < System.Math.Abs(threshold) * 2 * float.Epsilon )
      {
  		  result = start;
		    return 0;
  	  }
	
	    for( i = start + direction; (i>=0)&&(i<y.Length); i += direction)
      {
  		  if((y[i]-threshold)*diff0<0)
        {
    			found = true;
			    break;
		    }
	    }
	
  	  if( ! found )
      {
  		  return -1;
      }
	
	    diff = y[i] - y[i-direction];

	    if(System.Math.Abs(diff) < y[i] * 2 * float.Epsilon)
      {
		    result = i - 0.5f * direction;
		    return 0;
	    }

	    result = (i-direction) + direction*(threshold - y[i-direction])/diff;
	    return 0;
    }

    /// <summary>
    /// Linear fit values of 'y' beginning from function, returns 'a' and 'b' (from the equation of form 'y = ax + b').
    /// </summary>
    /// <param name="y"></param>
    /// <param name="a">result: a</param>
    /// <param name="b">result: b</param>
    /// <returns></returns>
    private static int linear_fit(float[] y, int start, int count, ref float a, ref float b)
    {
  	  int i, n = count;
  	  double sx = 0, sxx = 0, sy = 0, sxy = 0;

	    if( n <= 0 )
      {
		    return -1;
      }

  	  for( i = start; i < start + count; i++ )
      {
		    sy  += y[i];
		    sxy += y[i] * i;
	    }
  	
	    sxx = (n - 1)* (n - 0.5) * n / 3;
	    sx  = (n - 1) * n / 2;
	    a = (float)((n * sxy -  sx * sy) / (n * sxx - sx * sx));
	    b = (float)((sxx * sy - sx * sxy) / (n * sxx - sx * sx));
	    return 0;
    }

    /// <summary>
    /// Finds minimum and maximum elements among 'count' array elements, beginning at 'start'-th element.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="start"></param>
    /// <param name="count"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    private static void find_min_max(float[] data, int start, int count, ref float min, ref float max)
    {
      #region parameters sanity-check
      if (start < 0)
      {
        throw new ArgumentException("start");
      }
      if( count == 0 )
      {
        throw new ArgumentException("count");
      }
      if ((start + count) > data.Length)
      {
        throw new ArgumentException("(start+count) > data.Length");
      }
      #endregion

      min = data[0];
	    max = data[0];
      for (int i = start; i < start + count; ++i)
      {
		    if( min > data[i] )
        {
          min = data[i];
        }
		    if( max < data[i] )
        {
          max = data[i];
        }
	    }
    }

    /// <summary>
    /// Finds maximum element and its location (index) among 'count' array elements, beginning from 'start'-th element.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="start"></param>
    /// <param name="count"></param>
    /// <param name="max"></param>
    /// <param name="i_max"></param>
    private static void sgr_find_location_of_max(float[] data, int start, int count, ref float max, ref int i_max)
    {
      #region parameters sanity-check
      if (start < 0)
      {
        throw new ArgumentException("start");
      }
      if (count == 0)
      {
        throw new ArgumentException("count");
      }
      if ((start + count) > data.Length)
      {
        throw new ArgumentException("(start+count) > data.Length");
      }
      #endregion

	    max = data[start];
      i_max = start;

	    for(int i = start+1; i < start + count; ++i )
      {
		    if( max < data[i] )
        {
			    max   = data[i];
			    i_max = i;
		    }
	    }
    }

    /// <summary>
    /// This function approximates y with y' = a0*x + b0 at the point x=i_m.
    /// </summary>
    /// <param name="y"></param>
    /// <param name="n"></param>
    /// <param name="i_m"></param>
    /// <param name="a0"></param>
    /// <param name="b0"></param>
    /// <param name="width"></param>
    /// <returns></returns>
    private static void fit_linear_part(float[] y, int start, int count, int i_m, ref float a0, ref float b0, ref float width)
    {
	    if( count < SGR_MIN_POINTS_COUNT_FOR_TAN_ESTIMATION )
      {
        throw new Exception(string.Format(strings.LinearFit_TooFewPointsSpecified, count, SGR_MIN_POINTS_COUNT_FOR_TAN_ESTIMATION));
      }

      int i, i_w;
      float w, min = 0, max = 0;
      int n = count;
      
	    // First iteration. 
      i = start + i_m - SGR_MIN_POINTS_COUNT_FOR_TAN_ESTIMATION / 2;

      if ((i < 0) || (i + SGR_MIN_POINTS_COUNT_FOR_TAN_ESTIMATION >= start + count))
      {
		    throw new Exception(strings.FitLinearPart_OutOfRange);
      }

      linear_fit(y, i, SGR_MIN_POINTS_COUNT_FOR_TAN_ESTIMATION, ref a0, ref b0);
	    
      b0 -= a0 * i;

      find_min_max(y, start, count, ref min, ref max);

      if (max == min)
      {
        throw new Exception(strings.FitLinearPart_MaxEqualsMin);
      }

	    w = (max - min) / System.Math.Abs(a0);

	    i_w = (int)w;
      i = (int)(i_m - w);

      find_min_max(y, System.Math.Max(start, i), System.Math.Min(start + count - i, (int)(i + 2 * w)), ref min, ref max);

      if( max == min )
      {
        throw new Exception(strings.FitLinearPart_MaxEqualsMin);
      }

      i_w = (int)System.Math.Max(0.7 * (max - min) / System.Math.Abs(a0), SGR_MIN_POINTS_COUNT_FOR_TAN_ESTIMATION); //hardcoded constant
	    i = i_m - i_w / 2;
      if( (i < 0) || (i + i_w >= n) )
      {
        throw new Exception(strings.FitLinearPart_OutOfRange);
      }

      linear_fit(y, start + i, i_w, ref a0, ref b0);

	    b0 -= a0 * i;
	    width = System.Math.Abs((max - min) / a0);
    }

    /// <summary>
    /// Эта функция пытается аппроксимировать y функцией f0() в окрестности i_m. 
    /// Сначала выполняется аппроксимация прямой, затем выясняется, какой отрезок мы рассматриваем.
    /// Потом выясняется, какая "подставка" должна быть у f0.
    /// Производится тупой подбор пары (a,h).
    /// </summary>
    /// <param name="y"></param>
    /// <param name="start"></param>
    /// <param name="count"></param>
    /// <param name="i_m"></param>
    /// <param name="a"></param>
    /// <param name="x0"></param>
    /// <param name="h"></param>
    /// <param name="low_level"></param>
    /// <returns></returns>
    /// <remarks>UNCHECKED</remarks>
    private static int fit(float[] y, int start, int count, int i_m, ref float a, ref float x0, ref float h, ref float low_level)
    {
    	float a0 = 0, b0 = 0, w = 0;
      float min = 0, max = 0;
    	float h_min, s, s_min, a_min, b, b_min, diff, tmp;

      int i_h, i_a, i1, i2;
      int i = 0, n = count;

    	fit_linear_part(y, start, n, i_m, ref a0, ref b0, ref w);
#if DEBUG_ANALYZE_SGR
  	  System.Console.Error.WriteLine("{0} {1}", i_m-2, a0*i_m + b0);
  	  System.Console.Error.WriteLine("{0} {1}", i_m+2, a0*i_m + b0);
  	  System.Console.WriteLine(string.Empty);
  	  System.Console.WriteLine("w={0}", w);
#endif

	    if( a0 > 0 )
      {
    		i1 = (int)(i_m - w*1.5);
		    i2 = (int)(i_m + w*0.1);
	    }
      else
      {
  	  	i1 = (int)(i_m - w*0.1);
  	  	i2 = (int)(i_m + w*1.5);
  	  }	  
  
  	  i1 = System.Math.Max(0, i1);
      i2 = System.Math.Min(n - 1, i2);

      // search for local minimum & maximum at the interval [i1, i2]
      find_min_max(y, i1, i2 - i1 + 1, ref min, ref max);

	    s_min = -1;
	    h_min = 0;
	    a_min = a0;
	    b_min = b0;

	    for( i_a = 50; i_a >= 0; i_a-- )
      {
		    a = a0 * (1.0f - 0.15f * (25.0f - i_a) / 25.0f);
		    b = b0 + (a0 - a) * i_m;
		    for( i_h = 50; i_h > 0; i_h-- )
        {
  			  h = w * System.Math.Abs(a0) * 0.4f * i_h / 50;
			    s = 0;
			    for( i = i2; i >= i1; i-- )
          {
  				  tmp = f0(a, b, min, h, i);
				    diff = y[i] - tmp;
				    s += diff*diff;
			    }
			    if( (s_min<0) || (s_min>s) )
          {
				    s_min = s;
				    h_min = h;
				    a_min = a;
			  	  b_min = b;
		  	  }
	  	  }
  	  }

	    x0 = f0_cutoff(a_min, b_min, min, h_min, i);

#if DEBUG_ANALYZE_GSR
      System.Console.WriteLine( "a_min/a_edge = {0}", a_min / a0 );

      if( a0 > 0 )
      {
		    for(i=i2; i>=i1; i--)
        {
			    System.Console.Error.WriteLine("{0} {1}", i, f0(a_min, b_min, min, h_min, i));
        }
      }
  	  else
      {
		    for(i=i2; i >= i1; i--)
        {
			    System.Console.Error.WriteLine("{0} {1}", i, f0(a_min, b_min, min, h_min, i));
        }
      }

      System.Console.Error.WriteLine(string.Empty);
	    System.Console.Error.WriteLine("{0} {1}", x0, min - 0.02f * (max - min));
	    System.Console.Error.WriteLine("{0} {1}", x0, min + 0.02f * (max - min));
      System.Console.Error.WriteLine(string.Empty);
#endif

      h  = h_min;
      a  = a0;
	    low_level = min;

	    return 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="y"></param>
    /// <param name="rate"></param>
    /// <param name="L"></param>
    /// <param name="A"></param>
    /// <param name="V"></param>
    /// <param name="S"></param>
    /// <param name="D"></param>
    /// <param name="GS"></param>
    /// <returns></returns>
    /// <remarks>UNCHECKED</remarks>
    private static int process_sgr(float[] y, float rate, ref float L, ref float A, ref float V, ref float S, ref float D, ref float GS)
    {
	    float max = 0, min = 0, V2 = 0, ll1 = 0, ll2 = 0, integral;

	    float slope1_idx = 0, slope2_idx = 0, stub = 0;
	    int   a_idx = 0, i;
    	
	    if( y.Length <= SGR_MIN_POINTS_COUNT_FOR_TAN_ESTIMATION )
      {
		    throw new ArgumentException(string.Format(strings.LinearFit_TooFewPointsSpecified, y.Length, SGR_MIN_POINTS_COUNT_FOR_TAN_ESTIMATION));
      }

      // FIXME: // Redundant
	    find_min_max(y, 0, y.Length, ref min, ref max);

      // FIXME: Hardcoded constant
	    find_first_crossing_of_threshold(y, 0.5f*(max+min), 0, 1, ref slope1_idx);

      fit(y, 0, y.Length, (int)slope1_idx, ref V, ref L, ref stub, ref ll1);

	    L  = System.Math.Max(0, L); // Just in case

      // FIXME: Hardcoded constant
	    find_first_crossing_of_threshold(y, 0.5f*(max+min), (int) (slope1_idx + (max-min)/2/V), 1, ref slope2_idx);

      fit(y, 0, y.Length, (int)slope2_idx, ref V2, ref D, ref stub, ref ll2);

      D = System.Math.Min(y.Length-1, D);

	    if((V<=0)||(V2>=0))
      {
		    return -1;
      }

#if DEBUG_ANALYZE_GSR
    	System.Console.WriteLine("L=%f, D=%f\n", L, D);
#endif

      sgr_find_location_of_max(y, (int)L, (int)(D - L), ref A, ref a_idx);

	    a_idx += (int)L;

#if DEBUG_ANALYZE_GSR
	    System.Console.Error.WriteLine("{0} {1}", a_idx - 3, A);
	    System.Console.Error.WriteLine("{0} {1}", a_idx + 3, A);
      System.Console.Error.WriteLine(string.Empty);
#endif

      // calculate integral using method of trapezoids
	    integral = 0;
      for (i = (int)L; i < D; i++)
      {
        integral += y[i] + y[i + 1];
      }
	    integral *= 0.5f;

	    integral -= (float) ((D - L) * (ll1 + ll2) * 0.5); // subtract 'stand'.

      // TODO: Maybe location of this maximum must also be considered? In ideal case, zero levels to the left and to the rignt must be equal. So we have some arbitrary level here...
	    A = (float)(A - (ll1 + ll2) * 0.5);

    	L  = L / rate;
	    V  = V * rate;
	    S  = integral / rate;
	    D  = D / rate;
	    GS = A * D / rate;

	    return 0;
    }

    #endregion
  }
}
