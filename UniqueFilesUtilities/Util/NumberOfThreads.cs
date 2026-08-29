using System;

public class NumberOfThreads
{
    public static (int, int) Set(string drive)
    {
        int hardwareThreads = Environment.ProcessorCount;
        int numThreadsSolidStateDrive = hardwareThreads;
        int numThreadsRotatingDrive = Math.Min(hardwareThreads, 3);  // 3 threads arrived at by observation on several rotating drives (internal and USB)
        int numThreads;

        if (FileUtil.IsSystemDrive(drive))
            // Most current system drives are SSDs
            numThreads = numThreadsSolidStateDrive;
        else
            numThreads = numThreadsRotatingDrive;

        Console.Write("Run with {0} threads? ", numThreads);
        string input = Console.ReadLine();
        int i;
        if (Int32.TryParse(input, out i))
            numThreads = i;

        return(numThreads, hardwareThreads);
    }
}
