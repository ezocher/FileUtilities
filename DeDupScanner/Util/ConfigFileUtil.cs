using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class ConfigFileUtil
{
    public const string LineCommentStart = "#";
    public const char EscapeCharacter = '\\';  // Currently only implemented for comments
    const string OpenCategory = "[";
    const string CloseCategory = "]";
    const string KeyEqualsValue = "=";
    public const string OpenCloseEnvironmentVariable = "%";
    public const string OpenCloseSpecialFolder = "$";
    const string DefaultCategory = "";


    public static ConfigSettings[] LoadConfigFile(string path)
    {
        var settings = new List<ConfigSettings>();

        string category = DefaultCategory;
        try
        {
            foreach (string line in File.ReadLines(path))
            {
                string lineContents = line.StripComments();

                if (!IsBlankLine(lineContents))
                {
                    lineContents = lineContents.ResolveEnvironmentVariables().ResolveSpecialFolders();
                    if (!IsBlankLine(lineContents))
                    {
                        if (IsCategoryLine(lineContents, out string newCategory))
                            category = newCategory;
                        else if (IsKeyEqualValueLine(lineContents, out string key, out string value, out bool equalWithNoKey))
                            settings.Add(new ConfigSettings(category, key, value));
                        else if (!equalWithNoKey)
                            settings.Add(new ConfigSettings(category, lineContents.Trim()));
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

    private static bool IsKeyEqualValueLine(string line, out string key, out string value, out bool equalWithNoKey)
    {
        int equalsLocation = line.IndexOf(KeyEqualsValue);
        equalWithNoKey = false;

        if (equalsLocation != -1)
        {
            key = line.Substring(0, equalsLocation).Trim();
            value = line.Substring(equalsLocation + 1).Trim();

            // Don't allow an empty key, but allow an empty value
            if (key.Length == 0)
            {
                equalWithNoKey = true;
                return false;
            }
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
        int foundIndex;
        int startIndex = 0;

        string returnString = s;

        do
        {
            foundIndex = returnString.IndexOf(ConfigFileUtil.LineCommentStart, startIndex);
            if (foundIndex == -1)
                // Not found
                break;
            else if ( (foundIndex > 0) && (returnString[foundIndex - 1] == ConfigFileUtil.EscapeCharacter))
            // Remove escape character and skip over escaped comment start character, then continue searching
            {
                returnString = returnString.Remove(foundIndex - 1, 1);
                startIndex = foundIndex;
            }
            else
                // Found, but not escaped
                break;
        }
        while (true);


        if (foundIndex == -1)
            return returnString.Trim();
        else
            return returnString.Substring(0, foundIndex).Trim();
    }

    public static string ResolveSpecialFolders(this string s)
    {
        string returnString = s;
        do
        {
            int i = returnString.IndexOf(ConfigFileUtil.OpenCloseSpecialFolder);
            if (i == -1)
                return returnString.Trim();

            int j = returnString.IndexOf(ConfigFileUtil.OpenCloseSpecialFolder, i + 1);
            if ((j == -1) || (j == (i + 1)))
                return returnString.Trim();

            string folder = FileUtil.GetSpecialFolder(returnString.Substring(i + 1, (j - i) - 1));
            if (folder == null)
                returnString = returnString.Substring(0, i) + returnString.Substring(j + 1);
            else
                returnString = returnString.Substring(0, i) + folder + returnString.Substring(j + 1);
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

public class ConfigFileUtil_ManualTests
{
    public static void LoadConfigFile_Test()
    {
        const string testFileName = "TestConfigFile.txt";

        string configFolderPath = Path.Combine(Environment.GetFolderPath((Environment.SpecialFolder.UserProfile)), @"Repos\FileUtilities\Config\");
        var testList = ConfigFileUtil.LoadConfigFile(Path.Combine(configFolderPath, testFileName));
        Console.WriteLine("Loaded config file '{0}' - {1} config settings found", testFileName, testList.Length);

        int i = 0;
        foreach (ConfigSettings setting in testList)
            Console.WriteLine("{0} {1}", i++, setting);
    }
}

