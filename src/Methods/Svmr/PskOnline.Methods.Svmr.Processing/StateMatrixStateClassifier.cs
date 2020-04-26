namespace PskOnline.Methods.Svmr.Processing
{
  using System.Collections.Generic;
  using PskOnline.Methods.Processing.Logic;

  public static class StateMatrixStateClassifier
  {

    public static int GetStateMatrix_Row(double M_reaction)
    {
      return ClassificationHelper.GetClass(M_reaction, GetRowClassBoundary());
    }

    public static int GetStateMatrix_Col(int row, double SIGMA_reaction)
    {
      double[] colClassBoundaries = GetColClassBoundary();
      return ClassificationHelper.GetClass(SIGMA_reaction, colClassBoundaries);
    }

    public static double[] GetRowClassBoundary()
    {
      return new double[] { 100, 224, 240, 299, 400, 600 };
    }

    public static double[] GetColClassBoundary()
    {
      return new double[] { 10, 27, 35, 67, 92, 120 };
    }

    public static double[] GetColClassBoundary_Inverted()
    {
      var values = new List<double>(GetColClassBoundary());
      for( int i = 0; i < values.Count; ++i )
      {
        values[i] = 0 - values[i];
      }
      values.Reverse();
      return values.ToArray();
    }

  }

}
