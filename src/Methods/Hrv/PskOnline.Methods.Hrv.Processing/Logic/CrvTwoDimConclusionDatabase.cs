namespace PskOnline.Methods.Hrv.Processing.Logic
{
  using PskOnline.Methods.Hrv.Processing.Contracts;

  public abstract class CrvTwoDimConclusionDatabase : ITwoDimConclusionDatabase
  {
    private readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(CrvTwoDimConclusionDatabase));
    
    public virtual string GetTitle()
    {
      return strings.TwoDimConclusionDatabase;
    }
    
    /// <summary>
    /// non-strict about cultures (may fall back to parent cultures and ultimately to default culture)
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <returns></returns>
    public string GetConclusion(int row, int col)
    {
      return GetConclusion(row, col, System.Threading.Thread.CurrentThread.CurrentUICulture, false);
    }

    public string GetConclusion(int row, int col, System.Globalization.CultureInfo culture, bool bStrict)
    {
      System.Resources.ResourceManager rm = GetResourceManager();
      if( null != rm )
      {
        string stringId = GetStringId(row, col);
        string result = null;

        if ( bStrict)
        {
          System.Resources.ResourceSet rs = rm.GetResourceSet(culture, true, false);
          if (null != rs)
          {
            result = rs.GetString(stringId);
          }
          else
          {
            _log.Debug($"ResourceSet could not be acquired for culture '{culture}' (strict).");
          }
        }
        else
        {
          result = rm.GetString(stringId, culture);
        }

        if ((null == result) || (0 == result.Length))
        {
          _log.Debug($"No string with id='{stringId}' found for culture '{culture}'. ResourceManager: {rm.BaseName}");
          return string.Empty;
        }

        return result;
      }
      else
      {
        _log.Debug("Resource manager not acquired.");
      }
      return string.Empty;
    }

    protected abstract System.Resources.ResourceManager GetResourceManager();
    protected abstract string GetStringId(int row, int col);
  }
}
