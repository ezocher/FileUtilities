using System;
using System.IO;

public class ConfigFiles
{
    const string configFolderPathUserRelative = @"Repos\FileUtilities\CopyUniqueFiles\Config\";  // Directly on Dell tower

    // const string configFolderPathUserRelative = @"OneDrive\Documents\GitHub\FileUtilities\CopyUniqueFiles\Config\";  // On IdeaPad and on Dell tower after GitHub + OneDrive sync

    const string categoriesConfigFileName = "FileCategoriesByExtension.txt";
    const string directoriesConfigFileName = "DirectoriesIgnore.txt";
    const string filesIgnoreFileName = "FilesIgnoreByExtension.txt";

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
