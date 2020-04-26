namespace PskOnline.Server.Plugins.RusHydro.Logic
{
  using System;
  using System.IO;

  /// <summary>
  /// Предоставляет простые функции для работы с путями
  /// 
  /// </summary>
  public static class SummaryPathHelper
  {
    public static string DefaultRepositoryFileName
    {
      get
      {
        return "rushydro_psa_summary.repository";
      }
    }

    public static string MakeSummaryPath(string basePath)
    {
      string path = Path.Combine(basePath, strings.Rushydro_PSA_folder);
      System.IO.Directory.CreateDirectory(path);
      return path;
    }

    public static string MakeSummaryPath(Environment.SpecialFolder specialFolder)
    {
      string path = Path.Combine(Environment.GetFolderPath(specialFolder), strings.Rushydro_PSA_folder);
      System.IO.Directory.CreateDirectory(path);
      return path;
    }
  }
}
