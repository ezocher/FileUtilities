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

        public static void Run(string baseName, string scanRootDir, int numThreads)
        {
            ReportFiles.Open(baseName, scanRootDir);

            fst = new ConcurrentFilesystemTraverser(scanRootDir, true);  // true: exclude System and Hidden files and dirs

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
                    progress.FileCompleted(fi, fileChecksum);
                    parentFingerprint.FileCompleted(fileChecksum);

                    // filesProcessedByThisThread++;
                }
            }

            // Console.WriteLine("\nThread {0} completed - {1} files processed\n", threadIndex, filesProcessedByThisThread);
            progress.ThreadCompleted();
        }

    }
}
