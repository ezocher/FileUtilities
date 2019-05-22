using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class LoadFileList
{
    public static void Load(string path, FileDB db)
    {
        try
        {
            bool firstLine = true;
            foreach (string line in File.ReadLines(path))
            {
                if (firstLine)
                {
                    firstLine = false;   // And do nothing more with the first line which contains the header of column labels
                }
                else
                {
                    db.AddRecord(FileDescription.ParseRecord(line));
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("*** Exception loading report file '{0}' - {1}", path, e.Message);
        }

    }
}


