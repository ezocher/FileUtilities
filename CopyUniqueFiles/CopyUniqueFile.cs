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
    private static int sourcePathRootLength;

    private static bool divideFilesIntoCategories;
    private static bool copyFiles;
    private static Dictionary<string, string> FileExtensionToCategoryMap;
    private const string unknownCategoryName = "Unknown";

    public static void SetOptionDivideFilesIntoCategories(bool setting)
    {
        divideFilesIntoCategories = setting;
        if (divideFilesIntoCategories)
            LoadCategoryMap();
        else
            Console.WriteLine();
    }

    public static void SetOptionCopyFiles(bool setting)
    {
        copyFiles = setting;
    }
    
    private static void LoadCategoryMap()
    {
        FileExtensionToCategoryMap = new Dictionary<string, string>();
        HashSet<string> Categories = new HashSet<string>();

        ConfigSettings[] extensionList = ConfigFileUtil.LoadConfigFile(ConfigFiles.GetCategoriesFile());

        foreach (ConfigSettings settings in extensionList)
        {
            FileExtensionToCategoryMap.Add(settings.Value.ToLower(), settings.Category);
            Categories.Add(settings.Category);
        }

        Console.WriteLine("   Loaded {0} file extensions in {1} categories\n", FileExtensionToCategoryMap.Count, Categories.Count);
    }

    public static void SetDestinationVolume(string destinationVolume)
    {
        destVolume = destinationVolume;
    }

    public static void SetSourcePathRoot(string sourceRoot)
    {
        // If the source path is a volume root directory, e.g. "E:\", then set the length to 2 since the root we want is the
        // full directory name without the slash, which in this case is "E:"
        if (sourceRoot.Length == 3)
            sourcePathRootLength = 2;
        else
            sourcePathRootLength = sourceRoot.Length;
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

    public static void Copy(string sourceFilePath, out string destinationFilePath, out string category)
    {
        string destFilePath;

        if (divideFilesIntoCategories)
        {
            string sourceExtension = Path.GetExtension(sourceFilePath).ToLower();

            if (!FileExtensionToCategoryMap.TryGetValue(sourceExtension, out category))
                category = unknownCategoryName;

            destFilePath = destBasePath + Path.DirectorySeparatorChar + category + sourceFilePath.Remove(0, sourcePathRootLength);
        }
        else
        {
            destFilePath = destBasePath + sourceFilePath.Remove(0, sourcePathRootLength);
            category = "";
        }

        try
        {
            string destDirPath = Path.GetDirectoryName(destFilePath);
            if (copyFiles)
            {
                Directory.CreateDirectory(destDirPath);
                File.Copy(sourceFilePath, destFilePath, false);
            }

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

