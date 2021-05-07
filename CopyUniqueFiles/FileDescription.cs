using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class FileDescription
{
    public string VolumeName;              // Volume/Media/Machine (first one in scan priority order)
    public string Machine;
    public string FullPath;                // Only store full paths since Path.GetFileName() .GetExtension() etc. are cheap
    // Which dates? None for now
    public long Length;
    // TODO: List? of clouds it's on?
    public string Fingerprint;
    public int NumberOfCopies;             // During scan, duplicates found just increment this counter

    FileDescription()
    {

    }

    private static long ParseLength(string lengthString)
    {
        if (long.TryParse(lengthString, out long x))
            return (x);
        else
            return 0;
    }
    // From DeDuper project/xxxx.cs:
    //    const string FilesReportHeader = "Num\tVolume\tCreation Time\tLast Write Time\tLast Acc Time\tAttributes\tFull Path\tExt\tFile Name\tLength\tChecksum";
    //    const string FilesReportFormat = "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}";

    // Returns null if the line doesn't contain a full record
    public static FileDescription ParseRecord(string line)
    {
        const int FieldIndexVolumeName = 1;
        const int FieldIndexFullPath = 6;
        const int FieldIndexLength = 9;
        const int FieldIndexFingerprint = 10;

        FileDescription fd = new FileDescription();
        string[] fields = line.Split('\t');

        if (fields.Length < (FieldIndexFingerprint + 1))
        {
            ConsoleUtil.WriteLineColor(String.Format("*** Bad line read: '{0}'", line), ConsoleColor.Red);
            return null;
        }

        fd.VolumeName = fields[FieldIndexVolumeName];
        fd.Machine = ""; // TODO: Implement in scan then implement here
        fd.FullPath = fields[FieldIndexFullPath];
        fd.Length = ParseLength(fields[FieldIndexLength]);
        fd.Fingerprint = fields[FieldIndexFingerprint];
        fd.NumberOfCopies = 1; // The 1 is counting this file
        return (fd);
    }
}
