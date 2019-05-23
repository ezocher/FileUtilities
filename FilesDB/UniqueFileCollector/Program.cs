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
            string listFilePath = FileUtil.SelectTextFile();
            if (listFilePath != "")
            {
                LoadFileList.Load(listFilePath, db);
            }

            ConsoleUtil.WaitForKeyPress();
        }
    }
}
