using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Server.Plugins.RusHydro.Web
{
  /// <summary>
  /// the method set that requires SVMR (35+ responses) and HRV
  /// (100 inter-beat intervals) to generate a RusHydro-specific summary
  /// </summary>
  public static class RushydroPsaMethodSetId
  {
    public static string Value { get { return "rushydro_pre-shift_477842F3-F20D-4b82-A977-789BE58DCE59"; } }
  }
}
