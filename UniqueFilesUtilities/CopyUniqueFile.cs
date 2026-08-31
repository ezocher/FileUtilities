using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class CopyUniqueFile
{
    // Unique/collected files are written to:
    //      destSpecificPath wich is built starting with destPrefixPath + 
    // 

    private static string destPrefixPath;
    private static int sourcePathRootLength;

    public static bool divideFilesIntoCategories;
    private static bool copyFiles;
    private static bool moveFiles; // Move versus copy files, used by PhotoCollector only
    private static Dictionary<string, string> FileExtensionToCategoryMap;

    public static void SetOptionDivideFilesIntoCategories(bool setting)
    {
        divideFilesIntoCategories = setting;
    }

    public static void SetOptionMoveOrCopyFiles(bool setting)
    {
        moveFiles = setting;
    }

    public static void SetOptionCopyFiles(bool setting)
    {
        copyFiles = setting;
    }
    
    public static void LoadFileCategoryMap(bool reportPhotoVideoStats)
    {
        FileExtensionToCategoryMap = new Dictionary<string, string>();
        HashSet<string> Categories = new HashSet<string>();
        int numPhotoExtensions = 0, numVideoExtensions = 0;

        ConfigSettings[] extensionList = ConfigFileUtil.LoadConfigFile(ConfigFiles.GetCategoriesFile());

        foreach (ConfigSettings settings in extensionList)
        {
            FileExtensionToCategoryMap.Add(settings.Value.ToLower(), settings.Category);
            Categories.Add(settings.Category);
            if (reportPhotoVideoStats)
            {
                if (settings.Category == AppSettings.PhotoFileExtensionsCategory) numPhotoExtensions++;
                else if (settings.Category == AppSettings.VideoFileExtensionsCategory) numVideoExtensions++;
            }
        }

        ConsoleUtil.Yellow();
        if (reportPhotoVideoStats)
            Console.WriteLine("\tLoaded {0} photo file extensions and {1} video file extensions\n", numPhotoExtensions, numVideoExtensions);
        else
            Console.WriteLine("\tLoaded {0} file extensions in {1} categories\n", FileExtensionToCategoryMap.Count, Categories.Count);
        ConsoleUtil.RestoreColors();
    }

    public static string GetFileCategory(FileInfo fi)
    {
        string fileExtenson = Path.GetExtension(fi.FullName).ToLower();
        if (FileExtensionToCategoryMap.TryGetValue(fileExtenson, out string category))
            return category;
        else
            return AppSettings.unknownCategoryName;
    }


    public static void SetDestinationPrefixPath(string destinationPrefixPath)
    {
        destPrefixPath = destinationPrefixPath;
    }

    public static string DestinationRootPath(bool photo)
    {
        switch (AppSettings.WhichApp)
        {
            case App.FingerprintDBMaker:
                return "";
            case App.UniqueFileCopier:
                return Path.Combine(destPrefixPath, AppSettings.destRootPrefix + AppSettings.BaseName);
            case App.PhotoCollector:
                if (photo)
                    return Path.Combine(destPrefixPath, AppSettings.photosDestRootFolder);
                else
                    return Path.Combine(destPrefixPath, AppSettings.videosDestRootFolder);
        }
        return "";
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

    static void CopyExceptionMessage(string srcPath, string destPath, string exceptionMessage)
    {
        ConsoleUtil.WriteLineColor(String.Format("\n*** File Copy Exception '{0}' -> '{1}': {2}\n", srcPath, destPath, exceptionMessage),
                ConsoleColor.Red);
    }


    // For Photo collector:
    // Use File.Move() instead of .Copy() - this enables finding left behind things like like .pdf's of
    // calendars or .docx that went with a trip

    // For Photo collector:
    // Use File.Move() instead of .Copy() - this enables finding left behind things like like .pdf's of
    // calendars or .docx that went with a trip
    //
    // NEVER overwrite existing files, instead use increasing (n) naming, e.g. IMG1000 (2).jpg
    public static void MoveNoOverwrite(string sourceFilePath, out string destinationFilePath, out string category)
    {
        destinationFilePath = "";
        category = "";
    }

    public static void Copy(string sourceFilePath, out string destinationFilePath, out string category, string yearTaken)
    {
        string destFilePath;

        if (divideFilesIntoCategories)
        {
            string sourceExtension = Path.GetExtension(sourceFilePath).ToLower();
            if (sourceExtension.Length == 0)        // Change empty extension to "." for consistency with non-empty
                sourceExtension = ".";              //  extensions, e.g. ".jpg". This allows it to be matched to a category

            if (!FileExtensionToCategoryMap.TryGetValue(sourceExtension, out category))
                category = AppSettings.unknownCategoryName;

            destFilePath = DestinationRootPath(true) + Path.DirectorySeparatorChar + category + sourceFilePath.Remove(0, sourcePathRootLength);
        }
        else if (AppSettings.WhichApp == App.PhotoCollector)
        {
            string sourceExtension = Path.GetExtension(sourceFilePath).ToLower();
            if (!FileExtensionToCategoryMap.TryGetValue(sourceExtension, out category))
                throw new Exception("Not a photo or video file extension: " + sourceExtension);
            destFilePath = DestinationRootPath(category == AppSettings.PhotoFileExtensionsCategory) + Path.DirectorySeparatorChar + yearTaken + sourceFilePath.Remove(0, sourcePathRootLength);
        }
        else
        {
            destFilePath = DestinationRootPath(true) + sourceFilePath.Remove(0, sourcePathRootLength);
            category = "";
        }

        try
        {
            string destDirPath = Path.GetDirectoryName(destFilePath);
            if (copyFiles)
            {
                Directory.CreateDirectory(destDirPath);
                if (AppSettings.WhichApp == App.PhotoCollector)
                    // For photo collector: if a file with the same name exists then add the first unused (#)
                    destFilePath = FileUtil.GetUniqueFileName(destFilePath);
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

