using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace PhantomJs.NetCore {
    public class PdfGenerator {

        private string PhantomRootFolder;
        private OS Platform;

        public PdfGenerator(string phantomRootFolder) {

            if(!Directory.Exists(phantomRootFolder)) {
                throw new ArgumentException(String.Format("Invalid Path: No such folder exists: {0}",phantomRootFolder));
            }
            this.PhantomRootFolder = phantomRootFolder;
            this.Platform = GetOsPlatform();
        }

        enum OS {
            LINUX,
            WINDOWS,
            OSX
        }

        private OS GetOsPlatform() {
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                return OS.WINDOWS;
            }
        	else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
        		return OS.LINUX;
        	}
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                return OS.OSX;
            }

            throw new Exception("PdfGenerator: OS Environment could not be probed, halting!");
        }

        private string ExecutePhantomJs(string phantomJsExeToUse,string inputFileName, string outputFolder) {

            // The output file must be located in the output folder.
            string outputFilePath = Path.Combine(outputFolder, String.Format("{0}.pdf", inputFileName));
            

            string phantomJsAbsolutePath = Path.Combine(this.PhantomRootFolder,phantomJsExeToUse);
            ProcessStartInfo startInfo = new ProcessStartInfo(phantomJsAbsolutePath);
            startInfo.WorkingDirectory = this.PhantomRootFolder;
            startInfo.Arguments = 
                String.Format("rasterize.js \"{0}\" {1} \"A4\"",inputFileName,outputFilePath);
            startInfo.UseShellExecute = false;

            Process proc = new Process() { StartInfo = startInfo };
            proc.Start();

            proc.WaitForExit();

            return outputFilePath;
        }

        private string WriteHtmlToTempFile(string html) {
            string filename = Path.GetRandomFileName() + ".html";

            string absolutePath = Path.Combine(this.PhantomRootFolder, filename);

            File.WriteAllText(absolutePath, html);

            return filename;
        }

        /**
            This function takes in an 'html' string and generates a pdf
            containing its rendered version, and writes the pdf to the passed
            output 'outputFolder'.

            
         */
        public string GeneratePdf(string html, string outputFolder) {

            if(!Directory.Exists(outputFolder)) {
                throw new ArgumentException("The output folder is not a valid directory!");
            }


            string phantomExeToUse =
                (this.Platform == OS.LINUX) ? "linux64_phantomjs.exe" :
                (this.Platform == OS.WINDOWS) ? "windows_phantomjs.exe" :
                "osx_phantomjs.exe";
            
            // Write the passed html in a file.
            string htmlFileName = WriteHtmlToTempFile(html);

            string pdfFileName = "";
            try {
                pdfFileName = ExecutePhantomJs(phantomExeToUse,htmlFileName,outputFolder);
            } finally {
                // delete the temp file.
                File.Delete(Path.Combine(this.PhantomRootFolder,htmlFileName));
            }


            return pdfFileName;
        }
    }
}