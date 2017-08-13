using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;
using System.Threading.Tasks;
using APIMate;
using System.IO;
using FlexCel.XlsAdapter;
using System.Runtime.InteropServices;
using ObjCRuntime;

namespace ApiMate
{
    public partial class ApiMateDoc : AppKit.NSDocument
    {
        APIMate.ApiMate Api;
        string DocName;
        bool HasPass;


        // Called when created from unmanaged code
        public ApiMateDoc(IntPtr handle) : base (handle)
        {
        }

        void StartPasswordSheet()
        {
            PassInfo.StringValue = "Enter password for file:";
            PassPassword.BecomeFirstResponder();
            NSApplication.SharedApplication.BeginSheet(PasswordPanel, this.WindowForSheet, () => 
            {
                PasswordPanel.OrderOut(PasswordPanel);
            });
        }

        public override void WindowControllerDidLoadNib(NSWindowController windowController)
        {
            base.WindowControllerDidLoadNib(windowController);
            // Add code to here after the controller has loaded the document window

            if (HasPass)
            {
                StartPasswordSheet();
            }
            else
            {
                SetupDoc();
            }

        }

 
        public override bool ReadFromUrl(NSUrl url, string typeName, out NSError outError)
        {
            try
            {
                DocName = url.Path;
                CreateApiMate();
                LoadFile(DocName);
            }
            catch(Exception ex)
            {
                outError = NSError.FromDomain((NSString)ex.Message, -1);
                return false;
            }
            outError = null;
            return true;
        }

        // If this returns the name of a NIB file instead of null, a NSDocumentController 
        // is automatically created for you.
        public override string WindowNibName
        { 
            get
            {
                return "ApiMateDoc";
            }
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
            LangButton.SelectSegment(NSUserDefaults.StandardUserDefaults.IntForKey(AppConstants.langSelectedKey));
            FmlaButton.SelectSegment(NSUserDefaults.StandardUserDefaults.IntForKey(AppConstants.fmlaSelectedKey));
        }

        void CreateApiMate()
        {
            if (Api != null) return;
            Api = new APIMate.ApiMate(null);
        }
    
		[DllImport(Constants.ObjectiveCLibrary, EntryPoint="objc_msgSend")]
		static extern void IntPtr_objc_msgSend(IntPtr deviceHandle, IntPtr setterHandle);

        private void RefreshData()
        {
            try
            {
                Api.ShowComments = true;
                Api.SaveXls = true;
                //Api.SaveCsv = cbSaveCsv.Checked;
                //Api.SavePxl = cbSavePxl.Checked;
                //Api.SavePdf = cbSavePdf.Checked;
                //Api.SaveHtml = cbSaveHtml.Checked;
                //Api.SaveBmp = cbSaveBmp.Checked;
                //Api.AspNetCode = cbSaveAspNet.Checked;
                Api.R1C1Mode = FmlaButton.SelectedSegment == 1;

                if (Api.AspNetCode)
                {
                    int Checked = 0;
                    if (Api.SaveXls)
                        Checked++;
                    if (Api.SaveCsv)
                        Checked++;
                    if (Api.SavePxl)
                        Checked++;
                    if (Api.SavePdf)
                        Checked++;
                    if (Api.SaveHtml)
                        Checked++;
                    if (Api.SaveBmp)
                        Checked++;

                    if (Checked > 1)
                    {
                        edData.Value = "It is not possible to send a file to a browser in more than one file format.\r\nPlease uncheck \"Generate ASP.NET code\" or select only one format for saving.";
                        return;
                    }
                }

                var desc = new ObjCRuntime.Selector("noteClientStringWillChange");
                IntPtr_objc_msgSend(TextFinder.Handle, desc.Handle);

				edData.Value = Api.Process((Language)(int)LangButton.SelectedSegment, "xls", true);
            }
            catch (Exception ex)
            {
                AppDelegate.LogError(ex);
            }
        }

        void LoadFile(String DocName)
        {
            HasPass = false;
            if (DocName == null)
                return;
            try
            {
                Api.Open(DocName, null);
            }
            catch (FlexCelXlsAdapterException ex)
            {
                if (ex.ErrorCode == XlsErr.ErrFileIsPasswordProtected || ex.ErrorCode == XlsErr.ErrInvalidPassword)
                {
                    HasPass = true;
                }
                else throw;
            }

        }


        void SetupDoc()
        {
            if (Api == null) return;
            SheetControl.SegmentCount = Api.SheetCount;
            for (int i = 0; i < SheetControl.SegmentCount; i++)
            {
                SheetControl.SetLabel(Api.SheetName(i + 1), i);
            }

            SheetControl.SelectedSegment = Api.SelectedSheet - 1;
            //btnRefresh.Enabled = true;
            RefreshData();
        }

        partial void ChangeSheet(NSObject sender)
        {
            if (!Api.IsOpen) return;
            try
            {
				Api.TrySelectSheet(Api.SheetName((int)SheetControl.SelectedSegment + 1));
            Api.ChangeSheet();
            RefreshData();
            }

            catch (Exception ex)
            {
                AppDelegate.LogError(ex);
            }
        }

        partial void Refresh(NSObject sender)
        {
            try
            {
                Api.Refresh();
                RefreshData(); 
            }
            catch (FlexCelXlsAdapterException ex)
            {
                if (ex.ErrorCode == XlsErr.ErrFileIsPasswordProtected || ex.ErrorCode == XlsErr.ErrInvalidPassword)
                {
                    HasPass = true;
                    StartPasswordSheet();
                }
                else AppDelegate.LogError(ex);
            }

            catch (Exception ex)
            {
                AppDelegate.LogError(ex);
            }
        }


        partial void LangChanged(NSObject sender)
        {
            NSUserDefaults.StandardUserDefaults.SetInt(LangButton.SelectedSegment, AppConstants.langSelectedKey);
            if (!Api.IsOpen) return;
            RefreshData();
        }

        partial void SetLangCS(NSObject sender)
        {
            LangButton.SelectedSegment = 0;
            LangChanged(sender);
        }

        partial void SetLangVB(NSObject sender)
        {
            LangButton.SelectedSegment = 1;
            LangChanged(sender);
        }

        partial void FmlaChanged(NSObject sender)
        {
            NSUserDefaults.StandardUserDefaults.SetInt(FmlaButton.SelectedSegment, AppConstants.fmlaSelectedKey);
            if (!Api.IsOpen) return;
            RefreshData();
        }

        partial void PassSheetCancel(NSObject sender)
        {
            edData.Value = "Invalid password for file. Refresh this document to try again.";
            NSApplication.SharedApplication.EndSheet(PasswordPanel, -1);
        }

        partial void PassSheetOk(NSObject sender)
        {
            try
            {
                Api.Open(DocName, PassPassword.StringValue);
                SetupDoc();
            }
            catch (Exception ex)
            {
                AppDelegate.LogError(ex);
                return;
            }

            NSApplication.SharedApplication.EndSheet(PasswordPanel, 0);
        } 

 

    }
}

