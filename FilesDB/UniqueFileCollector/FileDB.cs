using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class FileDB
{
    Dictionary<string, FileDescription> db;
    long numUniqueFiles = 0, 
        numFilesWithDuplicates = 0,
        numTotalDuplicateFiles = 0,
        mostCopiesOfAFile = 0;

    public FileDB()
    {
        db = new Dictionary<string, FileDescription>();
    }

    public void AddRecord(FileDescription fd)
    {
        if (db.ContainsKey(fd.Fingerprint))
        {
            if ()
            db[fd.Fingerprint].NumberOfCopies++;
        else
        {

            numUniqueFiles++;
            db[fd.Fingerprint] = fd;
        }
            
    }
}

