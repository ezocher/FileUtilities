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

    static void CopyExceptionMessage(string srcPath, string destPath, string exceptionMessage)
    {
        ConsoleUtil.WriteLineColor(String.Format("\n*** File Copy Exception '{0}' -> '{1}': {2}\n", srcPath, destPath, exceptionMessage),
                ConsoleColor.Red);
    }

    public static void Copy(string sourceFilePath, out string destinationFilePath)
    {
        string destFilePath = destBasePath + sourceFilePath.Remove(0, 2);    // Remove "X:"

        try
        {
            string destDirPath = Path.GetDirectoryName(destFilePath);
            Directory.CreateDirectory(destDirPath);
            File.Copy(sourceFilePath, destFilePath, false);

            destinationFilePath = destFilePath;
        }
        catch (Exception e)
        {
            CopyExceptionMessage(sourceFilePath, destFilePath, e.ToString());

            destinationFilePath = "*** Exception - source file not copied ***";
        }

        // System.IO.PathTooLongException: 'The specified path, file name, or both are too long. The fully qualified file name must be less than 260 characters, and the directory name must be less than 248 characters.'
        // System.IO.IOException: 'The file 'F: \uu - ZB - DriveC\Users\ezoch\Desktop\temp.html' already exists.'
        // System.IO.FileNotFoundException: 'Could not find file 'C:\Users\ezoch\Desktop\LEFT MON\!Left DT - XMas\California wildfires- Is Trump right when he blames forest managers- - BBC News'.'
    }
}

