using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class RunParallelScan
{
    public static ReportProgress progress;
    static ConcurrentFilesystemTraverser fst;
    static FileDB db;
    static string volumeName;

    public static void ScanAndCopyUniques(string baseName, string scanRootDir, int numThreads, FileDB fileDB)
    {
        ReportFiles.Open(baseName, scanRootDir);

        CopyUniqueFile.SetSourcePathRoot(scanRootDir);

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

    static string GetYearTaken(FileInfo fi)
    {
        // Test PhotoDateUtils
        (int? YearFromPath, int? LevelPath) = PhotoDateUtil.GetYearFromPath(fi.FullName);
        int? YearFromMetadata = PhotoDateUtil.ExtractMetadataYearTaken(fi.FullName);


        // TODO: This is a temporary location for this report to get details which we won't need later
        if (AppSettings.CreatePhotosReport)
        {
            progress.ReportPhotoVideo(fi, (YearFromPath.HasValue)     ? YearFromPath.Value     : 0, 
                                          (LevelPath.HasValue)        ? LevelPath.Value        : 0,
                                          (YearFromMetadata.HasValue) ? YearFromMetadata.Value : 0,
                                          CopyUniqueFile.GetFileCategory(fi)                        );
        }

        // If year is available in the file path, use it first.
        //  Some photos have incorrect metadata because of camera dates not being set correctly
        if (YearFromPath.HasValue)
        {
            return YearFromPath.Value.ToString();
        }
        else if (YearFromMetadata.HasValue)
        {
            return YearFromMetadata.Value.ToString();
        }
        else 
        {
            return AppSettings.unknownYearName;
        }
    }


    static void FileProcessor(int threadIndex)
    {
        Tuple<FileInfo, DirectoryFingerprint> file;
        // int filesProcessedByThisThread = 0;

        while ((file = fst.NextFile()) != null)
        {
            FileInfo fi = file.Item1;
            DirectoryFingerprint parentFingerprint = file.Item2;

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
                    string destinationFullName, category, yearTaken = "";

                    if (AppSettings.WhichApp == App.PhotoCollector)
                        yearTaken = GetYearTaken(fi);

                    CopyUniqueFile.Copy(fi.FullName, out destinationFullName, out category, yearTaken);

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
