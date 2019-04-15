using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DeDupScanner
{
    class ConcurrentFilesystemTraverser
    {
        Stack<Tuple<DirectoryInfo, DirectoryFingerprint>> directories;
        Queue<Tuple<FileInfo, DirectoryFingerprint>> files;

        bool includeSystemHiddenFilesDirs;

        static readonly object _lockNextFile = new object();

        public ConcurrentFilesystemTraverser(string rootDirectoryPath, bool excludeSystemHiddenFilesDirs)
        {
            directories = new Stack<Tuple<DirectoryInfo, DirectoryFingerprint>> ();
            files = new Queue<Tuple<FileInfo, DirectoryFingerprint>>();

            // TBD: handle exceptions on root directory
            DirectoryInfo di = new DirectoryInfo(rootDirectoryPath);
            directories.Push(Tuple.Create<DirectoryInfo, DirectoryFingerprint>(di, null));

            includeSystemHiddenFilesDirs = !excludeSystemHiddenFilesDirs;

            InitDirSkipList(rootDirectoryPath);
        }

        // Path list below contains full paths of directories to skip, independent of volume letter
        // A path will be excluded from the list if it is higher in the directory tree than where the scan is starting
        static string[] DirectorySkipListPaths =
        {
            @"\Windows",
            @"\Windows.old",
            @"\Windows10Upgrade",
            @"\Program Files",
            @"\Program Files (x86)",
            @"\Users\ezoch\.nuget"
        };

        static HashSet<string> DirectorySkipList;

        void InitDirSkipList(string rootDirectoryPath)
        {
            DirectorySkipList = new HashSet<string>();

            string volume = rootDirectoryPath.Substring(0, @"X:".Length);
            foreach (string path in DirectorySkipListPaths)
            {
                string newSkipPath = volume + path;
                if (newSkipPath.Length >= rootDirectoryPath.Length)
                    DirectorySkipList.Add(newSkipPath);
            }
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
            int fileCount = 0;
            int directoryCount = 0;
            DirectoryInfo dirInfo = d.Item1;
            DirectoryFingerprint parentFingerprint = d.Item2;

            if (DirectoryInSkipList(dirInfo))
            {
                parentFingerprint?.ChildDirectorySkipped();
                RunParallelScan.progress.SkipListDirSkipped();
                return;
            }

            DirectoryFingerprint myFingerprint = new DirectoryFingerprint(dirInfo, parentFingerprint);

            try
            {
                FileInfo[] nextFiles = dirInfo.GetFiles();
                foreach (FileInfo fi in nextFiles)
                    if (FileIncluded(fi))
                    {
                        files.Enqueue(Tuple.Create<FileInfo, DirectoryFingerprint>(fi, myFingerprint));
                        fileCount++;
                    }
                    else
                        RunParallelScan.progress.HiddenSystemFileSkipped();
            }
            catch (UnauthorizedAccessException)
            {
                Error("Directory: " + dirInfo.FullName, "Access denied");
                directoryAccessException = true;
            }
            catch (DirectoryNotFoundException)
            {
                Error("Directory: " + dirInfo.FullName, "Directory not found");
                directoryAccessException = true;
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
                RunParallelScan.progress.DirException();
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
            return DirectorySkipList.Contains(di.FullName);
        }

        void Error(string item, string errorMsg)
        {
            ConsoleUtil.Red();
            Console.WriteLine("\n*** " + item + ": " + errorMsg);
            ConsoleUtil.RestoreColors();
        }

        bool FileIncluded(FileInfo fi)
        {
            if (includeSystemHiddenFilesDirs)
                return true;
            else
            {
                // Exclude Hidden and System files
                bool include = !FileUtil.IsSystemOrHidden(fi);
                // if (!include)
                //    Console.WriteLine("\nFile {0} is System and/or Hidden", fi.FullName);
                return (include);
            }
        }

        bool DirectoryIncluded(DirectoryInfo di)
        {
            if (includeSystemHiddenFilesDirs)
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
