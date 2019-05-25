using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class ConfigFileUtil
{
    public const string DoubleSlashComment = "//";
    public const string HashComment = "#";
    const string OpenCategory = "[";
    const string CloseCategory = "]";
    const string KeyEqualsValue = "=";
    public const string OpenCloseEnvironmentVariable = "%";
    public const string OpenCloseSpecialFolder = "$";


    public static ConfigSettings[] LoadConfigFile(string path)
    {
        const string DefaultCategory = "Default";

        var settings = new List<ConfigSettings>();

        string category = DefaultCategory;
        try
        {
            foreach (string line in File.ReadLines(path))
            {
                string lineContents = line.StripComments().ResolveEnvironmentVariables().ResolveSpecialFolders();

                if (!IsBlankLine(lineContents))
                {
                    if (IsCategoryLine(lineContents, out string newCategory))
                        category = newCategory;
                    else if (IsKeyEqualValueLine(lineContents, out string key, out string value))
                        settings.Add(new ConfigSettings(category, key, value));
                    else
                        settings.Add(new ConfigSettings(category, lineContents.Trim()));
                }
            }
        }
        catch (Exception e)
        {
            ConsoleUtil.WriteLineColor(String.Format("*** Exception loading config file '{0}' - {1}", path, e.Message),
                ConsoleColor.Red);
        }
        
        return settings.ToArray();
    }

    private static bool IsBlankLine(string line)
    {
        return (line.Trim() == "");
    }

    private static bool IsCategoryLine(string line, out string newCategory)
    {
        if (line.StartsWith(OpenCategory))
        {
            int closeIndex = line.IndexOf(CloseCategory);
            if (closeIndex != -1)
            {
                newCategory = line.Substring(1, closeIndex - 1).Trim();  // Leave out opening and closing characters and enclosed whitespace
                return true;
            }
        }

        newCategory = null;
        return false;
    }

    private static bool IsKeyEqualValueLine(string line, out string key, out string value)
    {
        int equalsLocation = line.IndexOf(KeyEqualsValue);

        // -1 means no equals 
        // Shortest valid line is "x=" where equalsLocation is 1
        if (equalsLocation > 0)
        {
            key = line.Substring(0, equalsLocation).Trim();
            value = line.Substring(equalsLocation + 1).Trim();

            // Don't allow an empty key, but allow an empty value
            if (key.Length == 0)
                return false;
            else
                return true;
        }
        else
        {
            key = null; value = null;
            return false;
        }
    }
}

public static class StringHelper
{
    public static string StripComments(this string s)
    {
        int i = s.IndexOf(ConfigFileUtil.DoubleSlashComment);
        int j = s.IndexOf(ConfigFileUtil.HashComment);

        if (i == j) return s.Trim();   // Can only be == when both are -1

        int commentStart;

        if (i == -1)
            commentStart = j;
        else if (j == -1)
            commentStart = i;
        else
            commentStart = Math.Min(i, j);

        return s.Substring(0, commentStart).Trim();
    }

    public static string ResolveSpecialFolders(this string s)
    {
        string x = s;
        do
        {
            int i = x.IndexOf(ConfigFileUtil.OpenCloseSpecialFolder);
            if (i == -1)
                return x.Trim();

            int j = x.IndexOf(ConfigFileUtil.OpenCloseSpecialFolder, i + 1);
            if ((j == -1) || (j == (i + 1)))
                return x.Trim();

            string folder = FileUtil.GetSpecialFolder(x.Substring(i + 1, (j - i) - 1));
            if (folder == null)
                x = x.Substring(0, i) + x.Substring(j + 1);
            else
                x = x.Substring(0, i) + folder + x.Substring(j + 1);
        }
        while (true);
    }

    public static string ResolveEnvironmentVariables(this string s)
    {
        string x = s;
        do
        {
            int i = x.IndexOf(ConfigFileUtil.OpenCloseEnvironmentVariable);
            if (i == -1)
                return x.Trim();

            int j = x.IndexOf(ConfigFileUtil.OpenCloseEnvironmentVariable, i + 1);
            if ((j == -1) || (j == (i + 1)))
                return x.Trim();

            string env = Environment.GetEnvironmentVariable(x.Substring(i + 1, (j - i) - 1));
            if (env == null)
                x = x.Substring(0, i) + x.Substring(j + 1);
            else
                x = x.Substring(0, i) + env + x.Substring(j + 1);
        }
        while (true);
    }
}

