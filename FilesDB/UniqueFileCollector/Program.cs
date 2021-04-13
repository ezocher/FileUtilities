using System;
using System.Collections.Generic;
using System.IO;
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

            CopyUniqueFile.SetSourceVolumeName("sbook C");
            CopyUniqueFile.Copy(@"C:\Users\ezoch\Desktop\TBS\Bainbridge speed limit - beach house dock - 4 second rule.xlsx");
            CopyUniqueFile.Copy(@"C:\Users\ezoch\Desktop\bitcoin.pdf");

            //FileDB db = new FileDB();
            //string listFilePath;

            //LoadFileList.LoadBaseFileLists(db);

            //do
            //{
            //    listFilePath = FileUtil.SelectTextFile();
            //    if (listFilePath == "")
            //        break;

            //    LoadFileList.Load(listFilePath, db);
            //    db.DisplayStatsToConsole();
            //}
            //while (true);

            ConsoleUtil.WaitForKeyPress();
        }
    }
}
