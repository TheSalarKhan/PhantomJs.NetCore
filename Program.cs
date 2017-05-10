using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using PhantomJs.NetCore;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {

            PdfGenerator generator = new PdfGenerator("/home/salar/CURRENT_PROJECTS/GeneratePdfNETCore/PhantomJsRoot");

            string outputPath = generator.GeneratePdf("<h1>Hello Salar</h1>","/home/salar/CURRENT_PROJECTS");

            Console.WriteLine("Hello World!");
        }
    }
}
