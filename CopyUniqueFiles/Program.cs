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

// TBD: Rename project and main namespace


namespace DeDupScanner
{
    public enum WhichApp
    {
        FingerprintDBMaker,
        UniqueFileCopier,
        PhotoCollector
    }

    class Program
    {

        //----------------------------------------------------------------------------------------------------------------------------------------
        //  This project can build three different related file management apps. Which app is being built is selected by the WhichApp enum.
        //
        //  The tree apps are:
        //  * Fingerprint Database Maker
        //  * Unique File Copier
        //  * Photo Collector and Organizer

        public static WhichApp WhichApp = WhichApp.PhotoCollector;

        public static string baseName;

        static int hardwareThreads = Environment.ProcessorCount;
        static int numThreadsSolidStateDrive = hardwareThreads; // 1; // for testing // 
        static int numThreadsRotatingDrive = Math.Min(hardwareThreads, 3);  // 3 threads arrived at by observation on several rotating drives (internal and USB)
        static int numThreads;

        private static FileDB fileDB;

        private static string destinationVolume = "C:", destinationPrefixPath;

        [STAThreadAttribute]
        public static void Main(string[] args)
        {
            string appName = "", appDescription = "", operationDescription = "";

            switch (WhichApp)
            {
                case WhichApp.FingerprintDBMaker:
                    appName = "Fingerprint Database Maker";
                    appDescription = "Fingerprints unique files in the target volume/directory and creates a DB file in ...";
                    operationDescription = "Fingerprinting unique files from";
                    break;

                case WhichApp.UniqueFileCopier:
                    appName = "Unique File Copier";
                    appDescription = "Copies unique files found in the target volume/directory. Optionally organizes them into folders by file type";
                    operationDescription = "Copying unique files from";
                    break;

                case WhichApp.PhotoCollector:
                    appName = "Photo Collector and Organizer";
                    appDescription = "Collects unique photos and videos and organizes them into folders by year taken";
                    operationDescription = "Collecting and organizing unique photos and videos from";
                    break;

            }
            ConsoleUtil.InitConsoleSettings(appName + " - Under Development");
            Console.WriteLine(appDescription);

        // Base file databases
            Console.WriteLine("Base file databases are loaded from '{0}'", LoadFileLists.BaseFileListsFolderPath());
            Console.WriteLine();

        // Select scan target volume/directory and set basename
        //      basename is name of directory e.g. "Music" or machine + drive name e.g. "MyLap-Drive C"
            string scanRootDir = FileUtil.SelectDirectory();
            baseName = FileUtil.GetBaseName(scanRootDir);
            Console.WriteLine(operationDescription + " '{0}'\n", scanRootDir);

            if ((scanRootDir == "") || (baseName == ""))
            {
                ConsoleUtil.WriteLineColor(String.Format("Error: scan directory '{0}' or directory name '{1}' is empty", scanRootDir, baseName),
                    ConsoleColor.Red);
                ConsoleUtil.WaitForKeyPress();
                return;
            }

        // Select destination volume for copied uniue files (not needed for FingerprintDBMaker)
            string input;
            if (Program.WhichApp != WhichApp.FingerprintDBMaker)
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
        //      Also used to the name destination folder for UniqueFileCopier
            Console.Write("Scan target's name is '{0}'? ", baseName);
            input = Console.ReadLine();
            if (input != String.Empty)
                baseName = input;
            CopyUniqueFile.SetSourceBaseName(baseName);

            if (FileUtil.IsSystemDrive(scanRootDir))
                // All my current system drives are SSDs
                numThreads = numThreadsSolidStateDrive;
            else
                numThreads = numThreadsRotatingDrive;

            bool copyFiles;
            if (Program.WhichApp == WhichApp.FingerprintDBMaker)
                copyFiles = false;
            else
                copyFiles = true;

            ConsoleUtil.White();
            if (copyFiles)
            {
                Console.WriteLine("   Copying unique files from '{0}' to {1}\n", scanRootDir, CopyUniqueFile.DestinationRootPath(true));
                if (Program.WhichApp == WhichApp.UniqueFileCopier)
                    CopyUniqueFile.SetOptionDivideFilesIntoCategories(ConsoleUtil.YesNoChoice("Divide files into categories (Y|N)? "));
                else
                    CopyUniqueFile.SetOptionDivideFilesIntoCategories(false);
            }   
            else
                Console.WriteLine("   Scanning and fingerprinting all unique files in '{0}' and writing DB and reports\n", scanRootDir);
            ConsoleUtil.RestoreColors();

            CopyUniqueFile.SetOptionCopyFiles(copyFiles);

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

            RunParallelScan.ScanAndCopyUniques(baseName, scanRootDir, numThreads, fileDB);

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
