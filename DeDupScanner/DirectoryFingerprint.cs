using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeDupScanner
{
    // Used to accumulate file and directory checksums of all items in a directory

    public class DirectoryFingerprint
    {
        DirectoryInfo di;
        DirectoryFingerprint parentFingerprint;

        readonly object _lockDirFingerprint = new object();
        string[] checksums;
        int checksumsStored = 0;

        int numberOfItems;
        int numberOfItemsCompleted = 0;

        bool completed = false;

        public DirectoryFingerprint(DirectoryInfo di, DirectoryFingerprint parentFingerprint)
        {
            this.di = di;
            this.parentFingerprint = parentFingerprint;
        }

        public void SetNumberOfItems(int numFiles, int numDirs)
        {
            this.numberOfItems = numFiles + numDirs;

            if (numberOfItems > 0)
                checksums = new string[numberOfItems];
            else
                EmptyDirectory();
        }

        public void ChildDirectorySkipped()
        {
            lock (_lockDirFingerprint)
            {
                numberOfItemsCompleted++;
            }
            CheckSelfCompleted();
        }

        public void ChildFileSkipped()
        {
            ChildDirectorySkipped();
        }

        public void FileCompleted(string fileChecksum)
        {
            lock (_lockDirFingerprint)
            {
                checksums[checksumsStored++] = fileChecksum;
                numberOfItemsCompleted++;
            }
            CheckSelfCompleted();

        }

        public void DirectoryCompleted(string dirChecksum)
        {
            FileCompleted(dirChecksum);
        }

        private void CheckSelfCompleted()
        {
            string directoryChecksum;

            lock (_lockDirFingerprint)
            {
                if (!completed) // Only do this once
                {
                    if (numberOfItemsCompleted == numberOfItems)
                    {
                        completed = true;

                        if (numberOfItemsCompleted == checksumsStored)
                            directoryChecksum = ComputeFingerprint.DirectoryChecksum(checksums);
                        else
                        {
                            string[] storedChecksums = new string[checksumsStored];
                            Array.Copy(checksums, storedChecksums, storedChecksums.Length);
                            directoryChecksum = ComputeFingerprint.DirectoryChecksum(storedChecksums);
                        }

                        RunParallelScan.progress.DirectoryCompleted(di, directoryChecksum, checksumsStored, numberOfItems);

                        // Pass checksum up to parent
                        parentFingerprint?.DirectoryCompleted(directoryChecksum);
                    }
                }
            }
        }

        private void EmptyDirectory()
        {
            string[] EmptyDirChecksumSeed = { String.Empty };
            string directoryChecksum;

            lock (_lockDirFingerprint)
            {
                completed = true;
                directoryChecksum = ComputeFingerprint.DirectoryChecksum(EmptyDirChecksumSeed);

                RunParallelScan.progress.DirectoryCompleted(di, directoryChecksum, checksumsStored, numberOfItems);

                // Don't Pass empty checksum up to parent
                parentFingerprint?.ChildDirectorySkipped();
            }

        }
    }
}
