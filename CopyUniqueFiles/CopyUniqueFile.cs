using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class CopyUniqueFile
{
    private static string destVolume;
    static string destBasePath;
    const string destRootPrefix = "uu-";

    public static void SetDestinationVolume(string destinationVolume)
    {
        destVolume = destinationVolume;
    }

    public static void SetSourceBaseName(string name)
    {
        destBasePath = destVolume + Path.DirectorySeparatorChar + destRootPrefix + name;
    }

    public static void Copy(string sourceFilePath, out string destinationFilePath)
    {
        string destFilePath = destBasePath + sourceFilePath.Remove(0, 2);    // Remove "X:"
        string destDirPath = Path.GetDirectoryName(destFilePath);

        Directory.CreateDirectory(destDirPath);
        File.Copy(sourceFilePath, destFilePath, false);

        // System.IO.IOException: 'The file 'F: \uu - ZB - DriveC\Users\ezoch\Desktop\temp.html' already exists.'
        // System.IO.FileNotFoundException: 'Could not find file 'C:\Users\ezoch\Desktop\LEFT MON\!Left DT - XMas\California wildfires- Is Trump right when he blames forest managers- - BBC News'.'

        destinationFilePath = destFilePath;
    }
}

