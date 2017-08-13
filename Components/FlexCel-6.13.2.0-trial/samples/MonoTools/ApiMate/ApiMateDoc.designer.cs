// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace ApiMate
{
	[Register ("ApiMateDoc")]
	partial class ApiMateDoc
	{
		[Outlet]
		AppKit.NSTextView edData { get; set; }

		[Outlet]
		AppKit.NSSegmentedControl FmlaButton { get; set; }

		[Outlet]
		AppKit.NSSegmentedControl LangButton { get; set; }

		[Outlet]
		AppKit.NSTextField PassInfo { get; set; }

		[Outlet]
		AppKit.NSSecureTextField PassPassword { get; set; }

		[Outlet]
		AppKit.NSPanel PasswordPanel { get; set; }

		[Outlet]
		AppKit.NSSegmentedControl SheetControl { get; set; }

		[Outlet]
		Foundation.NSObject TextFinder { get; set; }

		[Action ("ChangeSheet:")]
		partial void ChangeSheet (Foundation.NSObject sender);

		[Action ("FmlaChanged:")]
		partial void FmlaChanged (Foundation.NSObject sender);

		[Action ("LangChanged:")]
		partial void LangChanged (Foundation.NSObject sender);

		[Action ("PassSheetCancel:")]
		partial void PassSheetCancel (Foundation.NSObject sender);

		[Action ("PassSheetOk:")]
		partial void PassSheetOk (Foundation.NSObject sender);

		[Action ("Refresh:")]
		partial void Refresh (Foundation.NSObject sender);

		[Action ("SetLangCS:")]
		partial void SetLangCS (Foundation.NSObject sender);

		[Action ("SetLangVB:")]
		partial void SetLangVB (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (edData != null) {
				edData.Dispose ();
				edData = null;
			}

			if (FmlaButton != null) {
				FmlaButton.Dispose ();
				FmlaButton = null;
			}

			if (LangButton != null) {
				LangButton.Dispose ();
				LangButton = null;
			}

			if (PassInfo != null) {
				PassInfo.Dispose ();
				PassInfo = null;
			}

			if (TextFinder != null) {
				TextFinder.Dispose ();
				TextFinder = null;
			}

			if (PassPassword != null) {
				PassPassword.Dispose ();
				PassPassword = null;
			}

			if (PasswordPanel != null) {
				PasswordPanel.Dispose ();
				PasswordPanel = null;
			}

			if (SheetControl != null) {
				SheetControl.Dispose ();
				SheetControl = null;
			}
		}
	}
}
