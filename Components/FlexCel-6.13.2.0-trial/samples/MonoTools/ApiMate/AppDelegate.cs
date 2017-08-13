using System;
using CoreGraphics;
using Foundation;
using AppKit;
using ObjCRuntime;

namespace ApiMate
{
    public partial class AppDelegate : NSApplicationDelegate
    {
        public AppDelegate()
        {
            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
            {
                Exception ex = e.ExceptionObject as Exception;
                LogError(ex);
            };
        }
			
       
        public override bool ApplicationShouldOpenUntitledFile(NSApplication sender)
        {
            return true;
        }

        public override bool ApplicationOpenUntitledFile(NSApplication sender)
        {
            var controller = (NSDocumentController)NSDocumentController.SharedDocumentController;
            if (controller.Documents.Length == 0)
            {
                controller.OpenDocument(null);
            }
            return true;
        }

        public override bool ApplicationShouldTerminateAfterLastWindowClosed (NSApplication sender)
        {
            return true;
        }

        public static void LogError(Exception ex)
        {
            if (ex == null)
            {
                var alertu = new NSAlert();
                alertu.MessageText = "Unknown exception";
                alertu.AddButton("Ok");
                alertu.RunModal();
                return;
            }
            string Message = ex.Message;
            string Indent = String.Empty;
            while (ex.InnerException != null)
            {
                Indent += "    ";
                ex = ex.InnerException;
                Message += Indent + "->" + ex.Message + Environment.NewLine;
            }
            var alert = new NSAlert();
            alert.MessageText = Message;
            alert.AddButton("Ok");
            alert.RunModal();
        }


    }
}

