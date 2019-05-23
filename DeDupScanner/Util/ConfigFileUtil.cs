using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class ConfigFileUtil
{
    public static ConfigSettings[] LoadConfigFile(string path)
    {
        const string DefaultCategory = "Default";

        var settings = new List<ConfigSettings>();

        string category = DefaultCategory;
        try
        {
            foreach (string line in File.ReadLines(path))
            {
                if (!IsCommentOrBlankLine(line))
                {
                    if (IsCategoryLine(line, out string newCategory))
                    {
                        category = newCategory;
                    }
                    else
                    {
                        settings.Add(new ConfigSettings(category, line.StripTrailingComments()));
                    }
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

    public const string DoubleSlashComment = "//";
    public const string HashComment = "#";

    private static bool IsCommentOrBlankLine(string line)
    {
        return (line.Trim().StartsWith(DoubleSlashComment) || line.Trim().StartsWith(HashComment) || (line.Trim() == ""));
    }

    private static bool IsCategoryLine(string line, out string newCategory)
    {
        const string OpenCategory = "[";
        const string CloseCategory = "]";

        if (line.StartsWith(OpenCategory))
        {
            int closeIndex = line.IndexOf(CloseCategory);
            if (closeIndex != -1)
            {
                newCategory = line.Substring(1, closeIndex - 1);  // Leave out opening and closing characters
                return true;
            }
        }

        newCategory = null;
        return false;
    }
}
public static class StringHelper
{
    public static string StripTrailingComments(this string s)
    {
        int i = s.IndexOf(ConfigFileUtil.DoubleSlashComment);
        int j = s.IndexOf(ConfigFileUtil.HashComment);

        if (i == j) return s;   // Can only be == when both are -1

        int commentStart;

        if (i == -1)
            commentStart = j;
        else if (j == -1)
            commentStart = i;
        else
            commentStart = Math.Min(i, j);

        return s.Substring(0, commentStart).Trim();
    }
}

