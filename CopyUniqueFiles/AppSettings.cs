using MetadataExtractor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace DeDupScanner
{
    //----------------------------------------------------------------------------------------------------------------------------------------
    //  This project can build three different related file management apps. Which app is being built is selected by the WhichApp setting
    //
    //  The tree apps are:
    //  * Fingerprint Database Maker
    //  * Unique File Copier
    //  * Photo Collector and Organizer


    public enum App
    {
        FingerprintDBMaker, UniqueFileCopier, PhotoCollector
    }

    internal class AppSettings
    {
        private const string OneDriveRootEnv = "OneDriveConsumer";
        private const string AppRootDirectory = "Files and Storage";
        private const string AppConfigSubdirectory = "AppConfig";
        private const string SettingsFileName = "AppSettings.txt";
        private const string WhichAppCategoryName = "WhichApp";
        private const string AllAppsCategoryName = "All Apps";

        // Names of proerties that are bool values are preceeded with this char in AppSettings.txt
        // All other properties are string values
        private const char BoolPropertyPrefix = '?';



        public static App WhichApp { get; set; }
        public static bool ExcludeHiddenAndSystem { get; set; }
        public static string CategoriesConfigFileName { get; set; }
        public static string DirectoriesConfigFileName { get; set; }
        public static string FilesIgnoreFileName { get; set; }
        public static string ReportFilesSubdirectory { get; set; }

        public static string appName { get; set; }
        public static string appDescription { get; set; }
        public static string operationDescription { get; set; }


    // TODO: load settings from AppSettings.txt
        public static void LoadAppSettings(string solutionRelativePath)
        {
            string appSettingsFilePath = Path.Combine(Environment.GetEnvironmentVariable(OneDriveRootEnv), AppRootDirectory, AppConfigSubdirectory, SettingsFileName);
            ConfigSettings[] appSettingsList = ConfigFileUtil.LoadConfigFile(appSettingsFilePath);

            if (appSettingsList[0].Category == WhichAppCategoryName)
                WhichApp = (App) Enum.Parse(typeof(App), appSettingsList[0].Value);
            else
                throw new Exception("Invalid value for WhichApp in " + appSettingsFilePath);

            foreach (ConfigSettings setting in appSettingsList)
                if (setting.Category == AllAppsCategoryName || setting.Category == WhichApp.ToString())
                    SetNewSetting(setting.Key, setting.Value);
        }

        private static void SetNewSetting(string whichProperty, string value)
        {
            if (whichProperty[0] == BoolPropertyPrefix)
            {
                // bool property
                whichProperty = whichProperty.Substring(1);
                PropertyInfo prop = typeof(AppSettings).GetProperty(whichProperty, BindingFlags.Public | BindingFlags.Static);
                prop.SetValue(null, value == "True");
            }
            else
            {
                // string property
                PropertyInfo prop = typeof(AppSettings).GetProperty(whichProperty, BindingFlags.Public | BindingFlags.Static);
                prop.SetValue(null, value);
            }
        }

    }
}
