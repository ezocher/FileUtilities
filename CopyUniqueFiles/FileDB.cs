using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// In-memory database of existing files to check against for uniques
class FileDB
{
    Dictionary<string, FileDescription> db;

    long numUniqueFiles = 0,
        totalSizeOfUniqueFiles = 0,
        numFilesWithDuplicates = 0,
        numTotalDuplicateFiles = 0,
        totalSizeOfDuplicates = 0,
        mostCopiesOfAFile = 1;
    string mostCopiesFilePath = "";

    public FileDB()
    {
        db = new Dictionary<string, FileDescription>();
    }

    public void AddRecord(FileDescription fd)
    {
        if (db.ContainsKey(fd.Fingerprint))
        {
            numTotalDuplicateFiles++;
            totalSizeOfDuplicates += fd.Length;

            if (db[fd.Fingerprint].NumberOfCopies == 1)
            {
                numFilesWithDuplicates++; // Count this once when first duplicate is encountered
            }

            int n = db[fd.Fingerprint].NumberOfCopies++;
            if (n > mostCopiesOfAFile)
            {
                mostCopiesOfAFile = n;
                mostCopiesFilePath = db[fd.Fingerprint].FullPath;
            }
        }
        else
        {
            numUniqueFiles++;
            totalSizeOfUniqueFiles += fd.Length;
            db.Add(fd.Fingerprint, fd);
        }
    }

    public void DisplayStatsToConsole()
    {
        Console.WriteLine("   Number of unique files = {0:N0} (Total size = {1})", numUniqueFiles,
            FileUtil.FormatByteSize(totalSizeOfUniqueFiles));
        Console.WriteLine("   Number of files with duplicates = {0:N0} ({1:F1}%)", numFilesWithDuplicates,
            (float)numFilesWithDuplicates / (float)(numUniqueFiles + numFilesWithDuplicates) * 100.0f);
        Console.WriteLine("   Total number of duplicates = {0:N0} (Total size = {1}, {2:F1}%)", numTotalDuplicateFiles,
            FileUtil.FormatByteSize(totalSizeOfDuplicates), 
            (float)totalSizeOfDuplicates / (float)(totalSizeOfUniqueFiles + totalSizeOfDuplicates) * 100.0f);
        Console.WriteLine("\n   Most copies of a single file = {0:N0} ('{1}')\n", mostCopiesOfAFile, mostCopiesFilePath);
    }

    public void ResetFileStatistics()
    {
        numUniqueFiles = 0;
        totalSizeOfUniqueFiles = 0;
        numFilesWithDuplicates = 0;
        numTotalDuplicateFiles = 0;
        totalSizeOfDuplicates = 0;
        mostCopiesOfAFile = 1;
        mostCopiesFilePath = "";
    }

    readonly object _lockDBQueryAndInsert = new object();
    public bool IsUniqueFile(FileInfo fi, string fingerprint, string basename, out string originalFileFullPath)
    {
        bool result;

        // Lock prevents two identical files coming in at the same time, both thinking they're unique
        lock (_lockDBQueryAndInsert)
        {
            result = !db.ContainsKey(fingerprint);
            if (result)
                db.Add(fingerprint, new FileDescription(fi, fingerprint, basename));
        }

        if (result)
            originalFileFullPath = null;
        else
        {
            FileDescription fd = db[fingerprint];
            originalFileFullPath = fd.FullPath;
        }

        return result;
    }
}

