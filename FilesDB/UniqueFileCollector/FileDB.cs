using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class FileDB
{
    Dictionary<string, FileDescription> db;

    public FileDB()
    {
        db = new Dictionary<string, FileDescription>();
    }

    public void AddRecord(FileDescription fd)
    {
        if (db.ContainsKey(fd.Fingerprint))
            db[fd.Fingerprint].NumberOfCopies++;
        else
            db[fd.Fingerprint] = fd;
    }
}

