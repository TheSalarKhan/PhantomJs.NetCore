using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;

using PhantomJs.NetCore.Enums;

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
    /// <param name="outputFolder">The folder where the file will be created.
    /// <param name="param">An instance of <c>PdfGeneratorParams</c> class.</param>
    /// If the output folder is null, empty or doesn't exist, the current app
    /// directory will be used.</param>
    /// <returns>The absolute path to the new file created.</returns>
    public string GeneratePdf(string html, string outputFolder = null, PdfGeneratorParams param = null)
    {
      if (string.IsNullOrEmpty(outputFolder) || !Directory.Exists(outputFolder))
        outputFolder = PhantomRootFolder;

      string pdfFileName;
      var htmlFileName = WriteHtmlToTempFile(html);

      try
      {
        var exeToUse = GetOsExecutableName();
        WriteResourcesToDisk(exeToUse);
        WriteResourcesToDisk(Consts.Rasterize);
        pdfFileName = ExecutePhantomJs(exeToUse, htmlFileName, outputFolder, param);
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
      var filename = Path.GetRandomFileName() + ".html";
      var absolutePath = Path.Combine(PhantomRootFolder, filename);

      File.WriteAllText(absolutePath, html);

      return filename;
    }

    private void SetFilePermission(string fileName)
    {
      var startInfo = new ProcessStartInfo
      {
        FileName = "/bin/bash",
        WorkingDirectory = PhantomRootFolder,
        Arguments = $@"-c ""chmod +x {fileName}""",
        UseShellExecute = false,
        CreateNoWindow = true
      };

      var proc = new Process { StartInfo = startInfo };
      proc.Start();
      proc.WaitForExit();
    }

    private string ExecutePhantomJs(string phantomJsExeToUse, string inputFileName, string outputFolder, PdfGeneratorParams param)
    {
      if (param == null) param = new PdfGeneratorParams();

      var layout = param.PageWidth > 0 && param.PageHeight > 0
        ? $"{param.PageWidth}{param.DimensionUnit.GetValue()}*{param.PageHeight}{param.DimensionUnit.GetValue()}"
        : param.Format.ToString();
      var outputFileName = Path.GetFileNameWithoutExtension(inputFileName);
      var outputFilePath = Path.Combine(outputFolder, $"{outputFileName}.pdf");
      var exePath = Path.Combine(PhantomRootFolder, phantomJsExeToUse);

      // On OSX and - maybe - Linux (TODO clarify), we have to add the executable bit to
      // file permissions of our unzipped exe.
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
        || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
      {
        SetFilePermission(phantomJsExeToUse);
      }

      var startInfo = new ProcessStartInfo
      {
        FileName = exePath,
        WorkingDirectory = PhantomRootFolder,
        Arguments = $@"rasterize.js ""{inputFileName}"" ""{outputFilePath}"" ""{layout}"" {param.ZoomFactor}",
          // TODO: include orientation parameter ""{param.Orientation.GetValue()}"" ",
        UseShellExecute = false,
        CreateNoWindow = true
      };

      var proc = new Process { StartInfo = startInfo };
      proc.Start();
      proc.WaitForExit();

      return outputFilePath;
    }

    private void WriteResourcesToDisk(string fileName)
    {
      var zipFileName = $"{Path.GetFileNameWithoutExtension(fileName)}.zip";
      var zipFilePath = Path.Combine(PhantomRootFolder, zipFileName);
      var unzippedFilePath = Path.Combine(PhantomRootFolder, fileName);

      if (File.Exists(zipFilePath) || File.Exists(unzippedFilePath)) return;

      var assembly = Assembly.GetExecutingAssembly();
      var resourcePath = $"PhantomJs.NetCore.Resources.{zipFileName}";

      using (var stream = assembly.GetManifestResourceStream(resourcePath))
      using (var binaryReader = new BinaryReader(stream))
      using (var fileStream = new FileStream(zipFilePath, FileMode.Create))
      using (var binaryWriter = new BinaryWriter(fileStream))
      {
        byte[] byteArray = new byte[stream.Length];
        stream.Read(byteArray, 0, byteArray.Length);
        binaryWriter.Write(byteArray);
      }

      ZipFile.ExtractToDirectory(zipFilePath, PhantomRootFolder);
      File.Delete(zipFilePath);
    }

  }
}
