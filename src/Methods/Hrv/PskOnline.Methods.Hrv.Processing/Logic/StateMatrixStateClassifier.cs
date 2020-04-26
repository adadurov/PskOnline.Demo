namespace PskOnline.Methods.Hrv.Processing.Logic
{
  using System;
  using System.Collections.Generic;
  using System.Text;

  using PskOnline.Methods.Processing.Logic;
  using PskOnline.Methods.Hrv.Processing.Contracts;

  public static class StateMatrixStateClassifierFactory
  {
    /// <summary>
    /// ¬озвращает классификатор дл€ нормального значени€ пульса в покое "по умолчанию"
    /// или на базе "общепопул€ционной нормы"
    /// </summary>
    public static IStateMatrixStateClassifier GetStateMatrixClassifier()
    {
      return new PersonalStateMatrixClassifier();
    }

    /// <summary>
    /// ¬озвращает классификатор дл€ заданного нормального значени€ пульса в покое
    /// </summary>
    /// <param name="heart_rate_at_rest_bpm"></param>
    /// <returns></returns>
    public static IStateMatrixStateClassifier GetStateMatrixClassifier(int heart_rate_at_rest_bpm)
    {
      return new PersonalStateMatrixClassifier(heart_rate_at_rest_bpm);
    }
  }



  public class PersonalStateMatrixClassifier : IStateMatrixStateClassifier
  {
    internal PersonalStateMatrixClassifier()
    {
      Init(DefaultStateMatrix.GetHeartRateMidPointForSettings());
    }

    internal PersonalStateMatrixClassifier(int heart_rate_at_rest)
    {
      Init(heart_rate_at_rest);
    }

    private void Init(int heart_rate_at_rest)
    {
      _rowClassBoundaries = (double[])DefaultStateMatrix.GetRowClassBoundary().Clone();
      double double_midpoint = DefaultStateMatrix.GetMidPoint(_rowClassBoundaries);

      int default_heart_rate_at_rest = DefaultStateMatrix.GetHeartRateMidPointForSettings();
      if (default_heart_rate_at_rest != heart_rate_at_rest)
      {
        double double_heart_rate_at_rest = heart_rate_at_rest;
        double nn_interval_at_rest =
            DefaultStateMatrix.milliseconds_per_minute / double_heart_rate_at_rest;
        double offset = (nn_interval_at_rest - double_midpoint);

        for (int i = 0; i < _rowClassBoundaries.Length; ++i)
        {
          _rowClassBoundaries[i] += offset;
        }
      }

      _colClassBoundaries = DefaultStateMatrix.GetColClassBoundary();
    }

    public int GetStateMatrix_Row(double M_NN)
    {
      return ClassificationHelper.GetClass(M_NN, GetRowClassBoundary());
    }

    public int GetStateMatrix_Col(double SIGMA_NN)
    {
      return ClassificationHelper.GetClass(SIGMA_NN, GetColClassBoundary());
    }

    public double[] GetRowClassBoundary()
    {
      return (double[])_rowClassBoundaries;
    }

    public double[] GetColClassBoundary()
    {
      return (double[])_colClassBoundaries.Clone();
    }

    private double[] _rowClassBoundaries;
    private double[] _colClassBoundaries;

    #region IStateMatrixStateClassifier Members


    public double GetRowMidPoint()
    {
      throw new Exception("The method or operation is not implemented.");
    }

    public double GetColumnMidPoint()
    {
      throw new Exception("The method or operation is not implemented.");
    }

    public int GetHeartRateMidPointForSettings()
    {
      return DefaultStateMatrix.GetHeartRateMidPointForSettings(_rowClassBoundaries);
    }

    #endregion
  }
  
  public static class DefaultStateMatrix
  {
    public const double milliseconds_per_minute = 60.0d * 1000.0d;

    /// <summary>
    /// TODO: remove this function
    /// </summary>
    /// <param name="M_NN"></param>
    /// <returns>row index in the range of [0, 4]</returns>
    static int GetStateMatrix_Row(double M_NN)
    {
      return ClassificationHelper.GetClass(M_NN, GetRowClassBoundary());
    }

    /// <summary>
    /// TODO: remove this function
    /// </summary>
    /// <param name="SIGMA_NN"></param>
    /// <returns>column index in the range of [0, 4]</returns>
    static int GetStateMatrix_Col(double SIGMA_NN)
    {
      return ClassificationHelper.GetClass(SIGMA_NN, GetColClassBoundary());
    }

    public static double[] GetRowClassBoundary()
    {
      return new double[] { 0, 607, 673, 850, 938, 2000 };
    }

    public static double[] GetColClassBoundary()
    {
      return new double[] { 0, 22, 31, 55, 76, 100 };
    }

    /// <summary>
    /// returns heart rate mid point rounded
    /// to the nearest integer value
    /// based on bounds predefined in StateMatrixClassifier
    /// </summary>
    /// <returns></returns>
    public static int GetHeartRateMidPointForSettings()
    {
      return GetHeartRateMidPointForSettings(GetRowClassBoundary());
    }

    /// <summary>
    /// returns heart rate mid point rounded
    /// to the nearest integer value
    /// </summary>
    /// <returns></returns>
    public static int GetHeartRateMidPointForSettings(double[] bounds)
    {
      return (int)Math.Round(milliseconds_per_minute / GetMidPoint(bounds));
    }

    public static double GetMidPoint(double[] bounds)
    {
      if (0 == bounds.Length)
      {
        throw new Exception("processing.StateMatrixStateClassifier.GetRowClassBoundary() returned empty array!");
      }

      if (1 == bounds.Length)
      {
        return bounds[0];
      }

      int k = bounds.Length / 2;
      if (0 == (bounds.Length % 2))
      {
        return 0.5d * (bounds[k] + bounds[k - 1]);
      }
      else
      {
        return bounds[k];
      }

    }
  }

}
