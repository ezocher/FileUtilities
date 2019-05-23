using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class LoadFileList
{
    private const string BaseFileListsFolder = @"C:\FileDB\Base File Collection";

    // TODO - Update this to new naming convention with KnownFilesDBMaker
    private const string ListFileNameFilter = "File List - *.txt";

    public static void LoadBaseFileLists(FileDB db)
    {
        // Open directory with file lists and load them all
        FileInfo[] files;

        try
        {
            files = new DirectoryInfo(BaseFileListsFolder).GetFiles(ListFileNameFilter);
        }
        catch (Exception e)
        {
            ConsoleUtil.WriteLineColor(String.Format("Couldn't open base file collection directory {0}: {1}",
                BaseFileListsFolder, e.Message), ConsoleColor.Red);
            return;
        }

        foreach (FileInfo file in files)
        {
            Load(file.FullName, db);
            db.DisplayStatsToConsole();
        }

    }

    public static void Load(string path, FileDB db)
    {
        ConsoleUtil.WriteColor(String.Format("Loading '{0}'...\n", path), ConsoleColor.White);
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
                    FileDescription fd = FileDescription.ParseRecord(line);
                    if (fd != null)
                        db.AddRecord(fd);
                }
            }
        }
        catch (Exception e)
        {
            ConsoleUtil.WriteLineColor(String.Format("*** Exception loading report file '{0}' - {1}", path, e.Message),
                ConsoleColor.Red);
        }

    }
}


