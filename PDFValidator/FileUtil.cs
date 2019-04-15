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
    //	Dates: CreationTime, LastAccessTime, LastWriteTime,

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

    public static bool IsExtension(string extension, FileInfo fi)
    {
        return (fi.Extension.ToLower() == extension.ToLower());
    }


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
            return "Volume " + directory[0];
        else
            return d.Name;
    }

}
