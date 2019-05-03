using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class FileDB
{
    static Dictionary<string, FileDescription> db;

    public static void Initialize()
    {
        db = new Dictionary<string, FileDescription>();
    }

    public static void AddRecord(FileDescription fd)
    {
        if (db.ContainsKey(fd.Fingerprint))
            db[fd.Fingerprint].NumberOfCopies++;
        else
            db[fd.Fingerprint] = fd;   
    }
}

