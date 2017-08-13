using System;
using CoreGraphics;
using Foundation;
using AppKit;
using ObjCRuntime;
using System.IO;
using APIMate;

namespace ApiMate
{
    class MainClass
    {
        static void Main(string[] args)
        {
            if (args.Length == 2 && args[0] == "--dump") //can be used for setting ApiMate as a source for a diff program.
            {
                ExportFile(args[1]);
                return;
            }

            NSApplication.Init();
            NSApplication.Main(args);
        }

        private static void ExportFile(string source)
        {
            Console.WriteLine("hi");
            APIMate.ApiMate Api = new APIMate.ApiMate(null);

            Api.Open(source);

            Console.Write(Api.Process(Language.CSharp, "xls", true, true));

        }
    }
}	

