using UniqueFilesUtilities;
using System;
using System.IO;


public class ConfigFiles
{
    private static string ConfigDirectory() => 
        Path.Combine(AppSettings.AppRootPath, AppSettings.AppConfigSubdirectory);

    public static string GetCategoriesFile() => 
        Path.Combine(ConfigDirectory(), AppSettings.CategoriesConfigFileName);

    public static string GetDirectoriesIgnoreFile() => 
        Path.Combine(ConfigDirectory(), AppSettings.DirectoriesConfigFileName);

    public static string GetFilesIgnoreFile() => 
        Path.Combine(ConfigDirectory(), AppSettings.FilesIgnoreConfigFileName);
}
