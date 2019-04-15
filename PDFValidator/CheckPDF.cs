using System;
using System.IO;
using System.Text;

public class CheckPDF
{
    // Only checks the PDF and version string from the PDF header
    // 
    // Bad PDFs that I received from book scanning service were MBs of 0x00s - this quickly detects these

    public static string Check(FileInfo fi, StreamWriter listFile)
	{
        string path = fi.FullName;

        const int headerBufferSize = 32;
        string PDFHeaderStart = "%PDF-";
        const int PDFVersionLength = 3;     // e.g. "1.5"
        byte[] headerBuffer = new byte[headerBufferSize];

        try
        {
            FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);     // FileAccess.Read opens read-only files
            file.Read(headerBuffer, 0, headerBufferSize);
            file.Close();
        }
        catch (System.Exception ex)
        {
            return "** Exception opening/reading file: " + ex.Message; ;
        }


        string header = Encoding.ASCII.GetString(headerBuffer);

        if ( PDFHeaderStart == header.Substring(0, PDFHeaderStart.Length) )
        {
            return header.Substring(PDFHeaderStart.Length, PDFVersionLength);
        }
        else
        {
            FileAnalysis.CountByteValues(fi, listFile);
            return "** Invalid PDF Header";
        }
	}

    public static bool IsPDF(FileInfo fi)
    {
        return FileUtil.IsExtension(".pdf", fi);
    }
}
