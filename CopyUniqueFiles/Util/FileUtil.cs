using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public class FileUtil
{
    // Others To consider:
    //	Attributes: read only?
    //	Dates: Most recent date? Oldest date? from CreationTime, LastAccessTime, LastWriteTime,

    public static bool IsHidden(FileInfo fi)
    {
        return ((fi.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden);
    }

    public static bool IsSystemOrHidden(FileInfo fi)
    {
        return (
            ((fi.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden) ||
            ((fi.Attributes & FileAttributes.System) == FileAttributes.System)
            );
    }

    public static bool IsSystemOrHidden(DirectoryInfo di)
    {
        return (
            ((di.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden) ||
            ((di.Attributes & FileAttributes.System) == FileAttributes.System)
            );

    }

    public static string TrimFileName(string name, int maxLength)
    {
        const string Ellipsis = "...";

        if (name.Length > maxLength)
            return (name.Substring(0, maxLength - Ellipsis.Length) + Ellipsis);
        else
            return name;
    }

    // See docs for explanation of this function
    // https://docs.microsoft.com/en-us/windows/desktop/fileio/naming-a-file#maximum-path-length-limitation
    //
    // This didn't help for:
    //      FileStream stream = new FileStream(path, ...
    public static string PrefixPathIfTooLong(string path)
    {
        const int WindowsMaxPathLength = 260 - 1; // Minus 1 for the trailing null character
        const string ExtendedLengthPathPrefix = @"\\?\";

        if (path.Length > WindowsMaxPathLength)
            return (ExtendedLengthPathPrefix + path);
        else
            return path;
    }


    // Windows Forms, can be mixed with console app
    //
    // main() which calls this must have [STAThreadAttribute] set
    //
    // Returns "" if no directory selected
    public static string SelectDirectory()
    {
        string directory = "";
        FolderBrowserDialog fbd = new FolderBrowserDialog();
        DialogResult dr = fbd.ShowDialog();
        if (dr == DialogResult.OK)
            directory = fbd.SelectedPath;
        return directory;
    }

    public static string GetBaseName(string directory)
    {
        FileInfo d;

        try
        {
            d = new FileInfo(directory);
        }
        catch
        {
            return "";
        }

        if (d.Name == "")
            return Environment.MachineName + "-Drive " + directory[0];
        else
            return d.Name;
    }

    public static bool IsSystemDrive(string directory)
    {
        const string SystemDriveLetter = "C";

        FileInfo d;

        try
        {
            d = new FileInfo(directory);
        }
        catch
        {
            return false;
        }

        string driveLetter = directory[0].ToString().ToUpper();
        return (driveLetter == SystemDriveLetter);
    }

    public static string FormatByteSize(long size)
    {
        const long OneK = 1024;
        string[] SizeNames = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        int MaxExponent = SizeNames.Length - 1;

        long accumulator = size;
        int exponent = 0;

        while (accumulator > OneK)
        {
            accumulator /= OneK;
            exponent++;
        }

        exponent = Math.Min(exponent, MaxExponent);

        double scaledSize = size / Math.Pow(OneK, exponent);

        // Display size ranges with different formats for aesthetics/readability
        string sizeNumberFormat;
        if (exponent == 0)
            sizeNumberFormat = "N0";
        else if (exponent <= 2)
            sizeNumberFormat = "F1";
        else if (exponent == 3)
            sizeNumberFormat = "F2";
        else
            sizeNumberFormat = "F3";

        return String.Format("{0:" + sizeNumberFormat + "} {1}", scaledSize, SizeNames[exponent]);
    }

    // Returns the first unused file name of the form: "<input full name> (#).<input file ext>"
    //  increments the trailing number until it finds an unused one
    // -OR- returns the input full name if it is unused
    public static string GetUniqueFileName(string tryFullName)
    {
        int tryCounter = 0;
        string currentFullName = tryFullName;
        FileInfo currentFi = new FileInfo(currentFullName);
        string extension = currentFi.Extension;                 // Extension includes the '.'
        int lengthWithoutExtension = tryFullName.Length - extension.Length;

        while (currentFi.Exists)
        {
            tryCounter++;
            currentFullName = String.Format("{0} ({1}){2}", tryFullName.Substring(0, lengthWithoutExtension), tryCounter, extension);
            currentFi = new FileInfo(currentFullName);
        }

        return currentFullName;
    }

    // Returns the first unused directory name of the form: "<input full name> (#)"
    //  increments the trailing number until it finds an unused one
    // -OR- returns the input full name if it is unused
    public static string GetUniqueDirectoryName(string tryFullName)
    {
        int tryCounter = 0;
        string currentFullName = tryFullName;
        DirectoryInfo currentDi = new DirectoryInfo(currentFullName);

        while (currentDi.Exists)
        {
            tryCounter++;
            currentFullName = String.Format("{0} ({1})", tryFullName, tryCounter);
            currentDi = new DirectoryInfo(currentFullName);
        }

        return currentFullName;
    }

    public static string GetSpecialFolder(string specialFolderEnumName)
    {
        if (Enum.TryParse<Environment.SpecialFolder>(specialFolderEnumName, true, out Environment.SpecialFolder result))
            return Environment.GetFolderPath(result);
        else
            return null;
    }

    // Returns true if successfully written, false if there's an exception
    public static bool TestWritePath(string path)
    {
        // Assume this is a safe name
        string filePath = path + Path.DirectorySeparatorChar + "testwritevolume.temp";
        try
        {
            StreamWriter testFile = new StreamWriter(filePath, false);  // append = true
            testFile.WriteLine(filePath);   // Write something, anything
            testFile.Close();
            File.Delete(filePath);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}

public class SampleFileUtil
{
    public static void FormatByteSizeSamples()
    {
        long[] sampleValues = {333, 1000, 1024, 1025, 4096, 2000000, 40000000, 600000000,
            8000000000, 30000000000, 500000000000, 7000000000000, 90000000000000, 200000000000000,
            4000000000000000 };

        foreach (long i in sampleValues)
            Console.WriteLine("{0:N0} bytes = {1}", i, FileUtil.FormatByteSize(i));
    }
}
