using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DeDupScanner
{
    class Program
    {
        public static string baseName;

        static int hardwareThreads = Environment.ProcessorCount;
        static int numThreadsSolidStateDrive = hardwareThreads; // 1; // for testing // 
        static int numThreadsRotatingDrive = Math.Min(hardwareThreads, 3);  // 3 threads arrived at by observation on several rotating drives (internal and USB)
        static int numThreads;
        private static FileDB fileDB;

        [STAThreadAttribute]
        public static void Main(string[] args)
        {
            ConsoleUtil.InitConsoleSettings("CopyUniqueFiles - Under Development");

            // ConfigFileUtil.DumpConfigFiles(Path.Combine(Environment.GetFolderPath((Environment.SpecialFolder.UserProfile)), @"Repos\FileUtilities\Config\"));

            string scanRootDir = FileUtil.SelectDirectory();
            baseName = FileUtil.GetBaseName(scanRootDir); // e.g. "<system name> Vol C"
            Console.WriteLine("Copying unique files from '{0}'\n", scanRootDir);

            if ((scanRootDir == "") || (baseName == ""))
            {
                ConsoleUtil.WriteLineColor(String.Format("Error: scan directory '{0}' or directory name '{1}' is empty", scanRootDir, baseName),
                    ConsoleColor.Red);
                ConsoleUtil.WaitForKeyPress();
                return;
            }

            Console.Write("File list name is '{0}'? ", baseName);
            string input = Console.ReadLine();
            if (input != String.Empty)
                baseName = input;

            if (FileUtil.IsSystemDrive(scanRootDir))
                // All my current system drives are SSDs
                numThreads = numThreadsSolidStateDrive;
            else
                numThreads = numThreadsRotatingDrive;

            Console.Write("Run with {0} threads? ", numThreads);
            input = Console.ReadLine();
            int i;
            if (Int32.TryParse(input, out i))
                numThreads = i;

            Console.WriteLine("\nCreating report files '{0} - Unique Files Copied/Duplicate Files.tsv'", baseName);
            // Console.WriteLine("Read Buffer Size = {0}", FileUtil.FormatByteSize(ComputeFingerprint.ReadBufferSize));
            Console.WriteLine("Running {0} simultaneous threads on {1} hardware threads\n", numThreads, hardwareThreads);

            // Load in-memory database of existing files to check against for uniques
            fileDB = new FileDB();
            LoadFileLists.LoadBaseFileLists(fileDB);


            // RunParallelScan.ScanAndCopyUniques(baseName, scanRootDir, numThreads, fileDB);

            ConsoleUtil.WaitForKeyPress();
        }
    }
}


// Copied from FilesDB/UniqueFileCollector/Program.cs

//CopyUniqueFile.SetSourceVolumeName("ZB-DriveC");
//CopyUniqueFile.Copy(@"C:\Users\ezoch\Desktop\temp.html");
//CopyUniqueFile.Copy(@"C:\Users\ezoch\Desktop\LEFT MON\!Left DT - XMas\California wildfires- Is Trump right when he blames forest managers- - BBC News.url");

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
