using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class FileDB
{
    Dictionary<string, FileDescription> db;
    long numUniqueFiles = 0,
        totalSizeOfUniqueFiles = 0,
        numFilesWithDuplicates = 0,
        numTotalDuplicateFiles = 0,
        totalSizeOfDuplicates = 0,
        mostCopiesOfAFile = 1;
    string mostCopiedFilePath = "";

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
                numFilesWithDuplicates++; // Count this when first duplicate is encountered
            }

            int n = db[fd.Fingerprint].NumberOfCopies++;
            if (n > mostCopiesOfAFile)
            {
                mostCopiesOfAFile = n;
                mostCopiedFilePath = db[fd.Fingerprint].FullPath;
            }
        }
        else
        {
            numUniqueFiles++;
            totalSizeOfUniqueFiles += fd.Length;
            db[fd.Fingerprint] = fd;
        }

    }

    public void DisplayStatsToConsole()
    {
        Console.WriteLine("Number of unique files = {0:N0} (Total size = {1})", numUniqueFiles,
            FileUtil.FormatByteSize(totalSizeOfUniqueFiles));
        Console.WriteLine("   Number of files with duplicates = {0:N0} ({1:F1}%)", numFilesWithDuplicates,
            (float)numFilesWithDuplicates / (float)numUniqueFiles * 100.0f);
        Console.WriteLine("   Total number of duplicates = {0:N0} (Total size = {1})", numTotalDuplicateFiles,
            FileUtil.FormatByteSize(totalSizeOfDuplicates));
        Console.WriteLine("\n   Most duplicates of a single file = {0:N0} ('{1}')\n", mostCopiesOfAFile, mostCopiedFilePath);
    }

}

