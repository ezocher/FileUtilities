using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


class ConfigFileUtil
{
    const string CategoryRegex = @"^\[([^\]]*)\]";
    const string SpecialFoldersRegex = @"\$([^\$]*)\$";
    const string EnvironmentVariablesRegex = @"%([^%]*)%";

    const string UnescapedCommentRegex = @"(?<!\\)#";
    const string CommentStart = "#";
    const string EscapedCommentStart = @"\#";  // Currently only implemented for comments

    const string KeyEqualsValue = "=";
    const char ValueSeparator = ',';

    const string DefaultCategory = "";

    public static ConfigSettings[] LoadConfigFile(string path)
    {
        var settings = new List<ConfigSettings>();

        string category = DefaultCategory;
        try
        {
            foreach (string line in File.ReadLines(path))
            {
                string lineContents = StripComments(line);

                if (!IsBlankLine(lineContents))
                {
                    lineContents = ResolveEnvironmentVariables( ResolveSpecialFolders(lineContents) );
                    if (!IsBlankLine(lineContents))
                    {
                        if (IsCategoryLine(lineContents, out string newCategory))
                            category = newCategory;
                        else if (IsKeyEqualValueLine(lineContents, out string key, out string value, out bool equalWithNoKey))
                            settings.Add(new ConfigSettings(category, key, value));
                        else if (!equalWithNoKey)
                            // It must be a key-only line (possibly with comma seperation)
                            ProcessValuesOnlyLine(category, lineContents, settings);
                    }
                }
            }
        }
        catch (Exception e)
        {
            ConsoleUtil.WriteLineColor(String.Format("*** Exception loading config file '{0}' - {1}", path, e.Message),
                ConsoleColor.Red);
            throw e;
        }
        
        return settings.ToArray();
    }

    static string StripComments(string line)
    {
        // Strip unescaped comments
        Match m = Regex.Match(line, UnescapedCommentRegex);
        if (m.Success)
            line = line.Substring(0, m.Index);

        // Remove escape characters from escaped comment chars
        line = line.Replace(EscapedCommentStart, CommentStart);

        return line.Trim();
    }

    static bool IsBlankLine(string line)
    {
        return (line.Trim() == "");
    }

    static string ResolveSpecialFolders(string line)
    {
        do
        {
            Match m = Regex.Match(line, SpecialFoldersRegex);
            if (m.Success)
            {
                string specialFolder = FileUtil.GetSpecialFolder(m.Groups[1].ToString());
                if (specialFolder == null)
                    line = line.Remove(m.Index, m.Length);
                else
                    line = line.Remove(m.Index, m.Length).Insert(m.Index, specialFolder);
            }
            else
                return line.Trim();
        }
        while (true);
    }

    static string ResolveEnvironmentVariables(string line)
    {
        do
        {
            Match m = Regex.Match(line, EnvironmentVariablesRegex);
            if (m.Success)
            {
                string environmentValue = Environment.GetEnvironmentVariable(m.Groups[1].ToString());
                if (environmentValue == null)
                    line = line.Remove(m.Index, m.Length);
                else
                    line = line.Remove(m.Index, m.Length).Insert(m.Index, environmentValue);
            }
            else
                return line.Trim();
        }
        while (true);
    }

    static bool IsCategoryLine(string line, out string newCategory)
    {
        Match m = Regex.Match(line, CategoryRegex);
        if (m.Success)
        {
            newCategory = m.Groups[1].ToString().Trim();
            return true;
        }
        else
        {
            newCategory = null;
            return false;
        }
    }

    static bool IsKeyEqualValueLine(string line, out string key, out string value, out bool equalWithNoKey)
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

    static void ProcessValuesOnlyLine(string category, string line, List<ConfigSettings> settings)
    {
        int separatorLocation = line.IndexOf(ValueSeparator);

        if (separatorLocation == -1)
            settings.Add(new ConfigSettings(category, line.Trim()));
        else
        {
            string[] values = line.Split(new char[] { ValueSeparator });

            foreach (string v in values)
            {
                string value = v.Trim();
                if (value != "")
                    settings.Add(new ConfigSettings(category, value));
            }
        }
    }
}


//             ConfigFileUtil_ManualTests.Run();
public class ConfigFileUtil_ManualTests
{
    public static void Run()
    {
        LoadConfigFile_Test();
    }

    static void LoadConfigFile_Test()
    {
        const string testFileName = "TestConfigFile.txt";

        string configFolderPath = Path.Combine(Environment.GetFolderPath((Environment.SpecialFolder.UserProfile)), @"Repos\FileUtilities\Config\");
        var testList = ConfigFileUtil.LoadConfigFile(Path.Combine(configFolderPath, testFileName));
        Console.WriteLine("Loaded config file '{0}' - {1} config settings found", testFileName, testList.Length);

        int i = 0;
        foreach (ConfigSettings setting in testList)
            Console.WriteLine("{0} {1}", i++, setting.ToString());
    }
}