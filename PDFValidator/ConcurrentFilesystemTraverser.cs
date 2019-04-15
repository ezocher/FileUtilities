using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ConcurrentFilesystemTraverser
{
    class ConcurrentFilesystemTraverser
    {
        Stack<DirectoryInfo> directories;
        Queue<FileInfo> files;

        public ConcurrentFilesystemTraverser(string rootDirectoryPath)
        {
            directories = new Stack<DirectoryInfo>();
            files = new Queue<FileInfo>();

            // TBD: handle exceptions
            DirectoryInfo di = new DirectoryInfo(rootDirectoryPath);
            directories.Push(di);
        }

        public FileInfo NextFile()
        {
            FileInfo fi;

            // TBD: Lock this shit
            if (files.Count > 0)
                fi = files.Dequeue();
            else
            {
                while (files.Count == 0)
                {
                    if (directories.Count == 0)
                    {
                        fi = null;
                        goto finish; // exit the while and the else
                    }
                    ProcessDirectory(directories.Pop());
                }
                fi = files.Dequeue();
            }

            finish:
            // TBD: Unlock
            return fi;
        }
        
        void ProcessDirectory(DirectoryInfo dirInfo)
        {
            bool directoryAccessException = false;


            try
            {
                FileInfo[] nextFiles = dirInfo.GetFiles();
                foreach (FileInfo fi in nextFiles)
                    if (FileIncluded(fi))
                        files.Enqueue(fi);
            }
            catch (UnauthorizedAccessException)
            {
                Error("Directory: " + dirInfo.FullName, "Access denied");
                directoryAccessException = true;
            }
            catch (Exception ex)
            {
                Error("Directory: " + dirInfo.FullName, ex.ToString() + "\n");
            }

            // If we've had an UnauthorizedAccessException on this directory there's no need to try accessing it again as it will fail again
            if (!directoryAccessException)
            {
                try
                {
                    DirectoryInfo[] nextDirectories = dirInfo.GetDirectories();
                    Array.Reverse(nextDirectories); // Push the first directory found last so that it gets popped first
                                                    //  -- this results in a depth-first traversal in the natural directory order that Windows File Explorer shows
                    foreach (DirectoryInfo di in nextDirectories)
                        if (DirectoryIncluded(di))
                            directories.Push(di);
                }
                catch (Exception ex)
                {
                    Error("Directory: " + dirInfo.FullName, ex.ToString() + "\n");
                }
            }

            return;
        }

        void Error(string item, string errorMsg)
        {
            Console.WriteLine("\n*** " + item + "\n   " + errorMsg);
        }

        bool FileIncluded(FileInfo fi)
        {
            // Exclude Hidden and System files
            bool include = !FileUtil.IsSystemOrHidden(fi);
            // if (!include)
            //    Console.WriteLine("\nFile {0} is System and/or Hidden", fi.FullName);
            return (include);
        }

        bool DirectoryIncluded(DirectoryInfo di)
        {
            // Exclude Hidden and/or System directories
            bool include = !FileUtil.IsSystemOrHidden(di);
            // if (!include)
            //  Console.WriteLine("\nDirectory {0} is System and/or Hidden", di.FullName);
            return (include);
        }
    }
}
