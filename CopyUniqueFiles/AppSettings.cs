using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static App WhichApp { get; set; }

        // TODO: load settings from AppSettings.txt
        public static void LoadAppSettings(string solutionRelativePath)
        {
            WhichApp = App.PhotoCollector;
        }

    }
}
