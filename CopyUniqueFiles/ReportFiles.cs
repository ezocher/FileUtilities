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
        static StreamWriter excludedReport;
        static StreamWriter duplicatesReport;
        static StreamWriter uniquesReport;

        static int excludedReportLineNum = 1;

        const string FilesReportHeader = "Num\tVolume\tCreation Time\tLast Write Time\tLast Acc Time\tAttributes\tFull Path\tExt\tFile Name\tLength\tChecksum";
        const string FilesReportFormat = "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}";

        const string ExcludedReportHeader = "Num\tF or D\tFull Path\tReason\tDetail";
        const string ExcludedReportFormat = "{0}\t{1}\t{2}\t{3}\t{4}";

        const string DuplicatesReportHeader = "Num\tOriginal Full Path\tDuplicate Full Path\tDup Ext\tDup File Name\tLength\tChecksum";
        const string DuplicatesReportFormat = "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}";

        const string UniquesReportHeader = "Num\tSource Full Path\tCopied Full Path\tExt\tFile Name\tLength\tChecksum";
        const string UniquesReportFormat = "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}";


        public static void Open(string baseName, string scanRootDir)
        {
            const string ReportsFolderName = ".Reports";
            const string ReportFilesExtension = ".tsv";
            const string FilesReportNameSuffix = " - File List";
            const string ExcludedReportNameSuffix = " - Excluded List";
            const string DuplicatesReportNameSuffix = " - Duplicate Files";
            const string UniquesReportNameSuffix = " - Unique Files";

            string reportsDirectoryPath = System.Windows.Forms.Application.StartupPath + Path.DirectorySeparatorChar + ReportsFolderName;
            Directory.CreateDirectory(reportsDirectoryPath);

            string filesReportFullName = reportsDirectoryPath + Path.DirectorySeparatorChar + baseName + FilesReportNameSuffix + ReportFilesExtension;
            filesReportFullName = FileUtil.GetUniqueFileName(filesReportFullName);
            filesReport = new StreamWriter(filesReportFullName, false); // Append = true
            filesReport.WriteLine(FilesReportHeader);

            string excludedReportFullName = reportsDirectoryPath + Path.DirectorySeparatorChar + baseName + ExcludedReportNameSuffix + ReportFilesExtension;
            excludedReportFullName = FileUtil.GetUniqueFileName(excludedReportFullName);
            excludedReport = new StreamWriter(excludedReportFullName, false); // Append = true
            excludedReport.WriteLine(ExcludedReportHeader);

            string duplicatesReportFullName = reportsDirectoryPath + Path.DirectorySeparatorChar + baseName + DuplicatesReportNameSuffix + ReportFilesExtension;
            duplicatesReportFullName = FileUtil.GetUniqueFileName(duplicatesReportFullName);
            duplicatesReport = new StreamWriter(duplicatesReportFullName, false); // Append = true
            duplicatesReport.WriteLine(DuplicatesReportHeader);

            string uniquesReportFullName = reportsDirectoryPath + Path.DirectorySeparatorChar + baseName + UniquesReportNameSuffix + ReportFilesExtension;
            uniquesReportFullName = FileUtil.GetUniqueFileName(uniquesReportFullName);
            uniquesReport = new StreamWriter(uniquesReportFullName, false); // Append = true
            uniquesReport.WriteLine(UniquesReportHeader);
        }

        public static void Close()
        {
            filesReport.Close();
            excludedReport.Close();
            duplicatesReport.Close();
            uniquesReport.Close();
        }

        public static void WriteFileInfo(FileInfo fi, string baseName, string fileFingerprint, int numFilesCompleted)
        {
            filesReport.WriteLine(FilesReportFormat, numFilesCompleted, baseName,
                fi.CreationTime, fi.LastWriteTime, fi.LastAccessTime,
                fi.Attributes, // fi.IsReadOnly, - ReadOnly is included FileInfo.Attributes
                fi.FullName, fi.Extension, fi.Name, fi.Length, fileFingerprint);
        }

        public static void WriteExcludedInfo(bool isFile, string fullPath, string reason, string detail)
        {
            excludedReport.WriteLine(ExcludedReportFormat, excludedReportLineNum++, isFile ? "File" : "Dir", 
                fullPath, reason, detail);
        }

        public static void WriteDuplicateInfo(string originalFileFullPath, FileInfo fi, string fileFingerprint, int numDuplicatesFound)
        {
            duplicatesReport.WriteLine(DuplicatesReportFormat, numDuplicatesFound, originalFileFullPath,
                fi.FullName, fi.Extension, fi.Name, fi.Length, fileFingerprint);
        }

        public static void WriteUniqueInfo(FileInfo fi, string copiedFileFullPath, string fileFingerprint, int numUniquesFound)
        {
            uniquesReport.WriteLine(UniquesReportFormat, numUniquesFound,
                fi.FullName, copiedFileFullPath, fi.Extension, fi.Name, fi.Length, fileFingerprint);
        }
    }
}
