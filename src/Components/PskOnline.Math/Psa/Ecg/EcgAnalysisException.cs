using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Math.Psa.Ecg
{
  /// <summary>
  /// ECG/PPG analysis exceptions.
  /// </summary>
  public class EcgAnalysisException : System.Exception
  {
    public EcgAnalysisException(int ret_code)
    {
      this.return_code = ret_code;
    }

    /// <summary>
    /// External analysis function's error code
    /// </summary>
    public int return_code = 0;
  }

}
