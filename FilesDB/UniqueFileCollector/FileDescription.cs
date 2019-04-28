using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class FileDescription
{
    string VolumeName;              // Volume/Media/Machine (first one in scan priority order)
    string? Machine;
    string FullPath;                // Only store full paths since Path.GetFileName() .GetExtension() etc. are cheap
    // Which dates? None for now
    long Length;
    // TODO: List? of clouds it's on?
    int NumberOfCopies;             // During scan, duplicates found just increment this counter

    FileDescription()
    {

    }

    public FileDescription ParseRecord(string line)
    {
        FileDescription fd = new FileDescription();
        string[] fields = line.Split('\t');
        fd.VolumeName = fields[0];
        fd.Machine = null; // TODO: Implement in scan then implement here
        fd.FullPath = fields[999];
        fd.Length = fields[999].Try Parse????
    }
}
