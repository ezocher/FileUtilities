using MetadataExtractor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


//----------------------------------------------------------------------------------------------------------------------------------------
//  This project can run three different related file management apps. Which app is being run is selected by the WhichApp category,
//  which is the first setting in the AppSettings.txt file
//
//  The three apps are:
//  * Fingerprint Database Maker (FingerprintDBMaker)
//  * Unique File Copier (UniqueFileCopier)
//  * Photo Collector and Organizer (PhotoCollector)
//
// If the app's .exe is named one of these three names then the app name will override the WichApp setting in AppSettings.txt
//
//----------------------------------------------------------------------------------------------------------------------------------------

public enum App
{
    FingerprintDBMaker, UniqueFileCopier, PhotoCollector
}

internal class AppSettings
{
    // Settings needed to load AppSettings.txt and set which app
    public const string OneDriveRootEnvironment = "OneDriveConsumer";
    private const string AppRootDirectory = "Files and Storage";
    public static string AppRootPath { get; private set; }              // Combined $OneDrive$\Files and Storage
    public const string AppConfigSubdirectory = "AppConfig";
    private const string SettingsFileName = "AppSettings.txt";
    private const string WhichAppCategoryName = "WhichApp";
    private const string AllAppsCategoryName = "All Apps";

    // Names of proerties that are bool or int values are preceeded with these chars in AppSettings.txt
    // Properties with no prefix are string values
    private const char BoolPropertyPrefix = '?';
    private const char IntPropertyPrefix = '+';

    public static App WhichApp { get; set; }
    public static bool ExcludeHiddenAndSystem { get; set; }
    public static string DefaultDestinationVolume { get; set; }

    // Settings files
    public static string CategoriesConfigFileName { get; set; }
    public static string DirectoriesIgnoreFileName { get; set; }
    public static string FilesIgnoreFileName { get; set; }

    // Report and DB files
    public static string ReportFilesSubdirectory { get; set; }
    public static string ReportFilesExtension { get; set; }
    public static string FilesDBNameSuffix { get; set; }
    public static string ExcludedReportNameSuffix { get; set; }
    public static string DuplicatesReportNameSuffix { get; set; }
    public static string UniquesReportNameSuffix { get; set; }
    public static string PhotosReportNameSuffix { get; set; }

    public static string appName { get; set; }
    public static string appDescription { get; set; }
    public static string operationDescription { get; set; }
    public static string unknownCategoryName { get; set; }
    public static string destRootPrefix { get; set; }
    public static string photosDestRootFolder { get; set; }
    public static string videosDestRootFolder { get; set; }
    public static string BaseFileDBsDirectory { get; set; }
    public static string DBFileNameFilter { get; set; }

    public static string DirectoriesIgnoreCategory { get; set; }
    public static string ExtensionsIgnoreCategory { get; set; }
    public static string PhotoFileExtensionsCategory { get; set; }
    public static string VideoFileExtensionsCategory { get; set; }
    public static string unknownYearName { get; set; }
    public static bool CreatePhotosReport { get; set; }

    //----------------------------------------------------------------------------------------------------------------------------------------
    // Settings determined at run time for each run
    //----------------------------------------------------------------------------------------------------------------------------------------
    public static string BaseName { get; set; }

    // Load the AppSettings.txt file and set the WhichApp property and other properties in this class
    public static void LoadAppSettings()
    {
        AppRootPath = Path.Combine(Environment.GetEnvironmentVariable(OneDriveRootEnvironment), AppRootDirectory);

        string appSettingsFileFullPath = Path.Combine(AppRootPath, AppConfigSubdirectory, SettingsFileName);
        ConfigSettings[] appSettingsList = ConfigFileUtil.LoadConfigFile(appSettingsFileFullPath);

        if (appSettingsList[0].Category == WhichAppCategoryName)
            WhichApp = (App) Enum.Parse(typeof(App), appSettingsList[0].Value);
        else
            // throw exception if the first setting is not in the WhichApp category
            throw new Exception("WhichApp nust be the first setting in " + appSettingsFileFullPath);

        // If the .exe is named one of the three app names then override the WhichApp setting from the settings file
        string exeName = FileUtil.GetExeName();
        if (Enum.TryParse<App>(exeName, out App exeApp))
        {
            WhichApp = exeApp;
        }

        foreach (ConfigSettings setting in appSettingsList)
            if (setting.Category == AllAppsCategoryName || setting.Category == WhichApp.ToString())
                SetNewSetting(setting.Key, setting.Value);
    }

    // Uses reflection to convert strings from the settings file in values that it sets for properties in this class 
    private static void SetNewSetting(string whichProperty, string value)
    {
        if (whichProperty[0] == BoolPropertyPrefix)
        {
            // bool property
            whichProperty = whichProperty.Substring(1);
            PropertyInfo prop = typeof(AppSettings).GetProperty(whichProperty, BindingFlags.Public | BindingFlags.Static);
            prop.SetValue(null, value.ToLower() == "true");
        }
        else if (whichProperty[0] == IntPropertyPrefix)
        {
            // int property
            whichProperty = whichProperty.Substring(1);
            PropertyInfo prop = typeof(AppSettings).GetProperty(whichProperty, BindingFlags.Public | BindingFlags.Static);
            prop.SetValue(null, int.Parse(value));
        }
        else
        {
            // string property
            PropertyInfo prop = typeof(AppSettings).GetProperty(whichProperty, BindingFlags.Public | BindingFlags.Static);
            prop.SetValue(null, value);
        }
    }

}
