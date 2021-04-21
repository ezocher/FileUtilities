using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeDupScanner
{
    class TimerUtil
    {
        // Example usage:
        //  double elapsedTimeMs = MillisecondsFrac(stopwatch.ElapsedTicks);
        public static double MillisecondsFrac(long ticks)
        {
            const double MillisecondsPerSecond = 1000.0d;
            return ((((double)ticks / (double)System.Diagnostics.Stopwatch.Frequency) * MillisecondsPerSecond));
        }

        // Example usage:
        //  double elapsedTimeSec = SecondsFrac(stopwatch.ElapsedTicks);
        public static double SecondsFrac(long ticks)
        {
            return ((double)ticks / (double)System.Diagnostics.Stopwatch.Frequency);
        }

        public static string FormatMilliseconds(long ms)
        {
            double[] divisors = { 1000.0, 60.0, 60.0, 24.0, 365.25 };
            string[] TimeName = { "ms", "sec.", "min.", "hours", "days", "years" };
            int MaxLevels = divisors.Length;

            double accumulator = ms;
            int levels = 0;

            while (accumulator >= divisors[levels])
            {
                accumulator /= divisors[levels++];
                if (levels == MaxLevels)
                    break;
            }

            // Display times with different formats for aesthetics/readability
            string timeNumberFormat;
            if (levels == 0)
                timeNumberFormat = "N0";
            else if (levels <= 2)
                timeNumberFormat = "F1";
            else
                timeNumberFormat = "F2";

            return String.Format("{0:" + timeNumberFormat + "} {1}", accumulator, TimeName[levels]);
        }
    }

    class SampleTimerUtil
    {
        /* Put into main():
            TestTimerUtil.TestFormatMilliseconds();
            ConsoleUtil.WaitForKeyPress(); return;
        */
        public static void FormatMillisecondsSamples()
        {                       //  1/3     1     7
            long[] sampleValues = {   333, 1000, 7000,                        // seconds
                                    19980, 60000, 420000,                   // minutes
                                    1198800, 3600000, 25200000,             // hours
                                    28771200, 86400000, 604800000,          // days
                                    10508680800, 31557600000,  220903200000,    // years
                                    Int64.MaxValue
            };

            foreach (long i in sampleValues)
                Console.WriteLine("{0:N0} ms = {1}", i, TimerUtil.FormatMilliseconds(i));
        }
    }
}

