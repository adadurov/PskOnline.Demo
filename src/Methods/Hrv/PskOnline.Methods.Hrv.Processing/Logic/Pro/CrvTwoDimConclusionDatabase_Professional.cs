using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Methods.Hrv.Processing.Logic.Pro
{
  /// <summary>
  /// this class returns conclusion for given state matrix cell
  /// </summary>
  public class CrvTwoDimConclusionDatabase_Professional : CrvTwoDimConclusionDatabase
  {
    protected override System.Resources.ResourceManager GetResourceManager()
    {
      return two_dim_conclusions_pro.ResourceManager;
    }

    protected override string GetStringId(int row, int col)
    {
      return $"Conclusion_prof_{row}_{col}";
    }
  }
}
