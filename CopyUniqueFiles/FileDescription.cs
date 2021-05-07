using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// Per-file record for in-memory database of existing files to check against for uniques
//  Fingerprint is used as the database key (in a Dictionary<Fingerprint, FileDescription>) and is duplicated in each record for convenience
class FileDescription
{
    // Fields in file list reports 
    //      Num, Volume, Creation Time, Last Write Time, Last Acc Time, Attributes, Full Path, Ext, File Name, Length, Checksum
    public string VolumeName;              // Volume/Directory
    //  Creation Time, Last Write Time, Last Acc Time, Attributes -- exclude all for now
    public string FullPath;                // Only store full paths since Path.GetFileName() .GetExtension() etc. are cheap
    public long Length;

    public string Fingerprint;          // a/k/a Cheksum
    public int NumberOfCopies;          // During scan, duplicates found just increment this counter

    // TODO: List? of file clouds it's on? Would need to capture this at initial scan time or derive by recognizing cloud dirctory roots (vs. backups)

    private FileDescription() { }

    private static long ParseLength(string lengthString)
    {
        if (long.TryParse(lengthString, out long x))
            return (x);
        else
            return 0;
    }

    // From ReportFiles.cs
    //    const string FilesReportHeader = "Num\tVolume\tCreation Time\tLast Write Time\tLast Acc Time\tAttributes\tFull Path\tExt\tFile Name\tLength\tChecksum";
    //    const string FilesReportFormat = "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}";

    // Returns null if the line doesn't contain a full record
    public static FileDescription ParseRecord(string line)
    {
        const int FieldIndexVolumeName = 1;
        const int FieldIndexFullPath = 6;
        const int FieldIndexLength = 9;
        const int FieldIndexFingerprint = 10;

        const int FieldIndexLastField = FieldIndexFingerprint;

        FileDescription fd = new FileDescription();
        string[] fields = line.Split('\t');

        if (fields.Length < (FieldIndexLastField + 1))
        {
            ConsoleUtil.WriteLineColor(String.Format("*** Bad line read: '{0}'", line), ConsoleColor.Red);
            return null;
        }

        fd.VolumeName = fields[FieldIndexVolumeName];
        fd.FullPath = fields[FieldIndexFullPath];
        fd.Length = ParseLength(fields[FieldIndexLength]);
        fd.Fingerprint = fields[FieldIndexFingerprint];
        fd.NumberOfCopies = 1; // The 1 is counting this file
        return (fd);
    }
}
