using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class ReportFiles
{
    static StreamWriter filesDB;
    static StreamWriter excludedReport;
    static StreamWriter duplicatesReport;
    static StreamWriter uniquesReport;
    static StreamWriter photosReport;

    private static string reportsDirectoryPath;

    private static string ReportFileName(string baseName, string reportSuffix) =>
        (reportsDirectoryPath + Path.DirectorySeparatorChar + baseName + reportSuffix + AppSettings.ReportFilesExtension);

    public static void Open(string baseName, string scanRootDir)
    {
        reportsDirectoryPath = Path.Combine(AppSettings.AppRootPath, AppSettings.ReportFilesSubdirectory);
        Directory.CreateDirectory(reportsDirectoryPath);

        filesDB = new StreamWriter(FileUtil.GetUniqueFileName(
            ReportFileName(baseName, AppSettings.FilesDBNameSuffix)), false); // Append = true
        filesDB.WriteLine(FilesReportHeader);

        excludedReport = new StreamWriter(FileUtil.GetUniqueFileName(
            ReportFileName(baseName, AppSettings.ExcludedReportNameSuffix)), false); // Append = true
        excludedReport.WriteLine(ExcludedReportHeader);

        duplicatesReport = new StreamWriter(FileUtil.GetUniqueFileName(
            ReportFileName(baseName, AppSettings.DuplicatesReportNameSuffix)), false); // Append = true
        duplicatesReport.WriteLine(DuplicatesReportHeader);

        uniquesReport = new StreamWriter(FileUtil.GetUniqueFileName(
            ReportFileName(baseName, AppSettings.UniquesReportNameSuffix)), false); // Append = true
        uniquesReport.WriteLine(UniquesReportHeader);

        if ((AppSettings.WhichApp == App.PhotoCollector) && (AppSettings.CreatePhotosReport))
        {
            photosReport = new StreamWriter(FileUtil.GetUniqueFileName(
                ReportFileName(baseName, AppSettings.PhotosReportNameSuffix)), false); // Append = true
            photosReport.WriteLine(PhotosReportHeader);
        }
    }

    public static void Close()
    {
        filesDB.Close();
        excludedReport.Close();
        duplicatesReport.Close();
        uniquesReport.Close();
        if (photosReport != null)
            photosReport.Close();
    }


    const string FilesReportHeader = "Num\tVolume\tCreation Time\tLast Write Time\tLast Acc Time\tAttributes\tFull Path\tExt\tFile Name\tLength\tChecksum";
    const string FilesReportFormat = "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}";

    public static void WriteFileInfo(FileInfo fi, string copiedFileFullPath, string baseName, string fileFingerprint, int numFilesCompleted)
    {
        filesDB.WriteLine(FilesReportFormat, numFilesCompleted, baseName,
            fi.CreationTime, fi.LastWriteTime, fi.LastAccessTime,
            fi.Attributes, // fi.IsReadOnly, - ReadOnly is included FileInfo.Attributes
            copiedFileFullPath, fi.Extension, fi.Name, fi.Length, fileFingerprint);
    }


    const string ExcludedReportHeader = "Num\tF or D\tFull Path\tReason\tDetail";
    const string ExcludedReportFormat = "{0}\t{1}\t{2}\t{3}\t{4}";
    static int excludedReportLineNum = 1;

    public static void WriteExcludedInfo(bool isFile, string fullPath, string reason, string detail)
    {
        excludedReport.WriteLine(ExcludedReportFormat, excludedReportLineNum++, isFile ? "File" : "Dir", 
            fullPath, reason, detail);
    }


    const string DuplicatesReportHeader = "Num\tOriginal Full Path\tDuplicate Full Path\tDup Ext\tDup File Name\tLength\tChecksum";
    const string DuplicatesReportFormat = "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}";

    public static void WriteDuplicateInfo(string originalFileFullPath, FileInfo fi, string fileFingerprint, int numDuplicatesFound)
    {
        duplicatesReport.WriteLine(DuplicatesReportFormat, numDuplicatesFound, originalFileFullPath,
            fi.FullName, fi.Extension, fi.Name, fi.Length, fileFingerprint);
    }


    const string UniquesReportHeader = "Num\tSource Full Path\tCopied Full Path\tExt\tFile Name\tCategory\tLength\tChecksum";
    const string UniquesReportFormat = "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}";

    public static void WriteUniqueInfo(FileInfo fi, string copiedFileFullPath, string fileFingerprint, int numUniquesFound, string category)
    {
        if (AppSettings.WhichApp == App.FingerprintDBMaker)
            copiedFileFullPath = "";
            
        uniquesReport.WriteLine(UniquesReportFormat, numUniquesFound,
            fi.FullName, copiedFileFullPath, fi.Extension, fi.Name, category, fi.Length, fileFingerprint);
    }

    const string PhotosReportHeader = "Source Full Path\tYear Path\tPath Level\tYear Meta\tExt\tFile Name\tCategory\tLength";
    const string PhotosReportFormat = "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}";

    public static void WritePhotoVideoInfo(FileInfo fi, int yearFromPath, int pathLevel, int yearFromMetadata, string category)
    {
        photosReport.WriteLine(PhotosReportFormat, 
            fi.FullName, yearFromPath, pathLevel, yearFromMetadata, fi.Extension, fi.Name, category, fi.Length);
    }

}
