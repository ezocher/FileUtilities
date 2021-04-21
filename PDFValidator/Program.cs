using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ConcurrentFilesystemTraverser
{
    class Program
    {
        private static StreamWriter listFile;

        [STAThreadAttribute]
        static void Main(string[] args)
        {
            FileInfo fi;

            string scanRootDir = FileUtil.SelectDirectory();
            string baseName = FileUtil.GetBaseName(scanRootDir);

            if ((scanRootDir == "") || (baseName == ""))
            {
                Console.WriteLine("Error: scan directory '{0}' or directory name '{1}' is empty", scanRootDir, baseName);
                Console.ReadKey(true);
                return;
            }

            ConcurrentFilesystemTraverser fst = new ConcurrentFilesystemTraverser(scanRootDir);
            

            Console.WriteLine("Creating scan report file 'File List - {0}.tsv'", baseName);
            string listFileName = System.Windows.Forms.Application.StartupPath + Path.DirectorySeparatorChar
                + "File List - " + baseName + ".tsv";

            listFile = new StreamWriter(listFileName, false); // Append = true
            listFile.WriteLine("Starting scan of {0}", scanRootDir);
            listFile.WriteLine();
            listFile.WriteLine("Num\tFull Path\tFile Name\tLength\tPDF Version");

            int numPDFFiles = 1;
            int totalNumFiles = 1;
            string backspaces = new string('\b', 132);
            string spaces = new string(' ', 131);

            while ((fi = fst.NextFile()) != null)
            {
                if ((totalNumFiles % 1000) == 0)
                    Console.Write(backspaces + spaces + backspaces + numPDFFiles.ToString() + "/" + totalNumFiles.ToString() + " - " + TrimFileName(fi.Name, 110));

                if (CheckPDF.IsPDF(fi))
                {
                    if ((numPDFFiles % 10) == 0)
                    {
                        Console.Write(backspaces + spaces + backspaces + numPDFFiles.ToString() + "/" + totalNumFiles.ToString() + " - " + TrimFileName(fi.Name, 110));
                        listFile.Flush();
                    }

                    listFile.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", numPDFFiles,
                        fi.FullName, fi.Name, fi.Length,
                        CheckPDF.Check(fi, listFile));
                    numPDFFiles++;
                }

                totalNumFiles++;
            }

            listFile.Close();

            Console.WriteLine("\n\nScan Complete - Scanned {0} files", numPDFFiles - 1);
            Console.ReadKey(true);
         }

        public static string TrimFileName(string name, int maxLen)
        {
            if (name.Length <= maxLen)
                return name;
            else
                return (name.Substring(0, maxLen - 3) + "...");
        }



    }
}
