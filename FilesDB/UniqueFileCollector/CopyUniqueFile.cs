using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniqueFileCollector
{
    class CopyUniqueFile
    {
        static string destBasePath = @"D:\uu-unspecified";

        public static void SetSourceVolumeName(string name)
        {
            destBasePath = @"D:\uu-" + name;
        }

        public static void Copy(string filePath)
        {
            string destFilePath = destBasePath + filePath.Remove(0, 2);    // Remove "X:"
            string destDirPath = Path.GetDirectoryName(destFilePath);

            Directory.CreateDirectory(destDirPath);
            File.Copy(filePath, destFilePath, false);
        }
    }
}
