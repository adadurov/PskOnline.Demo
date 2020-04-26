using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Methods.Hrv.Processing.Contracts
{
  public interface IStateMatrixStateClassifier
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="M_NN"></param>
    /// <returns>row index in the range of [0, 4]</returns>
    int GetStateMatrix_Row(double M_NN);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="SIGMA_NN"></param>
    /// <returns>column index in the range of [0, 4]</returns>
    int GetStateMatrix_Col(double SIGMA_NN);

    double[] GetRowClassBoundary();

    //        double GetRowMidPoint();

    /// <summary>
    /// returns heart rate mid point (row midpoint) rounded
    /// to the nearest integer value
    /// based on bounds predefined in StateMatrixClassifier
    /// </summary>
    /// <returns></returns>
    int GetHeartRateMidPointForSettings();

    double[] GetColClassBoundary();

    //        double GetColumnMidPoint();
  }
}
