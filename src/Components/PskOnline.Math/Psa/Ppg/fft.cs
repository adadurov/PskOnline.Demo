using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Math.Psa.Ppg
{
  class fft
  {
    /// <summary>
    /// FIXME Можно заменить на более эффективную в отношении памяти и более чем в 2 раза быструю
    /// x - входной массив длиной n
    /// out - выходной массив длины n/2
    /// buffer - буффер размером 4*n
    /// </summary>
    /// <param name="input"></param>
    /// <param name="start_input"></param>
    /// <param name="power"></param>
    /// <param name="start_power"></param>
    /// <param name="n"></param>
    /// <param name="buffer"></param>
    public static void rft_psd_slow(float[] input, int start_input, float[] power, int start_power, int n, float[] buffer)
    {
      if (null == buffer || (buffer.Length < (4*n)) )
      {
        buffer = new float[4 * n];
      }

	    int i;
	    float[] r;
	
	    for( i= 0; i < n; i++ )
      {
		    buffer[2*i  ] = input[start_input + i];
		    buffer[2*i+1] = 0;
	    }
	
	    r = fft.fft_destructive(buffer, 0, buffer, 2*n, n);

      for (i = 0; i < n / 2; i++)
      {
        power[start_power + i] = (r[2 * i] * r[2 * i] + r[2 * i + 1] * r[2 * i + 1]);
      }
    }


    /// <summary>
    /// Fast Complex Fourrier Transform
    /// </summary>
    /// <param name="x">input data in the following format x[2*i]=inp[i].real, x[2*i+1]=inp[i].imag, array length == 2*n</param>
    /// <param name="b">array (for user as an intermediate buffer) array length must be - 2*n</param>
    /// <returns>reference to the resulting array (in fact it must be either x or b</returns>
    /// <remarks>the content of X is destroyed</remarks>
    public static float[] fft_destructive(float[] x, int start_x, float[] b, int start_b, int n)
    {
	    int step, n_, arrays;
	    int i, i1, i2, ia, id1, id2;
	    float[] ptmp;
	    double c,s,er,ei, tmp;

	    step = 1;
	    for( i = n; i > 1; i /= 2 )
      {
        // step size in array elements (not in pairs of re-im)
		    step *= 2;
      }

      // length of evaluated arrays of complex numbers
      n_ = 2;

      // number of arrays to be processed
      arrays = n/2;
	    
      while( arrays != 0 )
      {
		    c = System.Math.Cos(2.0 * System.Math.PI / n_);
        s = -System.Math.Sin(2.0 * System.Math.PI / n_);
		    for( ia = 0; ia < arrays; ia++ )
        {
			    er = 1;
			    ei = 0;
			    i1 = ia * 2;
			    i2 = ia * 2 + step;
			    id1 = i1;
			    id2 = i1 + step * n_/2;
			    for( i = n_/2 - 1; i >= 0; i-- )
          {
            b[start_b + id1] =      (float)(x[start_x + i1] +     x[start_x + i2] * er - x[start_x + i2 + 1] * ei);
            b[start_b + id1 + 1] =  (float)(x[start_x + i1 + 1] + x[start_x + i2] * ei + x[start_x + i2 + 1] * er);
            b[start_b + id2] =      (float)(x[start_x + i1] -     x[start_x + i2] * er + x[start_x + i2 + 1] * ei);
            b[start_b + id2 + 1] =  (float)(x[start_x + i1 + 1] - x[start_x + i2] * ei - x[start_x + i2 + 1] * er);

				    i1 += step * 2;
				    i2 += step * 2;
				    id1 += step;
				    id2 += step;
				    tmp = er*c - ei*s;
				    ei = er*s + ei*c;
				    er = tmp;
			    }
		    }
		    step /= 2;
		    arrays /= 2;
		    n_ *= 2;

		    ptmp = x;
		    x = b;
		    b = ptmp;
	    }
      return x;
    }
  }
}
