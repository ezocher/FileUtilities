﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;


namespace DeDupScanner
{
    public class ReportProgress
    {
        const double progressIntervalMs = 1000;

        Stopwatch progressStopwatch;
        System.Timers.Timer progressIntervalTimer;
        int runningThreads;

        int numFilesCompleted = 0;
        int numDirectoriesCompleted = 0;
        long totalBytesCompleted = 0;
        long lastBytesCompleted = 0;

        int numFilesSkipped = 0;
        int numHiddenSystemDirsSkipped = 0;
        int numSkipListDirsSkipped = 0;
        int numDirExceptions = 0;
        int numFileExceptions = 0;


        public ReportProgress(int numThreads)
        {
            progressStopwatch = new Stopwatch();

            progressIntervalTimer = new System.Timers.Timer(progressIntervalMs);
            progressIntervalTimer.Elapsed += DisplayProgress;

            runningThreads = numThreads;
        }

        public void Start()
        {
            progressStopwatch.Reset(); progressStopwatch.Start();
            progressIntervalTimer.Start();
        }

        public void Stop()
        {
            progressIntervalTimer.Stop();
            progressStopwatch.Stop();
        }

        readonly object _lockStats = new object();
        long lastElapsedMs = 0;
        string lastFileCompleted = "";

        public void FileCompleted(FileInfo fi, string checksum)
        {
            lock (_lockStats)
            {
                numFilesCompleted++;
                totalBytesCompleted += fi.Length;
                lastFileCompleted = fi.FullName;

                ReportFiles.WriteFileInfo(fi, Program.baseName, checksum, numFilesCompleted);
            }
        }


        public void HiddenSystemDirSkipped(DirectoryInfo di)
        {
            lock (_lockStats)
            {
                numHiddenSystemDirsSkipped++;
                ReportFiles.WriteExcludedInfo(false, di.FullName, "System or Hidden", "");
            }
        }
    
        public void SkipListDirSkipped(DirectoryInfo di)
        {
            lock (_lockStats)
            {
                numSkipListDirsSkipped++;
                ReportFiles.WriteExcludedInfo(false, di.FullName, "Dir in skip list", "");
            }
        }

        public void DirException(DirectoryInfo di, string message)
        {
            lock (_lockStats)
            {
                numDirExceptions++;
                ReportFiles.WriteExcludedInfo(false, di.FullName, "Exception", message);
            }
        }

        public void FileSkipped(FileInfo fi, string reason)
        {
            lock (_lockStats)
            {
                numFilesSkipped++;
                ReportFiles.WriteExcludedInfo(true, fi.FullName, reason, fi.Extension);
            }
        }

        public void FileException(string fullName, string message)
        {
            lock (_lockStats)
            {
                numFileExceptions++;
                ReportFiles.WriteExcludedInfo(true, fullName, "Exception", message);
            }
        }


        public void DirectoryCompleted(DirectoryInfo di, string checksum, int numItemsScanned, int totalNumItems)
        {
            lock (_lockStats)
            {
                numDirectoriesCompleted++;

                ReportFiles.WriteDirectoryInfo(di, Program.baseName, numItemsScanned, totalNumItems, checksum, numDirectoriesCompleted);
            }
        }

        public void ThreadCompleted()
        {
            lock (_lockStats)
            {
                runningThreads--;
            }
        }

        const int ConsoleLineLengthChars = 132;
        const string ProgressFormat = "{0}{1}{0}{2:N0} files / {3} completed in {4} @ {5}/min. - {6}";
        static int ConsoleMaxFileNameLength = ConsoleLineLengthChars - "9,999,999 files / 999.99 GB completed in 59.9 min. @ 999.9 MB/min. - ".Length - 1; // -1 because exactly full line will wrap

        static string backspaces = new string('\b', ConsoleLineLengthChars);
        static string spaces = new string(' ', ConsoleLineLengthChars - 1); // -1 because exactly full line will wrap

        const float MillisecondsPerSecond = 1000f;
        const float SecondsPerMinute = 60f;

        void DisplayProgress(object s, ElapsedEventArgs e)
        {
            int files, dirs;
            long bytesThisInterval;
            long elapsedMs;
            long msInterval;
            string fileName;

            lock (_lockStats)
            {
                files = numFilesCompleted;

                // Currently not using dirs completed, but keep them in case we want them later
                dirs = numDirectoriesCompleted;

                // Currently not using interval times or interval bytes, but keep them in case we want them later
                bytesThisInterval = totalBytesCompleted - lastBytesCompleted;
                lastBytesCompleted = totalBytesCompleted;

                elapsedMs = progressStopwatch.ElapsedMilliseconds;
                fileName = lastFileCompleted;
            }

            // Currently not using interval times or interval bytes, but keep them in case we want them later
            msInterval = elapsedMs - lastElapsedMs;
            lastElapsedMs = elapsedMs;

            double readSpeedBytesPerMin = ((double)lastBytesCompleted) / (elapsedMs / (MillisecondsPerSecond * SecondsPerMinute));

            lock (ConsoleUtil._lockGlobalConsole)
            {
                ConsoleUtil.White();
                Console.Write(ProgressFormat, backspaces, spaces, files, FileUtil.FormatByteSize(lastBytesCompleted), TimerUtil.FormatMilliseconds(lastElapsedMs), FileUtil.FormatByteSize((long)readSpeedBytesPerMin), 
                    FileUtil.TrimFileName(fileName, ConsoleMaxFileNameLength));
                ConsoleUtil.RestoreColors();
            }
        }

        public void DisplayFinalSummary()
        {
            // Wait for pending writes and then close files
            lock (_lockStats)
            {
                ReportFiles.Close();
            }

            double readSpeedbytesPerMin = (double)totalBytesCompleted / (progressStopwatch.ElapsedMilliseconds / (MillisecondsPerSecond * SecondsPerMinute));

            Console.WriteLine("\n\nRun Complete - {0:N0} files and {1:N0} directories processed in {2} - {3} at {4}/minute",
                numFilesCompleted, numDirectoriesCompleted, TimerUtil.FormatMilliseconds(progressStopwatch.ElapsedMilliseconds), FileUtil.FormatByteSize(totalBytesCompleted), FileUtil.FormatByteSize((long)readSpeedbytesPerMin));

            int totalFilesSkipped = 0, totalDirsSkipped = 0;

            totalFilesSkipped = numFilesSkipped + numFileExceptions;
            totalDirsSkipped = numSkipListDirsSkipped + numHiddenSystemDirsSkipped + numDirExceptions;

            if (totalFilesSkipped > 0)
            {
                Console.WriteLine("\nTotal files skipped = {0:N0}", totalFilesSkipped);
                if (numFilesSkipped > 0) Console.WriteLine("    Skipped by rules: {0:N0}", numFilesSkipped);
                if (numFileExceptions > 0) Console.WriteLine("    Files with exceptions: {0:N0}", numFileExceptions);
            }

            if (totalDirsSkipped > 0)
            {
                Console.WriteLine("\nTotal directories skipped = {0:N0}", totalDirsSkipped);
                if (numSkipListDirsSkipped > 0) Console.WriteLine("    Directories in skip list: {0:N0}", numSkipListDirsSkipped);
                if (numHiddenSystemDirsSkipped > 0) Console.WriteLine("    Hidden or System: {0:N0}", numHiddenSystemDirsSkipped);
                if (numDirExceptions > 0) Console.WriteLine("    Directories with exceptions: {0:N0}", numDirExceptions);
            }

        }

    }
}