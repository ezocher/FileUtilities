using System;
using System.IO;

public class ConfigFiles
{
    const string configFolderPathUserRelative = @"Repos\FileUtilities\Config\";
    
    const string categoriesConfigFileName = "FileCategories.txt";
    const string directoriesConfigFileName = "Directories.txt";
    const string filesIgnoreFileName = "FilesIgnore.txt";

    private static string ConfigDirectory()
    {
        return Path.Combine(Environment.GetFolderPath((Environment.SpecialFolder.UserProfile)), 
            configFolderPathUserRelative);
    }

    public static string GetCategoriesFile()
    {
        return Path.Combine(ConfigDirectory(), categoriesConfigFileName);
    }

    public static string GetDirectoriesFile()
    {
        return Path.Combine(ConfigDirectory(), directoriesConfigFileName);
    }

    public static string GetFilesIgnoreFile()
    {
        return Path.Combine(ConfigDirectory(), filesIgnoreFileName);
    }
}
