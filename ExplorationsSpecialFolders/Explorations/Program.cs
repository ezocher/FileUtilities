using System;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;

namespace Explorations
{
    class Program
    {
        [STAThreadAttribute]
        static void Main(string[] args)
        {
            string dir = SelectDirectory();
            Console.WriteLine(dir);

            Console.WriteLine("Processors = {0}", System.Environment.ProcessorCount);

            ShowSpecialFolders();

             /*

            string filename = @"C:\Users\ezocher\OneDrive\Videos\Kayaking\GPFull_hyperlapse_25x_adv.mp4";
            long filesizeMB = (new FileInfo(filename)).Length / (1024 * 1024);

            Console.WriteLine("Computing checksum of '{0}' ({1} MB)...", filename, filesizeMB);
            Stopwatch timer = Stopwatch.StartNew();
            string csum = GetChecksum(filename);

            timer.Stop();
            Console.WriteLine(csum + " " + timer.ElapsedMilliseconds.ToString());
            Console.WriteLine(filesizeMB + " MB @ " + ((filesizeMB * 1000) / timer.ElapsedMilliseconds) + " MB/sec");
            */
            Console.ReadKey(true);
        }

        private static string SelectDirectory()
        {
            string directory = "";
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult dr = fbd.ShowDialog();
            if (dr == DialogResult.OK)
                directory = fbd.SelectedPath;
            else
            {
                Console.WriteLine("No directory selected.");
            }

            return directory;
        }

        private static string GetChecksum(string path)
        {
            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1024 * 1024))
                {
                    SHA1CryptoServiceProvider cryp = new SHA1CryptoServiceProvider();
                    // MD5CryptoServiceProvider cryp = new MD5CryptoServiceProvider();
                    byte[] checksum = cryp.ComputeHash(stream);
                    return BitConverter.ToString(checksum).Replace("-", String.Empty);
                }
            }
            catch (Exception e)
            {
                    Console.WriteLine("Exception: " + e.Message);
                    return "";
            }
        }

        private static void ShowSpecialFolders()
        {
            int x = (int)Environment.SpecialFolder.Personal;
            Console.WriteLine("{0} ({1}) = {2}\n", "Personal", x, Environment.GetFolderPath((Environment.SpecialFolder.Personal)));

            string path = "";
            bool argException = false;

            const int numberScanned = 1000;
            const int progressInterval = 100;

            for (int i = 0; i < numberScanned; i++)
            {
                if ((i % progressInterval) == 0)
                    Console.WriteLine("Scanning {0}...", i);

                string enumname = ((Environment.SpecialFolder)i).ToString();
                try
                {
                    path = Environment.GetFolderPath((Environment.SpecialFolder)i);
                }
                catch
                {
                    argException = true;
                }

                if (!argException)
                    Console.WriteLine("{0} ({1}) = {2}", enumname, i, path);
                path = ""; argException = false;

            }

            Console.WriteLine("Scan of 0..{0} completed.", numberScanned - 1);
        }
}
}
