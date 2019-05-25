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

        [STAThreadAttribute]
        public static void Main(string[] args)
        {
            ConsoleUtil.InitConsoleSettings("DeDup Scanner - Under Development");

            string configFolderPath = Path.Combine(Environment.GetFolderPath((Environment.SpecialFolder.UserProfile)), @"Repos\FileUtilities\Config\");
            var testList = ConfigFileUtil.LoadConfigFile(Path.Combine(configFolderPath, "TestConfigFile.txt"));

            string scanRootDir = FileUtil.SelectDirectory();
            baseName = FileUtil.GetBaseName(scanRootDir); // e.g. "<system name> Vol C"

            if ((scanRootDir == "") || (baseName == ""))
            {
                Console.WriteLine("Error: scan directory '{0}' or directory name '{1}' is empty", scanRootDir, baseName);
                ConsoleUtil.WaitForKeyPress();
                return;
            }

            if (FileUtil.IsDriveSolidState(scanRootDir))
                numThreads = numThreadsSolidStateDrive;
            else
                numThreads = numThreadsRotatingDrive;

            Console.Write("Run with {0} threads? ", numThreads);
            string input = Console.ReadLine();
            int i;
            if (Int32.TryParse(input, out i))
                numThreads = i;

            Console.Write("Volume name is '{0}'? ", baseName);
            input = Console.ReadLine();
            if (input != String.Empty)
                baseName = input;

            Console.WriteLine("\nCreating scan report files 'File/Directory List - {0}.txt'", baseName);
            Console.WriteLine("Read Buffer Size = {0}", FileUtil.FormatByteSize(ComputeFingerprint.ReadBufferSize));
            Console.WriteLine("Running {0} simultaneous threads on {1} hardware threads\n", numThreads, hardwareThreads);

            RunParallelScan.Run(baseName, scanRootDir, numThreads);

            ConsoleUtil.WaitForKeyPress();
        }


    }

}
