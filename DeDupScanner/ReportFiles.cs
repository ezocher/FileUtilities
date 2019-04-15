using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeDupScanner
{
    class ReportFiles
    {
        static StreamWriter filesReport;
        static StreamWriter directoriesReport;

        const string FilesReportHeader = "Num\tVolume\tCreation Time\tLast Write Time\tLast Acc Time\tAttributes\tFull Path\tExt\tFile Name\tLength\tChecksum";
        const string FilesReportFormat = "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}";


        const string DirectoriesReportHeader = "Num\tVolume\tCreation Time\tLast Write Time\tLast Acc Time\tAttributes\tFull Path\tDir Name\tNum Scanned\tNum Items\tChecksum";
        const string DirectoriesReportFormat = "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}";

        public static void Open(string baseName, string scanRootDir)
        {
            const string ReportsFolderName = ".Reports";
            const string ReportFilesExtension = ".txt";
            const string FilesReportNamePrefix = "File List - ";
            const string DirectoriesReportNamePrefix = "Directory List - ";


            string reportsDirectoryPath = System.Windows.Forms.Application.StartupPath + Path.DirectorySeparatorChar + ReportsFolderName;
            Directory.CreateDirectory(reportsDirectoryPath);

            string filesReportFullName = reportsDirectoryPath + Path.DirectorySeparatorChar + FilesReportNamePrefix + baseName + ReportFilesExtension;
            filesReportFullName = FileUtil.GetUniqueFileName(filesReportFullName);
            filesReport = new StreamWriter(filesReportFullName, false); // Append = true
            // fileReport.WriteLine("Starting scan of {0}\n", scanRootDir);
            filesReport.WriteLine(FilesReportHeader);

            string directoriesReportFullName = reportsDirectoryPath + Path.DirectorySeparatorChar + DirectoriesReportNamePrefix + baseName + ReportFilesExtension;
            directoriesReportFullName = FileUtil.GetUniqueFileName(directoriesReportFullName);
            directoriesReport = new StreamWriter(directoriesReportFullName, false); // Append = true
            // directoryReport.WriteLine("Starting scan of {0}\n", scanRootDir);
            directoriesReport.WriteLine(DirectoriesReportHeader);
        }

        public static void Close()
        {
            filesReport.Close();
            directoriesReport.Close();
        }

        public static void WriteFileInfo(FileInfo fi, string baseName, string fileFingerprint, int numFilesCompleted)
        {
            filesReport.WriteLine(FilesReportFormat, numFilesCompleted, baseName,
                fi.CreationTime, fi.LastWriteTime, fi.LastAccessTime,
                fi.Attributes, // fi.IsReadOnly, - ReadOnly is included FileInfo.Attributes
                fi.FullName, fi.Extension, fi.Name, fi.Length, fileFingerprint);
        }

        public static void WriteDirectoryInfo(DirectoryInfo di, string baseName, int numItemsScanned, int totalNumItems, string directoryFingerprint, int numDirectoriesCompleted)
        {
            directoriesReport.WriteLine(DirectoriesReportFormat, numDirectoriesCompleted, baseName,
                di.CreationTime, di.LastWriteTime, di.LastAccessTime,
                di.Attributes, // fi.IsReadOnly, - ReadOnly is included FileInfo.Attributes
                di.FullName, di.Name, numItemsScanned, totalNumItems, directoryFingerprint);
        }

    }
}
