namespace PskOnline.Methods.Hrv.Processing.Logic.Personal
{
  /// <summary>
  /// this class returns conclusion for given state matrix cell
  /// </summary>
  public class CrvTwoDimConclusionDatabase_Personal : CrvTwoDimConclusionDatabase
  {
    protected override System.Resources.ResourceManager GetResourceManager()
    {
      return two_dim_conclusions_personal.ResourceManager;
    }

    protected override string GetStringId(int row, int col)
    {
      return $"Conclusion_user_{row}_{col}";
    }
  }
}
