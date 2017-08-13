using System;
using System.Text;
using FlexCel.Core;
using FlexCel.XlsAdapter;
using System.Globalization;
using System.IO;

using System.Collections.Generic;

namespace APIMate
{
    public delegate TResult FuncNet20<T1, T2, TResult>(T1 arg1, T2 arg2);  //Remove when FRAMEWORK20 is over.
#if(!FRAMEWORK30)
    internal class HashSet<T> : Dictionary<T, int>
    {
        internal HashSet():base()
        {
        }

        internal HashSet(IEnumerable<T> items)
            : base()
        {
            foreach (T item in items)
            {
                Add(item, 0);
            }
        }

        public bool Contains(T NextSid)
        {
            return base.ContainsKey(NextSid);
        }

        public void Add(T NextSid)
        {
            base.Add(NextSid, 0);
        }
    }
#endif

    public enum Language
    {
        CSharp = 0,
        VBNet = 1,
        Oxygene = 2,
        DelphiNet = 3,
        Diff = 4

    }

    /// <summary>
    /// Class that will do the conversion.
    /// </summary>
    public class ApiMate
    {
        #region Global Variables
        XlsFile Xls1;
        XlsFile Xls2;
        bool CurrentIsFirst;
        bool IsNewFile;
        string CurrentFileName;
        string CurrentPassword;

        string CurrentFileActiveSheetName;

        private bool FShowComments;
        string SectionComment;

        private bool FSaveXls;
        private bool FSaveCsv;
        private bool FSavePxl;

        private bool FSavePdf;
        private bool FSaveHtml;
        private bool FSaveBmp;
        private bool FAspNetCode;

        #endregion

        #region Constants
        private const string DataBarVariable = "DataBarDef";
        private const string ColorScaleVariable = "ColorScaleDef";
        private const string IconSetVariable = "IconSetDef";
        private const string StandardCFVariable = "CFDef";
        #endregion

        #region Interface

        public ApiMate(FuncNet20<OnPasswordEventArgs, string, string>  aPasswordHandler)
        {
            CurrentIsFirst = true;
            Xls1 = new XlsFile();
            if (aPasswordHandler != null) Xls1.Protection.OnPassword += delegate(OnPasswordEventArgs e) {CurrentPassword = aPasswordHandler(e, CurrentFileName);};
            Xls1.NewFile(1);
            Xls2 = new XlsFile();
            if (aPasswordHandler != null) Xls2.Protection.OnPassword += delegate(OnPasswordEventArgs e) {CurrentPassword = aPasswordHandler(e, CurrentFileName);};
            Xls2.NewFile(1);

            CurrentFileName = null;
            CurrentPassword = null;
            IsNewFile = true;
            ShowComments = true;
        }

        #region Properties

        public bool ShowComments { get { return FShowComments; } set { FShowComments = value; } }
        public bool R1C1Mode
        {
            get { return Xls1.FormulaReferenceStyle == TReferenceStyle.R1C1; }
            set
            {
                TReferenceStyle st = value? TReferenceStyle.R1C1: TReferenceStyle.A1;
                Xls1.FormulaReferenceStyle = st;
                Xls2.FormulaReferenceStyle = st;
            }
        }

        public bool SaveXls { get { return FSaveXls; } set { FSaveXls = value; } }
        public bool SaveCsv { get { return FSaveCsv; } set { FSaveCsv = value; } }
        public bool SavePxl { get { return FSavePxl; } set { FSavePxl = value; } }
        public bool SavePdf { get { return FSavePdf; } set { FSavePdf = value; } }
        public bool SaveHtml { get { return FSaveHtml; } set { FSaveHtml = value; } }
        public bool SaveBmp { get { return FSaveBmp; } set { FSaveBmp = value; } }
        public bool AspNetCode { get { return FAspNetCode; } set { FAspNetCode = value; } }

        #endregion

        private XlsFile CurrentFile
        {
            get
            {
                if (CurrentIsFirst) return Xls1; else return Xls2;
            }
        }

        private XlsFile LastFile
        {
            get
            {
                if (CurrentIsFirst) return Xls2; else return Xls1;
            }
        }

        public bool IsOpen
        {
            get { return CurrentFileName != null;}
        }

        string DisplayFileName
        {
            get
            {
                return CurrentFileName;
            }
        }

        #region Process
        public void Open(string FileName)
        {
            Open(FileName, null);
        }

        public void Open(string FileName, string Password)
        {
            CurrentIsFirst = true;

            IsNewFile = true;
            CurrentFileName = FileName;
            CurrentPassword = Password;
            CurrentFile.Protection.OpenPassword = CurrentPassword;
            CurrentFile.Open(FileName);

            LastFile.NewFile(1, CurrentFile.ExcelFileFormat);
            LastFile.Protection.OpenPassword = CurrentPassword;

        }

        public int SheetCount
        {
            get
            {
                if (CurrentFile == null) return 0;
                return CurrentFile.SheetCount;
            }
        }

        public int SelectedSheet
        {
            get
            {
                if (CurrentFile == null) return 0;
                return CurrentFile.ActiveSheet;
            }
        }

        public string SheetName(int i)
        {
            if (CurrentFile == null) return String.Empty;
            if (i < 0 || i > CurrentFile.SheetCount) return string.Empty;
            return CurrentFile.GetSheetName(i);
        }

        public void TrySelectSheet(string SheetName)
        {
            CurrentFileActiveSheetName = SheetName;
        }


        public void ChangeSheet()
        {
            if (!IsOpen) throw new Exception("Please open a file first");
            if (CurrentFileActiveSheetName != null)
            {
                int ci = CurrentFile.GetSheetIndex(CurrentFileActiveSheetName, false);
                if (ci > 0) CurrentFile.ActiveSheet = ci;
                int li = LastFile.GetSheetIndex(CurrentFileActiveSheetName, false);
                if (li > 0) LastFile.ActiveSheet = li;
            }
        }


        public void Refresh()
        {
            if (!IsOpen) throw new Exception("Please open a file first");
            CurrentIsFirst = !CurrentIsFirst;

            CurrentFile.Protection.OpenPassword = LastFile.Protection.OpenPassword;

            CurrentFile.Open(CurrentFileName);
            ChangeSheet();

            IsNewFile = false;
        }

        public string Process(Language SelectedLang, string XlsFileName, bool ShowUsing)
        {
            return Process(SelectedLang, XlsFileName, ShowUsing, false);
        }

        public string Process(Language SelectedLang, string XlsFileName, bool ShowUsing, bool AllSheets)
        {

            LanguageEngine Engine = new CSharpEngine(XlsFileName, CurrentFile);
            switch (SelectedLang)
            {
                case Language.VBNet: Engine = new VBEngine(XlsFileName, CurrentFile); break;
                case Language.Oxygene: Engine = new DelphiEngine(XlsFileName, CurrentFile, true); break;
                case Language.DelphiNet: Engine = new DelphiEngine(XlsFileName, CurrentFile, false); break;
                case Language.Diff: Engine = new DiffEngine(CurrentFile); break;
            }
            return Process(Engine, ShowUsing, AllSheets);
        }

        private string Process(LanguageEngine Engine, bool ShowUsing, bool AllSheets)
        {
            Engine.ShowComments = ShowComments;

            using (MemoryStream OldFile = new MemoryStream())
            {
                LastFile.Save(OldFile); //The engine will modify things in the LastFile, like the default styles. We need to restore the file as it was.
                try
                {
                    Engine.Start(ShowUsing, SaveXls, SaveCsv, SavePxl, SavePdf, SaveHtml, SaveBmp, AspNetCode);
                    
                    if (R1C1Mode)
                    {
                        Engine.SetProp("FormulaReferenceStyle", Engine.GetEnum(TReferenceStyle.R1C1));
                        Engine.WriteLine();
                    }

                    if (AllSheets)
                    {
                        TrySelectSheet(CurrentFile.GetSheetName(1));
                        ChangeSheet();
                    }

                    DoSheets(Engine);
                    DoWorkbookOptions(Engine);
                    DoGlobalSheetOptions(Engine);
                    DoSheetOptions(Engine);
                    DoStyles(Engine);
                    DoNormalCellFormat(Engine);
                    DoNamedRanges(Engine);
                    DoPrinterSettings(Engine);
                    DoTheme(Engine);

                    if (AllSheets)
                    {
                        for (int i = 1; i <= CurrentFile.SheetCount; i++)
                        {
                            TrySelectSheet(CurrentFile.GetSheetName(i));
                            ChangeSheet();
                            if (i > 1)
                            {
                                Engine.WriteLine();
                                Engine.WriteLine();
                                ActivateSheet(Engine);
                                DoSheetOptions(Engine);
                            }
                            DoOneSheet(Engine);
                        }

                    }
                    else
                    {
                        DoOneSheet(Engine);
                    }

                    DoDocumentProperties(Engine);
                    DoCustomXML(Engine);

                    Engine.Finish();
                }
                finally
                {
                    OldFile.Position = 0;
                    LastFile.Open(OldFile); //restore all changes that were made.
                }
            }

            return Engine.ToString();
        }

        private void DoDocumentProperties(LanguageEngine Engine)
        {
            DoStandardDocumentProperties(Engine);
            DoCustomDocumentProperties(Engine);
        }

        private void DoStandardDocumentProperties(LanguageEngine Engine)
        {
            SectionComment = "Standard Document Properties - Most are only for xlsx files. In xls files FlexCel will only change the Creation Date and Modified Date.";
            bool FirstLine = true;

            TPropertyId[] CurrentIds = CurrentFile.DocumentProperties.GetUsedStandardProperties(true);
            TPropertyId[] LastIds = LastFile.DocumentProperties.GetUsedStandardProperties(true);
            foreach(TPropertyId currentId in CurrentIds)
            {
                object value = CurrentFile.DocumentProperties.GetStandardProperty(currentId);
                if (!Object.Equals(value, LastFile.DocumentProperties.GetStandardProperty(currentId)))
                {
                    WriteComment(Engine, ref FirstLine, false);
                    StartDocumentProperty(Engine, currentId);                    
                    Engine.CallMethod("DocumentProperties.SetStandardProperty", Engine.GetEnum(currentId), Engine.GetString(ConvertToStringOrIso8601Date(value)));
                    EndDocumentProperty(Engine, currentId);
                }
            }

            HashSet<TPropertyId> CurrentIdFinder = new HashSet<TPropertyId>(CurrentIds);
            foreach(TPropertyId lastId in LastIds)
            {
                if (!CurrentIdFinder.Contains(lastId))
                {
                    WriteComment(Engine, ref FirstLine, false);
                    StartDocumentProperty(Engine, lastId);

                    Engine.CallMethod("DocumentProperties.SetStandardProperty", Engine.GetEnum(lastId), Engine.GetString(null));

                    EndDocumentProperty(Engine, lastId);
                }
            }

        }

        private string ConvertToStringOrIso8601Date(object value)
        {
            if (value is DateTime) return FlxDateTime.ToISO8601((DateTime)value);
            return Convert.ToString(value);
        }

        private static void StartDocumentProperty(LanguageEngine Engine, TPropertyId Id)
        {
            if (Id == TPropertyId.LastSavedBy)
            {
                Engine.WriteLine();
                Engine.AddComment("You will normally not set LastSavedBy, since this is a new file.", false);
                Engine.AddComment("If you don't set it, FlexCel will use the creator instead.", false);
                Engine.CommentMode(true);
            }

            if (Id == TPropertyId.CreateTimeDate)
            {
                Engine.WriteLine();
                Engine.AddComment("You will normally not set CreateDateTime, since this is a new file and FlexCel will automatically use the current datetime.", false);
                Engine.AddComment("But if you are editing a file and want to preserve the original creation date, you need to either set PreserveCreationDate to true:", false);
                Engine.CommentMode(true);
                Engine.SetProp("DocumentProperties.PreserveCreationDate", Engine.GetBool(true));
                Engine.AddComment("Or you can hardcode a creating date by setting it in UTC time, ISO8601 format:", false);

            }
        }

        private static void EndDocumentProperty(LanguageEngine Engine, TPropertyId Id)
        {
            if (Id == TPropertyId.LastSavedBy || Id == TPropertyId.CreateTimeDate)
            {
                Engine.CommentMode(false);
                Engine.WriteLine();
            }
        }

        private void DoCustomDocumentProperties(LanguageEngine Engine)
        {
            SectionComment = "Custom Document Properties - Only for xlsx files";
            bool FirstLine = true;

            string[] CurrentIds = CurrentFile.DocumentProperties.GetAllCustomProperties();
            foreach (string currentId in CurrentIds)
            {
                TDocumentCustomProperty value = CurrentFile.DocumentProperties.GetCustomProperty(currentId);
                if (!Object.Equals(value, LastFile.DocumentProperties.GetCustomProperty(currentId)))
                {
                    WriteComment(Engine, ref FirstLine, false);

                    Engine.CallMethod("DocumentProperties.SetCustomProperty", 
                        Engine.GetString(value.Name), 
                        Engine.GetEnum(value.PropType),
                        Engine.GetString(value.Value),
                        Convert.ToString(value.PId));
                }
            }
        }

        private void DoCustomXML(LanguageEngine Engine)
        {
            SectionComment = "Custom XML parts - Only for xlsx files";
            bool FirstLine = true;

            bool NeedsParts = false;
            if (CurrentFile.CustomXmlPartCount != LastFile.CustomXmlPartCount) NeedsParts = true;
            else
            {
                for (int i = 0; i < CurrentFile.CustomXmlPartCount; i++)
                {
                    if (!Object.Equals(CurrentFile.GetCustomXmlPart(i + 1), LastFile.GetCustomXmlPart(i + 1))) NeedsParts = true;
                }
            }

            if (!NeedsParts) return;

            for (int i = 0; i < CurrentFile.CustomXmlPartCount; i++)
            {
                TCustomXmlPart part = CurrentFile.GetCustomXmlPart(i + 1);
                WriteComment(Engine, ref FirstLine, false);
                Engine.InsertVar("xmldata", "string", Engine.GetString(part.Xml));
                if (part.Schemas == null || part.Schemas.Length == 0)
                {
                    Engine.InsertVar("xmlschemas", Engine.GetArrayDecl("string"), Engine.GetNullStr());
                }
                else
                {
                    Engine.InsertVar("xmlschemas", Engine.GetArrayDecl("string"));
                    Engine.WriteLn(Engine.CreateObjectArray("xmlschemas", "string", part.Schemas.Length));
                    for (int k = 0; k < part.Schemas.Length; k++)
                    {
                        string s = part.Schemas[k];
                        Engine.SetVarProp(Engine.GetCallArray("xmlschemas", k), Engine.GetString(s));
                    }
                }

                Engine.InsertVar("part", "TCustomXmlPart", Engine.CreateObject("TCustomXmlPart", Engine.GetParams("xmldata", "xmlschemas")));
                Engine.CallMethod("AddCustomXmlPart", "part");
            }
        }


        private void DoOneSheet(LanguageEngine Engine)
        {
            DoSheetBackground(Engine);
            DoRowsAndColumns(Engine);
            DoMergedCells(Engine);
            DoTables(Engine);
            DoCells(Engine);
            DoWhatIfTables(Engine);
            DoHyperLinks(Engine);
            DoPageBreaks(Engine);
            DoConditionalFormats(Engine);
            DoDataValidation(Engine);

            DoComments(Engine);
            DoImages(Engine);
            DoObjects(Engine);

            DoGroupAndOutline(Engine);
            DoAutoFilter(Engine);
            DoFreezePanes(Engine);
            DoCellSelection(Engine);

            DoProtection(Engine);
            DoFirstSheetVisible(Engine);
        }

        #endregion

        #endregion

        #region Utilites

        private void WriteComment(LanguageEngine Engine, ref bool FirstLine, bool AddLine)
        {
            if (AddLine || FirstLine)
                Engine.WriteLine();

            if (!FirstLine) return;
            FirstLine = false;

            Engine.AddComment(SectionComment, false);
        }

        private bool ArrayEquals(byte[] a1, byte[] a2)
        {
            if (a1 == null)
            {
                if (a2 == null) return true;
                return false;
            }

            if (a2 == null) return false;
            if (a1.Length != a2.Length) return false;
            for (int i = 0; i < a1.Length; i++)
            {
                if (a1[i] != a2[i]) return false;
            }

            return true;
        }


        private void DoProp(LanguageEngine Engine, string PropName, bool Current, bool Last, ref bool FirstLine)
        {
            if (Last != Current)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp(PropName, Engine.GetBool(Current));
            }
        }

