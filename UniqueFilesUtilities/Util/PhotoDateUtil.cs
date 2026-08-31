using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ExifLibrary;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

public class PhotoDateUtil
{
    // The oldest surviving photograph taken in America is a September 25, 1839, daguerreotype of Philadelphia’s Central High School captured by inventor Joseph Saxton from a window at the U.S. Mint
    // https://billypenn.com/2023/10/30/oldest-photograph-united-states-philadelphia-joseph-saxton/
    private const int MinPathYear = 1839;

    // Matches a run of exactly 4 digits, not part of a longer digit run (e.g. skips "12345" and "202", finds "2023" or 2023 in "2023-2024" ok, but rejects digits embedded in a longer number)
    private static readonly Regex FourDigitRegex = new Regex(@"(?<!\d)\d{4}(?!\d)", RegexOptions.Compiled);

    // Matches a run of exactly 8 digits, finds "20230410" in "Screenshot 20230410-233348.png"
    // Used to extract the year from the commonly occuring YYYYMMDD or YYYYDDMM date formats in file names
    private static readonly Regex EightDigitRegex = new Regex(@"(?<!\d)\d{8}(?!\d)", RegexOptions.Compiled);


    // Returns the year the photo was taken, read from the EXIF DateTimeOriginal tag
    // (falling back to DateTimeDigitized, then the IFD0 DateTime tag).
    // Returns null if the file has no EXIF data or none of those tags are present.
    public static int? ExtractMetadataYearTaken(string filePath)
    {
        IReadOnlyList<MetadataExtractor.Directory> directories;
        try
        {
            directories = ImageMetadataReader.ReadMetadata(filePath);
        }
        catch (Exception)
        {
            return null;
        }

        ExifSubIfdDirectory subIfd = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
        if (subIfd != null)
        {
            if (subIfd.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out DateTime dateTimeOriginal))
                return dateTimeOriginal.Year;

            if (subIfd.TryGetDateTime(ExifDirectoryBase.TagDateTimeDigitized, out DateTime dateTimeDigitized))
                return dateTimeDigitized.Year;
        }

        ExifIfd0Directory ifd0 = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
        if (ifd0 != null && ifd0.TryGetDateTime(ExifDirectoryBase.TagDateTime, out DateTime dateTime))
            return dateTime.Year;

        return null;
    }

    // Writes dateTaken into the EXIF DateTimeOriginal tag of the file at filePath, using
    // ExifLibrary (MetadataExtractor is read-only and cannot write tags). ExifLibrary edits
    // the EXIF metadata segment directly rather than re-encoding the image, but it only
    // supports JPEG and TIFF - not RAW formats like .arw or .cr2.
    public static void WriteDateTaken(DateTime dateTaken, string filePath)
    {
        ImageFile imageFile = ImageFile.FromFile(filePath);
        imageFile.Properties.Set(ExifTag.DateTimeOriginal, dateTaken);
        imageFile.Save(filePath);
    }

    // Searches the directory portion of filePath then the file name itself for a 4 digit
    // number between 1839 and the current year. Level 1 is the directory the file is in and
    // level 0 is the file name.
    //
    // Searches the root directory of the file first (level N), then each subdirectory in
    // turn (level N-1, N-2, ...2, 1, 0), and returns the first year found along with the level
    // it was found at. So if a photo lives at ...\Vacations\2020\Summer 2020 Trip\img99.jpg
    // it returns the 2020 and level 2. This enables the "Summer 2020 Trip" directory to be
    // preserved in the copy.
    //
    // Returns (null, null) if no such year is found anywhere in the path.

    // Rejects false years found in filenames like "IMG1999.jpg" or "PIC_2002.ARW"
    //  but extracts years if legit, eg Win 11 screenshots: "Screenshot 2026-04-10 233348.png"
    public static (int? Year, int? Level) GetYearFromPath(string filePath)
    {
        const int CharsInAYear = 4;
        int currentYear = DateTime.Now.Year;
        string[] pathLevels = filePath.Split(
            new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
            StringSplitOptions.RemoveEmptyEntries);

        for (int level = pathLevels.Length - 1; level >= 1 ; level--)
        {
            string pathLevel = pathLevels[pathLevels.Length - (level + 1)];

            foreach (Match match in FourDigitRegex.Matches(pathLevel))
            {
                int year = int.Parse(match.Value);
                if (year >= MinPathYear && year <= currentYear)
                {
                    return (year, level);
                }
            }
        }

        // Check for the year in the file name as YYYY part of YYYYMMDD
        Match matchfilename = EightDigitRegex.Match(pathLevels[pathLevels.Length - 1]);

        if (matchfilename.Success)
        {
            int year = int.Parse(matchfilename.Value.Substring(0, CharsInAYear));
            if (year >= MinPathYear && year <= currentYear)
                return (year, 0);
        }

        return (null, null);
    }

    public static void TestGetYearFromPath()
    {
        string[] testPaths =
        {
            @"C:\OneDrive\Photos\Projects\1999\IMG1999.jpg",
            @"C:\OneDrive\Photos\2020\2020 Trip to Bosnia\IMG1999.jpg",
            @"C:\OneDrive\Photos\+2021\Projects\2020 Trip to Bosnia\IMG1999.jpg",
            @"C:\+2022\aa\Projects\2020 Trip to Bosnia\IMG1999.jpg",
            @"C:\OneDrive\Photos\uu-2023\Projects\1820\aav\2020 Trip to Bosnia\IMG1999.jpg",
            @"C:\OneDrive\Photos\Kayaking 2005-2007\IMG1999.jpg",
            @"C:\OneDrive\Photos\2005-06\IMG1999.jpg",
            @"C:\OneDrive\Photos\200506\IMG1999.jpg",
            @"C:\Photos\img.arw",
            @"C:\Users\person\OneDrive\Pictures\Screenshots\Screenshot 2026-04-10 233348.png",
            @"C:\Users\person\OneDrive\Pictures\Screenshots\Screenshot 20260410-233348.png"
        };

        foreach (string filePath in testPaths)
        {
            Console.Write(filePath);
            (int? Year, int? Level) = GetYearFromPath(filePath);
            if (Year != null)
                Console.WriteLine(" - Year {0}, Level {1}", Year, Level);
            else
                Console.WriteLine(" - No year found in path");
        }
    }

}