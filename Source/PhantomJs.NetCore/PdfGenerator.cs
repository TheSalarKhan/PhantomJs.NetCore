using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PhantomJs.NetCore
{
  /// <summary>
  /// This class is responsible for encapsulation of all logic to transform
  /// a <c>html</c> page into a <c>PDF</c> document.
  /// </summary>
  public class PdfGenerator
  {
    private string PhantomRootFolder { get; } = AppDomain.CurrentDomain.BaseDirectory;

    /// <summary>
    /// This function takes in an 'html' string and generates a pdf
    /// containing its rendered version, and writes the pdf to the passed
    /// output 'outputFolder'.
    /// </summary>
    /// <param name="html">A string with html page contents.</param>
    /// <param name="outputFolder">The folder where the file will be created.</param>
    /// <returns>The absolute path to the new file created.</returns>
    public string GeneratePdf(string html, string outputFolder)
    {
      if (!Directory.Exists(outputFolder))
      {
        throw new DirectoryNotFoundException("The output folder is not a valid directory!");
      }

      string pdfFileName;
      var htmlFileName = WriteHtmlToTempFile(html);

      try
      {
        var exeToUse = GetOsExecutableName();
        WriteResourcesToDisk(exeToUse);
        WriteResourcesToDisk(Consts.Rasterize);
        pdfFileName = ExecutePhantomJs(exeToUse, htmlFileName, outputFolder);
      }
      finally
      {
        File.Delete(Path.Combine(PhantomRootFolder, htmlFileName));
      }

      return pdfFileName;
    }

    private string GetOsExecutableName() =>
      RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Consts.WinEXE
      : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? Consts.LinuxEXE
      : Consts.OSXEXE;

    private string WriteHtmlToTempFile(string html)
    {
      var filename = Path.GetRandomFileName();
      var absolutePath = Path.Combine(PhantomRootFolder, filename);

      File.WriteAllText(absolutePath, html);

      return filename;
    }

    private string ExecutePhantomJs(string phantomJsExeToUse, string inputFileName, string outputFolder)
    {
      var outputFileName = Path.GetFileNameWithoutExtension(inputFileName);
      var outputFilePath = Path.Combine(outputFolder, $"{outputFileName}.pdf");
      var exePath = Path.Combine(PhantomRootFolder, phantomJsExeToUse);

      var startInfo = new ProcessStartInfo
      {
        FileName = exePath,
        WorkingDirectory = outputFolder,
        Arguments = $@"rasterize.js ""{inputFileName}"" ""{outputFilePath}"" ""A4"" ",
        UseShellExecute = false,
        CreateNoWindow = true
      };

      var proc = new Process { StartInfo = startInfo };
      proc.Start();
      proc.WaitForExit();

      return outputFilePath;
    }

    private void WriteResourcesToDisk(string exeFile)
    {
      if (File.Exists(exeFile)) return;

      var assembly = Assembly.GetExecutingAssembly();
      var resourcePath = $"PhantomJs.NetCore.Resources.{exeFile}";
      var outputPath = Path.Combine(PhantomRootFolder, exeFile);

      using (var stream = assembly.GetManifestResourceStream(resourcePath))
      using (var binaryReader = new BinaryReader(stream))
      using (var fileStream = new FileStream(outputPath, FileMode.Create))
      using (var binaryWriter = new BinaryWriter(fileStream))
      {
        byte[] byteArray = new byte[stream.Length];
        stream.Read(byteArray, 0, byteArray.Length);
        binaryWriter.Write(byteArray);
      }
    }

  }
}