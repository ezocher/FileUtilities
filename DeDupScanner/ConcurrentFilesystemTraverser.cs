using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace DeDupScanner
{
    class ConcurrentFilesystemTraverser
    {
        Stack<Tuple<DirectoryInfo, DirectoryFingerprint>> directories;
        Queue<Tuple<FileInfo, DirectoryFingerprint>> files;

        bool excludeHiddenSystemFilesDirs;
        const string excludeHiddenSystemKey = "ExcludeHiddenAndSystem";

        static readonly object _lockNextFile = new object();

        const string configFolderPathUserRelative = @"Repos\FileUtilities\Config\";
        const string directoriesConfigFile = "Directories.txt";
        const string filesIgnoreFile = "FilesIgnore.txt";
        const string settingsCategory = "Settings";
        const string ignoreCategory = "Ignore";
        const string extensionsIgnoreCategory = "Extensions Ignore";

        public ConcurrentFilesystemTraverser(string rootDirectoryPath)
        {
            directories = new Stack<Tuple<DirectoryInfo, DirectoryFingerprint>> ();
            files = new Queue<Tuple<FileInfo, DirectoryFingerprint>>();

            // TBD: handle exceptions on root directory
            DirectoryInfo di = new DirectoryInfo(rootDirectoryPath);
            directories.Push(Tuple.Create<DirectoryInfo, DirectoryFingerprint>(di, null));

            string configFolderPath = Path.Combine(Environment.GetFolderPath((Environment.SpecialFolder.UserProfile)), configFolderPathUserRelative);

            InitDirSkipList( rootDirectoryPath, Path.Combine(configFolderPath, directoriesConfigFile) );
            InitFileSkipList( rootDirectoryPath, Path.Combine(configFolderPath, filesIgnoreFile) );
        }

        // The path list in the config file (Directories.txt) contains full paths of directories to skip. It may or may not
        // include a volume letter. The volume letter will be added or changed to the volume being scanned as necessary.
        // A path won't be added to the skip list if it is a shorter name than where the scan is starting
        //      example paths:
        //          C:\Users\Public
        //          \Program Files
        //          \Windows.old
        static HashSet<string> DirectorySkipList;

        void InitDirSkipList(string rootDirectoryPath, string directoryConfigFilePath)
        {
            DirectorySkipList = new HashSet<string>();

            string volume = rootDirectoryPath.Substring(0, @"X:".Length);
            var dirList = ConfigFileUtil.LoadConfigFile(directoryConfigFilePath);

            foreach (ConfigSettings settings in dirList)
            {
                
                if (settings.Category == ignoreCategory)
                {
                    string newSkipPath = settings.Value;
                    Match match = Regex.Match(newSkipPath, @"^[a-zA-Z]:");
                    if (match.Success)
                        newSkipPath = newSkipPath.Remove(0, volume.Length);
                    newSkipPath = volume + newSkipPath;

                    if (newSkipPath.Length >= rootDirectoryPath.Length)
                        DirectorySkipList.Add(newSkipPath.ToLower());
                }
                else if ((settings.Category == settingsCategory) && (settings.Key == excludeHiddenSystemKey))
                {
                    excludeHiddenSystemFilesDirs = !(settings.Value.ToLower() == "false");
                }
            }
        }

        // The file extension list in the config file (FilesIgnore.txt) lists the extensions of files to be excluded from the scan
        // Extensions get converted to lower case before loading into this list and file extensions are lower-cased before comparing them
        static HashSet<string> ExtensionSkipList;

        void InitFileSkipList(string rootDirectoryPath, string extensionConfigFilePath)
        {
            ExtensionSkipList = new HashSet<string>();

            var extList = ConfigFileUtil.LoadConfigFile(extensionConfigFilePath);

            foreach (ConfigSettings settings in extList)
                if (settings.Category == extensionsIgnoreCategory)
                    ExtensionSkipList.Add(settings.Value.ToLower());
        }

        public Tuple<FileInfo, DirectoryFingerprint> NextFile()
        {
            Tuple<FileInfo, DirectoryFingerprint> file;

            lock (_lockNextFile)
            {
                if (files.Count > 0)
                    file = files.Dequeue();
                else
                {
                    while (files.Count == 0)
                    {
                        if (directories.Count == 0)
                        {
                            file = null;
                            goto finish; // exit the while and the else
                        }
                        ProcessDirectory(directories.Pop());
                    }
                    file = files.Dequeue();
                }

                finish:
                ;
            }

            return file;
        }
        
        void ProcessDirectory(Tuple<DirectoryInfo, DirectoryFingerprint> d)
        {
            bool directoryAccessException = false;
            string exceptionMessage = "";
            int fileCount = 0;
            int directoryCount = 0;
            DirectoryInfo dirInfo = d.Item1;
            DirectoryFingerprint parentFingerprint = d.Item2;

            if (DirectoryInSkipList(dirInfo))
            {
                parentFingerprint?.ChildDirectorySkipped();
                RunParallelScan.progress.SkipListDirSkipped(dirInfo);
                return;
            }

            DirectoryFingerprint myFingerprint = new DirectoryFingerprint(dirInfo, parentFingerprint);

            try
            {
                string reason;
                FileInfo[] nextFiles = dirInfo.GetFiles();
                foreach (FileInfo fi in nextFiles)
                    if (FileIncluded(fi, out reason))
                    {
                        files.Enqueue(Tuple.Create<FileInfo, DirectoryFingerprint>(fi, myFingerprint));
                        fileCount++;
                    }
                    else
                        RunParallelScan.progress.FileSkipped(fi, reason);
            }
            catch (UnauthorizedAccessException)
            {
                Error("Directory: " + dirInfo.FullName, "Access denied");
                directoryAccessException = true;
                exceptionMessage = "UnauthorizedAccessException";
            }
            catch (DirectoryNotFoundException)
            {
                Error("Directory: " + dirInfo.FullName, "Directory not found");
                directoryAccessException = true;
                exceptionMessage = "DirectoryNotFoundException";
            }
            catch (Exception e)
            {
                Error("Directory (Unexpected REF1): " + dirInfo.FullName, e.ToString() + "\n");
                // Unexpected exception, keep trying
            }

            // If we've had an UnauthorizedAccessException on this directory there's no need to try accessing it again as it will fail again
            if (directoryAccessException)
            {
                parentFingerprint?.ChildDirectorySkipped();
                RunParallelScan.progress.DirException(dirInfo, exceptionMessage);
                return;
            }
            else
            {
                try
                {
                    DirectoryInfo[] nextDirectories = dirInfo.GetDirectories();
                    Array.Reverse(nextDirectories); // Push the first directory found last so that it gets popped first
                                                    //  -- this results in a depth-first traversal in the natural directory order that Windows File Explorer shows
                    foreach (DirectoryInfo di in nextDirectories)
                        if (DirectoryIncluded(di))
                        {
                            directories.Push(Tuple.Create<DirectoryInfo, DirectoryFingerprint>(di, myFingerprint));
                            directoryCount++;
                        }
                        else
                            RunParallelScan.progress.HiddenSystemDirSkipped();

                    myFingerprint.SetNumberOfItems(fileCount, directoryCount);
                }
                catch (Exception e)
                {
                    Error("Directory (Unexpected REF2): " + dirInfo.FullName, e.ToString() + "\n");
                    parentFingerprint?.ChildDirectorySkipped();
                }
            }

            return;
        }

        bool DirectoryInSkipList(DirectoryInfo di)
        {
            return DirectorySkipList.Contains(di.FullName.ToLower());
        }

        void Error(string item, string errorMsg)
        {
            ConsoleUtil.Red();
            Console.WriteLine("\n*** " + item + ": " + errorMsg);
            ConsoleUtil.RestoreColors();
        }

        // TODO: Add stats to correctly count exclusion for different reasons?
        // TODO: Change reason to an enum?
        bool FileIncluded(FileInfo fi, out string reason)
        {
            // Exclude files of zero length
            if (fi.Length == 0)
            {
                reason = "zero length";
                return false;
            }
            
            if (excludeHiddenSystemFilesDirs)
            {
                // Exclude Hidden and System files
                if (FileUtil.IsSystemOrHidden(fi))
                {
                    reason = "System or Hidden";
                    return false;
                }
            }

            // Exclude extensions in skip list
            if ( ExtensionSkipList.Contains(fi.Extension.ToLower()) )
            {
                reason = "exension in skip list";
                return false;
            }

            reason = "Included";
            return true;
        }

        bool DirectoryIncluded(DirectoryInfo di)
        {
            if (!excludeHiddenSystemFilesDirs)
                return true;
            else
            {
                // Exclude Hidden and/or System directories
                bool include = !FileUtil.IsSystemOrHidden(di);
                // if (!include)
                //  Console.WriteLine("\nDirectory {0} is System and/or Hidden", di.FullName);
                return (include);
            }
        }

    }
}
