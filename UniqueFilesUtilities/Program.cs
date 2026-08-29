using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;



namespace UniqueFilesUtilities
{
    class Program
    {
        //----------------------------------------------------------------------------------------------------------------------------------------
        //  This project can build three different related file management apps. Which app is being built is selected by the WhichApp setting
        //      see AppSettings.cs
        //
        //  The tree apps are:
        //  * Fingerprint Database Maker
        //  * Unique File Copier
        //  * Photo Collector and Organizer

        public static string baseName;

        static int hardwareThreads = Environment.ProcessorCount;
        static int numThreadsSolidStateDrive = hardwareThreads; // 1; // for testing // 
        static int numThreadsRotatingDrive = Math.Min(hardwareThreads, 3);  // 3 threads arrived at by observation on several rotating drives (internal and USB)
        static int numThreads;

        private static FileDB fileDB;

        private static string destinationVolume, destinationPrefixPath;

        [STAThreadAttribute]
        public static void Main(string[] args)
        {
            AppSettings.LoadAppSettings();

            ConsoleUtil.InitConsoleSettings(AppSettings.appName + " - Under Development");
            Console.WriteLine(AppSettings.appDescription);

            string baseFileListsDirectory = LoadFileDBs.BaseFileListsFolderPath();


        // Select scan target volume/directory and set default basename
        //      basename is name of directory e.g. "Music" or machine + drive name e.g. "MyLap-Drive C"
            string sourceRootDir = FileUtil.SelectDirectory();
            baseName = FileUtil.GetBaseName(sourceRootDir);
            Console.WriteLine(AppSettings.operationDescription + " '{0}'\n", sourceRootDir);

            if ((sourceRootDir == "") || (baseName == ""))
            {
                ConsoleUtil.WriteLineColor(String.Format("Error: scan directory '{0}' or directory name '{1}' is empty", sourceRootDir, baseName),
                    ConsoleColor.Red);
                ConsoleUtil.WaitForKeyPress();
                return;
            }

        // Select destination volume for copied unique files (not needed for FingerprintDBMaker)
            destinationVolume = AppSettings.DefaultDestinationVolume;
            string input;
            if (AppSettings.WhichApp != App.FingerprintDBMaker)
            {
                Console.Write("Destination Volume '{0}' (or enter new destination)?", destinationVolume);
                input = Console.ReadLine();
                if (input != String.Empty)
                    destinationVolume = input;

                // If the destination volume is C: then write results to the user's root directory since we can't write directly to C:\
                if (destinationVolume.ToUpper() == "C:")
                    destinationPrefixPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                else
                    destinationPrefixPath = destinationVolume;

                if ((destinationVolume.Length != 2) || (destinationVolume[1] != ':') || (!FileUtil.TestWritePath(destinationPrefixPath)))
                {
                    ConsoleUtil.WriteLineColor(String.Format("Error: destination volume of '{0}' is not valid", destinationPrefixPath),
                        ConsoleColor.Red);
                    ConsoleUtil.WaitForKeyPress();
                    return;
                }
                CopyUniqueFile.SetDestinationPrefixPath(destinationPrefixPath);
            }

        // Set the name for this scan target
        //      Used to name the generated DB and report files in all apps
        //      Also used as the suffix of the destination root folder name for UniqueFileCopier
            Console.Write("Scan target's base name is '{0}'? ", baseName);
            input = Console.ReadLine();
            if (input != String.Empty)
                baseName = input;
            CopyUniqueFile.SetSourceBaseName(baseName);

            if (FileUtil.IsSystemDrive(sourceRootDir))
                // Most current system drives are SSDs
                numThreads = numThreadsSolidStateDrive;
            else
                numThreads = numThreadsRotatingDrive;

            bool copyFiles;
            if (AppSettings.WhichApp == App.FingerprintDBMaker)
                copyFiles = false;
            else
                copyFiles = true;

            ConsoleUtil.White();
            if (copyFiles)
            {
                Console.WriteLine("\tCopying unique files from '{0}' to '{1}'\n", sourceRootDir, CopyUniqueFile.DestinationRootPath(true));
                if (AppSettings.WhichApp == App.UniqueFileCopier)
                {
                    ConsoleUtil.Green();
                    CopyUniqueFile.SetOptionDivideFilesIntoCategories(ConsoleUtil.YesNoChoice("Divide files into categories (Y|N)? "));
                    ConsoleUtil.RestoreColors();
                }
                else // AppSettings.WhichApp == App.PhotoCollector
                    CopyUniqueFile.SetOptionDivideFilesIntoCategories(false);
            }   
            else
                Console.WriteLine("   Scanning and fingerprinting all unique files in '{0}' and writing DB and reports\n", sourceRootDir);
            ConsoleUtil.RestoreColors();

            if (AppSettings.WhichApp == App.PhotoCollector || CopyUniqueFile.divideFilesIntoCategories)
                CopyUniqueFile.LoadFileCategoryMap(AppSettings.WhichApp == App.PhotoCollector);
                
            CopyUniqueFile.SetOptionCopyFiles(copyFiles);

            Console.Write("Run with {0} threads? ", numThreads);
            input = Console.ReadLine();
            int i;
            if (Int32.TryParse(input, out i))
                numThreads = i;

            ConsoleUtil.White();
            Console.WriteLine("\n\tCreating 4 report files '{0}-Unique/Duplicate/Excluded Files.tsv and -Files DB.tsv'", baseName);
            Console.WriteLine("\tRunning {0} simultaneous threads on {1} hardware threads", numThreads, hardwareThreads);
            Console.WriteLine("\tBase file databases are loaded from '{0}'", baseFileListsDirectory);
            Console.WriteLine();
            ConsoleUtil.RestoreColors();

            // Load in-memory database of existing files to check against for uniques
            fileDB = new FileDB();
            LoadFileDBs.LoadBaseFileLists(fileDB);

            RunParallelScan.ScanAndCopyUniques(baseName, sourceRootDir, numThreads, fileDB);

            ConsoleUtil.WaitForKeyPress();
        }

    }
}
