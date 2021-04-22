using System;
using System.IO;

public class FileAnalysis
{
	public static void CountByteValues(FileInfo fi, StreamWriter reportFile)
	{
        // Report header
        reportFile.WriteLine("Count of byte values for file {0} (size: {1} bytes)", fi.FullName, fi.Length);
        Console.WriteLine("\nCounting byte values in file {0} (size: {1} bytes)", fi.FullName, fi.Length);

        // Open file
        FileStream file;

        try
        {
            file = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read);     // FileAccess.Read opens read-only files
        }
        catch (System.Exception ex)
        {
            reportFile.WriteLine("** Exception opening file: " + ex.Message);
            Console.WriteLine("** Exception opening file: " + ex.Message);
            return  ;
        }


        // Read and count bytes
        long totalBytesRead = 0;
        long[] Counts = new long[Byte.MaxValue + 1];    // Automatically initialized to all zeros

        try
        {
            const int bufferSize = 1024 * 1024;
            byte[] buffer = new byte[bufferSize];

            int bytesRead;
            while ( (bytesRead = file.Read(buffer, 0, bufferSize)) > 0 )
            {
                totalBytesRead += bytesRead;
                for (int i = 0; i < bytesRead; i++)
                    Counts[buffer[i]]++;
            }

            file.Close();
        }
        catch (System.Exception ex)
        {
            reportFile.WriteLine("** Exception reading file: " + ex.Message);
            Console.WriteLine("** Exception reading file: " + ex.Message);
            return;
        }

        // Write report
        reportFile.WriteLine("Total bytes read: " + totalBytesRead);
        Console.WriteLine("Total bytes read: " + totalBytesRead);

        const int firstPrintableCharacterIndex = 32;

        for (int i = 0; i <= Byte.MaxValue; i++)
        {
            if (Counts[i] > 0)
            {
                if (i < firstPrintableCharacterIndex)
                {
                    reportFile.WriteLine("{0}\t\t{1}", i, Counts[i]);
                    Console.WriteLine("{0}      {1}", i, Counts[i]);
                }
                else
                {
                    reportFile.WriteLine("{0}\t'{1}'\t{2}", i, (char)i, Counts[i]);
                    Console.WriteLine("{0}   '{1}'   {2}", i, (char)i, Counts[i]);
                }
            }
        }

        return;
	}
}
