using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DeDupScanner
{
    // SHA1 chosen over MD5 (160 bits vs. 128 bits)

    // Consider TBD: Implement and test performance with BLAKE3.NET hash (256 bits)
    //  Pros:
    //      * Much faster than SHA256
    //  Cons:
    //      * Maybe not a big win because scanning is IO bound on most computers
    //      * Longer hashes will enlarge all report/DB files

    class ComputeFingerprint
    {
        public const int ReadBufferSize = 512 * 1024;

        public static string FileChecksum(string path)
        {
            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, ReadBufferSize))
                {
                    SHA1CryptoServiceProvider cryp = new SHA1CryptoServiceProvider();
                    byte[] checksum = cryp.ComputeHash(stream);
                    return BitConverter.ToString(checksum).Replace("-", String.Empty);
                }
            }
            catch (Exception e)
            {
                lock (ConsoleUtil._lockGlobalConsole)
                    Console.WriteLine("\n*** File SHA Exception: {0}\n      File: {1}", e.Message, path);
                RunParallelScan.progress.FileException(path, e.Message);
                return "";
            }
        }

        public static string DirectoryChecksum(string[] input)
        {
            // SORT and then concatenate array of checksums
            Array.Sort(input);
            string joinedInput = String.Join(String.Empty, input, 0, input.Length);

            byte[] bytes = Encoding.ASCII.GetBytes(joinedInput);

            try
            {
                using (SHA1CryptoServiceProvider cryp = new SHA1CryptoServiceProvider())
                {
                    byte[] checksum = cryp.ComputeHash(bytes);
                    return BitConverter.ToString(checksum).Replace("-", String.Empty);
                }
            }
            catch (Exception e)
            {
                lock (ConsoleUtil._lockGlobalConsole)
                    Console.WriteLine("\n*** Directrory SHA Exception (Unexpected REF3): {0}", e.Message);    // TBD: Should never happen, but should show where
                return "";
            }

        }
    }

}
