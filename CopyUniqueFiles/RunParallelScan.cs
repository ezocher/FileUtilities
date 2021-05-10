using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeDupScanner
{
    class RunParallelScan
    {
        public static ReportProgress progress;
        static ConcurrentFilesystemTraverser fst;
        static FileDB db;
        static string volumeName;

        public static void ScanAndCopyUniques(string baseName, string scanRootDir, int numThreads, FileDB fileDB)
        {
            ReportFiles.Open(baseName, scanRootDir);

            fst = new ConcurrentFilesystemTraverser(scanRootDir);

            db = fileDB;
            volumeName = baseName;

            progress = new ReportProgress(numThreads);
            progress.Start();

            int nThreads = numThreads;
            Parallel.For(0, nThreads, i => { FileProcessor(i); });

            progress.Stop();
            progress.DisplayFinalSummary();

            // ReportFiles.Close(); // Files closed under lock by progress.DisplayFinalSummary()
        }


        static void FileProcessor(int threadIndex)
        {
            Tuple<FileInfo, DirectoryFingerprint> file;
            // int filesProcessedByThisThread = 0;

            while ((file = fst.NextFile()) != null)
            {
                FileInfo fi = file.Item1;
                DirectoryFingerprint parentFingerprint = file.Item2;

                // TODO: check if file should be skipped

                string fileChecksum = ComputeFingerprint.FileChecksum(fi.FullName);
                if (fileChecksum == "")
                {
                    parentFingerprint.ChildFileSkipped();
                }
                else
                {
                    string originalFilePath;
                    if ( db.IsUniqueFile(fi, fileChecksum, volumeName, out originalFilePath))
                    {
                        string destinationFullName, category;
                        CopyUniqueFile.Copy(fi.FullName, out destinationFullName, out category);

                        progress.UniqueFileCompleted(fi, destinationFullName, fileChecksum, category);
                    }
                    else
                        progress.DuplicateFileCompleted(fi, originalFilePath, fileChecksum);

                    parentFingerprint.FileCompleted(fileChecksum);
                }
            }


            progress.ThreadCompleted();
        }

    }
}
