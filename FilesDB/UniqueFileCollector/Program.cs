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

            CopyUniqueFile.SetSourceVolumeName("ZB-DriveC");
            CopyUniqueFile.Copy(@"C:\Users\ezoch\Desktop\temp.html");
            CopyUniqueFile.Copy(@"C:\Users\ezoch\Desktop\LEFT MON\!Left DT - XMas\California wildfires- Is Trump right when he blames forest managers- - BBC News.url");

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