        private void DoProp(LanguageEngine Engine, string PropName, int Current, int Last, ref bool FirstLine)
        {
            if (Last != Current)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp(PropName, Engine.GetInt(Current));
            }
        }

        private void DoProp(LanguageEngine Engine, string PropName, int? Current, int? Last, ref bool FirstLine)
        {
            if (Last != Current)
            {
                WriteComment(Engine, ref FirstLine, false);
                if (Current.HasValue)
                {
                    Engine.SetProp(PropName, Engine.GetInt(Current.Value));
                }
                else
                {
                    Engine.SetProp(PropName, null);
                }
            }
        }

        private void DoProp(LanguageEngine Engine, string PropName, Enum Current, Enum Last, ref bool FirstLine)
        {
            if (Convert.ToInt32(Last) != Convert.ToInt32(Current))
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp(PropName, Engine.GetEnum(Current));
            }
        }

        #endregion

        #region DoSheets
        private void DoSheets(LanguageEngine Engine)
        {
            if (IsNewFile)
            {
                string SheetStr = CurrentFile.SheetCount > 1 ? " sheets." : " sheet.";
                Engine.AddComment("Create a new Excel file with " + CurrentFile.SheetCount.ToString() + SheetStr, true);
                Engine.AddSheets(CurrentFile.SheetCount, CurrentFile.ExcelFileFormat);

                Engine.WriteLine();
                Engine.AddComment("Set the names of the sheets", false);
                Engine.NameSheets();
                Engine.WriteLine();

                if (CurrentPassword != null && CurrentPassword.Length > 0)
                {
                    Engine.AddComment("Set a password to protect the file.", true);
                    Engine.SetProp(Engine.Call("Protection", "OpenPassword"), Engine.GetString(CurrentPassword));
                }

                ActivateSheet(Engine);
                return;
            }

            if (CurrentPassword != null && CurrentPassword.Length > 0)
            {
                Engine.AddComment("Password needed to open the file.", true);
                Engine.SetProp(Engine.Call("Protection", "OpenPassword"), Engine.GetString(CurrentPassword));
            }

            Engine.AddComment("Open an existing xls file for modification.", true);
            Engine.CallMethod("Open", Engine.GetString(DisplayFileName));
            Engine.AddComment("Set the sheet we are working in.", true);
            Engine.SetProp("ActiveSheet", CurrentFile.ActiveSheet);

        }

        private void ActivateSheet(LanguageEngine Engine)
        {
            Engine.AddComment("Set the sheet we are working in.", true);
            Engine.SetProp("ActiveSheet", CurrentFile.ActiveSheet);
        }
        #endregion

        #region Theme
        private void DoTheme(LanguageEngine Engine)
        {
            SectionComment = "Theme - You might use GetTheme/SetTheme methods here instead.";
            DoThemeColors(Engine);
            DoThemeFonts(Engine);
        }

        private void DoThemeColors(LanguageEngine Engine)
        {
            bool FirstLine = true;
            bool DefinedVar = false;

            for (int i = 0; i <= (int)TThemeColor.FollowedHyperLink; i++)
            {
                TThemeColor ThemeColor = (TThemeColor)i;
                TDrawingColor LastColor = LastFile.GetColorTheme(ThemeColor);
                TDrawingColor CurrColor = CurrentFile.GetColorTheme(ThemeColor);
                if (LastColor != CurrColor)
                {
                    WriteComment(Engine, ref FirstLine, false);

                    string ThemeColorDef;
                    TColorTransform[] ColorTransforms = CurrColor.GetColorTransform();

                    if (ColorTransforms == null || ColorTransforms.Length == 0)
                    {
                        ThemeColorDef = Engine.GetDrawingColor(CurrColor);
                    }
                    else
                    {
                        Engine.WriteLine();
                        ThemeColorDef = "ThemeDef";
                        if (!DefinedVar)
                        {
                            Engine.InsertVar("ThemeDef", "TDrawingColor", Engine.GetDrawingColor(CurrColor));
                            Engine.InsertVar("ColorTransforms", Engine.GetArrayDecl("TColorTransform"));
                            DefinedVar = true;
                        }
                        else
                        {
                            Engine.SetVarProp("ThemeDef", Engine.GetDrawingColor(CurrColor));
                        }

                        Engine.WriteLn(Engine.CreateObjectArray("ColorTransforms", "TColorTransform", ColorTransforms.Length));
                        for (int k = 0; k < ColorTransforms.Length; k++)
                        {
                            TColorTransform ct = ColorTransforms[k];
                            Engine.SetVarProp(Engine.GetCallArray("ColorTransforms", k), Engine.CreateObject("TColorTransform", Engine.GetParams(Engine.GetEnum(ct.ColorTransformType), ct.Value)));
                        }

                        Engine.SetVarProp("ThemeDef", Engine.GetCallVarMethod(String.Empty, Engine.Call("TDrawingColor", "AddTransform"), "ThemeDef", "ColorTransforms"));
                    }

                    Engine.CallMethod("SetColorTheme", Engine.GetEnum(ThemeColor), ThemeColorDef);
                }
            }
        }

        private void DoThemeFonts(LanguageEngine Engine)
        {
            DoThemeFont(Engine, TFontScheme.Major, "Major");
            DoThemeFont(Engine, TFontScheme.Minor, "Minor");
        }

        private void DoThemeFont(LanguageEngine Engine, TFontScheme scheme, string MinorMajor)
        {
            TThemeFont LastFont = LastFile.GetThemeFont(scheme);
            TThemeFont CurrFont = CurrentFile.GetThemeFont(scheme);
            string MMFont = MinorMajor + "Font";

            if (!Object.Equals(LastFont, CurrFont))
            {
                Engine.WriteLine();
                Engine.AddComment(MinorMajor + " font", true);
                Engine.WriteLine();
                DefineFontScript(Engine, MinorMajor, "Latin", CurrFont.Latin);
                DefineFontScript(Engine, MinorMajor, "EastAsian", CurrFont.EastAsian);
                DefineFontScript(Engine, MinorMajor, "ComplexScript", CurrFont.ComplexScript);

                Engine.InsertVar(MMFont, "TThemeFont", Engine.CreateObject("TThemeFont", Engine.GetParams(MinorMajor + "Latin", MinorMajor + "EastAsian", MinorMajor + "ComplexScript")));

                string[] FontScripts = CurrFont.GetFontScripts();
                foreach (string s in FontScripts)
                {
                    Engine.CallVarMethod(MMFont, "AddFont", Engine.GetString(s), Engine.GetString(CurrFont.GetFont(s)));
                }
                Engine.CallMethod("SetThemeFont", Engine.GetEnum(scheme), MMFont);
            }
        }

        private static void DefineFontScript(LanguageEngine Engine, string MinorMajor, string Name, TThemeTextFont CurrTextFont)
        {
            Engine.InsertVar(MinorMajor + Name, "TThemeTextFont", Engine.CreateObject("TThemeTextFont", Engine.GetParams(Engine.GetString(CurrTextFont.Typeface), Engine.GetString(CurrTextFont.Panose), Engine.GetEnum(CurrTextFont.Pitch), Engine.GetEnum(CurrTextFont.CharSet))));
        }

        #endregion

        #region Sheet background
        private void DoSheetBackground(LanguageEngine Engine)
        {
            bool FirstLine = true;
            SectionComment = "Set sheet background";

            TXlsImgType CurrentImgType;
            byte[] CurrentBk = CurrentFile.GetSheetBackground(out CurrentImgType);
            TXlsImgType LastImgType;
            byte[] LastBk = LastFile.GetSheetBackground(out LastImgType);

            if (CurrentImgType != LastImgType || !TExcelTypes.SameByteArray(CurrentBk, LastBk))
            {
                WriteComment(Engine, ref FirstLine, false);
                if (CurrentBk == null || CurrentBk.Length == 0)
                {
                    Engine.CallMethod("SetSheetBackground", Engine.GetNullStr());
                }
                else
                {
                    string FileName = "BkgImg" + Engine.GetExt(CurrentImgType);
                    Engine.CallMethod("SetSheetBackground", Engine.GetCallVarMethod("", Engine.Call("File", "ReadAllBytes"), Engine.GetString(FileName)));
                }
            }
        }
        #endregion

        #region Rows and Columns and Cells
        private void DoRowsAndColumns(LanguageEngine Engine)
        {
            SectionComment = "Set up rows and columns";

            bool FirstLine = true;
            bool FirstRowFmt = true;
            bool FirstColFmt = true;

            if (CurrentFile.DefaultColWidth != LastFile.DefaultColWidth)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp("DefaultColWidth", Engine.GetInt(CurrentFile.DefaultColWidth));
                LastFile.DefaultColWidth = CurrentFile.DefaultColWidth; //so we take it in account when comparing the columns.
            }

            TColumnBlock[] ColBlocks = CurrentFile.GetColumnBlocks(CurrentFile.ActiveSheet, 1, FlxConsts.Max_Columns + 1);
            foreach (TColumnBlock ColBlock in ColBlocks)
            {
                TColumnBlock[] LastColBlocks = LastFile.GetColumnBlocks(LastFile.ActiveSheet, ColBlock.FirstCol, ColBlock.FirstCol);

                bool NeedsToUpdate = LastColBlocks.Length != 1 || LastColBlocks[0].FirstCol != ColBlock.FirstCol || LastColBlocks[0].LastCol != ColBlock.LastCol;

                if (NeedsToUpdate || ColBlock.Width != LastColBlocks[0].Width)
                {
                    int cw = ColBlock.Width;
                    WriteComment(Engine, ref FirstLine, true);
                    if (cw > 0) Engine.AddComment(String.Format(CultureInfo.InvariantCulture, "({0:0.00} + 0.75) * 256", cw / 256.0 - 0.75), true);
                    Engine.CallMethod("SetColWidth", ColBlock.FirstCol, ColBlock.LastCol, cw);
                }

                if (NeedsToUpdate || ColBlock.Hidden != LastColBlocks[0].Hidden)
                {
                    if (LastColBlocks.Length != 0 || ColBlock.Hidden)
                    {
                        WriteComment(Engine, ref FirstLine, false);
                        Engine.CallMethod("SetColHidden", ColBlock.FirstCol, ColBlock.LastCol, Engine.GetBool(ColBlock.Hidden));
                    }
                }

                if (!NeedsToUpdate && ColBlock.XF == LastColBlocks[0].XF) continue; //common case.
                
                int CurrCol = ColBlock.FirstCol;
                int FirstCol = CurrCol;
                TFlxFormat CurrentFmt = LastFile.GetFormat(LastFile.GetColFormat(CurrCol));

                TFlxFormat fmt2 = CurrentFile.GetFormat(ColBlock.XF);

                while (CurrCol <= ColBlock.LastCol + 1)
                {
                    TFlxFormat fmt1 = CurrCol <= ColBlock.LastCol ? LastFile.GetFormat(LastFile.GetColFormat(CurrCol)) : CurrentFmt;

                    bool SameToPrevious = DiffFormats(fmt1, CurrentFmt).IsEmpty;

                    if (SameToPrevious && CurrCol <= ColBlock.LastCol)
                    {
                        CurrCol++;
                        continue;
                    }

                    TFlxApplyFormat Diffs = DiffFormats(CurrentFmt, fmt2);
                    if (!Diffs.IsEmpty)
                    {
                        WriteComment(Engine, ref FirstLine, true);

                        if (FirstColFmt)
                        {
                            Engine.InsertVar("ColFmt", "TFlxFormat");
                            FirstColFmt = false;
                        }

                        Engine.SetRowColFormat("Col", "ColFmt", FirstCol, CurrCol - 1, fmt2, Diffs);
                    }

                    FirstCol = CurrCol;
                    CurrCol++;
                }

            }

            if (CurrentFile.DefaultRowHeight != LastFile.DefaultRowHeight)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp("DefaultRowHeight", Engine.GetInt(CurrentFile.DefaultRowHeight));
                LastFile.DefaultRowHeight = CurrentFile.DefaultRowHeight;
            }

            if (CurrentFile.DefaultRowHeightAutomatic != LastFile.DefaultRowHeightAutomatic)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp("DefaultRowHeightAutomatic", Engine.GetBool(CurrentFile.DefaultRowHeightAutomatic));
                LastFile.DefaultRowHeightAutomatic = CurrentFile.DefaultRowHeightAutomatic;
            }

            if (CurrentFile.DefaultRowHidden != LastFile.DefaultRowHidden)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp("DefaultRowHidden", Engine.GetBool(CurrentFile.DefaultRowHidden));
                LastFile.DefaultRowHidden = CurrentFile.DefaultRowHidden;
            }

            bool FirstRow = true;
            for (int r = 1; r <= CurrentFile.RowCount; r++)
            {
                bool RowHeightSet = false;

                int rh = CurrentFile.GetRowHeight(r);
                if (rh != LastFile.GetRowHeight(r))
                {
                    WriteComment(Engine, ref FirstLine, FirstRow);
                    FirstRow = false;
                    if (rh > 0) Engine.AddComment(String.Format(CultureInfo.InvariantCulture, "{0:0.00} * 20", rh / 20.0), true);
                    Engine.CallMethod("SetRowHeight", r, rh);
                    RowHeightSet = true;
                }

                if (CurrentFile.GetRowHidden(r) != LastFile.GetRowHidden(r))
                {
                    WriteComment(Engine, ref FirstLine, FirstRow);
                    FirstRow = false;
                    Engine.CallMethod("SetRowHidden", r, Engine.GetBool(CurrentFile.GetRowHidden(r)));
                }

                TFlxFormat fmt1 = LastFile.GetFormat(LastFile.GetRowFormat(r));
                TFlxFormat fmt2 = CurrentFile.GetFormat(CurrentFile.GetRowFormat(r));

                TFlxApplyFormat Diffs = DiffFormats(fmt1, fmt2);
                if (!Diffs.IsEmpty)
                {
                    WriteComment(Engine, ref FirstLine, true);

                    if (FirstRowFmt)
                    {
                        Engine.InsertVar("RowFmt", "TFlxFormat");
                        FirstRowFmt = false;
                    }

                    Engine.SetRowColFormat("Row", "RowFmt", r, -1, fmt2, Diffs);
                }

                if (CurrentFile.GetAutoRowHeight(r) != LastFile.GetAutoRowHeight(r) && !RowHeightSet)
                {
                    WriteComment(Engine, ref FirstLine, FirstRow);
                    FirstRow = false;
                    Engine.CallMethod("SetAutoRowHeight", r, Engine.GetBool(CurrentFile.GetAutoRowHeight(r)));
                }

            }
        }

        private void DoCells(LanguageEngine Engine)
        {
            SectionComment = "Set the cell values";
            bool FirstFmt = true;
            bool FirstLine = true;


            for (int r = 1; r <= CurrentFile.RowCount; r++)
            {
                for (int cindex = 1; cindex <= CurrentFile.ColCountInRow(r); cindex++)
                {
                    int c = CurrentFile.ColFromIndex(r, cindex);

                    int XF1 = -1;
                    object Cell1 = LastFile.GetCellValue(r, c, ref XF1);
                    TFlxFormat fmt1 = LastFile.GetFormat(XF1);

                    int XF2 = -1;
                    object Cell2 = CurrentFile.GetCellValueIndexed(r, cindex, ref XF2);
                    TFlxFormat fmt2 = CurrentFile.GetFormat(XF2);

                    bool FormatFromStyle = false;
                    if (fmt1.NotNullParentStyle != fmt2.NotNullParentStyle)
                    {
                        fmt1 = CurrentFile.GetStyle(fmt2.NotNullParentStyle, true);
                        FormatFromStyle = true;
                    }

                    TFlxApplyFormat Diffs = DiffFormats(fmt1, fmt2);
                    if (!Diffs.IsEmpty || FormatFromStyle) //FormatFromStyle is not really needed, as if it is true, fmt.ParentStyle will be different and Diff.IsEmpty will be false. But we write it to make the logic clear.
                    {
                        WriteComment(Engine, ref FirstLine, true);

                        if (FirstFmt)
                        {
                            Engine.InsertVar("fmt", "TFlxFormat");
                            FirstFmt = false;
                        }

                        Engine.SetCellFormat("fmt", r, c, fmt2, Diffs, FormatFromStyle);
                    }

                    string Comment;

                    string NewCellValue = DiffValues(Engine, Cell1, Cell2, fmt2, r, c, out Comment);
                    if (NewCellValue != null)
                    {
                        WriteComment(Engine, ref FirstLine, false);
                        Engine.SetCellValue(r, c, NewCellValue);
                        if (Comment != null)
                        {
                            Engine.AddComment(Comment, false);
                            Engine.WriteLine();
                        }
                    }

                }
            }

        }


        private TFlxApplyFormat DiffFormats(TFlxFormat fmt1, TFlxFormat fmt2)
        {
            TFlxApplyFormat Result = new TFlxApplyFormat();

            //Font
            if (fmt1.Font.Name != fmt2.Font.Name) Result.Font.Name = true;
            if (fmt1.Font.Size20 != fmt2.Font.Size20) Result.Font.Size20 = true;
            if (fmt1.Font.Color != fmt2.Font.Color) Result.Font.Color = true;
            if (fmt1.Font.Style != fmt2.Font.Style) Result.Font.Style = true;
            if (fmt1.Font.Underline != fmt2.Font.Underline) Result.Font.Underline = true;
            if (fmt1.Font.Family != fmt2.Font.Family) Result.Font.Family = true;
            if (fmt1.Font.CharSet != fmt2.Font.CharSet) Result.Font.CharSet = true;
            if (fmt1.Font.Scheme != fmt2.Font.Scheme) Result.Font.Scheme = true; 

            //Borders
            if (fmt1.Borders.Left != fmt2.Borders.Left) Result.Borders.Left = true;
            if (fmt1.Borders.Right != fmt2.Borders.Right) Result.Borders.Right = true;
            if (fmt1.Borders.Top != fmt2.Borders.Top) Result.Borders.Top = true;
            if (fmt1.Borders.Bottom != fmt2.Borders.Bottom) Result.Borders.Bottom = true;
            if (fmt1.Borders.Diagonal != fmt2.Borders.Diagonal) Result.Borders.Diagonal = true;
            if (fmt1.Borders.DiagonalStyle != fmt2.Borders.DiagonalStyle) Result.Borders.DiagonalStyle = true;

            //Pattern
            if (fmt1.FillPattern != fmt2.FillPattern) //if they are pattern.none they are the same, even if colors differ.
            {
                if (fmt1.FillPattern.Pattern != fmt2.FillPattern.Pattern) Result.FillPattern.Pattern = true;
                if (fmt1.FillPattern.FgColor != fmt2.FillPattern.FgColor) Result.FillPattern.FgColor = true;
                if (fmt1.FillPattern.BgColor != fmt2.FillPattern.BgColor) Result.FillPattern.BgColor = true;
                if (!TExcelGradient.Equals(fmt1.FillPattern.Gradient, fmt2.FillPattern.Gradient)) Result.FillPattern.Gradient = true;
            }

            //Parent Style
            if (fmt1.NotNullParentStyle != fmt2.NotNullParentStyle) Result.ParentStyle = true;

            //Others
            if (fmt1.Format != fmt2.Format) Result.Format = true;

            if (fmt1.HAlignment != fmt2.HAlignment) Result.HAlignment = true;
            if (fmt1.VAlignment != fmt2.VAlignment) Result.VAlignment = true;

            if (fmt1.Locked != fmt2.Locked) Result.Locked = true;
            if (fmt1.Hidden != fmt2.Hidden) Result.Hidden = true;
            if (fmt1.WrapText != fmt2.WrapText) Result.WrapText = true;
            if (fmt1.ShrinkToFit != fmt2.ShrinkToFit) Result.ShrinkToFit = true;
            if (fmt1.Rotation != fmt2.Rotation) Result.Rotation = true;
            if (fmt1.ReadingOrder != fmt2.ReadingOrder) Result.ReadingOrder = true;
            if (fmt1.Indent != fmt2.Indent) Result.Indent = true;
            if (fmt1.Lotus123Prefix != fmt2.Lotus123Prefix) Result.Lotus123Prefix = true;

            return Result;
        }


        private string DiffValues(LanguageEngine Engine, object Cell1, object Cell2, TFlxFormat fmt2, int r, int c, out string Comment)
        {
            Comment = null;
            if (Cell2 == null)
            {
                if (Cell1 == null) return null;
                return Engine.GetString(null);
            }

            if (Cell2.Equals(Cell1)) return null;

            switch (Type.GetTypeCode(Cell2.GetType()))
            {
                case TypeCode.Boolean:
                    return Engine.GetBool((bool)Cell2);
                case TypeCode.Double:  //Remember, dates are doubles with date format.
                    TUIColor CellColor = TUIColor.Empty;
                    bool HasDate, HasTime;
                    TFlxNumberFormat.FormatValue(Cell2, fmt2.Format, ref CellColor, CurrentFile, out HasDate, out HasTime);

                    if (HasDate || HasTime)
                    {
                        DateTime dt = FlxDateTime.FromOADate((double)Cell2, CurrentFile.OptionsDates1904);
                        return Engine.GetDateTime(dt, HasDate, HasTime);
                    }
                    else
                    {
                        return Engine.GetDouble((double)Cell2);
                    }
                case TypeCode.String:
                    return Engine.GetString(Cell2.ToString());
            }

            TFormula fmla2 = Cell2 as TFormula;
            if (fmla2 != null)
            {
                TFormula fmla1 = Cell1 as TFormula;

                if (fmla1 == null || fmla1.Text != fmla2.Text || fmla1.Span != fmla2.Span)
                {
                    if (!fmla2.Span.IsTopLeft) return null; //don't set array formulas if they are not at the top left.
                    return Engine.CreateFormula(fmla2.Text, fmla2.Span);
                }
                return null;
            }

            TRichString RSt2 = Cell2 as TRichString;
            if (RSt2 != null)
            {
                TRichString RSt1 = Cell1 as TRichString;
                if (RSt1 == RSt2) return null;
                return Engine.GetAndWriteRichString(RSt2, r, c, out Comment);
            }

            if (Cell2 is TFlxFormulaErrorValue)
            {
                if (Cell2 == Cell1) return null;
                return Engine.GetError((TFlxFormulaErrorValue)Cell2);
            }

            throw new Exception("Unexpected value on cell");


        }

        #endregion

        #region Merged Cells
        private void DoMergedCells(LanguageEngine Engine)
        {
            SectionComment = "Merged Cells";
            bool FirstLine = true;
            MergeOrUnMerge(Engine, "UnMergeCells", LastFile, CurrentFile, ref FirstLine);
            MergeOrUnMerge(Engine, "MergeCells", CurrentFile, LastFile, ref FirstLine);

        }

        private void MergeOrUnMerge(LanguageEngine Engine, string Operation, ExcelFile Master, ExcelFile Other, ref bool FirstLine)
        {
            TCellMergedState state = TCellMergedState.Init();
            while (true)
            {
                TXlsCellRange CurrentRange = Master.CellMergedNext(ref state);
                if (CurrentRange == null) break;
                TXlsCellRange LastRange = Other.CellMergedBounds(CurrentRange.Top, CurrentRange.Left);
                if (CurrentRange.Top != LastRange.Top || CurrentRange.Left != LastRange.Left || CurrentRange.Bottom != LastRange.Bottom || CurrentRange.Right != LastRange.Right)
                {
                    WriteComment(Engine, ref FirstLine, false);
                    Engine.CallMethod(Operation, CurrentRange.Top, CurrentRange.Left, CurrentRange.Bottom, CurrentRange.Right);
                }
            }

        }

        #endregion

        #region Tables
        private void DoTables(LanguageEngine Engine)
        {
            if (SameTables()) return;
            SectionComment = "Tables";
            bool FirstLine = true;
            if (LastFile.TableCountInSheet > 0)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.CallMethod("DeleteAllTablesInSheet");
            }
            for (int i = 1; i <= CurrentFile.TableCountInSheet; i++)
            {
                TTableDefinition Table = CurrentFile.GetTable(i);
                DoOneTable(Engine, Table, ref FirstLine);
            }

        }

        private bool SameTables()
        {
            if (LastFile.TableCountInSheet != CurrentFile.TableCountInSheet) return false;
            for (int i = 1; i <= LastFile.TableCountInSheet; i++)
            {
                if (!Object.Equals(LastFile.GetTable(i), CurrentFile.GetTable(i))) return false;
            }
            return true;

        }

        private void DoOneTable(LanguageEngine Engine, TTableDefinition Table, ref bool FirstLine)
        {
            WriteComment(Engine, ref FirstLine, false);
            for (int c = Table.Range.Left; c <= Table.Range.Right; c++)
            {
                string Comment;
                int XF2 = -1;
                object Cell2 = CurrentFile.GetCellValue(Table.Range.Top, c, ref XF2);
                string NewCellValue = DiffValues(Engine, LastFile.GetCellValue(Table.Range.Top, c), Cell2, CurrentFile.GetFormat(XF2), Table.Range.Top, c, out Comment);
                if (NewCellValue != null)
                {
                    WriteComment(Engine, ref FirstLine, false);
                    Engine.SetCellValue(Table.Range.Top, c, NewCellValue);
                }
            }
            const string TableVariable = "Table";
            Engine.InsertVarAndTrack(TableVariable, "TTableDefinition", Engine.CreateObject("TTableDefinition", ""));
            Engine.SetVarProp(Engine.Call(TableVariable, "Name"), Engine.GetString(Table.Name));
            Engine.SetVarProp(Engine.Call(TableVariable, "Range"), Engine.GetRange(Table.Range));
            Engine.SetVarProp(Engine.Call(TableVariable, "HasAutofilter"), Engine.GetBool(Table.HasAutofilter));
            Engine.SetVarProp(Engine.Call(TableVariable, "HasHeaderRow"), Engine.GetBool(Table.HasHeaderRow));
            Engine.SetVarProp(Engine.Call(TableVariable, "HasTotalsRow"), Engine.GetBool(Table.HasTotalsRow));
            Engine.SetVarProp(Engine.Call(TableVariable, "Comment"), Engine.GetString(Table.Comment));

            Engine.SetVarProp(Engine.Call(TableVariable, "Style"), Engine.CreateObject("TTableStyle", Engine.GetParams(
                Engine.GetString(Table.Style.Name),
                Engine.GetBool(Table.Style.ShowColumnStripes),
                Engine.GetBool(Table.Style.ShowFirstColumn),
                Engine.GetBool(Table.Style.ShowLastColumn),
                Engine.GetBool(Table.Style.ShowRowStripes))));

            Engine.WriteLine();
            Engine.CallMethod("AddTable", TableVariable);
            Engine.WriteLine();
        }

        #endregion


        #region Printer Settings
        private void DoPrinterSettings(LanguageEngine Engine)
        {
            SectionComment = "Printer Settings";
            bool FirstLine = true;
            THeaderAndFooter LastHF = LastFile.GetPageHeaderAndFooter();
            THeaderAndFooter CurrentHF = CurrentFile.GetPageHeaderAndFooter();
            if (LastHF != CurrentHF)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.InsertVar("HeadersAndFooters", "THeaderAndFooter", Engine.CreateObject("THeaderAndFooter", string.Empty));
                Engine.SetVarProp(Engine.Call("HeadersAndFooters", "AlignMargins"), Engine.GetBool(CurrentHF.AlignMargins));
                Engine.SetVarProp(Engine.Call("HeadersAndFooters", "ScaleWithDoc"), Engine.GetBool(CurrentHF.ScaleWithDoc));
                Engine.SetVarProp(Engine.Call("HeadersAndFooters", "DiffFirstPage"), Engine.GetBool(CurrentHF.DiffFirstPage));
                Engine.SetVarProp(Engine.Call("HeadersAndFooters", "DiffEvenPages"), Engine.GetBool(CurrentHF.DiffEvenPages));

                Engine.SetVarProp(Engine.Call("HeadersAndFooters", "DefaultHeader"), Engine.GetString(CurrentHF.DefaultHeader));
                Engine.SetVarProp(Engine.Call("HeadersAndFooters", "DefaultFooter"), Engine.GetString(CurrentHF.DefaultFooter));
                Engine.SetVarProp(Engine.Call("HeadersAndFooters", "FirstHeader"), Engine.GetString(CurrentHF.FirstHeader));
                Engine.SetVarProp(Engine.Call("HeadersAndFooters", "FirstFooter"), Engine.GetString(CurrentHF.FirstFooter));
                Engine.SetVarProp(Engine.Call("HeadersAndFooters", "EvenHeader"), Engine.GetString(CurrentHF.EvenHeader));
                Engine.SetVarProp(Engine.Call("HeadersAndFooters", "EvenFooter"), Engine.GetString(CurrentHF.EvenFooter));

                Engine.CallMethod("SetPageHeaderAndFooter", "HeadersAndFooters");
            }

            bool FirstImage = true;
            foreach (THeaderAndFooterKind hkind in Enum.GetValues(typeof(THeaderAndFooterKind)))
            {
                foreach (THeaderAndFooterPos section in Enum.GetValues(typeof(THeaderAndFooterPos)))
                {
                    TXlsImgType ImageType1 = TXlsImgType.Unknown;
                    TXlsImgType ImageType2 = TXlsImgType.Unknown;
                    byte[] Img2 = CurrentFile.GetHeaderOrFooterImage(hkind, section, ref ImageType2);

                    byte[] Img1 = LastFile.GetHeaderOrFooterImage(hkind, section, ref ImageType1);

                    if (!ArrayEquals(Img1, Img2))
                    {
                        if (FirstImage)
                        {
                            Engine.WriteLine();
                            Engine.AddComment("There are &G tags in headers or footers, so we will add the images now", false);
                        }

                        Engine.SetHeaderImg(hkind, section, Img2, ImageType2, CurrentFile.GetHeaderOrFooterImageProperties(hkind, section), FirstImage);

                        FirstImage = false;
                    }
                }
            }


            DoProp(Engine, "PrintGridLines", CurrentFile.PrintGridLines, LastFile.PrintGridLines, ref FirstLine);
            DoProp(Engine, "PrintHeadings", CurrentFile.PrintHeadings, LastFile.PrintHeadings, ref FirstLine);
            DoProp(Engine, "PrintHCentered", CurrentFile.PrintHCentered, LastFile.PrintHCentered, ref FirstLine);
            DoProp(Engine, "PrintVCentered", CurrentFile.PrintVCentered, LastFile.PrintVCentered, ref FirstLine);

            DoMargins(Engine, ref FirstLine);

            DoProp(Engine, "PrintToFit", CurrentFile.PrintToFit, LastFile.PrintToFit, ref FirstLine);
            DoProp(Engine, "PrintNumberOfHorizontalPages", CurrentFile.PrintNumberOfHorizontalPages, LastFile.PrintNumberOfHorizontalPages, ref FirstLine);
            DoProp(Engine, "PrintNumberOfVerticalPages", CurrentFile.PrintNumberOfVerticalPages, LastFile.PrintNumberOfVerticalPages, ref FirstLine);
            DoProp(Engine, "PrintScale", CurrentFile.PrintScale, LastFile.PrintScale, ref FirstLine);
            DoProp(Engine, "PrintFirstPageNumber", CurrentFile.PrintFirstPageNumber, LastFile.PrintFirstPageNumber, ref FirstLine);
            DoProp(Engine, "PrintCopies", CurrentFile.PrintCopies, LastFile.PrintCopies, ref FirstLine);
            DoProp(Engine, "PrintXResolution", CurrentFile.PrintXResolution, LastFile.PrintXResolution, ref FirstLine);
            DoProp(Engine, "PrintYResolution", CurrentFile.PrintYResolution, LastFile.PrintYResolution, ref FirstLine);
            DoProp(Engine, "PrintOptions", CurrentFile.PrintOptions, LastFile.PrintOptions, ref FirstLine);
            DoProp(Engine, "PrintPaperSize", CurrentFile.PrintPaperSize, LastFile.PrintPaperSize, ref FirstLine);

            DoPrinterDriverSettings(Engine, ref FirstLine);

        }

        private void DoMargins(LanguageEngine Engine, ref bool FirstLine)
        {
            TXlsMargins Margins2 = CurrentFile.GetPrintMargins();
            TXlsMargins Margins1 = LastFile.GetPrintMargins();

            if (Margins1.Bottom != Margins2.Bottom || Margins1.Footer != Margins2.Footer || Margins1.Header != Margins2.Header
                || Margins1.Left != Margins2.Left || Margins1.Right != Margins2.Right || Margins1.Top != Margins2.Top)
            {
                WriteComment(Engine, ref FirstLine, false);

                Engine.WriteLine();
                Engine.AddComment("You can set the margins in 2 ways, the one commented here or the one below:", false);
                Engine.CommentMode(true);
                Engine.InsertVar("PrintMargins", "TXlsMargins", Engine.GetCallMethod("GetPrintMargins", String.Empty));
                if (Margins1.Left != Margins2.Left) Engine.SetVarProp(Engine.Call("PrintMargins", "Left"), Margins2.Left);
                if (Margins1.Top != Margins2.Top) Engine.SetVarProp(Engine.Call("PrintMargins", "Top"), Margins2.Top);
                if (Margins1.Right != Margins2.Right) Engine.SetVarProp(Engine.Call("PrintMargins", "Right"), Margins2.Right);
                if (Margins1.Bottom != Margins2.Bottom) Engine.SetVarProp(Engine.Call("PrintMargins", "Bottom"), Margins2.Bottom);
                if (Margins1.Header != Margins2.Header) Engine.SetVarProp(Engine.Call("PrintMargins", "Header"), Margins2.Header);
                if (Margins1.Footer != Margins2.Footer) Engine.SetVarProp(Engine.Call("PrintMargins", "Footer"), Margins2.Footer);
                Engine.CallMethod("SetPrintMargins", "PrintMargins");
                Engine.CommentMode(false);

                Engine.CallMethod("SetPrintMargins", Engine.CreateObject("TXlsMargins",
                    Engine.GetParams(Margins2.Left, Margins2.Top, Margins2.Right, Margins2.Bottom, Margins2.Header, Margins2.Footer)));
            }

        }

        private void DoPrinterDriverSettings(LanguageEngine Engine, ref bool FirstLine)
        {
            TPrinterDriverSettings b2 = CurrentFile.GetPrinterDriverSettings();
            TPrinterDriverSettings b1 = LastFile.GetPrinterDriverSettings();

            bool SameSettings = TPrinterDriverSettings.Equals(b1, b2);

            if (!SameSettings)
            {
                WriteComment(Engine, ref FirstLine, false);

                Engine.WriteLine();
                Engine.AddComment("Printer Driver Settings are a blob of data specific to a printer", false);
                Engine.AddComment("This code is commented by default because normally you do not want to hard code the printer settings of an specific printer.", false);

                Engine.CommentMode(true);
                if (b2 == null)
                    Engine.CallMethod("SetPrinterDriverSettings", Engine.GetString(null));
                else
                {
                    Engine.DefineByteArray("PrinterData", "byte", b2.GetData());
                    Engine.InsertVar("PrinterDriverSettings", "TPrinterDriverSettings", Engine.CreateObject("TPrinterDriveSettings", "PrinterData"));

                    Engine.CallMethod("SetPrinterDriverSettings", "PrinterDriverSettings");
                }
                Engine.CommentMode(false);

            }

        }

        #endregion

        #region Page Breaks
        private void DoPageBreaks(LanguageEngine Engine)
        {
            SectionComment = "Page Breaks";
            bool FirstLine = true;

            for (int r = 1; r <= CurrentFile.RowCount; r++)
            {
                bool hc = CurrentFile.HasHPageBreak(r);
                bool hl = LastFile.HasHPageBreak(r);

                if (hc & !hl)
                {
                    WriteComment(Engine, ref FirstLine, false);
                    Engine.CallMethod("InsertHPageBreak", r);
                }
                else
                    if (!hc & hl)
                    {
                        WriteComment(Engine, ref FirstLine, false);
                        Engine.CallMethod("DeleteHPageBreak", r);
                    }

            }

            int ColCount = CurrentFile.ColCount;
            for (int c = 1; c <= ColCount; c++)
            {
                bool hc = CurrentFile.HasVPageBreak(c);
                bool hl = LastFile.HasVPageBreak(c);

                if (hc & !hl)
                {
                    Engine.CallMethod("InsertVPageBreak", c);
                }
                else
                    if (!hc & hl)
                    {
                        Engine.CallMethod("DeleteVPageBreak", c);
                    }

            }

        }


        #endregion

        #region Named Ranges

        private object GetRow(int Coord)
        {
            //If this was from a biff8file, it was converted to max_rows2007 anyway when loading. no need to check the range is Max_Rows2003.
            if (Coord == FlxConsts.Max_Rows + 1) return "FlxConsts.Max_Rows + 1";
            return Coord;
        }

        private object GetCol(int Coord)
        {
            if (Coord == FlxConsts.Max_Columns + 1) return "FlxConsts.Max_Columns + 1";
            return Coord;
        }

        private void DoNamedRanges(LanguageEngine Engine)
        {
            SectionComment = "Named Ranges";
            bool FirstLine = true;
            bool FirstRange = true;
            bool FirstInternalName = true;

            for (int i = 1; i <= CurrentFile.NamedRangeCount; i++)
            {
                TXlsNamedRange Range2 = CurrentFile.GetNamedRange(i);
                if (Range2.Name == TXlsNamedRange.GetInternalName(InternalNameRange.Filter_DataBase)) continue;  //this is used in autofilters, it should not be set in code.
                if (Range2.Hidden) continue;

                TXlsNamedRange Range1 = null;
                int LastCount = LastFile.NamedRangeCount;
                for (int k = 1; k <= LastCount; k++)  //This is quadratic, but there shouldn't be so many ranges anyway.
                {
                    int StartK = (i - 1 + k - 1) % LastCount; //Start at the same bucket. If there are no changes it will be faster.
                    Range1 = LastFile.GetNamedRange(1 + StartK);
                    if (Range1 == Range2) break;
                }

                if (Range2 != Range1)
                {
                    WriteComment(Engine, ref FirstLine, false);
                    if (FirstRange)
                    {
                        Engine.InsertVar("Range", "TXlsNamedRange");
                        FirstRange = false;
                    }

                    string RangeName = Range2.Name;
                    if (RangeName.Length == 1 && RangeName[0] < ' ') //internal name
                    {
                        if (FirstInternalName)
                        {
                            Engine.InsertVar("RangeName", "string");
                            FirstInternalName = false;
                        }

                        Engine.SetInternalName("RangeName", RangeName[0]);
                        RangeName = "RangeName"; //We will use a variable here.
                    }
                    else
                    {
                        RangeName = Engine.GetString(RangeName);
                    }

                    string Obj = Engine.CreateObject("TXlsNamedRange", Engine.GetParams(RangeName, Range2.NameSheetIndex,
                        Range2.OptionFlags, Engine.GetString(Range2.RangeFormula)));

                    string Obj2 = Engine.CreateObject("TXlsNamedRange", Engine.GetParams(RangeName, Range2.NameSheetIndex,
                        Range2.SheetIndex, GetRow(Range2.Top), GetCol(Range2.Left), GetRow(Range2.Bottom), GetCol(Range2.Right), Range2.OptionFlags));

                    Engine.SetVarProp("Range", Obj);
                    if (Range2.Top > 0 && Range2.Left > 0)
                    {
                        Engine.AddComment("You could also use: " + Engine.GetSetVarProp("Range", Obj2), false);
                    }
                    if (Range2.Comment != null)
                    {
                        Engine.SetVarProp(Engine.Call("Range", "Comment"), Engine.GetString(Range2.Comment));
                    }

                    Engine.CallMethod("SetNamedRange", "Range");
                    Engine.WriteLine();

                }
            }

        }

        #endregion

        #region Comments
        private void DoComments(LanguageEngine Engine)
        {
            bool FirstLine = true;
            SectionComment = "Comments";
            DoComments(Engine, LastFile, CurrentFile, ref FirstLine, true);
            DoComments(Engine, CurrentFile, LastFile, ref FirstLine, false);
        }

        private void DoComments(LanguageEngine Engine, ExcelFile xls, ExcelFile other, ref bool FirstLine, bool Delete)
        {
            bool FirstCommentProp = true;

            for (int r = 1; r <= xls.CommentRowCount(); r++)
            {
                for (int i = 1; i <= xls.CommentCountRow(r); i++)
                {
                    int c = xls.GetCommentRowCol(r, i);

                    TRichString val = xls.GetCommentRow(r, i);
                    TRichString otherVal = other.GetComment(r, c);

                    TCommentProperties ImgProp = CurrentFile.GetCommentPropertiesRow(r, i);
                    TCommentProperties OtherImgProp = LastFile.GetCommentProperties(r, c);

                    if (otherVal != val || !TCommentProperties.EqualValues(ImgProp, OtherImgProp))
                    {
                        WriteComment(Engine, ref FirstLine, false);

                        string CommentStr;
                        if (val == null || Delete) CommentStr = Engine.GetString(null);
                        else if (val.RTFRunCount == 0) CommentStr = Engine.GetString(Convert.ToString(val));
                        else
                        {
                            string Dummy;
                            CommentStr = Engine.GetAndWriteRichString(val, -1, -1, out Dummy);
                        }
                        Engine.CallMethod("SetComment", r, c, CommentStr);
                        Engine.WriteLine();

                        bool AutofitWritten = false;
                        if (!Delete)
                        {
                            if (!TImageProperties.EqualValues(ImgProp, OtherImgProp))
                            {
                                
                                Engine.AddComment("You probably don't need to call the lines below. This code is needed only if you want to change the comment box properties like color or default location", false);
                                Engine.WriteCommentProps(r, c, ImgProp, "CommentProps", FirstCommentProp);
                                FirstCommentProp = false;
                                WriteCommentAutofit(Engine, r, c, CommentStr, true);
                                AutofitWritten = true;

                                WriteSetCommentProps(Engine, r, c);
                            }
                        }

                        if (!AutofitWritten)
                        {
                            WriteCommentAutofit(Engine, r, c, CommentStr, false);
                        }
                    }
                }
            }

        }

        private static void WriteSetCommentProps(LanguageEngine Engine, int r, int c)
        {
            Engine.CallMethod("SetCommentProperties", Engine.GetInt(r), Engine.GetInt(c), "CommentProps");
        }

        private void WriteCommentAutofit(LanguageEngine Engine, int r, int c, string s, bool HasCommentProps)
        {
            Engine.WriteLine();
            Engine.AddComment("Excel by doesn't autofit the comment box so it can hold all text.", false);
            Engine.AddComment("There is an option in TCommentProperties, but if you use it Excel will show the text in a single line.", false);
            Engine.AddComment("To have FlexCel autofit the comment for you, you can do it with the following code:", false);
            Engine.WriteLine();
            Engine.CommentMode(true);

            try
            {
                if (!HasCommentProps) Engine.SetVarProp("CommentProps", Engine.GetCallVarMethod(String.Empty, Engine.Call("TCommentProperties", "CreateStandard"), r, c, Engine.XlsFileName));
                Engine.SetVarProp(Engine.Call("CommentProps", "Anchor"),
                      Engine.GetCallMethod("AutofitComment", s, 1.5, true, 1.1, 0, Engine.Call("CommentProps", "Anchor")));
                if (!HasCommentProps) WriteSetCommentProps(Engine, r, c);
            }
            finally
            {
                Engine.CommentMode(false);
            }
            Engine.WriteLine();
        }

        #endregion

        #region Images
        private void DoImages(LanguageEngine Engine)
        {
            bool FirstLine = true;
            bool FirstImage = true;
            SectionComment = "Images";
            DoImages(Engine, LastFile, CurrentFile, ref FirstLine, true, ref FirstImage);
            DoImages(Engine, CurrentFile, LastFile, ref FirstLine, false, ref FirstImage);
        }

        private void DoImages(LanguageEngine Engine, ExcelFile xls, ExcelFile other, ref bool FirstLine, bool Delete, ref bool FirstImage)
        {
            for (int i = 1; i <= xls.ImageCount; i++)
            {
                TImageProperties ImgProps = xls.GetImageProperties(i);
                string ImageName = xls.GetImageName(i);
                TXlsImgType ImgType = TXlsImgType.Unknown;
                byte[] ImageData = xls.GetImage(i, ref ImgType);

                TImageProperties OtherImgProps = i <= other.ImageCount ? other.GetImageProperties(i) : null;
                string OtherImageName = i <= other.ImageCount ? xls.GetImageName(i) : null;
                TXlsImgType OtherImgType = TXlsImgType.Unknown;
                byte[] OtherImageData = i <= other.ImageCount ? other.GetImage(i, ref OtherImgType) : null;  //We will match the images by position. The "good" way would be to loop in all the images, but that would be too slow.


                if (!TImageProperties.EqualValues(ImgProps, OtherImgProps) || ImageName != OtherImageName || !ArrayEquals(ImageData, OtherImageData))
                {
                    WriteComment(Engine, ref FirstLine, false);

                    if (Delete)
                    {
                        Engine.CallMethod("DeleteImage", Engine.GetInt(i));
                    }
                    else
                    {
                        Engine.AddImg(ImageData, ImgProps, "ImgProps", ImgType, FirstImage);
                        FirstImage = false;
                    }
                }
            }
        }

        #endregion

        #region Objects
        private void DoObjects(LanguageEngine Engine)
        {
            bool FirstLine = true;
            SectionComment = "Objects";
            List<RbData> RadioButtonValues = new List<RbData>();
            DoObjects(Engine, LastFile, CurrentFile, RadioButtonValues, ref FirstLine, true);
            DoObjects(Engine, CurrentFile, LastFile, RadioButtonValues, ref FirstLine, false);
            SetRadioButtonsLinksAndValues(Engine, RadioButtonValues);
        }

        private void SetRadioButtonsLinksAndValues(LanguageEngine Engine, List<RbData> RadioButtonValues)
        {
            if (RadioButtonValues.Count == 0) return;
            Engine.AddComment("Radio button values and links must be set after creating them all", false);
            foreach (RbData rb in RadioButtonValues)
            {
                if (rb.Link != null)
                {
                    Engine.CallMethod("SetObjectLinkedCell", rb.VarName, null, rb.Link);
                }
                else //when there is a link, we don't need to set the value.
                {
                    if (rb.Checked) Engine.CallMethod("SetRadioButtonState", rb.VarName, null, true);
                }
            }

        }

        private static TShapeProperties GetNext(ref int i, ExcelFile xls)
        {
            while (i < xls.ObjectCount)
            {
                i++;
                TShapeProperties ObjProps = xls.GetObjectProperties(i, true);
                if (IsSupportedObjType(ObjProps.ObjectType, ObjProps.IsInternal, ObjProps.ShapeType))
                {
                    return ObjProps;
                }
            }

            return null;
        }

        private void DoObjects(LanguageEngine Engine, ExcelFile xls, ExcelFile other, List<RbData> RadioButtonValues, ref bool FirstLine, bool Delete)
        {
            int ObjId1 = 0;
            TShapeProperties ObjProps = null;
            while ((ObjProps = GetNext(ref ObjId1, xls)) != null)
            {
                TShapeProperties OtherObjProps = ObjId1 > other.ObjectCount? null: other.GetObjectProperties(ObjId1, true);
                if (OtherObjProps != null && !IsSupportedObjType(OtherObjProps.ObjectType, ObjProps.IsInternal, ObjProps.ShapeType)) OtherObjProps = null;
                //We will match the objects by position. The "good" way would be to loop in all the objects, but that would be too slow.

                DoOneObject(Engine, xls, other, ref FirstLine, Delete, ObjId1, ObjProps, OtherObjProps, RadioButtonValues);
             }
        }

        private void DoOneObject(LanguageEngine Engine, ExcelFile xls, ExcelFile other, ref bool FirstLine,
            bool Delete, int OrigObjId, TShapeProperties ObjProps, TShapeProperties OtherObjProps, List<RbData> RadioButtonValues)
        {
            if (SameObjects(xls, other, ObjProps, OtherObjProps)) return;
            WriteComment(Engine, ref FirstLine, false);
            int ObjId;
            string ObjectPath;
            if (Delete)CalcObjIdAndPath(Engine, xls, OrigObjId, ObjProps, out ObjId, out ObjectPath);
            else CalcObjIdAndPath(Engine, other, OrigObjId, OtherObjProps, out ObjId, out ObjectPath);

            bool Modif = ObjectCanBeModified(ObjProps, OtherObjProps);

            if (Delete)
            {
                if (!Modif)
                {
                    Engine.CallMethod("DeleteObject", ObjId, ObjectPath);
                }
            }
            else
            {
                if (Modif)
                {
                    ModifyObject(Engine, xls, other, ObjProps, OtherObjProps, ObjId, ObjectPath);
                }
                else
                {
                    AddObject(Engine, xls, ObjProps, OrigObjId, RadioButtonValues);
                }
            }
        }

        private static void CalcObjIdAndPath(LanguageEngine Engine, ExcelFile other, int OrigObjId, TShapeProperties OtherObjProps, out int ObjId, out string ObjectPath)
        {
            ObjId = 0;
            ObjectPath = null;
            if (OtherObjProps != null)
            {
                ObjectPath = Engine.GetString(other.FindObjectPath(OtherObjProps.ShapeName));
                if (ObjectPath == Engine.GetString(null))
                {
                    ObjectPath = Engine.GetString(OtherObjProps.ObjectPath);
                    ObjId = OrigObjId;
                }
            }
        }

        private void AddObject(LanguageEngine Engine, ExcelFile xls, TShapeProperties ObjProps, int ObjId, List<RbData> RadioButtonValues)
        {
            string ObjAnchor = Engine.GetAnchor(ObjProps.Anchor);
            string ObjText = GetObjText(Engine, ObjProps.Text);
            string ObjMacro = xls.GetObjectMacro(0,ObjProps.ObjectPathAbsolute);

            switch (ObjProps.ObjectType)
            {
                case TObjectType.CheckBox:
                    TCheckboxState cb = xls.GetCheckboxState(0, ObjProps.ObjectPathAbsolute);
                    TCellAddress LinkedCell = xls.GetObjectLinkedCell(0, ObjProps.ObjectPathAbsolute);
                    CallAddObject(Engine, false, "Checkbox"+ ObjId.ToString(), ObjMacro,
                        "AddCheckbox", ObjAnchor, ObjText, Engine.GetEnum(cb), LinkedCell, Engine.GetString(ObjProps.ShapeName));
                    break;

                case TObjectType.OptionButton:
                    bool rb = xls.GetRadioButtonState(0, ObjProps.ObjectPathAbsolute);
                    TCellAddress rLinkedCell = xls.GetObjectLinkedCell(0, ObjProps.ObjectPathAbsolute);

                    string varName = "RadioButton" + ObjId.ToString();

                    CallAddObject(Engine, rLinkedCell != null || rb, varName, ObjMacro,
                        "AddRadioButton", ObjAnchor, ObjText, Engine.GetString(ObjProps.ShapeName));
                   
                    //Radio button state and links must be set after we add all objects, as the link affects
                    //all buttons in the group, and groups can change when we are adding objects.
                    if (rLinkedCell != null) RadioButtonValues.Add(new RbData(varName, rLinkedCell));
                    else if (rb) RadioButtonValues.Add(new RbData(varName, rb));
                    break;

                case TObjectType.GroupBox:
                    CallAddObject(Engine, false, "GroupBox" + ObjId.ToString(), ObjMacro,
                        "AddGroupBox", ObjAnchor, ObjText, Engine.GetString(ObjProps.ShapeName));
                    break;

                case TObjectType.Button:
                    string bMacro = xls.GetObjectMacro(0, ObjProps.ObjectPathAbsolute); 
                    Engine.CallMethod("AddButton", ObjAnchor, ObjText, Engine.GetString(ObjProps.ShapeName), Engine.GetString(bMacro));
                    break;

                case TObjectType.ComboBox:
                case TObjectType.ListBox:
                    int SelectedItem = xls.GetObjectSelection(0, ObjProps.ObjectPathAbsolute);
                    TCellAddress cLinkedCell = xls.GetObjectLinkedCell(0, ObjProps.ObjectPathAbsolute);
                    TCellAddressRange cInputRange = xls.GetObjectInputRange(0, ObjProps.ObjectPathAbsolute);
                    if (ObjProps.ObjectType == TObjectType.ComboBox)
                    {
                        CallAddObject(Engine, false, "ComboBox" + ObjId.ToString(), ObjMacro,
                            "AddComboBox", ObjAnchor, Engine.GetString(ObjProps.ShapeName), cLinkedCell, cInputRange, SelectedItem);
                    }
                    else
                    {
                        CallAddObject(Engine, false, "ListBox" + ObjId.ToString(), ObjMacro,
                            "AddListBox", ObjAnchor, Engine.GetString(ObjProps.ShapeName), cLinkedCell, cInputRange,
                            Engine.GetEnum(TListBoxSelectionType.Single), SelectedItem);                    }
                    break;

                case TObjectType.Label:
                        CallAddObject(Engine, false, "Label" + ObjId.ToString(), ObjMacro,
                            "AddLabel", ObjAnchor, ObjText, Engine.GetString(ObjProps.ShapeName));
                    break;

                case TObjectType.ScrollBar:
                case TObjectType.Spinner:
                    string sAddMethod = ObjProps.ObjectType == TObjectType.ScrollBar ? "AddScrollBar" : "AddSpinner";
                    TCellAddress sLinkedCell = xls.GetObjectLinkedCell(0, ObjProps.ObjectPathAbsolute);
                    TSpinProperties SpinProps = xls.GetObjectSpinProperties(0, ObjProps.ObjectPathAbsolute);
                    if (SpinProps != null)
                    {
                        CallAddObject(Engine, false, "SpinScroll" + ObjId.ToString(), ObjMacro,
                            sAddMethod, ObjAnchor, Engine.GetString(ObjProps.ShapeName),
                            sLinkedCell,
                            GetSpinProperties(Engine, SpinProps));
                    }
                    break;

                case TObjectType.MicrosoftOfficeDrawing:
                    string ShapeOptionsVar = "ShapeOptions" + ObjId.ToString();
                    SetObjProps(Engine, ObjProps, ShapeOptionsVar);
                    CallAddObject(Engine, false, "Shape" + ObjId.ToString(), ObjMacro,
                        "AddAutoShape", ShapeOptionsVar);
                    Engine.WriteLine();

                    break;
            }

        }

        private void SetObjProps(LanguageEngine Engine, TShapeProperties ShapeProps, string ShapeOptionsVar)
        {
            Engine.WriteShapeOptions(ShapeOptionsVar, ShapeProps);
        }

        private static void CallAddObject(LanguageEngine Engine, bool ForceVar, string varName, string Macro, params object[] p)
        {
            if (ForceVar || Macro != null)
            {
                Engine.InsertVar(varName, Engine.IntegerString(),
                    Engine.GetCallMethod(p));
            }
            else
            {
                Engine.CallMethod(p);
            }

            if (Macro != null)
            {
                Engine.CallMethod("SetObjectMacro", varName, null, Engine.GetString(Macro));
            }
        }

        private static string GetSpinProperties(LanguageEngine Engine, TSpinProperties SpinProps)
        {
            if (SpinProps.IsHorizontal) return Engine.CreateObject("TSpinProperties", Engine.GetParams(SpinProps.Min, SpinProps.Max, SpinProps.Incr, SpinProps.Page, SpinProps.IsHorizontal));
            return Engine.CreateObject("TSpinProperties", Engine.GetParams(SpinProps.Min, SpinProps.Max, SpinProps.Incr, SpinProps.Page));
        }

        private static string GetObjText(LanguageEngine Engine, TDrawingRichString rs)
        {
            // While we could make a better conversion here, it is not worth it since you can't change fonts in objects like a checkbox.
            // You could actually do it, but not trough Excel UI). If we wanted to provide this option, we should also consider that the default is "Tahoma 7"
            // and if all there is in rs is a "Tahoma 7" rich text, we should return plain text.
            return Engine.GetString(rs);
        }

        private static void ModifyObject(LanguageEngine Engine, ExcelFile xls, ExcelFile other, 
            TShapeProperties ObjProps, TShapeProperties OtherObjProps, int ObjId, string ObjectPath)
        {
            switch (ObjProps.ObjectType)
            {
                case TObjectType.CheckBox:
                    TCheckboxState cb = xls.GetCheckboxState(0, ObjProps.ObjectPathAbsolute);
                    TCheckboxState ocb = other.GetCheckboxState(0, OtherObjProps.ObjectPathAbsolute);
                    if (cb != ocb && !HasLinkedCell(xls, ObjProps))
                    {
                        Engine.CallMethod("SetCheckboxState", ObjId, ObjectPath, Engine.GetEnum(cb));
                    }
                    break;

                case TObjectType.OptionButton:
                    bool rb = xls.GetRadioButtonState(0, ObjProps.ObjectPathAbsolute);
                    bool orb = other.GetRadioButtonState(0, OtherObjProps.ObjectPathAbsolute);
                    if (rb != orb && !HasLinkedCell(xls, ObjProps))
                    {
                        Engine.CallMethod("SetRadioButtonState", ObjId, ObjectPath, Engine.GetBool(rb));
                    }

                    break;

                case TObjectType.GroupBox:
                    break;

                case TObjectType.Button:
                    break;

                case TObjectType.ComboBox:
                case TObjectType.ListBox:
                    int sv= xls.GetObjectSpinValue(0, ObjProps.ObjectPathAbsolute);
                    int osv = other.GetObjectSpinValue(0, OtherObjProps.ObjectPathAbsolute);
                    if (sv != osv && !HasLinkedCell(xls, ObjProps))
                    {
                        Engine.CallMethod("SetObjectSpinValue", ObjId, ObjectPath, sv);
                    }
                    break;

                case TObjectType.Label:
                    break;

                case TObjectType.Spinner:
                case TObjectType.ScrollBar:
                    break;

            }

            ModifyObjectSelection(Engine, xls, other, ObjProps, OtherObjProps, ObjId, ObjectPath);
            ModifySpinProperties(Engine, xls, other, ObjProps, OtherObjProps, ObjId, ObjectPath);           
            ModifyLinkedCell(Engine, xls, other, ObjProps, OtherObjProps, ObjId, ObjectPath);
            ModifyInputRange(Engine, xls, other, ObjProps, OtherObjProps, ObjId, ObjectPath);
            ModifyMacro(Engine, xls, other, ObjProps, OtherObjProps, ObjId, ObjectPath);
            TDrawingRichString rs = ObjProps.Text;
            TDrawingRichString ors = OtherObjProps.Text;
            if (!TDrawingRichString.Equals(rs, ors))
            {
                Engine.CallMethod("SetObjectText", ObjId, ObjectPath, GetObjText(Engine, rs));
            }

            if (!TClientAnchor.Equals(ObjProps.Anchor, OtherObjProps.Anchor))
            {
                Engine.CallMethod("SetObjectAnchor", ObjId, ObjectPath, Engine.GetAnchor(ObjProps.Anchor));
            }

            if (!String.Equals(ObjProps.ShapeName, OtherObjProps.ShapeName))
            {
                Engine.CallMethod("SetObjectName", ObjId, ObjectPath, Engine.GetString(ObjProps.ShapeName));
            }

        }

        private static void ModifySpinProperties(LanguageEngine Engine, ExcelFile xls, ExcelFile other, TShapeProperties ObjProps, TShapeProperties OtherObjProps, int ObjId, string ObjectPath)
        {
            TSpinProperties sd = xls.GetObjectSpinProperties(0, ObjProps.ObjectPathAbsolute);
            TSpinProperties osd = other.GetObjectSpinProperties(0, OtherObjProps.ObjectPathAbsolute);
            if (!Object.Equals(sd, osd) && sd != null && osd != null)
            {
                Engine.CallMethod("SetObjectSpinProperties", ObjId, ObjectPath, GetSpinProperties(Engine, sd));
            }
        }

        private static void ModifyMacro(LanguageEngine Engine, ExcelFile xls, ExcelFile other, TShapeProperties ObjProps, TShapeProperties OtherObjProps, int ObjId, string ObjectPath)
        {
            string Macro = xls.GetObjectMacro(0, ObjProps.ObjectPathAbsolute);
            string oMacro = other.GetObjectMacro(0, OtherObjProps.ObjectPathAbsolute);

            if (Macro != oMacro)
            {
                Engine.CallMethod("SetObjectMacro", ObjId, ObjectPath, Engine.GetString(Macro));
            }
        }

        private static void ModifyInputRange(LanguageEngine Engine, ExcelFile xls, ExcelFile other, TShapeProperties ObjProps, TShapeProperties OtherObjProps, int ObjId, string ObjectPath)
        {
            TCellAddressRange InputRange = xls.GetObjectInputRange(0, ObjProps.ObjectPathAbsolute);
            TCellAddressRange oInputRange = other.GetObjectInputRange(0, OtherObjProps.ObjectPathAbsolute);
           
            if (!TCellAddressRange.Equals(InputRange, oInputRange))
            {
                Engine.CallMethod("SetObjectInputRange", ObjId, ObjectPath, InputRange);
            }
        }

        private static void ModifyObjectSelection(LanguageEngine Engine, ExcelFile xls, ExcelFile other, TShapeProperties ObjProps, TShapeProperties OtherObjProps, int ObjId, string ObjectPath)
        {
            int si = xls.GetObjectSelection(0, ObjProps.ObjectPathAbsolute);
            int osi = other.GetObjectSelection(0, OtherObjProps.ObjectPathAbsolute);
            if (si != osi && !HasLinkedCell(xls, ObjProps))
            {
                Engine.CallMethod("SetObjectSelection", ObjId, ObjectPath, Engine.GetInt(si));
            }
        }

        private static bool HasLinkedCell(ExcelFile xls, TShapeProperties ObjProps)
        {
            return xls.GetObjectLinkedCell(0, ObjProps.ObjectPathAbsolute) != null;
        }

        private static void ModifyLinkedCell(LanguageEngine Engine, ExcelFile xls, ExcelFile other, TShapeProperties ObjProps, TShapeProperties OtherObjProps, int ObjId, string ObjectPath)
        {
            TCellAddress LinkedCell = xls.GetObjectLinkedCell(0, ObjProps.ObjectPathAbsolute);
            TCellAddress oLinkedCell = other.GetObjectLinkedCell(0, OtherObjProps.ObjectPathAbsolute);
            if (!TCellAddress.Equals(LinkedCell, oLinkedCell))
            {
                Engine.CallMethod("SetObjectLinkedCell", ObjId, ObjectPath, LinkedCell);
            }
        }

        private static bool ObjectCanBeModified(TShapeProperties ObjProps, TShapeProperties OtherObjProps)
        {
            return OtherObjProps != null && 
                ObjProps.ObjectType == OtherObjProps.ObjectType 
                && IsSupportedObjType(ObjProps.ObjectType, ObjProps.IsInternal || OtherObjProps.IsInternal, ObjProps.ShapeType);
        }

        private static bool SameObjects(ExcelFile xls, ExcelFile other, TShapeProperties ObjProps, TShapeProperties OtherObjProps)
        {
            if (OtherObjProps == null) return false;
            if (!ObjectCanBeModified(ObjProps, OtherObjProps)) return true;

            switch (ObjProps.ObjectType)
            {
                case TObjectType.CheckBox:
                    if (!ObjSameCommonProps(ObjProps, OtherObjProps)) return false;
                    if (!TCellAddress.Equals(xls.GetObjectLinkedCell(0, ObjProps.ObjectPathAbsolute), other.GetObjectLinkedCell(0, OtherObjProps.ObjectPathAbsolute))) return false;
                    if (!HasLinkedCell(xls, ObjProps) &&
                        xls.GetCheckboxState(0, ObjProps.ObjectPathAbsolute) != other.GetCheckboxState(0, OtherObjProps.ObjectPathAbsolute)) return false;
                    break;

                case TObjectType.OptionButton:
                    if (!ObjSameCommonProps(ObjProps, OtherObjProps)) return false;
                    if (!TCellAddress.Equals(xls.GetObjectLinkedCell(0, ObjProps.ObjectPathAbsolute), other.GetObjectLinkedCell(0, OtherObjProps.ObjectPathAbsolute))) return false;
                    if (!HasLinkedCell(xls, ObjProps) &&
                        xls.GetRadioButtonState(0, ObjProps.ObjectPathAbsolute) != other.GetRadioButtonState(0, OtherObjProps.ObjectPathAbsolute)) return false;
                    break;

                case TObjectType.GroupBox:
                    if (!ObjSameCommonProps(ObjProps, OtherObjProps)) return false;
                    break;
                
                case TObjectType.Button:
                    if (!ObjSameCommonProps(ObjProps, OtherObjProps)) return false;
                    break;

                case TObjectType.ComboBox:
                case TObjectType.ListBox:
                    if (!ObjSameCommonProps(ObjProps, OtherObjProps)) return false;
                    if (!TCellAddress.Equals(xls.GetObjectLinkedCell(0, ObjProps.ObjectPathAbsolute), other.GetObjectLinkedCell(0, OtherObjProps.ObjectPathAbsolute))) return false;
                    if (!TCellAddressRange.Equals(xls.GetObjectInputRange(0, ObjProps.ObjectPathAbsolute), other.GetObjectInputRange(0, OtherObjProps.ObjectPathAbsolute))) return false;
                    if (!HasLinkedCell(xls, ObjProps) &&
                        xls.GetObjectSelection(0, ObjProps.ObjectPathAbsolute) != other.GetObjectSelection(0, OtherObjProps.ObjectPathAbsolute)) return false;
                    break;

                case TObjectType.Label:
                    if (!ObjSameCommonProps(ObjProps, OtherObjProps)) return false;
                    break;

                case TObjectType.Spinner:
                case TObjectType.ScrollBar:
                    if (!ObjSameCommonProps(ObjProps, OtherObjProps)) return false;
                    TSpinProperties sd = xls.GetObjectSpinProperties(0, ObjProps.ObjectPathAbsolute);
                    TSpinProperties osd = other.GetObjectSpinProperties(0, OtherObjProps.ObjectPathAbsolute);
                    if (!Object.Equals(sd, osd)) return false;
                    int si = xls.GetObjectSpinValue(0, ObjProps.ObjectPathAbsolute);
                    int osi = other.GetObjectSpinValue(0, OtherObjProps.ObjectPathAbsolute);
                    if (!HasLinkedCell(xls, ObjProps) && si != osi) return false;

                    break;

            }

            if (xls.GetObjectMacro(0, ObjProps.ObjectPathAbsolute) != other.GetObjectMacro(0, OtherObjProps.ObjectPathAbsolute)) return false;

            return true;                
        }

        private static bool ObjSameCommonProps(TShapeProperties ObjProps, TShapeProperties OtherObjProps)
        {
            return ObjProps.ShapeName == OtherObjProps.ShapeName
                && TClientAnchor.Equals(ObjProps.Anchor, OtherObjProps.Anchor)
                && TRichString.Equals(ObjProps.Text, OtherObjProps.Text);
        }

        private static bool IsSupportedObjType(TObjectType objType, bool IsInternal, TShapeType shapeType)
        {
            if (IsInternal) return false;
            switch (objType)
            {
                case TObjectType.CheckBox:
                case TObjectType.OptionButton:
                case TObjectType.GroupBox:
                case TObjectType.Button:
                case TObjectType.ComboBox:
                case TObjectType.ListBox:
                case TObjectType.Label:
                case TObjectType.Spinner:
                case TObjectType.ScrollBar:
                    return true;

                case TObjectType.MicrosoftOfficeDrawing:
                    return true;
                default:
                    return false;
            }
        }


        #endregion

        #region Conditional Formats
        private void DoConditionalFormats(LanguageEngine Engine)
        {
            if (SameCondFmt()) return;


            SectionComment = "Conditional Formats";
            bool FirstLine = true;

            if (LastFile.ConditionalFormatCount != 0)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.CallMethod("ClearConditionalFormatsInSheet");
            }
            AddCFs(Engine, CurrentFile, ref FirstLine);

        }

        private bool SameCondFmt()
        {
            if (LastFile.ConditionalFormatCount != CurrentFile.ConditionalFormatCount) return false;
            for (int i = 1; i <= LastFile.ConditionalFormatCount; i++)
            {
                if (!Object.Equals(LastFile.GetConditionalFormat(i), CurrentFile.GetConditionalFormat(i))) return false;
            }
            return true;
        }

        private void AddCFs(LanguageEngine Engine, ExcelFile Master, ref bool FirstLine)
        {
            for (int i = 1; i <= Master.ConditionalFormatCount; i++)
            {
                TConditionalFormat CurrentCF = Master.GetConditionalFormat(i);
                AddOneCF(Engine, CurrentCF, ref FirstLine);
            }
        }

        private void AddOneCF(LanguageEngine Engine, TConditionalFormat CurrentCF, ref bool FirstLine)
        {
            WriteComment(Engine, ref FirstLine, false);
            const string RangesVariable = "Ranges";
            Engine.DefineArray(true, RangesVariable, "TXlsCellRange", 1, CurrentCF.Ranges.Length,
                delegate (int i)
                {
                    TXlsCellRange Range = CurrentCF.Ranges[i];
                    return Engine.GetRange(Range);
                });

            Engine.WriteLine();
            const string RulesVariable = "Rules";
            Engine.InsertVarAndTrack(RulesVariable, Engine.GetArrayDecl("TConditionalFormatRule"), Engine.CreateObjectArray(null, "TConditionalFormatRule", CurrentCF.Rules.Length));            
            for (int i = 0; i < CurrentCF.Rules.Length; i++)
            {
                WriteOneCFRule(i, Engine, CurrentCF.Rules[i]);
                Engine.WriteLine();
            }
            Engine.CallMethod("AddConditionalFormat", Engine.CreateObject("TConditionalFormat", Engine.GetParams(RangesVariable, RulesVariable, Engine.GetBool(CurrentCF.IsPivot))));
            Engine.WriteLine();
        }

        private void WriteOneCFRule(int index, LanguageEngine Engine, TConditionalFormatRule cf)
        {
            string RuleVarName = GetRuleVarName(cf.Kind);
            string RuleVarKind = GetCondFmtClass(cf.Kind);
            string RuleArrayName = Engine.GetCallArray("Rules", index);
            Engine.InsertVarAndTrack(RuleVarName, RuleVarKind, Engine.CreateObject(RuleVarKind, GetCondFmtInitializer(Engine, cf)));
            DefineCondFmtFormat(Engine, cf.Kind, cf, RuleVarName);
            Engine.SetVarProp(RuleArrayName, RuleVarName);
        }

        private void DefineCondFmtFormat(LanguageEngine Engine, TConditionalFormatKind Kind, TConditionalFormatRule cf, string RuleVarName)
        {
            switch (Kind)
            {
                case TConditionalFormatKind.ColorScale:
                    DefineCondFmtFormatColorScale(Engine, ((TConditionalColorScaleRule)cf).FormatDef, RuleVarName);
                    break;
                case TConditionalFormatKind.DataBar:
                    DefineCondFmtFormatDataBar(Engine, ((TConditionalDataBarRule)cf).FormatDef, RuleVarName);
                    break;
                case TConditionalFormatKind.IconSet:
                    DefineCondFmtFormatIconSet(Engine, ((TConditionalIconSetRule)cf).FormatDef, RuleVarName);
                    break;
                default:
                    DefineCondFmtFormatStandard(Engine, ((TConditionalFormatStandardDefRule)cf).FormatDef, RuleVarName);
                    break;
            }
        }

        private void DefineCondFmtFormatColorScale(LanguageEngine Engine, TConditionalFormatDefColorScale cf, string RuleVarName)
        {
            if (cf == null)
            {
                return;
            }
            Engine.InsertVarAndTrack(ColorScaleVariable, "TConditionalFormatDefColorScale", Engine.Call(RuleVarName, "FormatDef"));

            DefineCFVOs(Engine, Engine.Call(Engine.Call(ColorScaleVariable, "ValuesAndColors"), "Values"), cf.ValuesAndColors.Values,
                Engine.Call(Engine.Call(ColorScaleVariable, "ValuesAndColors"), "Colors"), cf.ValuesAndColors.Colors);
        }

        private void DefineCFVOs(LanguageEngine Engine, string CfvoList, TConditionalFormatValueList values, string ColorList, TConditionalFormatColorList Colors)
        {
            if (values == null) return;
            for (int i = 0; i < values.Count; i++)
            {
                Engine.CallVarMethod(CfvoList, "Add", DefineCFVO(Engine, values[i]));
                if (Colors != null && i < Colors.Count)
                {
                    Engine.CallVarMethod(ColorList, "Add", Engine.GetColor(Colors[i], true));
                }
            }
        }

        private string DefineCFVO(LanguageEngine Engine, TConditionalFormatValueObject cfvo)
        {
            if (cfvo == null) return Engine.GetNullStr();
            return Engine.CreateObject("TConditionalFormatValueObject", Engine.GetParams(cfvo.GreaterThanOrEqual, cfvo.VoType, Engine.GetString(cfvo.Value)));
        }

        private void DefineCondFmtFormatDataBar(LanguageEngine Engine, TConditionalFormatDefDataBar cf, string RuleVarName)
        {
            if (cf == null)
            {
                return;
            }
            Engine.InsertVarAndTrack(DataBarVariable, "TConditionalFormatDefDataBar", Engine.Call(RuleVarName, "FormatDef"));
            Engine.SetVarProp(Engine.Call(DataBarVariable, "MinLengthValue"), DefineCFVO(Engine, cf.MinLengthValue));
            Engine.SetVarProp(Engine.Call(DataBarVariable, "MaxLengthValue"), DefineCFVO(Engine, cf.MaxLengthValue));
            DefineCondFmtDataBarColors(Engine, cf.Colors);
            Engine.SetVarProp(Engine.Call(DataBarVariable, "MinBarWidth"), cf.MinBarWidth);
            Engine.SetVarProp(Engine.Call(DataBarVariable, "MaxBarWidth"), cf.MaxBarWidth);
            Engine.SetVarProp(Engine.Call(DataBarVariable, "ShowValues"), cf.ShowValues);
            Engine.SetVarProp(Engine.Call(DataBarVariable, "HasBorders"), cf.HasBorders);
            Engine.SetVarProp(Engine.Call(DataBarVariable, "IsGradient"), cf.IsGradient);
            Engine.SetVarProp(Engine.Call(DataBarVariable, "Direction"), cf.Direction);
            Engine.SetVarProp(Engine.Call(DataBarVariable, "IsNegativeBarColorSameAsPositive"), cf.IsNegativeBarColorSameAsPositive);
            Engine.SetVarProp(Engine.Call(DataBarVariable, "IsNegativeBarBorderColorSameAsPositive"), cf.IsNegativeBarBorderColorSameAsPositive);
            Engine.SetVarProp(Engine.Call(DataBarVariable, "AxisPosition"), cf.AxisPosition);

        }

        private void DefineCondFmtDataBarColors(LanguageEngine Engine, TDataBarColors Colors)
        {
            SetDataBarColor(Engine, Colors.FillColor, true, "FillColor");
            SetDataBarColor(Engine, Colors.BorderColor, false, "BorderColor");
            SetDataBarColor(Engine, Colors.NegativeFillColor, true, "NegativeFillColor");
            SetDataBarColor(Engine, Colors.NegativeBorderColor, false, "NegativeBorderColor");
            SetDataBarColor(Engine, Colors.AxisColor, true, "AxisColor");
        }

        private static void SetDataBarColor(LanguageEngine Engine, TExcelColor Color, bool DefaultsToWhite, string ColorVariable)
        {
            if (!Color.IsAutomatic) Engine.SetVarProp(Engine.Call(Engine.Call(DataBarVariable, "Colors"), ColorVariable), Engine.GetColor(Color, DefaultsToWhite));
        }

        private void DefineCondFmtFormatIconSet(LanguageEngine Engine, TConditionalFormatDefIconSet cf, string RuleVarName)
        {
            if (cf == null)
            {
                return;
            }
            Engine.InsertVarAndTrack(IconSetVariable, "TConditionalFormatDefIconSet", Engine.Call(RuleVarName, "FormatDef"));
            Engine.SetVarProp(Engine.Call(IconSetVariable, "IconSet"), cf.IconSet);
            DefineCFVOs(Engine, Engine.Call(IconSetVariable, "Values"), cf.Values, "", null);
            Engine.SetVarProp(Engine.Call(IconSetVariable, "Reverse"), cf.Reverse);
            Engine.SetVarProp(Engine.Call(IconSetVariable, "ShowValues"), cf.ShowValues);

            if (cf.IsCustom)
            {
                Engine.DefineArray(false, Engine.Call(IconSetVariable, "CustomIcons"), "TConditionalFormatCustomIconDef", 1, cf.CustomIcons.Length,
                    delegate (int i)
                    {
                        TConditionalFormatCustomIconDef CustomIcon = cf.CustomIcons[i];
                        return Engine.CreateObject("TConditionalFormatCustomIconDef", Engine.GetParams(CustomIcon.IconSet, CustomIcon.IconNumber));
                    });
            }

        }

        private void DefineCondFmtFormatStandard(LanguageEngine Engine, TConditionalFormatDefStandard cf, string RuleVarName)
        {
            if (cf == null)
            {
                return;
            }
            Engine.InsertVarAndTrack(StandardCFVariable, "TConditionalFormatDefStandard", Engine.Call(RuleVarName, "FormatDef"));

            if (cf.ApplyFont.Size20)
            {
                Engine.SetVarProp(Engine.Call(Engine.Call(StandardCFVariable, "ApplyFont"), "Size20"), true);
                Engine.SetVarProp(Engine.Call(Engine.Call(StandardCFVariable, "Font"), "Size20"), cf.Font.Size20);
            }

            if (cf.ApplyFont.Color)
            {
                Engine.SetVarProp(Engine.Call(Engine.Call(StandardCFVariable, "ApplyFont"), "Color"), true);
                Engine.SetVarProp(Engine.Call(Engine.Call(StandardCFVariable, "Font"), "Color"), Engine.GetColor(cf.Font.Color, false));
            }

            if (cf.ApplyFont.BoldAndItalic)
            {
                Engine.SetVarProp(Engine.Call(Engine.Call(StandardCFVariable, "ApplyFont"), "BoldAndItalic"), true);
            }

            if (cf.ApplyFont.Strikeout)
            {
                Engine.SetVarProp(Engine.Call(Engine.Call(StandardCFVariable, "ApplyFont"), "Strikeout"), true);
            }

            if (cf.ApplyFont.SubSuperscript)
            {
                Engine.SetVarProp(Engine.Call(Engine.Call(StandardCFVariable, "ApplyFont"), "SubSuperscript"), true);
            }

            if (cf.ApplyFont.BoldAndItalic || cf.ApplyFont.Strikeout || cf.ApplyFont.SubSuperscript)
            {
                Engine.SetVarProp(Engine.Call(Engine.Call(StandardCFVariable, "Font"), "Style"), cf.Font.Style);
            }

            if (cf.ApplyFont.Underline)
            {
                Engine.SetVarProp(Engine.Call(Engine.Call(StandardCFVariable, "ApplyFont"), "Underline"), true);
                Engine.SetVarProp(Engine.Call(Engine.Call(StandardCFVariable, "Font"), "Underline"), cf.Font.Underline);
            }


            if (cf.ApplyNumericFormat)
            {
                Engine.SetVarProp(Engine.Call(StandardCFVariable, "ApplyNumericFormat"), true);
                Engine.SetVarProp(Engine.Call(StandardCFVariable, "NumericFormat"), Engine.GetString(cf.NumericFormat));
            }

            if (cf.ApplyFill.Pattern)
            {
                Engine.SetVarProp(Engine.Call(Engine.Call(StandardCFVariable, "ApplyFill"), "Pattern"), true);
                Engine.SetVarProp(Engine.Call(Engine.Call(StandardCFVariable, "Fill"), "Pattern"), cf.Fill.Pattern);
                if (cf.Fill.Pattern == TFlxPatternStyle.Gradient)
                {
                    Engine.DefineGradient(StandardCFVariable, cf.Fill, "Fill");
                }
            }

            if (cf.ApplyFill.BgColor && cf.Fill.Pattern != TFlxPatternStyle.Gradient)
            {
                Engine.SetVarProp(Engine.Call(Engine.Call(StandardCFVariable, "ApplyFill"), "BgColor"), true);
                Engine.SetVarProp(Engine.Call(Engine.Call(StandardCFVariable, "Fill"), "BgColor"), Engine.GetColor(cf.Fill.BgColor, true));
            }

            if (cf.ApplyFill.FgColor && cf.Fill.Pattern != TFlxPatternStyle.Gradient)
            {
                Engine.SetVarProp(Engine.Call(Engine.Call(StandardCFVariable, "ApplyFill"), "FgColor"), true);
                Engine.SetVarProp(Engine.Call(Engine.Call(StandardCFVariable, "Fill"), "FgColor"), Engine.GetColor(cf.Fill.FgColor, true));
            }

            if (cf.ApplyBorders.Left)
            {
                Engine.SetVarProp(Engine.Call(Engine.Call(StandardCFVariable, "ApplyBorders"), "Left"), true);
                Engine.SetVarProp(Engine.Call(Engine.Call(StandardCFVariable, "Borders"), "Left"), Engine.GetOneBorder(cf.Borders.Left));
            }

            if (cf.ApplyBorders.Top)
            {
                Engine.SetVarProp(Engine.Call(Engine.Call(StandardCFVariable, "ApplyBorders"), "Top"), true);
                Engine.SetVarProp(Engine.Call(Engine.Call(StandardCFVariable, "Borders"), "Top"), Engine.GetOneBorder(cf.Borders.Top));
            }

            if (cf.ApplyBorders.Right)
            {
                Engine.SetVarProp(Engine.Call(Engine.Call(StandardCFVariable, "ApplyBorders"), "Right"), true);
                Engine.SetVarProp(Engine.Call(Engine.Call(StandardCFVariable, "Borders"), "Right"), Engine.GetOneBorder(cf.Borders.Right));
            }

            if (cf.ApplyBorders.Bottom)
            {
                Engine.SetVarProp(Engine.Call(Engine.Call(StandardCFVariable, "ApplyBorders"), "Bottom"), true);
                Engine.SetVarProp(Engine.Call(Engine.Call(StandardCFVariable, "Borders"), "Bottom"), Engine.GetOneBorder(cf.Borders.Bottom));
            }
        }

        private string GetCondFmtInitializer(LanguageEngine Engine, TConditionalFormatRule cf)
        {
            switch (cf.Kind)
            {
                case TConditionalFormatKind.Expression:
                    return Engine.GetParams(cf.Priority, cf.StopIfTrue, Engine.GetString(((TConditionalExpressionRule)cf).Formula));

                case TConditionalFormatKind.CellIs:
                    TConditionalCellIsRule cir = (TConditionalCellIsRule)cf;
                    return Engine.GetParams(cf.Priority, cf.StopIfTrue, cir.ConditionType, Engine.GetString(cir.Formula1), Engine.GetString(cir.Formula2));

                case TConditionalFormatKind.ColorScale:
                    return Engine.GetParams(cf.Priority, Engine.GetString(((TConditionalFormatValuesRule)cf).Formula));

                case TConditionalFormatKind.DataBar:
                    return Engine.GetParams(cf.Priority, Engine.GetString(((TConditionalFormatValuesRule)cf).Formula));

                case TConditionalFormatKind.IconSet:
                    return Engine.GetParams(cf.Priority, Engine.GetString(((TConditionalFormatValuesRule)cf).Formula));

                case TConditionalFormatKind.TopN:
                    TConditionalTopNRule tnr = (TConditionalTopNRule)cf;
                    return Engine.GetParams(cf.Priority, cf.StopIfTrue, tnr.Rank, tnr.IsBottom, tnr.IsPercent);

                case TConditionalFormatKind.UniqueValues:
                case TConditionalFormatKind.DuplicateValues:
                    return Engine.GetParams(cf.Priority, cf.StopIfTrue);

                case TConditionalFormatKind.ContainsText:
                case TConditionalFormatKind.NotContainsText:
                case TConditionalFormatKind.BeginsWith:
                case TConditionalFormatKind.EndsWith:
                    return Engine.GetParams(cf.Priority, cf.StopIfTrue, Engine.GetString(((TConditionalBaseTextRule)cf).Text));

                case TConditionalFormatKind.ContainsBlanks:
                case TConditionalFormatKind.NotContainsBlanks:
                case TConditionalFormatKind.ContainsErrors:
                case TConditionalFormatKind.NotContainsErrors:
                    return Engine.GetParams(cf.Priority, cf.StopIfTrue);

                case TConditionalFormatKind.TimePeriod:
                    return Engine.GetParams(cf.Priority, cf.StopIfTrue, ((TConditionalTimePeriodRule)cf).TimePeriod);

                case TConditionalFormatKind.AboveAverage:
                    TConditionalAboveAverageRule aar = (TConditionalAboveAverageRule)cf;
                    return Engine.GetParams(cf.Priority, cf.StopIfTrue, aar.IsBelowAverage, aar.IncludeAverage, aar.StdDev);

                default:
                    return "(Unknown rule)";
            }
        }

        private string GetCondFmtClass(TConditionalFormatKind kind)
        {
            switch (kind)
            {
                case TConditionalFormatKind.Expression:
                    return "TConditionalExpressionRule";
                case TConditionalFormatKind.CellIs:
                    return "TConditionalCellIsRule";
                case TConditionalFormatKind.ColorScale:
                    return "TConditionalColorScaleRule";
                case TConditionalFormatKind.DataBar:
                    return "TConditionalDataBarRule";
                case TConditionalFormatKind.IconSet:
                    return "TConditionalIconSetRule";
                case TConditionalFormatKind.TopN:
                    return "TConditionalTopNRule";
                case TConditionalFormatKind.UniqueValues:
                    return "TConditionalUniqueValuesRule";
                case TConditionalFormatKind.DuplicateValues:
                    return "TConditionalDuplicateValuesRule";
                case TConditionalFormatKind.ContainsText:
                    return "TConditionalContainsTextRule";
                case TConditionalFormatKind.NotContainsText:
                    return "TConditionalNotContainsTextRule";
                case TConditionalFormatKind.BeginsWith:
                    return "TConditionalBeginsWithTextRule";
                case TConditionalFormatKind.EndsWith:
                    return "TConditionalEndsWithTextRule";
                case TConditionalFormatKind.ContainsBlanks:
                    return "TConditionalContainsBlanksRule";
                case TConditionalFormatKind.NotContainsBlanks:
                    return "TConditionalNotContainsBlanksRule";
                case TConditionalFormatKind.ContainsErrors:
                    return "TConditionalContainsErrorsRule";
                case TConditionalFormatKind.NotContainsErrors:
                    return "TConditionalNotContainsErrorsRule";
                case TConditionalFormatKind.TimePeriod:
                    return "TConditionalTimePeriodRule";
                case TConditionalFormatKind.AboveAverage:
                    return "TConditionalAboveAverageRule";
                default:
                    return "UnknownRuleType";
            }
        }

        private string GetRuleVarName(TConditionalFormatKind kind)
        {
            switch (kind)
            {
                case TConditionalFormatKind.Expression:
                    return "ExpressionRule";
                case TConditionalFormatKind.CellIs:
                    return "CellIsRule";
                case TConditionalFormatKind.ColorScale:
                    return "ColorScaleRule";
                case TConditionalFormatKind.DataBar:
                    return "DataBarRule";
                case TConditionalFormatKind.IconSet:
                    return "IconSetRule";
                case TConditionalFormatKind.TopN:
                    return "TopNRule";
                case TConditionalFormatKind.UniqueValues:
                    return "UniqueValuesRule";
                case TConditionalFormatKind.DuplicateValues:
                    return "DuplicateValuesRule";
                case TConditionalFormatKind.ContainsText:
                    return "ContainsTextRule";
                case TConditionalFormatKind.NotContainsText:
                    return "NotContainsTextRule";
                case TConditionalFormatKind.BeginsWith:
                    return "BeginsWithRule";
                case TConditionalFormatKind.EndsWith:
                    return "EndsWithRule";
                case TConditionalFormatKind.ContainsBlanks:
                    return "ContainsBlanksRule";
                case TConditionalFormatKind.NotContainsBlanks:
                    return "NotContainsBlanksRule";
                case TConditionalFormatKind.ContainsErrors:
                    return "ContainsErrorsRule";
                case TConditionalFormatKind.NotContainsErrors:
                    return "NotContainsErrorsRule";
                case TConditionalFormatKind.TimePeriod:
                    return "TimePeriodRule";
                case TConditionalFormatKind.AboveAverage:
                    return "AboveAverageRule";
                default:
                    return "UnknownRule";
            };
        }

        #endregion


        #region Data Validation
        private void DoDataValidation(LanguageEngine Engine)
        {
            SectionComment = "Data Validation";
            bool FirstLine = true;
            ValidateOrClear(Engine, LastFile, CurrentFile, ref FirstLine, true);
            ValidateOrClear(Engine, CurrentFile, LastFile, ref FirstLine, false);

        }

        private bool EqualsRange(TXlsCellRange[] Range1, TXlsCellRange[] Range2)
        {
            if (Range1.Length != Range2.Length) return false;
            for (int i = 0; i < Range1.Length; i++)
            {
                if (Range1[i] != Range2[i]) return false;
            }
            return true;
        }

        private void ValidateOrClear(LanguageEngine Engine, ExcelFile Master, ExcelFile Other, ref bool FirstLine, bool Clear)
        {
            bool FirstValidation = true;

            for (int i = 1; i <= Master.DataValidationCount; i++)
            {
                TXlsCellRange[] CurrentRange = Master.GetDataValidationRanges(i);
                TDataValidationInfo CurrentInfo = Clear ? null : Master.GetDataValidationInfo(i);

                TXlsCellRange[] LastRange = null;
                TDataValidationInfo LastInfo = null;

                for (int k = 1; k <= Other.DataValidationCount; k++)
                {
                    TXlsCellRange[] LRange = Other.GetDataValidationRanges(k);
                    if (EqualsRange(LRange, CurrentRange))
                    {
                        LastRange = LRange;
                        if (!Clear) LastInfo = Other.GetDataValidationInfo(k);
                        break;
                    }
                }

                if (LastRange == null || LastInfo != CurrentInfo)
                {
                    WriteComment(Engine, ref FirstLine, false);
                    if (Clear)
                    {
                        foreach (TXlsCellRange Range in CurrentRange)
                        {
                            Engine.CallMethod("ClearDataValidation", Engine.GetRange(Range));
                        }
                    }
                    else
                    {
                        string dvVar = "Validation";
                        if (FirstValidation)
                        {
                            Engine.InsertVar(dvVar, "TDataValidationInfo");
                            FirstValidation = false;
                        }
                        Engine.CreateDataValidation(CurrentInfo, dvVar);
                        foreach (TXlsCellRange Range in CurrentRange)
                        {
                            Engine.CallMethod("AddDataValidation",
                                Engine.GetRange(Range),
                                dvVar);
                        }
                    }
                }
            }

        }

        #endregion

        #region What-if tables
        private void DoWhatIfTables(LanguageEngine Engine)
        {
            SectionComment = "What-if Tables. This is an alternative to setting the what-if tables directly with SetCellValue (as was done in this file)";
            bool FirstLine = true;
            Engine.CommentMode(true);

            TCellAddress[] NewAddr = CurrentFile.GetWhatIfTableList();

            //No need to delete old what-if tables. They will be deleted when you delete the range.

            bool FirstTable = true;
            bool FirstRowInputCell = true;
            bool FirstColInputCell = true;
            foreach (TCellAddress addr in NewAddr)
            {
                TCellAddress RowInputCell, ColInputCell;
                TXlsCellRange TableRange = CurrentFile.GetWhatIfTable(CurrentFile.ActiveSheet, addr.Row, addr.Col, out RowInputCell, out ColInputCell);

                TCellAddress LastRowInputCell, LastColInputCell;
                TXlsCellRange LastTableRange = LastFile.GetWhatIfTable(LastFile.ActiveSheet, addr.Row, addr.Col, out LastRowInputCell, out LastColInputCell);

                if (!TableRange.Equals(LastTableRange) || !SameCell(LastRowInputCell, RowInputCell) || !SameCell(LastColInputCell, ColInputCell))
                {
                    WriteComment(Engine, ref FirstLine, false);

                    string TableStr = Engine.GetRange(TableRange);

                    if (FirstTable)
                    {
                        Engine.InsertVar("TableRange", "TXlsCellRange", TableStr);
                        FirstTable = false;
                    }
                    else
                    {
                        Engine.WriteLine();
                        Engine.SetVarProp("TableRange", TableStr);
                    }

                    string RowInputStr = GetInputStr(Engine, "Row", RowInputCell, ref FirstRowInputCell);
                    string ColInputStr = GetInputStr(Engine, "Col", ColInputCell, ref FirstColInputCell);

                    Engine.CallMethod("SetWhatIfTable", "TableRange", RowInputStr, ColInputStr);
                }
            }

            Engine.CommentMode(false);
        }

        private static string GetInputStr(LanguageEngine Engine, string RowCol, TCellAddress InputCell, ref bool FirstInputCell)
        {
            if (InputCell == null) return Engine.GetString(null);

            string Result = RowCol + "InputCell";

            string Params = Engine.CreateObject("TCellAddress", Engine.GetParams(InputCell.Row, InputCell.Col));
            if (FirstInputCell)
            {
                Engine.InsertVar(Result, "TCellAddress", Params);
                FirstInputCell = false;
            }
            else
            {
                Engine.SetVarProp(Result, Params);
            }

            return Result;
        }

        private static bool SameCell(TCellAddress a1, TCellAddress a2)
        {
            if (a1 == null) return a2 == null;
            if (a2 == null) return false;

            return a1.CellRef == a2.CellRef;
        }

        #endregion

        #region HyperLinks
        private void DoHyperLinks(LanguageEngine Engine)
        {
            SectionComment = "Hyperlinks";
            bool FirstLine = true;
            AddOrRemoveHyperlink(Engine, LastFile, CurrentFile, ref FirstLine, true);
            AddOrRemoveHyperlink(Engine, CurrentFile, LastFile, ref FirstLine, false);

        }

        private void AddOrRemoveHyperlink(LanguageEngine Engine, ExcelFile Master, ExcelFile Other, ref bool FirstLine, bool Clear)
        {
            bool FirstHyperlink = true;

            for (int i = 1; i <= Master.HyperLinkCount; i++)
            {
                TXlsCellRange CurrentRange = Master.GetHyperLinkCellRange(i);
                THyperLink CurrentLink = Clear ? null : Master.GetHyperLink(i);

                TXlsCellRange LastRange = null;
                THyperLink LastLink = null;

                for (int k = 1; k <= Other.HyperLinkCount; k++)
                {
                    TXlsCellRange LRange = Other.GetHyperLinkCellRange(k);
                    if (LRange == CurrentRange)
                    {
                        LastRange = LRange;
                        if (!Clear) LastLink = Other.GetHyperLink(k);
                        break;
                    }
                }

                if (LastRange == null || IsDifferent(LastLink, CurrentLink))
                {
                    WriteComment(Engine, ref FirstLine, false);
                    if (Clear)
                    {
                        Engine.CallMethod("DeleteHyperLink", i);
                    }
                    else
                    {
                        string LinkVar = "Link";
                        if (FirstHyperlink)
                        {
                            Engine.InsertVar(LinkVar, "THyperLink");
                            FirstHyperlink = false;
                        }

                        Engine.CreateHyperLink(CurrentLink, LinkVar);
                        Engine.CallMethod("AddHyperLink",
                            Engine.GetRange(CurrentRange),
                            LinkVar);
                    }
                }
            }
        }

        private bool IsDifferent(THyperLink a, THyperLink b)
        {
            if (a == null) return b != null;
            if (b == null) return a != null;

            return
                a.LinkType != b.LinkType ||
                a.Description != b.Description ||
                a.TargetFrame != b.TargetFrame ||
                a.TextMark != b.TextMark ||
                a.Text != b.Text ||
                a.Hint != b.Hint;
        }

        #endregion

        #region Workbook Options
        private void DoWorkbookOptions(LanguageEngine Engine)
        {
            SectionComment = "Global Workbook Options";
            bool FirstLine = true;

            if (LastFile.OptionsDates1904 != CurrentFile.OptionsDates1904)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp("OptionsDates1904", Engine.GetBool(CurrentFile.OptionsDates1904));
            }

            if (LastFile.OptionsR1C1 != CurrentFile.OptionsR1C1)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp("OptionsR1C1", Engine.GetBool(CurrentFile.OptionsR1C1));
            }

            if (LastFile.OptionsSaveExternalLinkValues != CurrentFile.OptionsSaveExternalLinkValues)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp("OptionsSaveExternalLinkValues", Engine.GetBool(CurrentFile.OptionsSaveExternalLinkValues));
            }

            if (LastFile.OptionsPrecisionAsDisplayed != CurrentFile.OptionsPrecisionAsDisplayed)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp("OptionsPrecisionAsDisplayed", Engine.GetBool(CurrentFile.OptionsPrecisionAsDisplayed));
            }

            if (LastFile.OptionsAutoCompressPictures != CurrentFile.OptionsAutoCompressPictures)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp("OptionsAutoCompressPictures", Engine.GetBool(CurrentFile.OptionsAutoCompressPictures));
            }

            if (LastFile.OptionsBackup != CurrentFile.OptionsBackup)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp("OptionsBackup", Engine.GetBool(CurrentFile.OptionsBackup));
            }

            if (LastFile.OptionsCheckCompatibility != CurrentFile.OptionsCheckCompatibility)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp("OptionsCheckCompatibility", Engine.GetBool(CurrentFile.OptionsCheckCompatibility));
            }

            if (LastFile.OptionsForceFullRecalc != CurrentFile.OptionsForceFullRecalc)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp("OptionsForceFullRecalc", Engine.GetBool(CurrentFile.OptionsForceFullRecalc));
            }

            if (LastFile.OptionsRecalcCircularReferences != CurrentFile.OptionsRecalcCircularReferences)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp("OptionsRecalcCircularReferences", Engine.GetBool(CurrentFile.OptionsRecalcCircularReferences));
            }

            if (LastFile.OptionsRecalcMaxIterations != CurrentFile.OptionsRecalcMaxIterations)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp("OptionsRecalcMaxIterations", Engine.GetInt(CurrentFile.OptionsRecalcMaxIterations));
            }

            if (LastFile.OptionsRecalcMaxChange != CurrentFile.OptionsRecalcMaxChange)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp("OptionsRecalcMaxChange", Engine.GetDouble(CurrentFile.OptionsRecalcMaxChange));
            }

            if (LastFile.OptionsMultithreadRecalc != CurrentFile.OptionsMultithreadRecalc)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp("OptionsMultithreadRecalc", Engine.GetInt(CurrentFile.OptionsMultithreadRecalc));
            }

            if (LastFile.OptionsRecalcMode != CurrentFile.OptionsRecalcMode)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp("OptionsRecalcMode", Engine.GetEnum(CurrentFile.OptionsRecalcMode));
            }
        }
        #endregion

        #region Sheet Options

        private void StartSheetComment(LanguageEngine Engine, ref bool FirstComment)
        {
            if (!FirstComment) return;
            FirstComment = false;
            Engine.CommentMode(true);
        }

        private void EndSheetComment(LanguageEngine Engine, bool FirstComment)
        {
            if (FirstComment) return;
            Engine.CommentMode(false);
        }

        private void DoSheetOptions(LanguageEngine Engine)
        {
            SectionComment = "Sheet Options";
            bool FirstLine = true;
            bool FirstComment = true;

            if (CurrentFile.SheetName != LastFile.SheetName)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp("SheetName", Engine.GetString(CurrentFile.SheetName));
            }

            if (CurrentFile.SheetZoom != LastFile.SheetZoom)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp("SheetZoom", Engine.GetInt(CurrentFile.SheetZoom));
            }

            if (!Object.Equals(CurrentFile.SheetView, LastFile.SheetView))
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp("SheetView", Engine.CreateObject("TSheetView", 
                    Engine.GetParams(Engine.GetEnum(CurrentFile.SheetView.ViewType),
                               CurrentFile.SheetView.ShowWhitespace,
                               CurrentFile.SheetView.ShowRulers,
                               CurrentFile.SheetView.ZoomNormal,
                               CurrentFile.SheetView.ZoomPageLayout,
                               CurrentFile.SheetView.ZoomPageBreakPreview
                               )));
            }

            if (CurrentFile.SheetOptions != LastFile.SheetOptions)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.WriteLine();
                Engine.AddComment("There are 2 ways to set the sheet options. You can use the code above to set them all, or the commented code after it to set them one by one.", false);
                Engine.SetProp("SheetOptions", Engine.GetEnum(CurrentFile.SheetOptions));
            }

            if (CurrentFile.ShowFormulaText != LastFile.ShowFormulaText)
            {
                StartSheetComment(Engine, ref FirstComment);
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp("ShowFormulaText", Engine.GetBool(CurrentFile.ShowFormulaText));
            }

            if (CurrentFile.HideZeroValues != LastFile.HideZeroValues)
            {
                StartSheetComment(Engine, ref FirstComment);
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp("HideZeroValues", Engine.GetBool(CurrentFile.HideZeroValues));
            }

            if (CurrentFile.ShowGridLines != LastFile.ShowGridLines)
            {
                StartSheetComment(Engine, ref FirstComment);
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp("ShowGridLines", Engine.GetBool(CurrentFile.ShowGridLines));
            }

            if (CurrentFile.SheetIsRightToLeft != LastFile.SheetIsRightToLeft)
            {
                StartSheetComment(Engine, ref FirstComment);
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp("SheetIsRightToLeft", Engine.GetBool(CurrentFile.SheetIsRightToLeft));
            }

            EndSheetComment(Engine, FirstLine);

            TExcelColor CurrentColor = CurrentFile.GridLinesColor;
            TExcelColor LastColor = LastFile.GridLinesColor;

            if (CurrentColor != LastColor)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp("GridLinesColor", Engine.GetColor(CurrentFile.GridLinesColor, false));
            }

        }
        #endregion

        #region Global Sheet Options

        private void DoGlobalSheetOptions(LanguageEngine Engine)
        {
            SectionComment = "Global Sheet Options";
            bool FirstLine = true;

            if (CurrentFile.SheetWindowOptions != LastFile.SheetWindowOptions)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp("SheetWindowOptions", Engine.GetEnum(CurrentFile.SheetWindowOptions));
            }

        }
        #endregion

        #region Autofilter
        private void DoAutoFilter(LanguageEngine Engine)
        {
            SectionComment = "AutoFilter";
            bool FirstLine = true;

            TXlsCellRange CurrentRange = CurrentFile.GetAutoFilterRange();
            TXlsCellRange LastRange = LastFile.GetAutoFilterRange();

            if (LastRange != CurrentRange)
            {
                WriteComment(Engine, ref FirstLine, false);

                if (CurrentRange == null)
                {
                    Engine.CallMethod("RemoveAutoFilter", String.Empty);
                }
                else
                {
                    Engine.CallMethod("SetAutoFilter", Engine.GetInt(CurrentRange.Top), Engine.GetInt(CurrentRange.Left), Engine.GetInt(CurrentRange.Right));
                }
            }

        }
        #endregion

        #region Freeze Panes
        private void DoFreezePanes(LanguageEngine Engine)
        {
            SectionComment = "Freeze Panes";
            bool FirstLine = true;

            TCellAddress CurrentRange = CurrentFile.GetFrozenPanes();
            TCellAddress LastRange = LastFile.GetFrozenPanes();

            if (LastRange.CellRef != CurrentRange.CellRef)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.CallMethod("FreezePanes", CurrentRange);
            }

            TPoint CurrentSplit = CurrentFile.GetSplitWindow();
            TPoint LastSplit = LastFile.GetSplitWindow();

            if (LastSplit != CurrentSplit)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.CallMethod("SplitWindow", Engine.GetDouble(CurrentSplit.X), Engine.GetDouble(CurrentSplit.Y));
            }

        }
        #endregion

        #region Group and Outline
        private void DoGroupAndOutline(LanguageEngine Engine)
        {
            SectionComment = "Group and outline";

            bool FirstLine = true;
            int c = 1;
            int ColCount = CurrentFile.ColCount;
            while (c <= ColCount)
            {
                int outline = CurrentFile.GetColOutlineLevel(c);
                if (outline != LastFile.GetColOutlineLevel(c))
                {
                    int c1 = c + 1;
                    while (c1 <= ColCount)
                    {
                        if (CurrentFile.GetColOutlineLevel(c1) != outline) break;
                        c1++;
                    }

                    WriteComment(Engine, ref FirstLine, false);
                    if (c1 == c + 1)
                    {
                        Engine.CallMethod("SetColOutlineLevel", Engine.GetInt(c), Engine.GetInt(outline));
                    }
                    else
                    {
                        Engine.CallMethod("SetColOutlineLevel", Engine.GetInt(c), Engine.GetInt(c1 - 1), Engine.GetInt(outline));
                        c += c1 - 1 - c;
                    }
                }

                c++;
            }

            int r = 1;
            while (r <= CurrentFile.RowCount)
            {
                int outline = CurrentFile.GetRowOutlineLevel(r);
                if (outline != LastFile.GetRowOutlineLevel(r))
                {
                    int r1 = r + 1;
                    while (r1 <= CurrentFile.RowCount)
                    {
                        if (CurrentFile.GetRowOutlineLevel(r1) != outline) break;
                        r1++;
                    }

                    WriteComment(Engine, ref FirstLine, false);
                    if (r1 == r + 1)
                    {
                        Engine.CallMethod("SetRowOutlineLevel", Engine.GetInt(r), Engine.GetInt(outline));
                    }
                    else
                    {
                        Engine.CallMethod("SetRowOutlineLevel", Engine.GetInt(r), Engine.GetInt(r1 - 1), Engine.GetInt(outline));
                        r += r1 - 1 - r;
                    }
                }

                r++;
            }

            if (CurrentFile.OutlineSummaryColsRightToDetail != LastFile.OutlineSummaryColsRightToDetail)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp("OutlineSummaryColsRightToDetail", Engine.GetBool(CurrentFile.OutlineSummaryColsRightToDetail));
            }
            if (CurrentFile.OutlineSummaryRowsBelowDetail != LastFile.OutlineSummaryRowsBelowDetail)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp("OutlineSummaryRowsBelowDetail", Engine.GetBool(CurrentFile.OutlineSummaryRowsBelowDetail));
            }
            if (CurrentFile.OutlineAutomaticStyles != LastFile.OutlineAutomaticStyles)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp("OutlineAutomaticStyles", Engine.GetBool(CurrentFile.OutlineAutomaticStyles));
            }

        }
        #endregion

        #region Protection
        private void DoProtection(LanguageEngine Engine)
        {
            SectionComment = "Protection";
            bool FirstLine = true;

            TProtection CurrentProtection = CurrentFile.Protection;
            TProtection LastProtection = LastFile.Protection;

            if (CurrentProtection.EncryptionType != LastProtection.EncryptionType)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp(Engine.Call("Protection", "EncryptionType"), Engine.GetEnum(CurrentProtection.EncryptionType));
            }

            if (CurrentProtection.EncryptionAlgorithmXlsx != LastProtection.EncryptionAlgorithmXlsx)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp(Engine.Call("Protection", "EncryptionAlgorithmXlsx"), Engine.GetEnum(CurrentProtection.EncryptionAlgorithmXlsx));
            }

            if (CurrentProtection.HasModifyPassword != LastProtection.HasModifyPassword ||
                CurrentProtection.RecommendReadOnly != LastProtection.RecommendReadOnly)
            {
                WriteComment(Engine, ref FirstLine, false);
                string pass = CurrentProtection.HasModifyPassword ? "******" : null;
                Engine.CallMethod(Engine.Call("Protection", "SetModifyPassword"), Engine.GetString(pass),
                    Engine.GetBool(CurrentProtection.RecommendReadOnly), Engine.GetString(CurrentProtection.WriteAccess));
            }

            TSheetProtectionOptions CurrentSheetOp = CurrentProtection.GetSheetProtectionOptions();
            TSheetProtectionOptions LastSheetOp = LastProtection.GetSheetProtectionOptions();
            if (CurrentProtection.HasSheetPassword != LastProtection.HasSheetPassword || IsDifferent(LastSheetOp, CurrentSheetOp))
            {
                WriteComment(Engine, ref FirstLine, false);
                string pass = CurrentProtection.HasSheetPassword ? "******" : null;

                Engine.WriteLine();

                Engine.InsertVar("SheetProtectionOptions", "TSheetProtectionOptions");
                Engine.CreateSheetProtection("SheetProtectionOptions", CurrentSheetOp);
                Engine.CallMethod(Engine.Call("Protection", "SetSheetProtection"), Engine.GetString(pass), "SheetProtectionOptions");
            }

            TWorkbookProtectionOptions CurrentWorkOp = CurrentProtection.GetWorkbookProtectionOptions();
            TWorkbookProtectionOptions LastWorkOp = LastProtection.GetWorkbookProtectionOptions();
            if (CurrentProtection.HasWorkbookPassword != LastProtection.HasWorkbookPassword || IsDifferent(LastWorkOp, CurrentWorkOp))
            {
                WriteComment(Engine, ref FirstLine, false);
                string pass = CurrentProtection.HasWorkbookPassword ? "******" : null;

                Engine.WriteLine();

                Engine.InsertVar("WorkbookProtectionOptions", "TWorkbookProtectionOptions");
                Engine.CreateWorkbookProtection("WorkbookProtectionOptions", CurrentWorkOp);
                Engine.CallMethod(Engine.Call("Protection", "SetWorkbookProtection"), Engine.GetString(pass), "WorkbookProtectionOptions");
            }

            TSharedWorkbookProtectionOptions CurrentSharedWorkOp = CurrentProtection.GetSharedWorkbookProtectionOptions();
            TSharedWorkbookProtectionOptions LastSharedWorkOp = LastProtection.GetSharedWorkbookProtectionOptions();
            if (CurrentProtection.HasSharedWorkbookPassword != LastProtection.HasSharedWorkbookPassword || IsDifferent(LastSharedWorkOp, CurrentSharedWorkOp))
            {
                WriteComment(Engine, ref FirstLine, false);
                string pass = CurrentProtection.HasSharedWorkbookPassword ? "******" : null;

                Engine.WriteLine();

                Engine.InsertVar("SharedWorkbookProtectionOptions", "TSharedWorkbookProtectionOptions");
                Engine.CreateSharedWorkbookProtection("SharedWorkbookProtectionOptions", CurrentSharedWorkOp);
                Engine.CallMethod(Engine.Call("Protection", "SetSharedWorkbookProtection"), Engine.GetString(pass), "SharedWorkbookProtectionOptions");
            }

            /*we don't want writeaccess to appear each time.
            if (CurrentProtection.WriteAccess != LastProtection.WriteAccess) 
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp(Engine.Call("Protection" , "WriteAccess"), Engine.GetString(CurrentProtection.WriteAccess));
            }*/

            if (!IsProtectedRangeEqual(CurrentProtection, LastProtection))
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.WriteLine();
                if (LastProtection.ProtectedRangeCount > 0)
                {
                    Engine.CallMethod(Engine.Call("Protection", "ClearProtectedRanges"));
                }

                if (CurrentProtection.ProtectedRangeCount > 1)
                {
                    Engine.InsertVar("ProtCellRanges", Engine.GetArrayDecl("TXlsCellRange"));
                }
                for (int i = 1; i <= CurrentProtection.ProtectedRangeCount; i++)
                {
                    TProtectedRange prot = CurrentProtection.GetProtectedRange(i);

                    Engine.WriteLn(Engine.CreateObjectArray("ProtCellRanges", "TXlsCellRange", prot.Ranges.Length));
                    for (int z = 0; z < prot.Ranges.Length; z++)
                    {
                        Engine.SetVarProp(Engine.GetCallArray("ProtCellRanges", z), Engine.CreateObject("TXlsCellRange", Engine.GetString(prot.Ranges[z].CellRef)));
                    }

                    Engine.InsertVarAndTrack("ProtRange", "TProtectedRange", Engine.CreateObject("TProtectedRange",
                        Engine.GetParams(Engine.GetString(prot.Name), Engine.GetString("*****"), "ProtCellRanges")));

                    if (!String.IsNullOrEmpty(prot.SecurityDescriptorXLSX))
                    {
                        Engine.SetVarProp(Engine.Call("ProtRange", "SecurityDescriptorXLSX"),Engine.GetString(prot.SecurityDescriptorXLSX));
                    }

                     if (prot.SecurityDescriptorXLS != null && prot.SecurityDescriptorXLS.Length > 0)
                    {
                        Engine.DefineByteArray("SecurityDescriptorXLS", "byte", prot.SecurityDescriptorXLS);
                        Engine.SetVarProp(Engine.Call("ProtRange", "SecurityDescriptorXLS"), "SecurityDescriptorXLS");
                    }

                    Engine.CallMethod(Engine.Call("Protection", "AddProtectedRange"), "ProtRange");
                    Engine.WriteLine();
                }
            }

        }

        private bool IsProtectedRangeEqual(TProtection CurrentProtection, TProtection LastProtection)
        {
            if (CurrentProtection.ProtectedRangeCount != LastProtection.ProtectedRangeCount) return false;
            for (int i = 1; i <= CurrentProtection.ProtectedRangeCount; i++)
            {
                TProtectedRange rng = CurrentProtection.GetProtectedRange(i);
                if (!rng.Equals(LastProtection.GetProtectedRange(i))) return false;
            }
            return true;
        }

        private bool IsDifferent(TSheetProtectionOptions a, TSheetProtectionOptions b)
        {
            if (a == null) return b != null;
            if (b == null) return a != null;
            if (!a.Contents && !b.Contents) return false;

            return
                a.Contents != b.Contents ||
                a.Objects != b.Objects ||
                a.Scenarios != b.Scenarios ||

                a.CellFormatting != b.CellFormatting ||
                a.ColumnFormatting != b.ColumnFormatting ||
                a.RowFormatting != b.RowFormatting ||
                a.InsertColumns != b.InsertColumns ||
                a.InsertRows != b.InsertRows ||
                a.InsertHyperlinks != b.InsertHyperlinks ||
                a.DeleteColumns != b.DeleteColumns ||
                a.DeleteRows != b.DeleteRows ||
                a.SelectLockedCells != b.SelectLockedCells ||
                a.SortCellRange != b.SortCellRange ||
                a.EditAutoFilters != b.EditAutoFilters ||
                a.EditPivotTables != b.EditPivotTables ||
                a.SelectUnlockedCells != b.SelectUnlockedCells;
        }

        private bool IsDifferent(TWorkbookProtectionOptions a, TWorkbookProtectionOptions b)
        {
            if (a == null) return b != null;
            if (b == null) return a != null;

            return a.Structure != b.Structure || a.Window != b.Window;

        }

        private bool IsDifferent(TSharedWorkbookProtectionOptions a, TSharedWorkbookProtectionOptions b)
        {
            if (a == null) return b != null;
            if (b == null) return a != null;

            return a.SharingWithTrackChanges != b.SharingWithTrackChanges;

        }

        #endregion

        #region Styles
        private Dictionary<string, int> GetStyles(ExcelFile xls)
        {
            Dictionary<string, int> Styles = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 1; i <= xls.StyleCount; i++)
            {
                Styles[xls.GetStyleName(i)] = i;
            }
            return Styles;
        }

        private void DoStyles(LanguageEngine Engine)
        {
            SectionComment = "Styles.";

            //Make a hashtable for fast lookup
            Dictionary<string, int> CurrentStyles = GetStyles(CurrentFile);
            Dictionary<string, int> LastStyles = GetStyles(LastFile);
            Dictionary<string, int> DoneStyles = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            string[] Keys = new String[CurrentStyles.Keys.Count];
            CurrentStyles.Keys.CopyTo(Keys, 0);
            Array.Sort(Keys); 

            bool AddVar = true;
            bool FirstLine = true;
            foreach (string st in Keys)
            {
                ProcessStyle(Engine, LastStyles, DoneStyles, ref AddVar, ref FirstLine, st);
            }

            foreach (string st in LastStyles.Keys)
            {
                if (!CurrentStyles.ContainsKey(st))
                {
                    DeleteStyle(Engine, st, ref FirstLine);
                }
            }
        }

        private void ProcessStyle(LanguageEngine Engine, Dictionary<string, int> LastStyles, Dictionary<string, int> DoneStyles, ref bool AddVar, ref bool FirstLine, string st)
        {
            if (DoneStyles.ContainsKey(st)) return;
            DoneStyles.Add(st, 0);
            TFlxFormat CurrentStyle = CurrentFile.GetStyle(st);
            if (CurrentStyle != null) ProcessStyle(Engine, LastStyles, DoneStyles, ref AddVar, ref FirstLine, CurrentStyle.NotNullParentStyle);

            TBuiltInStyle stKind; int stLevel;
            int stPos = -1;
            if (CurrentFile.TryGetBuiltInStyleType(st, out stKind, out stLevel)) stPos = (int)stKind;

            if (stPos >= 0 || LastStyles.ContainsKey(st))
            {
                ModifyStyle(Engine, st, stPos, stLevel, ref AddVar, ref FirstLine);
            }
            else
            {
                AddStyle(Engine, st, stPos, stLevel, ref AddVar, ref FirstLine);
            }
        }

        private void ModifyStyle(LanguageEngine Engine, string name, int stylePos, int level, ref bool AddVar, ref bool FirstLine)
        {
            TFlxFormat LastStyle = LastFile.GetStyle(name);
            TFlxFormat CurrentStyle = CurrentFile.GetStyle(name);
            if (CurrentStyle == null) return; //invalid format anyway.

            TFlxApplyFormat Diffs = DiffFormats(LastStyle, CurrentStyle);
            if (Diffs.IsEmpty) return;

            LastFile.SetStyle(name, CurrentStyle); //modify lastfile so all changes here are reflected in the file.

            WriteComment(Engine, ref FirstLine, true);

            if (AddVar) Engine.InsertVar("StyleFmt", "TFlxFormat");
            AddVar = false;

            string DisplayName = Engine.GetStyleDisplayName(name, stylePos, level);
            Engine.SetVarProp("StyleFmt", Engine.GetCallMethod("GetStyle", DisplayName));
            Engine.SetFormat("StyleFmt", CurrentStyle, Diffs);
            Engine.CallMethod("SetStyle", DisplayName, "StyleFmt");
        }

        private void AddStyle(LanguageEngine Engine, string name, int stylePos, int level, ref bool AddVar, ref bool FirstLine)
        {
            TFlxFormat CurrentStyle = CurrentFile.GetStyle(name);
            if (CurrentStyle == null) return;


            WriteComment(Engine, ref FirstLine, true);
            if (AddVar) Engine.InsertVar("StyleFmt", "TFlxFormat");
            AddVar = false;

            TFlxFormat NormalStyle = CurrentFile.GetStyle(CurrentFile.GetBuiltInStyleName(TBuiltInStyle.Normal, 0));

            LastFile.SetStyle(name, CurrentStyle); //modify lastfile so all changes here are reflected in the file.

            TFlxApplyFormat Diffs = DiffFormats(NormalStyle, CurrentStyle);

            Engine.SetVarProp("StyleFmt", Engine.GetCallMethod("GetStyle", Engine.GetCallMethod("GetBuiltInStyleName", "TBuiltInStyle.Normal", "0")));
            Engine.SetFormat("StyleFmt", CurrentStyle, Diffs);

            string DisplayName = Engine.GetStyleDisplayName(name, stylePos, level);

            Engine.CallMethod("SetStyle", DisplayName, "StyleFmt");
        }

        private void DeleteStyle(LanguageEngine Engine, string name, ref bool FirstLine)
        {
            TBuiltInStyle st; int level;
            if (LastFile.TryGetBuiltInStyleType(name, out st, out level)) return; //internal styles can't be deleted.
            WriteComment(Engine, ref FirstLine, true);
            LastFile.DeleteStyle(name); //modify lastfile so all changes here are reflected in the file.

            Engine.CallMethod("DeleteStyle", Engine.GetString(name));
        }

        #endregion

        #region Normal Style
        private void DoNormalCellFormat(LanguageEngine Engine)
        {
            SectionComment = "Normal cell format. Applies to all unformatted cells in the sheet.";
            bool FirstLine = true;
            string fntStr = Engine.CallBase("DefaultFormatId");
            TFlxFormat fmt1 = LastFile.GetFormat(LastFile.DefaultFormatId);
            TFlxFormat fmt2 = CurrentFile.GetFormat(CurrentFile.DefaultFormatId);

            TFlxApplyFormat Diffs = DiffFormats(fmt1, fmt2);
            if (!Diffs.IsEmpty)
            {
                WriteComment(Engine, ref FirstLine, true);

                Engine.InsertVar("NormalFmt", "TFlxFormat");
                Engine.AddComment("This is the cell format used for all empty cells. It uses the \"normal\" style as parent style.", true);
                Engine.SetMainFormat("NormalFmt", fmt2, Diffs, fntStr);

                LastFile.SetFormat(LastFile.DefaultFormatId, fmt2); //Change the main format in lastfile so all changes will propagate to cells and rows.
            }
        }
        #endregion

        #region Cell Selection
        private bool IsDifferent(TXlsCellRange[] a, TXlsCellRange[] b)
        {
            if (a == null) return b != null;
            if (b == null) return true;

            if (a.Length != b.Length) return true;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] == null ^ b[i] == null) return true;
                if (a[i] != null)
                {
                    if (!a[i].Equals(b[i])) return true;
                }
            }
            return false;
        }


        private void DoCellSelection(LanguageEngine Engine)
        {
            SectionComment = "Cell selection and scroll position.";
            bool FirstLine = true;

            TXlsCellRange[] LastSelected = LastFile.GetSelectedCells();
            TXlsCellRange[] CurrentSelected = CurrentFile.GetSelectedCells();

            if (IsDifferent(LastSelected, CurrentSelected))
            {
                WriteComment(Engine, ref FirstLine, true);

                if (CurrentSelected != null)
                {
                    if (CurrentSelected.Length == 1)
                    {
                        Engine.CallMethod("SelectCell", Engine.GetInt(CurrentSelected[0].Top), Engine.GetInt(CurrentSelected[0].Left), Engine.GetBool(false));
                    }
                    else
                    {
                        Engine.InsertVar("Selection", Engine.GetArrayDecl("TXlsCellRange"));
                        Engine.WriteLn(Engine.CreateObjectArray("Selection", "TXlsCellRange", CurrentSelected.Length));
                        for (int i = 0; i < CurrentSelected.Length; i++)
                        {
                            TXlsCellRange Range = CurrentSelected[i];
                            Engine.SetVarProp(Engine.GetCallArray("Selection", i), Engine.GetRange(Range));
                        }

                        Engine.CallMethod("SelectCells", "Selection");
                    }
                }
            }

            ScrollWindow(Engine, TPanePosition.UpperLeft, ref FirstLine);
            if (CurrentFile.GetSplitWindow() != new TPoint(0, 0))
            {
                ScrollWindow(Engine, TPanePosition.UpperRight, ref FirstLine);
                ScrollWindow(Engine, TPanePosition.LowerLeft, ref FirstLine);
                ScrollWindow(Engine, TPanePosition.LowerRight, ref FirstLine);
            }
        }

        private void ScrollWindow(LanguageEngine Engine, TPanePosition PanePos, ref bool FirstLine)
        {
            TCellAddress CurrentAddress = CurrentFile.GetWindowScroll(PanePos);
            TCellAddress LastAddress = LastFile.GetWindowScroll(PanePos);
            if (CurrentAddress.Row != LastAddress.Row || CurrentAddress.Col != LastAddress.Col)
            {
                if (PanePos == TPanePosition.UpperLeft) //Call the simpler method.
                {
                    Engine.CallMethod("ScrollWindow", Engine.GetInt(CurrentAddress.Row), Engine.GetInt(CurrentAddress.Col));
                }
                else
                {
                    Engine.CallMethod("ScrollWindow", Engine.GetEnum(PanePos), Engine.GetInt(CurrentAddress.Row), Engine.GetInt(CurrentAddress.Col));
                }

            }
        }
        #endregion

        #region First sheet visible

        private void DoFirstSheetVisible(LanguageEngine Engine)
        {
            SectionComment = "First sheet visible in the sheet bar";
            bool FirstLine = true;

            if (CurrentFile.FirstSheetVisible != LastFile.FirstSheetVisible)
            {
                WriteComment(Engine, ref FirstLine, false);
                Engine.SetProp("FirstSheetVisible", CurrentFile.FirstSheetVisible);
            }

        }
        #endregion

    }

    class RbData
    {
        public string VarName;
        public TCellAddress Link;
        public bool Checked;

        public RbData(string aVarName, TCellAddress aLink)
        {
            VarName = aVarName;
            Link = aLink;
        }

        public RbData(string aVarName, bool aChecked)
        {
            VarName = aVarName;
            Checked = aChecked;
        }

    }
}

