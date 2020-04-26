namespace PskOnline.Methods.Processing.Logic
{
  using System;
  using System.Collections.Generic;
  using System.Text;

  public static class ClassificationHelper
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="bounds"></param>
    /// <returns>class index in the range of [0, bounds.Length-2]</returns>
    public static int GetClass(double value, double[] bounds)
    {
      if (bounds.Length < 2)
      {
        throw new ArgumentException("bounds array must have more than one element");
      }
      for (int i = 1; i < (bounds.Length - 1); ++i)
      {
        if (bounds[i - 1] >= bounds[i])
        {
          throw new ArgumentException("bounds array must be monotone-accending");
        }
        if (bounds[i] > value)
        {
          return i - 1;
        }
      }
      return bounds.Length - 2;
    }
  }
}
