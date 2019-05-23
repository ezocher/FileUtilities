using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniqueFileCollector
{
    class Program
    {
        [STAThreadAttribute]
        public static void Main(string[] args)
        {
            ConsoleUtil.InitConsoleSettings("Unique File Collector - Under Development");

            FileDB db = new FileDB();
            string listFilePath;

            LoadFileList.LoadBaseFileLists(db);

            do
            {
                listFilePath = FileUtil.SelectTextFile();
                if (listFilePath == "")
                    break;

                LoadFileList.Load(listFilePath, db);
                db.DisplayStatsToConsole();
            }
            while (true);

            ConsoleUtil.WaitForKeyPress();
        }
    }
}
