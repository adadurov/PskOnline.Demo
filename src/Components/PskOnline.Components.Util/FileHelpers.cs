namespace PskOnline.Components.Util
{
  using System;
  using System.IO;
  using System.Reflection;
  using log4net;

  public static class FileHelpers
  {

    /// <summary>
    /// Copy a Directory, SubDirectories and Files Given a Source and Destination DirectoryInfo
    ///    Object, Given a SubDirectory Filter and a File Filter.
    /// IMPORTANT: The search strings for SubDirectories and Files applies to every Folder and
    ///    File within the Source Directory.
    /// <param name="SourceDirectory">A DirectoryInfo Object Pointing to the Source Directory</param>
    /// <param name="DestinationDirectory">A DirectoryInfo Object Pointing to the Destination Directory</param>
    /// <param name="SourceDirectoryFilter">Search String on SubDirectories (Example: "System*" will return
    ///    all subdirectories starting with "System")</param>
    /// <param name="SourceFileFilter">File Filter: Standard DOS-Style Format (Examples: "*.txt" or "*.exe")</param>
    /// <param name="Overwrite">Whether or not to Overwrite Copied Files in the Destination Directory</param>
    /// </summary>

    public static void Copy(DirectoryInfo SourceDirectory, DirectoryInfo DestinationDirectory,
                            string SourceDirectoryFilter, string SourceFileFilter, bool Overwrite)
    {
      DirectoryInfo[] SourceSubDirectories;
      FileInfo[] SourceFiles;

      //Check for File Filter
      if (SourceFileFilter != null)
      {
        SourceFiles = SourceDirectory.GetFiles(SourceFileFilter.Trim());
      }
      else
      {
        SourceFiles = SourceDirectory.GetFiles();
      }

      // Check for Folder Filter
      if (SourceDirectoryFilter != null)
      {
        SourceSubDirectories = SourceDirectory.GetDirectories(SourceDirectoryFilter.Trim());
      }
      else
      {
        SourceSubDirectories = SourceDirectory.GetDirectories();
      }

      // Create the Destination Directory
      if (!DestinationDirectory.Exists)
      {
        DestinationDirectory.Create();
      }

      // Recursively Copy Every SubDirectory and it's Contents (according to folder filter)
      foreach (DirectoryInfo SourceSubDirectory in SourceSubDirectories)
      {
        Copy(SourceSubDirectory, new DirectoryInfo(DestinationDirectory.FullName + @"\" + SourceSubDirectory.Name), SourceDirectoryFilter, SourceFileFilter, Overwrite);
      }

      //Copy Every File to Destination Directory (according to file filter)
      foreach (FileInfo SourceFile in SourceFiles)
      {
        SourceFile.CopyTo(DestinationDirectory.FullName + @"\" + SourceFile.Name, Overwrite);
      }
    }

    /// <summary>
    /// Создает резервную копию существующего файла
    /// (копирует его в исходную папку, добавляя к имени .backup
    /// В остальном аналогична перегруженной версии с 3 параметрами
    /// </summary>
    public static void BackupExistingFile(string filename, bool removeOriginal)
    {
      BackupExistingFile(filename, "backup", removeOriginal);
    }

    /// <summary>
    /// Создает резервную копию существующего файла
    /// (копирует его в исходную папку, добавляя к имени .backup[.x],
    /// где X-порядковый номер резервной копии, если их существует несколько.
    /// X -- первое число от 1 до 10, для которого резервная копия с таким номером не существует.
    /// т.е. если в папке есть копии 1, 2, 3, 4, 7, 8, 9, 10, то при следующем вызове этой функции
    /// будет создана копия №5
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="removeOriginal">удалить исходный файл после данной операции?</param>
    /// <remarks>Создает не более 10 резервных копий одного файла.
    /// Если в папке уже есть 10-я резервная копия, она будет замещена.
    /// если удалить 10-ю копию не удается, функция не выполнит свою функцию ;-)
    /// </remarks>
    public static bool BackupExistingFile(string filename, string backupSuffix, bool removeOriginal)
    {
      if (!File.Exists(filename))
      {
        log.WarnFormat("BackupExistingFile: the specified file does not exist ({0})", filename);
        return false;
      }

      try
      {
        int counter = 0;
        string backup_tail = "." + backupSuffix;
        string name_with_tail = filename + backup_tail;

        string backup_name = name_with_tail;

        while (File.Exists(backup_name) && (counter < 11))
        {
          backup_name = name_with_tail + String.Format(".{0}", ++counter);
        }

        File.Delete(backup_name);

        if (removeOriginal)
        {
          File.Move(filename, backup_name);
        }
        else
        {
          File.Copy(filename, backup_name);
        }
        return true;
      }
      catch (Exception ex)
      {
        log.ErrorFormat(
          "Cannot backup file {0} (suffix={1}, remove original={2}). Details follow.",
          filename, backupSuffix, removeOriginal);
        log.Error(ex);
        return false;
      }
    }

    /// <summary>
    /// комбинирует относительный путь relative_path с путем к сборке, выполняющей данный вызов 
    /// метода GetFullPathRelativeToExecutingAssemblyPath.
    /// </summary>
    /// <param name="relative_path"></param>
    /// <returns></returns>
    public static string GetPathFromExecutingAssembly(string relative_path)
    {
      return Path.GetFullPath(Path.Combine(GetAssemblyFolderPath(Assembly.GetCallingAssembly()), relative_path));
    }

    public static string GetAssemblyFolderPath(Assembly assembly)
    {
      Uri uri = new Uri(assembly.CodeBase);
      string path = uri.LocalPath;
      return Path.GetDirectoryName(path);
    }

    public static string GetExecutingAssemblyFolderPath()
    {
      return GetAssemblyFolderPath(Assembly.GetCallingAssembly());
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="fromPath">You can specify a file or a folder here. Folder name must have trailing slash. Otherwise, it will have </param>
    /// <param name="toPath"></param>
    /// <returns></returns>
    public static String MakeRelativePath(String fromPath, String toPath)
    {
      if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
      if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

      Uri fromUri = new Uri(fromPath);
      Uri toUri = new Uri(toPath);

      if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

      Uri relativeUri = fromUri.MakeRelativeUri(toUri);
      String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

      if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
      {
        relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
      }

      return relativePath;
    }

    private static ILog log = LogManager.GetLogger(typeof(FileHelpers));
  }
}
