using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class ConsoleUtil
{
    static Stack<ConsoleColor> savedForegroundColor;
    static Stack<ConsoleColor> savedBackgroundColor;

    public static readonly object _lockGlobalConsole = new object();

    const int wideTermWidth = 132; // like the old days
    const int largeBufferHeight = 999; // i think this might be the largest allowed
    public static string backspaceClearLine = new string('\b', wideTermWidth);


    public static void WaitForKeyPress()
    {
        lock (_lockGlobalConsole)
        {
            White();
            Console.WriteLine("\n\n(press any key to exit)");
            RestoreColors();
        }
        Console.ReadKey(true);
    }

    public static void InitConsoleSettings(string consoleWindowTitle)
    {

        Console.ForegroundColor = ConsoleColor.Green;
        Console.BackgroundColor = ConsoleColor.Black;
        savedForegroundColor = new Stack<ConsoleColor>();
        savedBackgroundColor = new Stack<ConsoleColor>();
        Console.WindowWidth = wideTermWidth;
        Console.BufferWidth = wideTermWidth;
        Console.BufferHeight = largeBufferHeight;
        if (consoleWindowTitle != "")
            Console.Title = consoleWindowTitle;
    }

    public static void SaveColors()
    {
        savedForegroundColor.Push(Console.ForegroundColor);
        savedBackgroundColor.Push(Console.BackgroundColor);
    }

    public static void RestoreColors()
    {
        Console.ForegroundColor = savedForegroundColor.Pop();
        Console.BackgroundColor = savedBackgroundColor.Pop();
    }

    public static void WriteColor(string s, ConsoleColor c)
    {
        lock (_lockGlobalConsole)
        {
            SaveColors();
            Console.ForegroundColor = c;
            Console.Write(s);
            RestoreColors();
        }
    }

    public static void WriteLineColor(string s, ConsoleColor c)
    {
        WriteColor(s + "\n", c);
    }

    public static void Red()
    {
        SaveColors();
        Console.ForegroundColor = ConsoleColor.Red;
    }

    public static void Green()
    {
        SaveColors();
        Console.ForegroundColor = ConsoleColor.Green;
    }

    public static void Blue()
    {
        SaveColors();
        Console.ForegroundColor = ConsoleColor.Blue;
    }

    public static void White()
    {
        SaveColors();
        Console.ForegroundColor = ConsoleColor.White;
    }

    public static void Cyan()
    {
        SaveColors();
        Console.ForegroundColor = ConsoleColor.Cyan;
    }

    public static void Magenta()
    {
        SaveColors();
        Console.ForegroundColor = ConsoleColor.Magenta;
    }

    public static void Yellow()
    {
        SaveColors();
        Console.ForegroundColor = ConsoleColor.Yellow;
    }

    public static void Red(string s)
    {
        WriteColor(s, ConsoleColor.Red);
    }

    public static void Green(string s)
    {
        WriteColor(s, ConsoleColor.Green);
    }

    public static void Blue(string s)
    {
        WriteColor(s, ConsoleColor.Blue);
    }

    public static void White(string s)
    {
        WriteColor(s, ConsoleColor.White);
    }

    public static void Cyan(string s)
    {
        WriteColor(s, ConsoleColor.Cyan);
    }

    public static void Magenta(string s)
    {
        WriteColor(s, ConsoleColor.Magenta);
    }

    public static void Yellow(string s)
    {
        WriteColor(s, ConsoleColor.Yellow);
    }
}
