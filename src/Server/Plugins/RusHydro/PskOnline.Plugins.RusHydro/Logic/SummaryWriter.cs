namespace PskOnline.Server.Plugins.RusHydro.Logic
{
  using System;
  using System.IO;
  using System.Collections.Generic;

  using PskOnline.Server.Plugins.RusHydro.ObjectModel;

  /// <summary>
  /// Сохраняет на диск сводку по результатам предсменного контроля.
  /// 
  /// </summary>
  public class SummaryWriter : ISummaryWriter
  {
    private log4net.ILog _log = log4net.LogManager.GetLogger(typeof (SummaryWriter));
    private string[] _preferredPaths = new string[0];
    private SummaryWritingParameters _parameters;
    private DateTime _shiftStartDate;
    private bool _isDayShift;

    public bool SaveSummary(
      SummaryWritingParameters parameters,
      string summaryContent,
      out string usedSummaryFileName
      )
    {
      _parameters = parameters;
      InitFallbackPaths(parameters.baseSummaryFolderPath);
      _shiftStartDate = RusHydroScheduler.GetShiftStartDate(parameters.completionTime.DateTime, out _isDayShift, out int _);

      bool bSummarySaved = false;
      string fullFileName = string.Empty;
      int attempt = 0;
      do
      {
        string folderName = CreateFolderForSummary(_shiftStartDate, _isDayShift, _preferredPaths[attempt], attempt);
        if( string.IsNullOrEmpty(folderName) )
        {
          // cannot create required subfolders in any of fallback folders
          break;
        }
        string fileName = BuildUniqueFileNameForSummary(
          folderName, 
          parameters.filenameExtension, 
          parameters.completionTime, 
          parameters.employee, 
          parameters.hostName
          );
        fullFileName = Path.Combine(folderName, fileName);
        bSummarySaved = WriteSummaryToFile(fullFileName, summaryContent);
      }
      while (!bSummarySaved && ++attempt < _preferredPaths.Length);
      if( bSummarySaved )
      {
        usedSummaryFileName = fullFileName;
      }
      else
      {
        usedSummaryFileName = string.Empty;
      }

      return bSummarySaved;
    }

    /// <summary>
    /// builds unique file name in the given folder, following spec
    /// 
    ///     <ГГГГ>.<ММ>.<ДД>_<ЧЧ:мм>_<ФИО>_<ИМЯ_ЭВМ>
    /// 
    /// </summary>
    /// <param name="completionDate"></param>
    /// <param name="employee"></param>
    /// <param name="hostName"></param>
    /// <returns>full path to the unique file</returns>
    private string BuildUniqueFileNameForSummary(string path, string extension, DateTimeOffset completionDate, Employee employee, string hostName)
    {
      char[] invalidPathChars = Path.GetInvalidPathChars();
      string employeeName = employee.FullName;
      foreach( char c in invalidPathChars )
      {
        string s = new string(c, 1);
        string s_ = System.Web.HttpUtility.UrlEncode(s);
        if( s != s_ )
        {
          employeeName = employeeName.Replace(s, s_);
        }
        else
        {
          employeeName = employeeName.Replace(s, "_");
        }
      }

      // <ГГГГ>.<ММ>.<ДД>_<ЧЧ:мм>_<ФИО>_<ИМЯ_ЭВМ>
      string filename = string.Format("{0}_{1}_{2}", 
        completionDate.ToString("yyyy.MM.dd_HH.mm") + $"_{completionDate.Offset.Hours}", employeeName, hostName);
      string fullPath;

      while (File.Exists(fullPath = Path.Combine(path, filename + "." + extension)))
      {
        filename = filename + "_" + new Random((int)DateTime.Now.ToBinary()).Next().ToString("X");
      }

      return fullPath;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="summary"></param>
    /// <returns>true upon success, false otherwise</returns>
    private bool WriteSummaryToFile(string filename, string summary)
    {
      try
      {
        File.WriteAllLines(filename, new string[] { summary });
      }
      catch (Exception ex)
      {
        _log.Error(ex);
        return false;
      }
      return true;
    }

    /// <summary>
    /// tries to create subfolders required to store summary in preferredPath
    /// 
    /// falls back to _PreferredPath[attemptedPathIndex + 1] etc
    /// in case initial folder can't be used to create subfolders
    /// logs errors
    /// 
    /// </summary>
    /// <param name="completionDate"></param>
    /// <param name="isDayShift"></param>
    /// <param name="preferredPath"></param>
    /// <param name="attemptedPathIndex"></param>
    /// <returns>created path or empty string in case of failure</returns>
    private string CreateFolderForSummary(
      DateTime completionDate,
      bool isDayShift,
      string preferredPath,
      int attemptedPathIndex)
    {
      bool pathCreated = false;
      string basePath = preferredPath;
      string problematicPath = basePath;

      do
      {
        try
        {
          Directory.CreateDirectory(basePath);

          string pathYear = Path.Combine(basePath, completionDate.Year.ToString("0000"));
          problematicPath = pathYear;
          Directory.CreateDirectory(pathYear);
          
          string pathMonth = Path.Combine(pathYear, completionDate.Month.ToString("00"));
          problematicPath = pathMonth;
          Directory.CreateDirectory(pathMonth);
          
          string pathDay = Path.Combine(pathMonth, completionDate.Day.ToString("00"));
          problematicPath = pathDay;
          Directory.CreateDirectory(pathDay);

          string pathShift = Path.Combine(pathDay, isDayShift ? strings._day : strings._night);
          Directory.CreateDirectory(pathShift);
          pathCreated = true;
          basePath = pathShift;
        }
        catch (Exception ex)
        {
          basePath = GetFallbackPath(++attemptedPathIndex);
          _log.Error(ex);
          _log.ErrorFormat(strings.ErrorCreateFolder_UseFallback_Format, problematicPath, basePath);
        }
      } while (!pathCreated && attemptedPathIndex <= _preferredPaths.Length);

      if( pathCreated )
      {
        return basePath;
      }
      
      // фатальная ошибка -- не смогли создать нужный подкаталог
      // ни в одной из резервных папок
      return string.Empty;
    }

    private void InitFallbackPaths(string basePath)
    {
      List<string> paths = new List<string>(5);

      paths.AddRange(new [] {
          SummaryPathHelper.MakeSummaryPath(Environment.SpecialFolder.MyDocuments),
          SummaryPathHelper.MakeSummaryPath(Environment.SpecialFolder.Desktop),
          SummaryPathHelper.MakeSummaryPath(Environment.SpecialFolder.Personal),
          SummaryPathHelper.MakeSummaryPath(Environment.SpecialFolder.LocalApplicationData)
        }
      );
 
      if (string.IsNullOrEmpty(basePath))
      {
        _log.ErrorFormat(strings.BaseFolderForSummaryNotSpecified_Format, paths[0]);
      }
      else
      {
        paths.Insert(0, basePath);
      }

      _preferredPaths = paths.ToArray();
    }

    private string GetFallbackPath(int attempt)
    {
      if( attempt >= _preferredPaths.Length )
      {
        throw new ArgumentException(
          string.Format("Attempt index ({0}) is greater than max possible index in fallback options array ({1})", attempt, _preferredPaths.Length - 1), "attempt"
          );
      }
      return _preferredPaths[attempt];
    }

  }
}
