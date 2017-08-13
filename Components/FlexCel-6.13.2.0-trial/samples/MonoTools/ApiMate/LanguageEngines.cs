using System;
using System.Text;
using System.Globalization;
using System.Reflection;

using FlexCel.Core;
using FlexCel.Render;
using System.Collections.Generic;
using System.IO;

namespace APIMate
{
    /// <summary>
    /// Base engine for a language.
    /// </summary>
    public abstract class LanguageEngine
    {
        public string XlsFileName;
        protected string Eol;
        protected string TrueStr;
        protected string FalseStr;
        protected string EqualStr;
        protected string NullString;
        protected string Indent;
        protected string Indent0;

        protected ExcelFile Xls;

        protected StringBuilder s;

        protected bool FShowComments;
        protected bool FCommentMode;

        private bool DefinedRtfRun;
        private bool DefinedRtfFont;

        private string WaitingComment;

        protected Dictionary<string, string> DefinedVars;

        protected LanguageEngine(string aXlsFileName, string aEol, string aTrueStr, string aFalseStr, string aEqualStr, string aNullString, string aIndent, ExcelFile aXls)
        {
            XlsFileName = aXlsFileName;
            Eol = aEol;
            TrueStr = aTrueStr;
            FalseStr = aFalseStr;
            EqualStr = aEqualStr;
            NullString = aNullString;
            Indent = aIndent;
            Indent0 = aIndent;
            Xls = aXls;
            ShowComments = true;
            FCommentMode = false;
            s = new StringBuilder();
            DefinedRtfRun = false;
            DefinedRtfFont = false;
            DefinedVars = new Dictionary<string,string>();
        }

        public void CommentMode(bool value)
        {
            FCommentMode = value;
        }


        public bool ShowComments {get {return FShowComments;} set{FShowComments = value;}}

        #region Writing

        protected void WriteLine(string s1)
        {
            if (FCommentMode)
            {
                if (ShowComments) s.Append(StartComment(String.Empty)); else return;
            }
            s.Append(s1);
            if (WaitingComment != null) s.Append(WaitingComment);
            WaitingComment = null;
            s.Append(Environment.NewLine);
        }

        public void WriteLine()
        {
            WriteLine(String.Empty);
        }

        public string CheckVar(string Name, string TextToDefine, string TextIfExists)
        {
            if (DefinedVars.ContainsKey(Name)) return TextIfExists;
            DefinedVars.Add(Name, "");
            return TextToDefine;
        }

        public void InsertVar(string Name, string VarType)
        {
            InsertVar(Name, VarType, null);
        }

        public abstract void InsertVar(string Name, string VarType, string Def);

        public abstract void InsertVarAndTrack(string Name, string VarType, string Def);

        public virtual void AddComment(string Comment, bool InNextLine)
        {
            if (!ShowComments) return;
            if (InNextLine) WaitingComment = StartComment(Comment);
            else 
            {
                s.Append(StartComment(Comment));
                s.Append(Environment.NewLine);
            }
        }

        public abstract string StartComment(string Comment);
    
        #endregion

        #region Formatting
        public abstract string GetReservedWord(string s);
        public virtual string Concat(string s1, string s2)
        {
            return s1 + " + " + s2;
        }

        public abstract string GetString(string s1);
        public abstract string GetChar(string s1);

        public virtual string GetBool(bool b)
        {
            if (b) return TrueStr; else return FalseStr;
        }

        public virtual string GetError(TFlxFormulaErrorValue err)
        {
            return GetEnum(err);
        }

        public virtual string GetEnum(object e)
        {
            Type ty = e.GetType();
            string t = ty.ToString();
            int dot = t.LastIndexOf(".");
            if (dot > 0) t = t.Substring(dot + 1);

            foreach (object ob in ty.GetCustomAttributes(false))
            {
                FlagsAttribute flag = ob as FlagsAttribute;
                if (flag != null) return GetFlagEnum(t, e);
            }

            string EName = Enum.GetName(e.GetType(), e);
            if (EName == null || EName.Length == 0)
            {
                return Cast(t, Convert.ToInt32(e));
            }
            return t + "." + EName;
        }

        public abstract string Cast(string aType, object value);

        public virtual string GetFlagEnum(string t, object e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (int i in Enum.GetValues(e.GetType()))
            {
                if (((int)e & i) != 0) 
                {
                    if (sb.Length > 0) sb.Append(" " + GetBinaryOr + " ");
                    sb.Append(t + "." +Enum.GetName(e.GetType(), i));
                }
            }

            if (sb.Length == 0)  //return none.
            {
                string s = Enum.GetName(e.GetType(), 0);
                if (s != null && s.Length > 0) return t + "." + s;
                return Cast(t, 0);
            }
            return sb.ToString();
        }

        public abstract string GetBinaryOr{get;}
        public abstract string Hexa(byte b);

        public virtual string GetAndWriteRichString(TRichString rs, int r, int c, out string Comment)
        {
            if (r > 0)
            {
                string html = Xls.GetHtmlFromCell(r, c, THtmlVersion.Html_401, THtmlStyle.Simple, Encoding.UTF8);
                Comment = "We could also have used: " + XlsFileName + ".SetCellFromHtml(" + r.ToString(CultureInfo.InvariantCulture) + ", " + c.ToString(CultureInfo.InvariantCulture) +
                    ", " + GetString(html)  + ")";
            }
            else
            {
                Comment = null;
            }

            string RichParams = String.Empty;
            
            if (rs.RTFRunCount > 0) 
            {
                RichParams = ", Runs, " + XlsFileName;

                WriteLine();
                if (!DefinedRtfRun)
                {
                    InsertVar("Runs", GetArrayDecl("TRTFRun"));
                    DefinedRtfRun = true;
                }

                WriteLine(Indent + CreateObjectArray("Runs", "TRTFRun", rs.RTFRunCount) + Eol);

                for (int i = 0; i < rs.RTFRunCount; i++)
                {
                    TRTFRun run = rs.RTFRun(i);
                    WriteLine(Indent + "Runs" + GetCallArray(i) + ".FirstChar " + EqualStr + " " + run.FirstChar.ToString(CultureInfo.InvariantCulture) + Eol);

                    string fnt = "fnt";
                    if (!DefinedRtfFont) 
                    {
                        InsertVar(fnt, "TFlxFont");
                        DefinedRtfFont = true;
                    }

                    SetVarProp(fnt, Call(XlsFileName, "GetDefaultFont"));

                    TFlxApplyFont ApplyFont = new TFlxApplyFont();
                    TFlxFont RichFont = Xls.GetFont(run.FontIndex);
                    TFlxFont EmptyFont = Xls.GetDefaultFont;
                    ApplyFont.CharSet = EmptyFont.CharSet != RichFont.CharSet;

                    ApplyFont.Color = EmptyFont.Color != RichFont.Color;
                    ApplyFont.Family = EmptyFont.Family != RichFont.Family;
                    ApplyFont.Name = EmptyFont.Name != RichFont.Name;
                    ApplyFont.Size20 = EmptyFont.Size20 != RichFont.Size20;
                    ApplyFont.Style = EmptyFont.Style != RichFont.Style;
                    ApplyFont.Underline = EmptyFont.Underline != RichFont.Underline;
                    ApplyFont.Scheme = EmptyFont.Scheme != RichFont.Scheme;
                    SetFont(fnt, RichFont, ApplyFont);
                    
                    WriteLine(Indent + "Runs" + GetCallArray(i) + ".FontIndex " + EqualStr + " " + 
                        GetCallMethod("AddFont", fnt) + Eol);
                }
            }
            return CreateObject("TRichString", GetString(rs.Value) + RichParams);
        }

        public virtual string GetDouble(double b)
        {
            return b.ToString(CultureInfo.InvariantCulture);
        }

        public virtual string GetInt(int b)
        {
            return b.ToString(CultureInfo.InvariantCulture);
        }

        public virtual string GetLong(long b)
        {
            return b.ToString(CultureInfo.InvariantCulture);
        }

        public virtual string GetDateTime(DateTime dt, bool HasDate, bool HasTime)
        {
            return String.Format(CultureInfo.InvariantCulture, "FlxDateTime.ToOADate({0}, {1})" , CreateDateTime(dt, HasDate, HasTime), XlsFileName + ".OptionsDates1904");
        }

        public abstract string CreateObject(string ObjectName, string Params);
        
        public virtual void WriteLn(string s)
        {
            WriteLine(Indent + s + Eol);
        }

        public virtual string CreateObjectArray(string ObjectName, string ArrayType, int ArrayLen)
        {
            return CreateObjectArray(ObjectName, ArrayType, ArrayLen.ToString(CultureInfo.InvariantCulture));
        }

        public abstract string CreateObjectArray(string ObjectName, string ArrayType, string ArrayLen);
        public abstract string GetArrayDecl(string ArrType);
        public abstract void DefineArray(bool DefineVar, string Name, string ArrType, int BreakAt, int DataLength, Func<int, string> DataGenerator);

        public void DefineByteArray(string Name, string ArrType, byte[] Data)
        {
            int DataLength = Data == null ? 0 : Data.Length;
            DefineArray(true, Name, ArrType, 50, DataLength, delegate (int i) { return Hexa(Data[i]); });
        }
        public abstract string GetCallArray(int index);

        public virtual string GetCallArray(string obj, int index)
        {
            return obj + GetCallArray(index);
        }


        public virtual string CreateDateTime(DateTime dt, bool HasDate, bool HasTime)
        {
            if (HasDate)
            {
                if (HasTime)
                {
                    return CreateObject("DateTime", string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}, {3}, {4}, {5}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second));
                }

                return CreateObject("DateTime", string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}", dt.Year, dt.Month, dt.Day));
            }

            return CreateObject("DateTime", string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}, {3}, {4}, {5}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second));
        }

        public virtual string CreateFormula(string FmlaText, TFormulaSpan Span)
        {
            if (Span.IsOneCell)
            {
                return CreateObject("TFormula", GetString(FmlaText));
            }

            return CreateObject("TFormula", GetParams(GetString(FmlaText), NullString, CreateObject("TFormulaSpan", GetParams(Span.RowSpan, Span.ColSpan, Span.IsTopLeft))));
        }

        public string GetColor(TExcelColor aColor, bool DefaultWhite)
        {
            string TintStr = aColor.Tint == 0 ? String.Empty : ", " + aColor.Tint.ToString(CultureInfo.InvariantCulture);
            switch (aColor.ColorType)
            {
                case TColorType.Automatic:
                    return "TExcelColor.Automatic";

                case TColorType.Theme:
                    return "TExcelColor.FromTheme(TThemeColor." + Enum.GetName(typeof(TThemeColor), aColor.Theme) + TintStr + ")";
            }

            if (aColor.Tint == 0 || aColor.ColorType != TColorType.RGB)
            {
                TUIColor def = DefaultWhite ? Colors.White : Colors.Black;
                TUIColor c = aColor.ToColor(Xls, def);
                /*if (c.IsNamedColor)
                {
                    return "UIColors." + c.Name;
                }*/

                return "TUIColor.FromArgb(" + Hexa(c.R) + ", " + Hexa(c.G) + ", " + Hexa(c.B) + ")";
            }

            long rgb = aColor.RGB;
            unchecked
            {
            return "TExcelColor.FromArgb(" + Hexa((byte)(rgb >> 16)) + ", " + Hexa((byte)(rgb >> 8)) + ", " + Hexa((byte)rgb) + TintStr + ")";
            }
        }

        public string GetDrawingColor(TDrawingColor aColor)
        {
            switch (aColor.ColorType)
            {
                case TDrawingColorType.HSL:
                    return "TDrawingColor.FromHSL(" + GetHSL(aColor.HSL) + ")";

                case TDrawingColorType.Preset:
                    return "TDrawingColor.FromPreset(" + GetEnum(aColor.Preset) + ")";

                case TDrawingColorType.RGB:
                    long rgb = aColor.RGB;
                    unchecked
                    {
                        return "TUIColor.FromArgb(" + Hexa((byte)(rgb >> 16)) + ", " + Hexa((byte)(rgb >> 8)) + ", " + Hexa((byte)rgb) + ")";
                    }
                    
                case TDrawingColorType.scRGB:
                    return "TDrawingColor.FromScRgb(" + GetScRgb(aColor.ScRGB) + ")";


                case TDrawingColorType.System:
                    return "TDrawingColor.FromSystem(" + GetEnum(aColor.System) + ")";
                                        
                case TDrawingColorType.Theme:
                    return "TDrawingColor.FromTheme(" + GetEnum(aColor.Theme) + ")";

                default:
                    return "Unknown color";
            }
        }

        private string GetHSL(THSLColor aHSLColor)
        {
            return CreateObject("THSLColor", GetParams(aHSLColor.Hue, aHSLColor.Sat, aHSLColor.Lum));
        }

        private string GetScRgb(TScRGBColor aScRGBColor)
        {
            return CreateObject("TScRGBColor", GetParams(aScRGBColor.ScR, aScRGBColor.ScG, aScRGBColor.ScB));
        }


        private string WriteGradientStops(TExcelGradient aGradient)
        {
            string Result = "GradientStops";
            CreateAndDefineObjectArray(Result, "TGradientStop", aGradient.Stops == null? -1: aGradient.Stops.Length);
         
            if (aGradient.Stops == null) return Result;

            for (int i = 0; i < aGradient.Stops.Length; i++)
            {
                TGradientStop gs = aGradient.Stops[i];
                WriteLine(Indent + Result + GetCallArray(i) + ".Position " + EqualStr + " " + gs.Position.ToString(CultureInfo.InvariantCulture) + Eol);
                WriteLine(Indent + Result + GetCallArray(i) + ".Color " + EqualStr + " " + GetColor(gs.Color, true) + Eol);
            }

            return Result;
        }

        protected virtual void CreateAndDefineObjectArray(string VarName, string VarType, int ArrayLen)
        {
            string ObjDef = ArrayLen < 0 ?
                            NullString :
                            CreateObjectArray(null, VarType, ArrayLen);

            InsertVarAndTrack(VarName, GetArrayDecl(VarType), ObjDef);

        }

        private string GetGradient(string Stops, TExcelGradient aGradient)
        {
            if (aGradient == null) return NullString;

            switch (aGradient.GradientType)
            {
                case TGradientType.Linear:
                    return CreateObject("TExcelLinearGradient", GetParams(Stops, ((TExcelLinearGradient)aGradient).RotationAngle));

                case TGradientType.Rectangular:
                    TExcelRectangularGradient rg = (TExcelRectangularGradient)aGradient;
                    return CreateObject("TExcelRectangularGradient", GetParams(Stops, rg.Top, rg.Left, rg.Bottom, rg.Right));
     
                default:
                    return "Error creating Gradient";
            }
        }


        public virtual string Call(string ClassDef, string Method)
        {
            return ClassDef + "." + Method;
        }

        public virtual string CallBase(string Method)
        {
            return XlsFileName + "." + Method;
        }

        public virtual string GetCallMethod(params object[] args)
        {
            return GetCallVarMethod(XlsFileName + ".", args);
        }

        public abstract string GetCallVarMethod(string Prefix, params object[] args);

        private bool IsEmpty(params object[] args)
        {
            if (args.Length < 2) return true;
            if (args.Length == 2 && (args[1] == null || args[1] .ToString().Length == 0)) return true;
            return false;
        }

        public virtual string GetCallVarMethodInternal(string Prefix, bool ParentsIfEmpty, params object[] args)
        {
            string parent1 = !IsEmpty(args) || ParentsIfEmpty? "(": "";
            string parent2 = !IsEmpty(args) || ParentsIfEmpty? ")": "";

            String fmt = "{0}" + parent1;
            for (int i = 1; i < args.Length - 1; i++)
            {
                fmt += "{" + i.ToString(CultureInfo.InvariantCulture) + "}, ";
            }

            if (args.Length > 1) fmt += "{" + (args.Length - 1).ToString(CultureInfo.InvariantCulture) + "}";
            fmt += parent2;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] is bool) args[i] = GetBool((bool)args[i]);
                if (args[i] == null) args[i] = NullString;
                TCellAddress ca = args[i] as TCellAddress;
                if (ca != null) args[i] = CreateObject("TCellAddress", GetString(ca.CellRef));

                TCellAddressRange car = args[i] as TCellAddressRange;
                if (car != null) args[i] = CreateObject("TCellAddressRange",
                    GetParams(
                    CreateObject("TCellAddress", GetString(car.TopLeft.CellRef)), 
                    CreateObject("TCellAddress", GetString(car.BottomRight.CellRef)))
                    );
            }
            return( Prefix + String.Format(CultureInfo.InvariantCulture, fmt, args));
        }

        public virtual string GetParams(params object[] args)
        {
            String fmt = String.Empty;
            string Sep = String.Empty;
            for (int i = 0; i < args.Length; i++)
            {
                fmt += Sep  + "{" + i.ToString(CultureInfo.InvariantCulture) + "}";
                if (args[i] is bool) args[i] = GetBool((bool)args[i]);
                if (args[i] is Enum) args[i] = GetEnum(args[i]);
                Sep = ", ";
            }

            return(String.Format(CultureInfo.InvariantCulture, fmt, args));
        }

        public virtual void CallMethod(params object[] args)
        {
            WriteLine(Indent + GetCallMethod(args) + Eol);
        }

        public virtual void CallVarMethod(string ParentClass, params object[] args)
        {
            string dot = ParentClass.Length == 0? String.Empty: ".";
            WriteLine(Indent + GetCallVarMethod(ParentClass + dot, args) + Eol);
        }

        public virtual void SetProp(string Name, object value)
        {
            SetVarProp(Call(XlsFileName, Name), value);
        }

        public void SetVarProp(string Name, object value)
        {
            string val = Convert.ToString(value, CultureInfo.InvariantCulture);
            if (value == null) val = NullString;
            if (value is Enum) val = GetEnum(value);
            if (value is bool) val = GetBool((bool)value);

            WriteLine(Indent + Name + " " + EqualStr + " " + val + Eol);
        }

        public string GetSetVarProp(string Name, object value)
        {
            string val = Convert.ToString(value, CultureInfo.InvariantCulture);
            if (value == null) val = NullString;
            return Name + " " + EqualStr + " " + val + Eol;
        }

        public string GetExt(TXlsImgType ImgType)
        {
            switch (ImgType)
            {
                case TXlsImgType.Bmp: return ".bmp";
                case TXlsImgType.Emf: return ".emf";
                case TXlsImgType.Jpeg:return ".jpg";
                case TXlsImgType.Wmf: return ".wmf";
                case TXlsImgType.Tiff: return ".tif";
                case TXlsImgType.Gif: return ".gif";
                default: return ".png";                        
            }

        }

        public abstract void StartUsing(string ClassName, string VarName, string VarValue);
        public abstract void EndUsing(string VarName, bool Close);
        #endregion


        #region Excel Methods
        public abstract void Start(bool ShowUsing, bool SaveXls, bool SaveCsv, bool SavePxl, bool SavePdf, bool SaveHtml, bool SaveBmp, bool AspNetCode);
        public abstract void Finish();

        private string GetStreamType(bool AspNetCode)
        {
            if (AspNetCode) return "MemoryStream";
            return "FileStream";
        }

        private string GetStreamParams(bool AspNetCode)
        {
            if (AspNetCode) return String.Empty;
            return GetParams("openFileDialog1.FileName", "FileMode.Create");

        }

        public virtual void SaveTheFile(bool SaveXls, bool SaveCsv, bool SavePxl, bool SavePdf, bool SaveHtml, bool SaveBmp, bool AspNetCode)
        {
            if(SaveXls)
            {
                WriteLine();
                AddComment("Save the file as XLS", false);
                if (AspNetCode)
                {
                    string fsVar = "XlsStream";
                    StartUsing(GetStreamType(AspNetCode), fsVar, CreateObject(GetStreamType(AspNetCode), GetStreamParams(AspNetCode)));
                    CallMethod("Save", fsVar);
                    WriteLine();
                    CallVarMethod(String.Empty, "SendToBrowser", fsVar, GetString("application/excel"), GetString("test.xls"));
                    EndUsing(fsVar, true);
                }
                else
                {
                    CallMethod("Save", "openFileDialog1.FileName");
                }
            }

            if(SaveCsv)
            {
                WriteLine();
                AddComment("Save the file as CSV (Comma separated values)", false);
                if (AspNetCode)
                {
                    string fsVar = "XlsStream";
                    StartUsing(GetStreamType(AspNetCode), fsVar, CreateObject(GetStreamType(AspNetCode), GetStreamParams(AspNetCode)));
                    CallMethod("Save", fsVar, "TFileFormats.Text", GetChar(","), "Encoding.Unicode");
                    WriteLine();
                    CallVarMethod(String.Empty, "SendToBrowser", fsVar, GetString("text/csv"), GetString("test.csv"));
                    EndUsing(fsVar, true);
                }
                else
                {
                    CallMethod("Save", "openFileDialog1.FileName", "TFileFormats.Text", GetChar(","), "Encoding.Unicode");
                }
            }

            if(SavePxl)
            {
                WriteLine();
                AddComment("Save the file as Pxl  (Pocket Excel)", false);
                if (AspNetCode)
                {
                    string fsVar = "XlsStream";
                    StartUsing(GetStreamType(AspNetCode), fsVar, CreateObject(GetStreamType(AspNetCode), GetStreamParams(AspNetCode)));
                    CallMethod("Save", fsVar, "TFileFormats.Pxl");
                    WriteLine();
                    CallVarMethod(String.Empty, "SendToBrowser", fsVar, GetString("application/excel"), GetString("test.pxl"));
                    EndUsing(fsVar, true);
                }
                else
                {
                    CallMethod("Save", "openFileDialog1.FileName", "TFileFormats.Pxl");
                }
            }

            if(SavePdf)
            {
                string fsVar = "PdfStream";

                WriteLine();
                AddComment("Save the file as Pdf", false);
                StartUsing("FlexCelPdfExport", "Pdf", CreateObject("FlexCelPdfExport", GetParams(XlsFileName, GetBool(true))));
                StartUsing(GetStreamType(AspNetCode), fsVar, CreateObject(GetStreamType(AspNetCode), GetStreamParams(AspNetCode)));
                CallVarMethod("Pdf", "BeginExport", fsVar);
                SetVarProp(Call("Pdf", "PageLayout"), "TPageLayout.Outlines");
                CallVarMethod("Pdf", "ExportAllVisibleSheets", GetBool(false), GetString("My Pdf File"));
                CallVarMethod("Pdf", "EndExport", string.Empty);
                if (AspNetCode)
                {
                    WriteLine();
                    CallVarMethod(String.Empty, "SendToBrowser", fsVar, GetString(StandardMimeType.Pdf), GetString("test.pdf"));
                }
                EndUsing(fsVar, true);
                EndUsing("Pdf", false);
            }

            if(SaveHtml)
            {
                WriteLine();
                AddComment("Save the file as HTML", false);
                StartUsing("FlexCelHtmlExport", "Html", CreateObject("FlexCelHtmlExport", GetParams(XlsFileName, GetBool(true))));
                if (AspNetCode)
                {
                    AddComment("In order to stream an HTML file to a browser you need to stream the images too.", false);
                    AddComment("This code will stream only the text, you need to stream the images yourself.", false);
                    AddComment("You might also want to use the FlexCelAspViewer component instead.", false);
                    CallVarMethod("Response", "Clear", string.Empty);
                    CallVarMethod("Html", "Export", Call("Response", "Output"), NullString, NullString);
                    CallVarMethod("Response", GetReservedWord("End"), String.Empty);
                }
                else
                {
                    CallVarMethod("Html", "Export", "openFileDialog1.FileName", GetString("images"));
                }
                EndUsing("Html", false);
            }

            if(SaveBmp)
            {
                string fsVar = "ImgStream";
                WriteLine();
                AddComment("Save the file as Images", false);
                AddComment("In this example we will save as Multipage Tiff. For more formats, take a look at the \"Print, Preview and Export\" demo", false);
                StartUsing("FlexCelImgExport", "ImgExport", CreateObject("FlexCelImgExport", GetParams(XlsFileName, GetBool(true))));
                
                SetVarProp(Call("ImgExport", "AllVisibleSheets"), GetBool(true));
                SetVarProp(Call("ImgExport", "ResetPageNumberOnEachSheet"), GetBool(false));
                SetVarProp(Call("ImgExport", "Resolution"), GetInt(96));

                StartUsing(GetStreamType(AspNetCode), fsVar, CreateObject(GetStreamType(AspNetCode), GetStreamParams(AspNetCode)));
                CallVarMethod("ImgExport", "SaveAsImage", fsVar, GetEnum(ImageExportType.Tiff), GetEnum(ImageColorDepth.Color256));
                if (AspNetCode)
                {
                    WriteLine();
                    CallVarMethod(String.Empty, "SendToBrowser", fsVar, GetString(StandardMimeType.Tiff), GetString("test.tif"));
                }
                EndUsing(fsVar, true);
                EndUsing("ImgExport", false);
            }

        }

        protected virtual string GetLengthArray(string ArrayName)
        {
            return ArrayName + ".Length";
        }

        public virtual void AddSendToBrowser()
        {
            DefinedVars.Clear();
            CallVarMethod("Response", "Clear", string.Empty);
            CallVarMethod("Response", "AddHeader", GetString("Content-Disposition"), Concat(GetString("attachment; filename="), "FileName"));
            InsertVar("MemData", GetArrayDecl("byte"), GetCallVarMethod(Call("OutStream", string.Empty), "ToArray", string.Empty));
            CallVarMethod("Response", "AddHeader", GetString("Content-Length"), GetCallVarMethod(Call("Convert", String.Empty), "ToString", GetLengthArray("MemData"), "CultureInfo.InvariantCulture" ));
            SetVarProp("Response.ContentType", "MimeType");
            CallVarMethod("Response", "BinaryWrite", "MemData");
            CallVarMethod("Response", GetReservedWord("End"), String.Empty);
        }

        public virtual void AddSheets(int SheetCount, TExcelFileFormat ExcelFileFormat)
        {
            WriteLine(Indent + XlsFileName + "." + "NewFile(" + SheetCount.ToString() + ", " + GetEnum(ExcelFileFormat) + ")" + Eol);
        }

        public void NameSheets()
        {
            int SaveActiveSheet = Xls.ActiveSheet;
            try
            {
                for	(int i = 1; i <= Xls.SheetCount; i++)
                {
                    Xls.ActiveSheet = i;
                    SetProp("ActiveSheet", i);
                    SetProp("SheetName", GetString(Xls.SheetName));
                    //if (Xls.SheetCodeName != null && Xls.SheetCodeName.Length > 0) SetProp("SheetCodeName", GetString(Xls.SheetCodeName));
                }
            }
            finally
            {
                Xls.ActiveSheet = SaveActiveSheet;
            }
        }


        public string GetStyleDisplayName(string name, int stylePos, int level)
        {
            if (Enum.IsDefined(typeof(TBuiltInStyle), stylePos))
            {
                return GetCallMethod("GetBuiltInStyleName", "TBuiltInStyle." + Enum.GetName(typeof(TBuiltInStyle), stylePos), level.ToString(CultureInfo.InvariantCulture));
            }
            return GetString(name);
        }


        public virtual void SetCellFormat(string VarName, int row, int col, TFlxFormat fmt, TFlxApplyFormat Apply, bool FormatFromStyle)
        {
            if (FormatFromStyle)
            {
                WriteLine(Indent + VarName + " " + EqualStr + " " + XlsFileName + ".GetStyle(" +
                    GetStyleName(fmt) + ", " + TrueStr + ")" + Eol);
            }
            else
            {
                WriteLine(Indent + VarName + " " + EqualStr + " " + XlsFileName + ".GetCellVisibleFormatDef(" +
                    row.ToString(CultureInfo.InvariantCulture) + ", " + col.ToString(CultureInfo.InvariantCulture) + ")" + Eol);
            }

            SetFormat(VarName, fmt, Apply);

            WriteLine(Indent + XlsFileName + ".SetCellFormat("+ 
                row.ToString(CultureInfo.InvariantCulture) + ", " + col.ToString(CultureInfo.InvariantCulture) + ", " + XlsFileName + ".AddFormat(" + VarName + "))" + Eol);
        }

        public virtual void SetRowColFormat(string id, string VarName, int rc, int rcLast, TFlxFormat fmt, TFlxApplyFormat Apply)
        {
            WriteLine(Indent + VarName + " " + EqualStr + " " + XlsFileName + ".GetFormat("+ XlsFileName + ".Get" + id + "Format("+
                rc.ToString(CultureInfo.InvariantCulture) + "))" + Eol);

            SetFormat(VarName, fmt, Apply);

            string rcLastStr = (rcLast < 0) ? "" : rcLast.ToString(CultureInfo.InvariantCulture) + ", ";

            WriteLine(Indent + XlsFileName + ".Set" + id + "Format("+ 
                rc.ToString(CultureInfo.InvariantCulture) + ", " 
                + rcLastStr 
                + XlsFileName + ".AddFormat(" + VarName + "))" + Eol);
        }

        public virtual void SetMainFormat(string VarName, TFlxFormat fmt, TFlxApplyFormat Apply, string fntId)
        {
            WriteLine(Indent + VarName + " " + EqualStr + " " + XlsFileName + ".GetFormat(" + fntId + ")"+ Eol);

            SetFormat(VarName, fmt, Apply);

            WriteLine(Indent + XlsFileName + ".SetFormat(" + fntId +", "+ 
                VarName + ")" + Eol);
        }


        public virtual void SetFont(string VarName, TFlxFont fmt, TFlxApplyFont Apply)
        {
            if (Apply.Name) WriteLine(Indent + VarName + ".Name " + EqualStr + " " + GetString(fmt.Name) + Eol);
            if (Apply.Size20) WriteLine(Indent + VarName + ".Size20 " + EqualStr + " " + fmt.Size20 + Eol);
            if (Apply.Color) WriteLine(Indent + VarName + ".Color " + EqualStr + " " + GetColor(fmt.Color, false) + Eol);
            if (Apply.Style) WriteLine(Indent + VarName + ".Style " + EqualStr + " " + GetEnum(fmt.Style) + Eol);
            if (Apply.Underline) WriteLine(Indent + VarName + ".Underline " + EqualStr + " " + GetEnum(fmt.Underline) + Eol);
            if (Apply.Family) WriteLine(Indent + VarName + ".Family " + EqualStr + " " + fmt.Family + Eol);
            if (Apply.CharSet) WriteLine(Indent + VarName + ".CharSet " + EqualStr + " " + fmt.CharSet + Eol);
            if (Apply.Scheme) WriteLine(Indent + VarName + ".Scheme " + EqualStr + " " + GetEnum(fmt.Scheme) + Eol);
        }

        public string GetOneBorder(TFlxOneBorder border)
        {
            return CreateObject("TFlxOneBorder", GetParams(border.Style, GetColor(border.Color, false)));
        }

        public virtual void SetFormat(string VarName, TFlxFormat fmt, TFlxApplyFormat Apply)
        {
            //Font
            SetFont(VarName + ".Font", fmt.Font, Apply.Font);

            //Borders
            if (Apply.Borders.Left) 
            {
                WriteLine(Indent + VarName + ".Borders.Left.Style " + EqualStr + " " + GetEnum(fmt.Borders.Left.Style) + Eol);
                WriteLine(Indent + VarName + ".Borders.Left.Color " + EqualStr + " " + GetColor(fmt.Borders.Left.Color, false) + Eol);
            }
            if (Apply.Borders.Right) 
            {
                WriteLine(Indent + VarName + ".Borders.Right.Style " + EqualStr + " " + GetEnum(fmt.Borders.Right.Style) + Eol);
                WriteLine(Indent + VarName + ".Borders.Right.Color " + EqualStr + " " + GetColor(fmt.Borders.Right.Color, false) + Eol);
            }
            if (Apply.Borders.Top) 
            {
                WriteLine(Indent + VarName + ".Borders.Top.Style " + EqualStr + " " + GetEnum(fmt.Borders.Top.Style) + Eol);
                WriteLine(Indent + VarName + ".Borders.Top.Color " + EqualStr + " " + GetColor(fmt.Borders.Top.Color, false) + Eol);
            }

            if (Apply.Borders.Bottom) 
            {
                WriteLine(Indent + VarName + ".Borders.Bottom.Style " + EqualStr + " " + GetEnum(fmt.Borders.Bottom.Style) + Eol);
                WriteLine(Indent + VarName + ".Borders.Bottom.Color " + EqualStr + " " + GetColor(fmt.Borders.Bottom.Color, false) + Eol);
            }

            if (Apply.Borders.Diagonal) 
            {
                WriteLine(Indent + VarName + ".Borders.Diagonal.Style " + EqualStr + " " + GetEnum(fmt.Borders.Diagonal.Style) + Eol);
                WriteLine(Indent + VarName + ".Borders.Diagonal.Color " + EqualStr + " " + GetColor(fmt.Borders.Diagonal.Color, false) + Eol);
            }

            if (Apply.Borders.DiagonalStyle) WriteLine(Indent + VarName + ".Borders.DiagonalStyle " + EqualStr + " " + GetEnum(fmt.Borders.DiagonalStyle) + Eol);
            
            //Pattern
            if (Apply.FillPattern.Pattern) WriteLine(Indent + VarName + ".FillPattern.Pattern " + EqualStr + " " + GetEnum(fmt.FillPattern.Pattern) + Eol);

            //we need to apply fg and bg too for Excel 2003.
            if (Apply.FillPattern.FgColor) WriteLine(Indent + VarName + ".FillPattern.FgColor " + EqualStr + " " + GetColor(fmt.FillPattern.FgColor, true) + Eol);
            if (Apply.FillPattern.BgColor) WriteLine(Indent + VarName + ".FillPattern.BgColor " + EqualStr + " " + GetColor(fmt.FillPattern.BgColor, true) + Eol);
            if (Apply.FillPattern.Gradient && fmt.FillPattern.Pattern == TFlxPatternStyle.Gradient)
            {
                DefineGradient(VarName, fmt.FillPattern, "FillPattern");
            }

            //Parent Style
            string DisplayName = GetStyleName(fmt);
            if (Apply.ParentStyle) WriteLine(Indent + VarName + ".ParentStyle " + EqualStr + " " + DisplayName + Eol);

            //Others
            if (Apply.Format) WriteLine(Indent + VarName + ".Format " + EqualStr + " " + GetFormatString(fmt.Format) + Eol);
        
            if (Apply.HAlignment) WriteLine(Indent + VarName + ".HAlignment " + EqualStr + " " + GetEnum(fmt.HAlignment) + Eol);
            if (Apply.VAlignment) WriteLine(Indent + VarName + ".VAlignment " + EqualStr + " " + GetEnum(fmt.VAlignment) + Eol);

            if (Apply.Locked) WriteLine(Indent + VarName + ".Locked " + EqualStr + " " + GetBool(fmt.Locked) + Eol);
            if (Apply.Hidden) WriteLine(Indent + VarName + ".Hidden " + EqualStr + " " + GetBool(fmt.Hidden) + Eol);
            if (Apply.WrapText) WriteLine(Indent + VarName + ".WrapText " + EqualStr + " " + GetBool(fmt.WrapText) + Eol);
            if (Apply.ShrinkToFit) WriteLine(Indent + VarName + ".ShrinkToFit " + EqualStr + " " + GetBool(fmt.ShrinkToFit) + Eol);
            if (Apply.Rotation) WriteLine(Indent + VarName + ".Rotation " + EqualStr + " " + fmt.Rotation + Eol);
            if (Apply.Indent) WriteLine(Indent + VarName + ".Indent " + EqualStr + " " + fmt.Indent + Eol);
            if (Apply.ReadingOrder) WriteLine(Indent + VarName + ".ReadingOrder " + EqualStr + " " + GetEnum(fmt.ReadingOrder) + Eol);
            if (Apply.Lotus123Prefix) WriteLine(Indent + VarName + ".Lotus123Prefix " + EqualStr + " " + GetBool(fmt.Lotus123Prefix) + Eol);
        }

        public void DefineGradient(string VarName, TFlxFillPattern fmt, string PatternVar)
        {
            string GradientStops = NullString;
            if (fmt.Gradient != null)
            {
                GradientStops = WriteGradientStops(fmt.Gradient);
            }
            WriteLine(Indent + VarName + "." + PatternVar + ".Gradient " + EqualStr + " " + GetGradient(GradientStops, fmt.Gradient) + Eol);
        }

        private string GetFormatString(string fmt)
        {
            if (fmt == TFlxNumberFormat.RegionalDateString) return  Call("TFlxNumberFormat", "RegionalDateString");
            return GetString(fmt);
        }

        private string GetStyleName(TFlxFormat fmt)
        {
            TBuiltInStyle stKind; int stLevel;
            int stPos = -1;
            if (Xls.TryGetBuiltInStyleType(fmt.NotNullParentStyle, out stKind, out stLevel)) stPos = (int)stKind;
            string DisplayName = stPos == 0 ? NullString : GetStyleDisplayName(fmt.ParentStyle, stPos, stLevel);
            return DisplayName;
        }

        public virtual void SetCellValue(int row, int col, string val)
        {
            WriteLine(Indent + XlsFileName + "." + String.Format(CultureInfo.InvariantCulture, "SetCellValue({0}, {1}, {2})" + Eol, row, col, val));
        }

        public abstract void SetHeaderImg(THeaderAndFooterKind HKind, THeaderAndFooterPos Section, byte[] Img, TXlsImgType ImgType, THeaderOrFooterImageProperties ImgProps, bool FirstImg);
        public abstract void AddImg(byte[] ImageData, TImageProperties ImgProps, string ImgPropsName, TXlsImgType ImgType, bool FirstImg);


        public virtual void WriteBaseImgProps(TBaseImageProperties ImgProps, string ImgPropsName, string TypeName, bool DefineVar, string CreateString)
        {
            if (CreateString == null) CreateString = CreateObject(TypeName, String.Empty);
            if (DefineVar)
            {
                InsertVar(ImgPropsName, TypeName, CreateString);
            }
            else
            {
                SetVarProp(ImgPropsName, CreateString);
            }

            if (ImgProps.FileName != null && ImgProps.FileName.Length > 0)
            {
                WriteLine(Indent + ImgPropsName + ".FileName " + EqualStr + " " + GetString(ImgProps.FileName) + Eol);
            }
            if (ImgProps.Brightness != FlxConsts.DefaultBrightness)
            {
                WriteLine(Indent + ImgPropsName + ".Brightness " + EqualStr + " " + ImgProps.Brightness + Eol);
            }
            if (ImgProps.Contrast != FlxConsts.DefaultContrast)
            {
                WriteLine(Indent + ImgPropsName + ".Contrast " + EqualStr + " " + ImgProps.Contrast + Eol);
            }
            if (ImgProps.Gamma != FlxConsts.DefaultGamma)
            {
                WriteLine(Indent + ImgPropsName + ".Gamma " + EqualStr + " " + ImgProps.Gamma + Eol);
            }
            if (ImgProps.TransparentColor != FlxConsts.NoTransparentColor)
            {
                WriteLine(Indent + ImgPropsName + ".TransparentColor " + EqualStr + " " + ImgProps.TransparentColor + Eol);
            }

            if (ImgProps.CropArea != null && !ImgProps.CropArea.IsEmpty())
            {
                WriteLine(Indent + ImgPropsName + ".CropArea " + EqualStr + " " + CreateObject("TCropArea", string.Format(CultureInfo.InvariantCulture,"{0}, {1}, {2}, {3}",
                    ImgProps.CropArea.CropFromTop, ImgProps.CropArea.CropFromBottom, ImgProps.CropArea.CropFromLeft, ImgProps.CropArea.CropFromRight)) + Eol);
            }

            if (!ImgProps.Lock)
            {
                WriteLine(Indent + ImgPropsName + ".Lock " + EqualStr + " " + FalseStr + Eol);
            }

            if (ImgProps.DefaultSize)
            {
                WriteLine(Indent + ImgPropsName + ".DefaultSize " + EqualStr + " " + TrueStr + Eol);
            }

            if (ImgProps.Published)
            {
                WriteLine(Indent + ImgPropsName + ".Published " + EqualStr + " " + TrueStr + Eol);
            }

            if (!ImgProps.Print)
            {
                WriteLine(Indent + ImgPropsName + ".Print " + EqualStr + " " + FalseStr + Eol);
            }

            if (ImgProps.Disabled)
            {
                WriteLine(Indent + ImgPropsName + ".Disabled " + EqualStr + " " + TrueStr + Eol);
            }

            if (ImgProps.DefaultsToLockedAspectRatio)
            {
                if (!ImgProps.PreferRelativeSize)
                {
                    WriteLine(Indent + ImgPropsName + ".PreferRelativeSize " + EqualStr + " " + FalseStr + Eol);
                }

                if (!ImgProps.LockAspectRatio)
                {
                    WriteLine(Indent + ImgPropsName + ".LockAspectRatio " + EqualStr + " " + FalseStr + Eol);
                }
            }
            else
            {
                if (ImgProps.PreferRelativeSize)
                {
                    WriteLine(Indent + ImgPropsName + ".PreferRelativeSize " + EqualStr + " " + TrueStr + Eol);
                }

                if (ImgProps.LockAspectRatio)
                {
                    WriteLine(Indent + ImgPropsName + ".LockAspectRatio " + EqualStr + " " + TrueStr + Eol);
                }
            }

            if (ImgProps.BiLevel)
            {
                WriteLine(Indent + ImgPropsName + ".BiLevel " + EqualStr + " " + TrueStr + Eol);
            }

            if (ImgProps.Grayscale)
            {
                WriteLine(Indent + ImgPropsName + ".Grayscale " + EqualStr + " " + TrueStr + Eol);
            }

        }

        public virtual void WriteHeaderImgProps(THeaderOrFooterImageProperties ImgProps, string ImgPropsName)
        {
            WriteBaseImgProps(ImgProps, ImgPropsName, "THeaderOrFooterImageProperties", true, null);
            if (ImgProps.Anchor != null)
            {
                WriteLine(Indent + ImgPropsName + ".Anchor " + EqualStr + " " + CreateObject("THeaderOrFooterAnchor", string.Format(CultureInfo.InvariantCulture,"{0}, {1}", ImgProps.Anchor.Width, ImgProps.Anchor.Height)) + Eol);
            }

        }

        public virtual void WriteImgProps(TImageProperties ImgProps, string ImgPropsName, string TypeName, bool DefineVar, string CreateString)
        {
            WriteBaseImgProps(ImgProps, ImgPropsName, TypeName, DefineVar, CreateString);
            if (ImgProps.Anchor != null)
            {
                WriteLine(Indent + ImgPropsName + ".Anchor " + EqualStr + " " + GetAnchor(ImgProps.Anchor) + Eol);
            }

            if (ImgProps.ShapeName != null)
            {
                WriteLine(Indent + ImgPropsName + ".ShapeName " + EqualStr + " " + GetString(ImgProps.ShapeName) + Eol);
            }
        
            if (ImgProps.AltText != null)
            {
                WriteLine(Indent + ImgPropsName + ".AltText " + EqualStr + " " + GetString(ImgProps.AltText) + Eol);
            }
        }

        public string GetAnchor(TClientAnchor Anchor)
        {
            if (Anchor.ChartCoords)
            {
                return CreateObject("TClientAnchor",
                          GetParams(
                                    GetBool(Anchor.ChartCoords), GetEnum(Anchor.AnchorType),
                                    GetInt(Anchor.Row1), GetInt(Anchor.Dy1),
                                    GetInt(Anchor.Col1), GetInt(Anchor.Dx1),
                                    GetInt(Anchor.Row2), GetInt(Anchor.Dy2),
                                    GetInt(Anchor.Col2), GetInt(Anchor.Dx2)
                                    ));
            }
            else
            {
                return CreateObject("TClientAnchor",
                          GetParams(
                                    GetEnum(Anchor.AnchorType),
                                    GetInt(Anchor.Row1), GetInt(Anchor.Dy1),
                                    GetInt(Anchor.Col1), GetInt(Anchor.Dx1),
                                    GetInt(Anchor.Row2), GetInt(Anchor.Dy2),
                                    GetInt(Anchor.Col2), GetInt(Anchor.Dx2)
                                    ));
            }
        }

        public virtual void WriteCommentProps(int r, int c, TCommentProperties ImgProps, string ImgPropsName, bool DefineVar)
        {
            WriteImgProps(ImgProps, ImgPropsName, "TCommentProperties", DefineVar, 
                GetCallVarMethod(String.Empty, Call("TCommentProperties", "CreateStandard"), r, c, XlsFileName));

            WriteTextProperties(ImgPropsName + ".TextProperties", ImgProps.TextProperties);

            if (ImgProps.AutoSize)
            {
                WriteLine(Indent + ImgPropsName + ".AutoSize " + EqualStr + " " + TrueStr + Eol);
            }

            //if (ImgProps.Fill != null)
            {
              //PENDING: WriteLine(Indent + ImgPropsName + ".FillColor " + EqualStr + " " + GetFillType(ImgProps.Fill) + Eol);
               
            }

        }

        private void WriteTextProperties(string TextPropsName, TObjectTextProperties TextProps)
        {
            if (TextProps != null)
            {
                if (!TextProps.LockText)
                {
                    WriteLine(Indent + TextPropsName + ".LockText " + EqualStr + " " + FalseStr + Eol);
                }

                if (TextProps.HAlignment != THFlxAlignment.left)
                {
                    WriteLine(Indent + TextPropsName + ".HAlignment " + EqualStr + " " + GetEnum(TextProps.HAlignment) + Eol);
                }

                if (TextProps.VAlignment != TVFlxAlignment.top)
                {
                    WriteLine(Indent + TextPropsName + ".VAlignment " + EqualStr + " " + GetEnum(TextProps.VAlignment) + Eol);
                }

                switch (TextProps.TextRotation)
                {
                    case TTextRotation.Rotated90Degrees:
                    case TTextRotation.RotatedMinus90Degrees:
                    case TTextRotation.Vertical:
                        WriteLine(Indent + TextPropsName + ".TextRotation " + EqualStr + " " + GetEnum(TextProps.TextRotation) + Eol);

                        break;
                }

            }
        }

        public void WriteShapeOptions(string ShapeOptionsVar, TShapeProperties ShapeProps)
        {
            string CreateString = CreateObject("TShapeProperties", String.Empty);
            InsertVar(ShapeOptionsVar, "TShapeProperties", CreateString);

            WriteLine(Indent + ShapeOptionsVar + ".Anchor " + EqualStr + " " + GetAnchor(ShapeProps.Anchor) + Eol);

            WriteLine(Indent + ShapeOptionsVar + ".ShapeType " + EqualStr + " " + GetEnum(ShapeProps.ShapeType) + Eol);
            WriteLine(Indent + ShapeOptionsVar + ".ObjectType " + EqualStr + " " + GetEnum(ShapeProps.ObjectType) + Eol);
            WriteLine(Indent + ShapeOptionsVar + ".ShapeName " + EqualStr + " " + GetString(ShapeProps.ShapeName) + Eol);
            
            if (ShapeProps.Text != null && !String.IsNullOrEmpty(ShapeProps.Text.Value))
            {
                WriteLine(Indent + ShapeOptionsVar + ".Text " + EqualStr + " " + GetString(ShapeProps.Text) + Eol);
                WriteLine(Indent + ShapeOptionsVar + ".TextFlags " + EqualStr + " " + GetInt(ShapeProps.TextFlags) + Eol);
            }

            if (ShapeProps.TextRotation != 0) WriteLine(Indent + ShapeOptionsVar + ".TextRotation " + EqualStr + " " + GetInt(ShapeProps.TextRotation) + Eol);
            if (ShapeProps.RotateTextWithShape) WriteLine(Indent + ShapeOptionsVar + ".RotateTextWithShape " + EqualStr + " " + GetBool(ShapeProps.RotateTextWithShape) + Eol);

            if (ShapeProps.ShapeThemeFont != null)
            {
                WriteLine(Indent + ShapeOptionsVar + ".ShapeThemeFont " + EqualStr + " " + CreateObject("TShapeFont", GetParams(GetEnum(ShapeProps.ShapeThemeFont.ThemeScheme), GetDrawingColor(ShapeProps.ShapeThemeFont.ThemeColor)))  + Eol);
            }

            if (ShapeProps.FlipH) WriteLine(Indent + ShapeOptionsVar + ".FlipH " + EqualStr + " " + GetBool(ShapeProps.FlipH) + Eol);
            if (ShapeProps.FlipV) WriteLine(Indent + ShapeOptionsVar + ".FlipV " + EqualStr + " " + GetBool(ShapeProps.FlipV) + Eol);
            if (ShapeProps.Print) WriteLine(Indent + ShapeOptionsVar + ".Print " + EqualStr + " " + GetBool(ShapeProps.Print) + Eol);
            if (ShapeProps.Visible) WriteLine(Indent + ShapeOptionsVar + ".Visible " + EqualStr + " " + GetBool(ShapeProps.Visible) + Eol);

            if (ShapeProps.ShapeGeometry != null)
            {
                WriteLine(Indent + ShapeOptionsVar + ".ShapeGeometry " + EqualStr + " " + GetString(ShapeProps.ShapeGeometry) + Eol);
            }

            foreach (TShapeOption so in ShapeProps.ShapeOptions.Keys)
            {
                string val = null;
                string Method = "SetValue";
                switch (TShapeOptionList.ShapeOptionType(so))
                {
                    case TShapeOptionType.Bool:
                        TShapeOption bkey = so;
                        while (TShapeOptionList.ShapeOptionType(bkey) == TShapeOptionType.Bool)
                        {
                            bool v = ShapeProps.ShapeOptions.AsBool(bkey, false);
                            if (v) WriteLine(Indent + GetCallVarMethod(ShapeOptionsVar, Call(".ShapeOptions", "SetValue"), GetEnum(bkey), GetBool(true)) + Eol);
                            bkey--;
                        }
                        break;

                    case TShapeOptionType.FixedPoint:
                        val = GetDouble(ShapeProps.ShapeOptions.As1616(so, 0));
                        break;

                    case TShapeOptionType.SignedInt:
                        val = GetLong(ShapeProps.ShapeOptions.AsLong(so, 0));
                        break;

                    case TShapeOptionType.UnsignedInt:
                        val = GetLong(ShapeProps.ShapeOptions.AsLong(so, 0));
                        break;

                    case TShapeOptionType.String:
                        val = GetString(ShapeProps.ShapeOptions.AsUnicodeString(so, ""));
                        break;

                    case TShapeOptionType.Hyperlink:
                        val = GetHyperlink(ShapeProps.ShapeOptions.AsHyperLink(so, null));
                        break;

                    case TShapeOptionType.ByteArray:
                        DefineByteArray("DataByte", "byte", ShapeProps.ShapeOptions.AsByteArray(so));
                        val = "DataByte";
                        Method = "SetBytes";
                        break;

                    case TShapeOptionType.Image:
                        DefineByteArray("ImageData", "byte", ShapeProps.ShapeOptions.AsImage(Xls, ShapeProps.ObjectPath, so));
                        val = "ImageData";
                        Method = "SetImage";
                        break;

                }

                if (val != null)
                {
                    WriteLine(Indent + GetCallVarMethod(ShapeOptionsVar, Call(".ShapeOptions", Method), GetEnum(so), val) + Eol);
                }
            }
        }

        private string GetHyperlink(TDrawingHyperlink hl)
        {
            if (hl == null) return "";
            return CreateObject("TDrawingHyperlink",GetParams(GetString(hl.Url), GetString(hl.TargetFrame),
                GetString(hl.Hint), GetString(hl.Action), GetBool(hl.HighlightClick), GetBool(hl.AddToHistory), 
                GetBool(hl.EndsSounds)));
        }

        public virtual void CreateDataValidation(TDataValidationInfo dv, string dvVar)
        {
            TDataValidationInfo refdv = new TDataValidationInfo();
            WriteLine();
            SetVarProp(dvVar, CreateObject("TDataValidationInfo", String.Empty));
            Indent = Indent0 + Indent0;
            if (refdv.ValidationType != dv.ValidationType) SetVarProp(dvVar + ".ValidationType", GetEnum(dv.ValidationType));
            if (refdv.Condition != dv.Condition) SetVarProp(dvVar + ".Condition", GetEnum(dv.Condition));
            if (refdv.FirstFormula != dv.FirstFormula) SetVarProp(dvVar + ".FirstFormula", GetString(dv.FirstFormula));
            if (refdv.SecondFormula != dv.SecondFormula) SetVarProp(dvVar + ".SecondFormula", GetString(dv.SecondFormula));
            
            if (refdv.IgnoreEmptyCells != dv.IgnoreEmptyCells) SetVarProp(dvVar + ".IgnoreEmptyCells", GetBool(dv.IgnoreEmptyCells));
            if (refdv.InCellDropDown != dv.InCellDropDown) SetVarProp(dvVar + ".InCellDropDown", GetBool(dv.InCellDropDown));
            if (refdv.ExplicitList != dv.ExplicitList) SetVarProp(dvVar + ".ExplicitList", GetBool(dv.ExplicitList));
            if (refdv.ShowErrorBox != dv.ShowErrorBox) SetVarProp(dvVar + ".ShowErrorBox", GetBool(dv.ShowErrorBox));
            if (refdv.ErrorBoxCaption != dv.ErrorBoxCaption) SetVarProp(dvVar + ".ErrorBoxCaption", GetString(dv.ErrorBoxCaption));
            if (refdv.ErrorBoxText != dv.ErrorBoxText) SetVarProp(dvVar + ".ErrorBoxText", GetString(dv.ErrorBoxText));
            if (refdv.ShowInputBox != dv.ShowInputBox) SetVarProp(dvVar + ".ShowInputBox", GetBool(dv.ShowInputBox));
            if (refdv.InputBoxCaption != dv.InputBoxCaption) SetVarProp(dvVar + ".InputBoxCaption", GetString(dv.InputBoxCaption));
            if (refdv.InputBoxText != dv.InputBoxText) SetVarProp(dvVar + ".InputBoxText", GetString(dv.InputBoxText));
            if (refdv.ErrorIcon != dv.ErrorIcon) SetVarProp(dvVar + ".ErrorIcon", GetEnum(dv.ErrorIcon));
            if (refdv.ImeMode != dv.ImeMode) SetVarProp(dvVar + ".ImeMode", GetEnum(dv.ImeMode));

            Indent = Indent0;
        }


        public virtual void CreateHyperLink(THyperLink hl, string LinkVar)
        {
            SetVarProp(LinkVar, CreateObject("THyperLink", GetParams(GetEnum(hl.LinkType), GetString(hl.Text), 
                GetString(hl.Description), GetString(hl.TargetFrame), GetString(hl.TextMark))));
            if (hl.Hint != null && hl.Hint.Length > 0) SetVarProp(Call(LinkVar, "Hint"), GetString(hl.Hint));
        }

        private bool GetBetterUseNone(TSheetProtectionOptions op)
        {
            int UseNone = 0; int UseAll = 0;
            if (op.Contents) UseAll++; else UseNone++;
            if (op.Objects) UseAll++; else UseNone++;
            if (op.Scenarios) UseAll++; else UseNone++;

            if (op.CellFormatting) UseNone++; else UseAll++;
            if (op.ColumnFormatting) UseNone++; else UseAll++;
            if (op.RowFormatting) UseNone++; else UseAll++;
            if (op.InsertColumns) UseNone++; else UseAll++;
            if (op.InsertRows) UseNone++; else UseAll++;
            if (op.InsertHyperlinks) UseNone++; else UseAll++;
            if (op.DeleteColumns) UseNone++; else UseAll++;
            if (op.DeleteRows) UseNone++; else UseAll++;
            if (op.SelectLockedCells) UseNone++; else UseAll++;
            if (op.SortCellRange) UseNone++; else UseAll++;
            if (op.EditAutoFilters) UseNone++; else UseAll++;
            if (op.EditPivotTables) UseNone++; else UseAll++;
            if (op.SelectUnlockedCells) UseNone++; else UseAll++;

            return UseNone > UseAll;
        }

        public virtual void CreateSheetProtection(string varname, TSheetProtectionOptions op)
        {
            bool BetterUseNone = GetBetterUseNone(op);
            string SpInit = BetterUseNone ? GetEnum(TProtectionType.None) : GetEnum(TProtectionType.All);
            SetVarProp(varname, CreateObject("TSheetProtectionOptions", SpInit));
            Indent = Indent0 + Indent0;

            if (BetterUseNone ^ !op.Contents) SetVarProp(varname + ".Contents", GetBool(op.Contents));
            if (BetterUseNone ^ !op.Objects) SetVarProp(varname + ".Objects", GetBool(op.Objects));
            if (BetterUseNone ^ !op.Scenarios) SetVarProp(varname + ".Scenarios", GetBool(op.Scenarios));

            if (BetterUseNone ^ op.CellFormatting) SetVarProp(varname + ".CellFormatting", GetBool(op.CellFormatting));
            if (BetterUseNone ^ op.ColumnFormatting) SetVarProp(varname + ".ColumnFormatting", GetBool(op.ColumnFormatting));
            if (BetterUseNone ^ op.RowFormatting) SetVarProp(varname + ".RowFormatting", GetBool(op.RowFormatting));
            if (BetterUseNone ^ op.InsertColumns) SetVarProp(varname + ".InsertColumns", GetBool(op.InsertColumns));
            if (BetterUseNone ^ op.InsertRows) SetVarProp(varname + ".InsertRows", GetBool(op.InsertRows));
            if (BetterUseNone ^ op.InsertHyperlinks) SetVarProp(varname + ".InsertHyperlinks", GetBool(op.InsertHyperlinks));
            if (BetterUseNone ^ op.DeleteColumns) SetVarProp(varname + ".DeleteColumns", GetBool(op.DeleteColumns));
            if (BetterUseNone ^ op.DeleteRows) SetVarProp(varname + ".DeleteRows", GetBool(op.DeleteRows));
            if (BetterUseNone ^ op.SelectLockedCells) SetVarProp(varname + ".SelectLockedCells", GetBool(op.SelectLockedCells));
            if (BetterUseNone ^ op.SortCellRange) SetVarProp(varname + ".SortCellRange", GetBool(op.SortCellRange));
            if (BetterUseNone ^ op.EditAutoFilters) SetVarProp(varname + ".EditAutoFilters", GetBool(op.EditAutoFilters));
            if (BetterUseNone ^ op.EditPivotTables) SetVarProp(varname + ".EditPivotTables", GetBool(op.EditPivotTables));
            if (BetterUseNone ^ op.SelectUnlockedCells) SetVarProp(varname + ".SelectUnlockedCells", GetBool(op.SelectUnlockedCells));

            Indent = Indent0;
        }


        public virtual void CreateWorkbookProtection(string varname, TWorkbookProtectionOptions op)
        {
            SetVarProp(varname, CreateObject("TWorkbookProtectionOptions", String.Empty));
            Indent = Indent0 + Indent0;

            if (op.Window) SetVarProp(varname + ".Window", GetBool(op.Window));
            if (op.Structure) SetVarProp(varname + ".Structure", GetBool(op.Structure));

            Indent = Indent0;
        }

        public virtual void CreateSharedWorkbookProtection(string varname, TSharedWorkbookProtectionOptions op)
        {
            SetVarProp(varname, CreateObject("TSharedWorkbookProtectionOptions", String.Empty));
            Indent = Indent0 + Indent0;

            if (op.SharingWithTrackChanges) SetVarProp(varname + ".SharingWithTrackChanges", GetBool(op.SharingWithTrackChanges));

            Indent = Indent0;
        }


        public virtual void SetInternalName(string VarName, char Value)
        {
            string InternalName;
            string IntName = Enum.GetName(typeof(InternalNameRange), (InternalNameRange)Value);
                        
            if (IntName == null || IntName.Length == 0) //Not in the enum
            {
                InternalName = Cast("InternalNameRange", Hexa((byte)Value));
            }
            else
            {
                InternalName = GetEnum((InternalNameRange)Value);
            }

            SetVarProp(VarName, GetCallVarMethod(String.Empty, Call("TXlsNamedRange", "GetInternalName"), InternalName));
        }

        #endregion

        public override string ToString()
        {
            return s.ToString();
        }


        public abstract string IntegerString();

        internal string GetNullStr()
        {
            return NullString;
        }

        public string GetRange(TXlsCellRange Range)
        {
            return CreateObject("TXlsCellRange", GetParams(Range.Top, Range.Left, Range.Bottom, Range.Right));
        }
    }

    public class CSharpEngine: LanguageEngine
    {
        public CSharpEngine(string aXlsFileName, ExcelFile aXls): base (aXlsFileName, ";", "true", "false", "=", "null", "    ", aXls){}

        #region Write
        public override void InsertVar(string Name, string VarType, string Def)
        {
            if (Def != null)
                WriteLine(Indent + VarType + " " + Name + " = " + Def + ";");
            else
                WriteLine(Indent + VarType + " " + Name + ";");
        }

        public override void InsertVarAndTrack(string Name, string VarType, string Def)
        {
            if (Def != null)
                WriteLine(Indent + CheckVar(Name, VarType + " ", "") + Name + " = " + Def + ";");
            else
            {
                string sv = CheckVar(Name, Indent + VarType + " " + Name + ";", "");
                if (sv != "") WriteLine(sv);
            }
        }

        public override string StartComment(string Comment)
        {
            return Indent + "//" + Comment;
        }

        #endregion

        #region Formatting
        public override string GetReservedWord(string s)
        {
            return s;
        }

        public override string CreateObject(string ObjectName, string Params)
        {
            return "new " + ObjectName + "(" + Params + ")";
        }

        public override string CreateObjectArray(string ObjectName, string ArrayType, string ArrayLen)
        {
            string VarName = ObjectName == null ? String.Empty : ObjectName + " = ";
            return VarName + "new " + ArrayType + "[" + ArrayLen + "]";			
        }

        public override string GetArrayDecl(string ArrType)
        {
            return ArrType + "[]";
        }

        public override void DefineArray(bool DefineVar, string Name, string ArrType, int BreakAt, int DataLength, Func<int, string> DataGenerator)
        {
            string VarDef = DefineVar ? CheckVar(Name, GetArrayDecl(ArrType) + " ", ""): "";
            WriteLine(Indent + VarDef + Name + " " + EqualStr + " " + CreateObjectArray(null, ArrType, "") + " {");

            StringBuilder List = new StringBuilder();
            for (int i = 0; i < DataLength; i++)
            {
                List.Append(DataGenerator(i));
                if (i < DataLength - 1) List.Append(", ");
                if ((i + 1) % BreakAt == 0)
                {
                    WriteLine(Indent + Indent + List.ToString());
                    List = new StringBuilder();
                }
            }
            if (List.Length > 0) WriteLine(Indent + Indent + List.ToString());


            WriteLine(Indent + "}" + Eol);
        }


        public override string GetCallArray(int index)
        {
            return "[" + index.ToString(CultureInfo.InvariantCulture) + "]";
        }

        public override string GetCallVarMethod(string Prefix, params object[] args)
        {
            return GetCallVarMethodInternal(Prefix, true, args);
        }

        public override string GetChar(string s1)
        {
            return "'" + s1 + "'";
        }


        public override string GetString(string s1)
        {
            const int LineLimit = 80;
            int AcumLen = 0;
            if (s1 == null) return NullString;
            
            StringBuilder sb = new StringBuilder(s1.Length);
            for (int i = 0; i < s1.Length; i++)
            {
                if (s1[i] == ' ' && sb.Length - AcumLen > LineLimit)
                {
                    sb.Append("\"" + Environment.NewLine + Indent + "+ \"");
                    AcumLen = sb.Length;
                }

                if (s1[i] == '\\' || s1[i] =='\"') sb.Append("\\");

                if (s1[i] < ' ')
                {
                    switch (s1[i])
                    {
                        case '\n': sb.Append("\\n");break;
                        case '\t': sb.Append("\\t");break;
                        case '\r': sb.Append("\\r");break;
                        default: sb.AppendFormat(CultureInfo.InvariantCulture, "\\x{0:X4}", (int)s1[i] );break;
                    }
                }
                else
                {
                    sb.Append(s1[i]);
                }
            }
            return "\"" +  sb.ToString() + "\"";
        }

        public override string Cast(string aType, object value)
        {
            return "(" + aType + ")" + Convert.ToString(value);
        }


        public override string GetBinaryOr
        {
            get
            {
                return "|";
            }
        }

        public override string Hexa(byte b)
        {
            return "0x" + String.Format(CultureInfo.InvariantCulture, "{0:X2}", b);
        }

        public override void StartUsing(string ClassName, string VarName, string VarValue)
        {
            WriteLine(Indent + "using (" + ClassName + " " + VarName + " = " + VarValue + ")");
            WriteLine(Indent + "{");
            Indent += Indent0;
        }

        public override void EndUsing(string VarName, bool Close)
        {
            Indent = Indent.Remove(0, Indent0.Length);
            WriteLine(Indent + "}");
        }


        #endregion

        #region Excel
        public override void Start(bool ShowUsing, bool SaveXls, bool SaveCsv, bool SavePxl, bool SavePdf, bool SaveHtml, bool SaveBmp, bool AspNetCode)
        {
            DefinedVars.Clear();
            if (ShowUsing)
            {
                WriteLine("using System.IO;");
                WriteLine("using System.Globalization;");
                if (SavePxl) 
                    WriteLine("using System.Text;");
                WriteLine("using FlexCel.Core;");
                WriteLine("using FlexCel.XlsAdapter;");
                if (SaveHtml || SavePdf || SaveBmp) 
                    WriteLine("using FlexCel.Render;");
                if (SavePdf) 
                    WriteLine("using FlexCel.Pdf;");
                WriteLine(String.Empty);
            }

            if (AspNetCode)
                WriteLine("public void CreateAndSendFile()");
                else WriteLine("public void CreateAndSaveFile()");
            WriteLine("{");
            WriteLine(Indent + "XlsFile "+ XlsFileName +" = new XlsFile(true);");
            WriteLine(Indent + GetCallVarMethod(String.Empty, "CreateFile", XlsFileName) + Eol);
            
            SaveTheFile(SaveXls, SaveCsv, SavePxl, SavePdf, SaveHtml, SaveBmp, AspNetCode);

            Finish();
            WriteLine();

            if (AspNetCode) AddSendToBrowser();

            WriteLine("public void CreateFile(ExcelFile " + XlsFileName +")");
            WriteLine("{");
        }
       
        public override void Finish()
        {
            WriteLine("}");
        }

        public override void AddSendToBrowser()
        {
            WriteLine("private void SendToBrowser(MemoryStream OutStream, string MimeType, string FileName)");
            WriteLine("{");
            base.AddSendToBrowser ();
            WriteLine("}");
            WriteLine();
        }


        public override void SetHeaderImg(THeaderAndFooterKind HKind, THeaderAndFooterPos Section, byte[] Img, TXlsImgType ImgType, THeaderOrFooterImageProperties ImgProps, bool FirstImg)
        {
            string ImgName = NullString;
            string ImgPropsName = NullString;
            if (Img != null)
            {
                ImgName = "HeaderImgData";
                ImgPropsName = "HeaderImgProps";
                WriteLine(Indent + "using (FileStream ms = new FileStream(\"imagename" + GetExt(ImgType) + "\", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))");
                WriteLine(Indent + "{");
                Indent += Indent0;

                InsertVar(ImgName, GetArrayDecl("Byte"));
                WriteLine(Indent + CreateObjectArray(ImgName, "Byte", "ms.Length") + Eol);
                WriteLine(Indent + Call("ms", "Read") + String.Format(CultureInfo.InvariantCulture, "({0}, 0, {0}.Length)", ImgName) + Eol);
                WriteHeaderImgProps(ImgProps, ImgPropsName);
            }

            CallMethod("SetHeaderOrFooterImage", 
                GetEnum(HKind),
                GetEnum(Section),
                ImgName,
                GetEnum(ImgType),
                ImgPropsName
                );
            
            if (ImgName != NullString)
            {
                Indent = Indent0;
                WriteLine(Indent + "}");
            }

        }


        public override void AddImg(byte[] ImageData, TImageProperties ImgProps, string ImgPropsName, TXlsImgType ImgType, bool FirstImg)
        {
            WriteLine(Indent + "using (FileStream fs = new FileStream(\"imagename" + GetExt(ImgType) + "\", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))");
            WriteLine(Indent + "{");
            Indent += Indent0;

            WriteImgProps(ImgProps, "ImgProps", "TImageProperties", true, null);

            CallMethod("AddImage", 
                "fs",
                ImgPropsName
                );
            
            Indent = Indent0;
            WriteLine(Indent + "}");
        }

        #endregion

        public override string IntegerString()
        {
            return "int";
        }
    }

    public class VBEngine: LanguageEngine
    {
        public VBEngine(string aXlsFileName, ExcelFile aXls): base (aXlsFileName, String.Empty, "True", "False", "=", "Nothing", "    ", aXls){}

        #region Write
        public override void InsertVar(string Name, string VarType, string Def)
        {
            if (Def != null)
                WriteLine(Indent + "Dim " + Name + " As " + VarType + " = " + Def);
            else
                WriteLine(Indent + "Dim " + Name + " As " + VarType);
        }

        public override void InsertVarAndTrack(string Name, string VarType, string Def)
        {
            if (Def != null)
                WriteLine(Indent + CheckVar(Name, "Dim " + Name + " As " + VarType, Name) + " = " + Def);
            else
                WriteLine(Indent + "Dim " + Name + " As " + VarType);
        }

        public override string StartComment(string Comment)
        {
            return Indent + "'" + Comment;
        }

        #endregion

        #region Formatting
        public override string GetReservedWord(string s)
        {
            return "[" + s + "]";
        }

        
        public override string GetChar(string s1)
        {
            return "\"" + s1 + "\"";
        }

        public override string GetString(string s1)
        {
            bool LastWasChar = false;
            bool LastWasCode = false;

            if (s1 == null) return NullString;

            StringBuilder sb = new StringBuilder(s1.Length);
            for (int i = 0; i < s1.Length; i++)
            {
                if (s1[i] == '\"') sb.Append("\"");

                if (s1[i] < ' ' || s1[i] == '\u201c' || s1[i] == '\u201d')  //Fancy quotes get converted to normal ones in vb, not in c#
                {
                    if (LastWasChar) sb.Append("\"");
                    if (i > 0) sb.Append(" + ");
                    sb.Append("ChrW(" + ((int)s1[i]).ToString(CultureInfo.InvariantCulture) + ")");

                    LastWasChar = false;
                    LastWasCode = true;
                }
                else
                {
                    if (LastWasCode) sb.Append (" + ");
                    if (LastWasCode || i == 0) sb.Append("\"");
                    sb.Append(s1[i]);
                    LastWasChar = true;
                    LastWasCode = false;
                }
            }

            if (LastWasChar) sb.Append("\"");
            while (sb.Length < 2) sb.Append("\"");
            return sb.ToString();
        }

        public override string CreateObject(string ObjectName, string Params)
        {
            return "New " + ObjectName + "(" + Params + ")";
        }

        public override string CreateObjectArray(string ObjectName, string ArrayType, int ArrayLen)
        {
            string VarName = ObjectName == null ? String.Empty : ObjectName + " = ";
            return VarName + "New " + ArrayType + "(" + (ArrayLen - 1).ToString(CultureInfo.InvariantCulture) + ") {}";			
        }

        public override string CreateObjectArray(string ObjectName, string ArrayType, string ArrayLen)
        {
            string VarName = ObjectName == null ? String.Empty : ObjectName + " = ";
            return VarName + "New " + ArrayType + "(" + ArrayLen + " - 1) {}";			
        }

        public override string GetArrayDecl(string ArrType)
        {
            return ArrType + "()";
        }


        public override void DefineArray(bool DefineVar, string Name, string ArrType, int BreakAt, int DataLength, Func<int, string> DataGenerator)
        {
            if (DefineVar)
            {
                InsertVarAndTrack(Name, GetArrayDecl(ArrType), " New " + ArrType + "(" + (DataLength - 1).ToString(CultureInfo.InvariantCulture)
                    + ") {");
            }
            else
            {
                WriteLine(Indent + Name + " " + EqualStr + " {");
            }

            StringBuilder List = new StringBuilder();
            for (int i = 0; i < DataLength; i++)
            {
                List.Append(DataGenerator(i));
                if (i < DataLength - 1) List.Append(", ");
                if ((i + 1) % 50 == 0)
                {
                    WriteLine(Indent + Indent + List.ToString());
                    List = new StringBuilder();
                }
            }
            if (List.Length > 0) WriteLine(Indent + Indent + List.ToString());


            WriteLine(Indent + "}" + Eol);
        }


        public override string GetCallArray(int index)
        {
            return "(" + index.ToString(CultureInfo.InvariantCulture) + ")";
        }

        public override string GetCallVarMethod(string Prefix, params object[] args)
        {
            return GetCallVarMethodInternal(Prefix, true, args);
        }

        public override string Cast(string aType, object value)
        {
            return "DirectCast(" + Convert.ToString(value) + ", " +aType +  ")";
        }

        public override string GetBinaryOr
        {
            get
            {
                return "Or";
            }
        }

        public override string Hexa(byte b)
        {
            return "&H" + String.Format(CultureInfo.InvariantCulture, "{0:X2}", b);
        }

        public override void StartUsing(string ClassName, string VarName, string VarValue)
        {
            InsertVar(VarName, ClassName, VarValue);
            WriteLine(Indent + "Try");
            Indent += Indent0;
        }

        public override void EndUsing(string VarName, bool Close)
        {
            Indent = Indent.Remove(0, Indent0.Length);
            WriteLine(Indent + "Finally");
            string Op = Close? ".Close" : ".Dispose";
            WriteLine(Indent + Indent0 + VarName + Op);
            WriteLine(Indent + "End Try");
        }


        #endregion

        #region Excel
        public override void Start(bool ShowUsing, bool SaveXls, bool SaveCsv, bool SavePxl, bool SavePdf, bool SaveHtml, bool SaveBmp, bool AspNetCode)
        {
            DefinedVars.Clear();
            if (ShowUsing)
            {
                WriteLine("Imports System.IO");
                WriteLine("Imports System.Globalization");

                if (SavePxl) 
                    WriteLine("Imports System.Text");

                WriteLine("Imports FlexCel.Core");
                WriteLine("Imports FlexCel.XlsAdapter");
                if (SaveHtml || SavePdf || SaveBmp) 
                    WriteLine("Imports FlexCel.Render");
                if (SavePdf) 
                    WriteLine("Imports FlexCel.Pdf");
                WriteLine(String.Empty);
            }

            if (AspNetCode)
                WriteLine("Public Sub CreateAndSendFile()");
                else WriteLine("Public Sub CreateAndSaveFile()");
            
            WriteLine("    Dim "+ XlsFileName +" As XlsFile = New XlsFile(True)");
            WriteLine(Indent + GetCallVarMethod(String.Empty, "CreateFile", XlsFileName));
            
            SaveTheFile(SaveXls, SaveCsv, SavePxl, SavePdf, SaveHtml, SaveBmp, AspNetCode);

            Finish();
            WriteLine();

            if (AspNetCode) AddSendToBrowser();

            WriteLine("Public Sub CreateFile("+ XlsFileName + " As ExcelFile)");

        }

        public override void Finish()
        {
            WriteLine("End Sub");
        }

        public override void AddSendToBrowser()
        {
            WriteLine("Private Sub SendToBrowser(OutStream As MemoryStream , MimeType As String, FileName As String)");
            base.AddSendToBrowser ();
            WriteLine("End Sub" + Eol);
            WriteLine();
        }


        public override void SetHeaderImg(THeaderAndFooterKind HKind, THeaderAndFooterPos Section, byte[] Img, TXlsImgType ImgType, THeaderOrFooterImageProperties ImgProps, bool FirstImg)
        {
            string ImgName = NullString;
            string ImgPropsName = NullString;
            if (Img != null)
            {
                ImgName = "HeaderImgData";
                ImgPropsName = "HeaderImgProps";
                string Define = FirstImg? "Dim ms As FileStream" : "ms";
                WriteLine(Indent + Define + " = New FileStream(\"imagename" + GetExt(ImgType) + "\", FileMode.Open, FileAccess.Read, FileShare.ReadWrite)");
                WriteLine(Indent + "Try");
                Indent += Indent0;

                InsertVar(ImgName, GetArrayDecl("Byte"));
                WriteLine(Indent + CreateObjectArray(ImgName, "Byte", "ms.Length") + Eol);
                WriteLine(Indent + Call("ms", "Read") + String.Format(CultureInfo.InvariantCulture, "({0}, 0, {0}.Length)", ImgName) + Eol);

                WriteHeaderImgProps(ImgProps, ImgPropsName);
            }
            CallMethod("SetHeaderOrFooterImage", 
                GetEnum(HKind),
                GetEnum(Section),
                ImgName,
                GetEnum(ImgType),
                ImgPropsName
                );
            
            if (ImgName != NullString)
            {
                Indent = Indent0;
                WriteLine(Indent + "Finally");
                WriteLine(Indent + Indent0 + "ms.Close");
                WriteLine(Indent + "End Try");
            }

        }

        public override void AddImg(byte[] ImageData, TImageProperties ImgProps, string ImgPropsName, TXlsImgType ImgType, bool FirstImg)
        {
            string Define = FirstImg? "Dim fs As FileStream" : "fs";
            WriteLine(Indent + Define + " = New FileStream(\"imagename" + GetExt(ImgType) + "\", FileMode.Open, FileAccess.Read, FileShare.ReadWrite)");
            WriteLine(Indent + "Try");
            Indent += Indent0;

            WriteImgProps(ImgProps, "ImgProps", "TImageProperties", true, null);

            CallMethod("AddImage", 
                "fs",
                ImgPropsName
                );
            
            Indent = Indent0;
            WriteLine(Indent + "Finally");
            WriteLine(Indent + Indent0 + "fs.Close");
            WriteLine(Indent + "End Try");
        }

        #endregion

        public override string IntegerString()
        {
            return "Integer";
        }
    }

    public class DelphiEngine: LanguageEngine
    {
        private int VarPos;
        private bool HasVar;
        private bool Oxygene;

        public DelphiEngine(string aXlsFileName, ExcelFile aXls, bool aOxygene): base (aXlsFileName, ";", "true", "false", ":=", "nil", "  ", aXls)
        {
            Oxygene = aOxygene;
        }

        #region Write
        public override string StartComment(string Comment)
        {
            return Indent + "//" + Comment;
        }

        public override void InsertVarAndTrack(string Name, string VarType, string Def)
        {
            if (Oxygene)
            {
                if (Def != null)
                    WriteLine(Indent + CheckVar(Name, "var " + Name + ": " + VarType, Name) + " := " + Def + ";");
                else
                {
                    string sw = CheckVar(Name, "var " + Name + ": " + VarType + ";", "");
                    if (sw != "") WriteLine(Indent + sw);
                }
            }
            else
            {
                InsertVar(Name, VarType, Def);
            }
        }

        public override void InsertVar(string Name, string VarType, string Def)
        {
            if (Oxygene)
            {
                if (Def != null)
                    WriteLine(Indent + "var " + Name + ": " + VarType + " := " + Def + ";");
                else
                {
                    string sw = "var " + Name + ": " + VarType + ";";
                    if (sw != "") WriteLine(Indent + sw);
                }
            }
            else
            {
                if (!DefinedVars.ContainsKey(Name))  //Avoid defining more than one var in the var section. In C# or VB.NET we could define a variable twice in different scopes.
                {
                    string VarDef = Indent0 + Name + ": " + VarType + ";" + Environment.NewLine;
                    if (FCommentMode)
                    {
                        AddComment("var " + Name + ": " + VarType + ";", false);
                    }
                    else
                    {
                        if (!HasVar)
                        {
                            string vv = "var" + Environment.NewLine;
                            s.Insert(VarPos, vv);
                            VarPos += vv.Length;
                            HasVar = true;
                            s.Insert(VarPos, Environment.NewLine);
                        }
                        s.Insert(VarPos, VarDef); 
                        VarPos += VarDef.Length;
                        DefinedVars.Add(Name, Name);
                    }
                }

                if (Def != null) SetVarProp(Name, Def);
            }
        }
        #endregion

        #region Formatting
        public override string GetReservedWord(string s)
        {
            return "&" + s;
        }

        public override string GetChar(string s1)
        {
            return "'" + s1 + "'";
        }

        public override string GetString(string s1)
        {
            const int LineLimit = 80;
            int AcumLen = 0;

            bool LastWasChar = false;
            bool LastWasCode = false;

            if (s1 == null) return NullString;

            StringBuilder sb = new StringBuilder(s1.Length);
            for (int i = 0; i < s1.Length; i++)
            {
                if (s1[i] == ' ' && sb.Length - AcumLen > LineLimit)
                {
                    sb.Append("\'" + Environment.NewLine + Indent + "+ \'");
                    AcumLen = sb.Length;
                }

                if (s1[i] == '\'') sb.Append("\'");

                if (s1[i] < ' ')
                {
                    if (LastWasChar) sb.Append("\'");
                    if (i > 0) sb.Append(" + ");
                    sb.Append("#" + ((int)s1[i]).ToString(CultureInfo.InvariantCulture));

                    LastWasChar = false;
                    LastWasCode = true;
                }
                else
                {
                    if (LastWasCode) sb.Append (" + ");
                    if (LastWasCode || i == 0) sb.Append("\'");
                    sb.Append(s1[i]);
                    LastWasChar = true;
                    LastWasCode = false;
                }
            }

            if (LastWasChar) sb.Append("\'");
            while (sb.Length < 2) sb.Append("\'");
            return sb.ToString();
        }

        public override string CreateObject(string ObjectName, string Params)
        {
            if (Oxygene)
            {
                if (Params == null || Params.Length == 0) return "new " + ObjectName;
                return "new " + ObjectName + "(" + Params + ")";
            }
            else
            {
                return ObjectName + ".Create(" + Params + ")";
            }
        }

        public override string CreateObjectArray(string ObjectName, string ArrayType, string ArrayLen)
        {
            string VarName = ObjectName == null ? String.Empty : ObjectName + " := ";
            
            if (Oxygene)
            {
                return VarName + "new " + ArrayType + "[" + ArrayLen + "]";			
            }
            else
            {
                return "SetLength(" + ObjectName + ", " + ArrayLen + ")";
            }
        }

        protected override void CreateAndDefineObjectArray(string VarName, string VarType, int ArrayLen)
        {
            if (Oxygene)
            {
                base.CreateAndDefineObjectArray(VarName, VarType, ArrayLen);
                return;
            }

            InsertVarAndTrack(VarName, GetArrayDecl(VarType), NullString); //needed to clear old assignments.

            if (ArrayLen < 0)
            {
                SetVarProp(VarName, NullString);
            }
            else
            {
                WriteLine(Indent + CreateObjectArray(VarName, VarType, ArrayLen) + Eol);
            }
        }

        public override string GetArrayDecl(string ArrType)
        {
            return "Array of " + ArrType;
        }

        public override void DefineArray(bool DefineVar, string Name, string ArrType, int BreakAt, int DataLength, Func<int, string> DataGenerator)
        {
            if (Oxygene)
            {
                string VarDef = DefineVar ? CheckVar(Name, "var " + Name + ": " + GetArrayDecl(ArrType), Name): Name;
                WriteLine(Indent + VarDef  + EqualStr + " [");

                StringBuilder List = new StringBuilder();
                for (int i = 0; i < DataLength; i++)
                {
                    List.Append(DataGenerator(i));
                    if (i < DataLength - 1) List.Append(", ");
                    if ((i + 1) % 50 == 0)
                    {
                        WriteLine(Indent + Indent + List.ToString());
                        List = new StringBuilder();
                    }
                }
                if (List.Length > 0) WriteLine(Indent + Indent + List.ToString());

                WriteLine(Indent + "]" + Eol);
            }

            else
            {
                Eol = "";
                try
                {
                    InsertVar(Name, GetArrayDecl(ArrType), " CreateArray " + "(");
                }
                finally
                {
                    Eol = ";";
                }

                StringBuilder List = new StringBuilder();

                for (int i = 0; i < DataLength; i++)
                {
                    List.Append(DataGenerator(i));
                    if (i < DataLength - 1) List.Append(", ");
                    if ((i + 1) % 50 == 0)
                    {
                        WriteLine(Indent + Indent + List.ToString());
                        List = new StringBuilder();
                    }
                }
                if (List.Length > 0) WriteLine(Indent + Indent + List.ToString());

                WriteLine(Indent + ")" + Eol);
            }
        }

        public override string GetCallArray(int index)
        {
            return "[" + index.ToString(CultureInfo.InvariantCulture) + "]";
        }

        public override string GetCallVarMethod(string Prefix, params object[] args)
        {
            return GetCallVarMethodInternal(Prefix, false, args);
        }

        public override string Cast(string aType, object value)
        {
            return aType + "(" + Convert.ToString(value) + ")";
        }

        public override string GetBinaryOr
        {
            get
            {
                return "or";
            }
        }

        public override string Hexa(byte b)
        {
            return "$" + String.Format(CultureInfo.InvariantCulture, "{0:X2}", b);
        }

        public override void StartUsing(string ClassName, string VarName, string VarValue)
        {
            if (Oxygene)
            {
                WriteLine(Indent + "using " + VarName + " := " + VarValue + " do");
                WriteLine(Indent + "begin");
                Indent += Indent0;
            }
            else
            {
                InsertVar(VarName, ClassName, VarValue);
                WriteLine(Indent + "try");
                Indent += Indent0;
            }
        }

        public override void EndUsing(string VarName, bool Close)
        {
            if (Oxygene)
            {
                Indent = Indent.Remove(0, Indent0.Length);
                WriteLine(Indent + "end;");
            }
            else
            {
                Indent = Indent.Remove(0, Indent0.Length);
                WriteLine(Indent + "finally");
                string Op = Close? ".Close;" : ".Dispose;";
                WriteLine(Indent + Indent0 + VarName + Op);
                WriteLine(Indent + "end;");
            }
        }


        #endregion

        #region Excel
        public override void Start(bool ShowUsing, bool SaveXls, bool SaveCsv, bool SavePxl, bool SavePdf, bool SaveHtml, bool SaveBmp, bool AspNetCode)
        {
            DefinedVars.Clear();
            if (ShowUsing)
            {
                string uses = "uses System.IO, System.Globalization";
                if (SavePxl) 
                    uses += ", System.Text";

                uses += ", FlexCel.Core, FlexCel.XlsAdapter";
                if (SaveHtml || SavePdf || SaveBmp) 
                    uses +=", FlexCel.Render";
                if (SavePdf) uses +=", FlexCel.Pdf";
                WriteLine(uses + ";");
                WriteLine(String.Empty);
                if (!Oxygene)
                {
                    WriteLine("{$AUTOBOX ON}");
                    WriteLine(String.Empty);
                }
            }

            if (AspNetCode)
                WriteLine("procedure CreateAndSendFile;");
            else
                WriteLine("procedure CreateAndSaveFile;");

            if (Oxygene)
            {
                WriteLine("begin");
                WriteLine(Indent + "var " + XlsFileName +" := new XlsFile(true);");
            }
            else
            {
                WriteLine("var");
                WriteLine("  "+ XlsFileName +": XlsFile;");
                DefinedVars.Clear();
                HasVar = true;
                VarPos = s.Length;
                WriteLine("begin");
                WriteLine("  " + XlsFileName + " := XlsFile.Create(true);");
            }
            
            WriteLine(Indent + GetCallVarMethod(String.Empty, "CreateFile", XlsFileName) + Eol);
            SaveTheFile(SaveXls, SaveCsv, SavePxl, SavePdf, SaveHtml, SaveBmp, AspNetCode);

            Finish();
            WriteLine();

            if (AspNetCode) AddSendToBrowser();

            WriteLine("procedure CreateFile(const " + XlsFileName + ":ExcelFile);");
            if (!Oxygene)
            {
                DefinedVars.Clear();
                HasVar = false;
                VarPos = s.Length;
            }
            WriteLine("begin");
        }

        public override void Finish()
        {
            WriteLine("end;");
        }

        protected override string GetLengthArray(string ArrayName)
        {
            return "Length(" + ArrayName + ")";
        }

        public override void AddSendToBrowser()
        {
            WriteLine("procedure SendToBrowser(const OutStream: MemoryStream; const MimeType: string; const FileName: string);");
            HasVar = false;
            VarPos = s.Length;
            WriteLine("begin");
            base.AddSendToBrowser ();
            WriteLine("end" + Eol);
            WriteLine();
        }

        public override void SetHeaderImg(THeaderAndFooterKind HKind, THeaderAndFooterPos Section, byte[] Img, TXlsImgType ImgType, THeaderOrFooterImageProperties ImgProps, bool FirstImg)
        {
            string ImgName = NullString;
            string ImgPropsName = NullString;
            if (Img != null)
            {
                ImgName = "HeaderImgData";
                ImgPropsName = "HeadImgProps";

                if (Oxygene)
                {
                    WriteLine(Indent + "using ms := new FileStream('imagename" + GetExt(ImgType) + "', FileMode.Open, FileAccess.Read, FileShare.ReadWrite) do");
                    WriteLine(Indent + "begin");
                }
                else
                {
                    InsertVar("ms", "FileStream");
                    WriteLine(Indent + "ms := FileStream.Create('imagename" + GetExt(ImgType) + "', FileMode.Open, FileAccess.Read, FileShare.ReadWrite);");
                    WriteLine(Indent + "try");
                }
                Indent += Indent0;

                InsertVar(ImgName, GetArrayDecl("Byte"));
                WriteLine(Indent + CreateObjectArray(ImgName, "Byte", "ms.Length") + Eol);
                WriteLine(Indent + Call("ms", "Read") + String.Format(CultureInfo.InvariantCulture, "({0}, 0, Length({0}))", ImgName) + Eol);
                WriteHeaderImgProps(ImgProps, ImgPropsName);

            }
            CallMethod("SetHeaderOrFooterImage",
                GetEnum(HKind),
                GetEnum(Section),
                ImgName,
                GetEnum(ImgType),
                ImgPropsName
                );
            

            if (ImgName != NullString)
            {
                Indent = Indent0;
                if (!Oxygene)
                {
                    WriteLine(Indent + "finally");
                    WriteLine(Indent + Indent0 + "ms.Close;");
                }
                WriteLine(Indent + "end;");
            }

        }

        public override void AddImg(byte[] ImageData, TImageProperties ImgProps, string ImgPropsName, TXlsImgType ImgType, bool FirstImg)
        {
            if (Oxygene)
            {
                WriteLine(Indent + "using fs := new FileStream('imagename" + GetExt(ImgType) + "', FileMode.Open, FileAccess.Read, FileShare.ReadWrite) do");
                WriteLine(Indent + "begin");
            }
            else
            {
                InsertVar("fs", "FileStream");
                WriteLine(Indent + "fs := FileStream.Create('imagename" + GetExt(ImgType) + "', FileMode.Open, FileAccess.Read, FileShare.ReadWrite);");
                WriteLine(Indent + "try");
            }

            Indent += Indent0;

            WriteImgProps(ImgProps, "ImgProps", "TImageProperties", FirstImg || Oxygene, null);

            CallMethod("AddImage", 
                "fs",
                ImgPropsName
                );
            
            Indent = Indent0;
            if (!Oxygene)
            {
                WriteLine(Indent + "finally");
                WriteLine(Indent + Indent0 + "fs.Close;");
            }
            WriteLine(Indent + "end;");
        }


        #endregion

        public override string IntegerString()
        {
            return "integer";
        }
    }

    public class DiffEngine : LanguageEngine
    {
        public DiffEngine(ExcelFile aXls): base("", "", "true", "false", "=", "null", "  ", aXls)
        {

        }

        public override String GetBinaryOr
        {
            get
            {
                return " or ";
            }
        }

        public override void AddImg(byte[] ImageData, TImageProperties ImgProps, String ImgPropsName, TXlsImgType ImgType, Boolean FirstImg)
        {
            using (System.Security.Cryptography.MD5 md = System.Security.Cryptography.MD5.Create())
            {
                WriteLine(Indent + "Image: " + GetExt(ImgType) + " MD5 HASH: " + PrettyPrint(md.ComputeHash(ImageData)));
            }
            WriteImgProps(ImgProps, "ImgProps", "TImageProperties", true, null);

        }

        private String PrettyPrint(Byte[] v)
        {
            return BitConverter.ToString(v);
        }

        public override String Cast(String aType, Object value)
        {
            return "";
        }

        public override String CreateObject(String ObjectName, String Params)
        {
            return Params;
        }

        public override String CreateObjectArray(String ObjectName, String ArrayType, String ArrayLen)
        {
            return ArrayType + "[" + ArrayLen.ToString() + "]";
        }

        public override void DefineArray(bool DefineVar, string Name, string ArrType, int BreakAt, int DataLength, Func<int, string> DataGenerator)
        {
        }

        public override void EndUsing(String VarName, Boolean Close)
        {      
        }

        public override void Finish()
        {    
        }

        public override String GetArrayDecl(String ArrType)
        {
            return ArrType + "[]";
        }

        public override String GetCallArray(Int32 index)
        {
            return index.ToString();
        }

        public override String GetCallVarMethod(String Prefix, params Object[] args)
        {
            return GetCallVarMethodInternal(Prefix, true, args);
        }

        public override String GetChar(String s1)
        {
            return "\"" + s1 + "\"";
        }

        public override String GetReservedWord(String s)
        {
            return s;
        }

        public override String GetString(String s1)
        {
            return "\"" + s1 + "\"";
        }

        public override String Hexa(Byte b)
        {
            return String.Format(CultureInfo.InvariantCulture, "{0:X2}", b) + "hex";

        }

        public override void InsertVarAndTrack(string Name, string VarType, string Def)
        {
        }

        public override void InsertVar(String Name, String VarType, String Def)
        {
        }

        public override String IntegerString()
        {
            return "int";
        }

        public override void SetHeaderImg(THeaderAndFooterKind HKind, THeaderAndFooterPos Section, Byte[] Img, TXlsImgType ImgType, THeaderOrFooterImageProperties ImgProps, Boolean FirstImg)
        {
            //throw new NotImplementedException();
        }

        public override void Start(Boolean ShowUsing, Boolean SaveXls, Boolean SaveCsv, Boolean SavePxl, Boolean SavePdf, Boolean SaveHtml, Boolean SaveBmp, Boolean AspNetCode)
        {
                DefinedVars.Clear();
        }

        public override String StartComment(String Comment)
        {
            return "";
        }

        public override void StartUsing(String ClassName, String VarName, String VarValue)
        {            
        }

        public override void AddComment(String Comment, Boolean InNextLine)
        {
        }

        public override void AddSendToBrowser()
        {
        }

        public override void AddSheets(Int32 SheetCount, TExcelFileFormat ExcelFileFormat)
        {
            WriteLine(Indent + "File Format: "  + ExcelFileFormat.ToString());
            WriteLine(Indent + "Sheets: "  + SheetCount.ToString());
        }

        public override String Call(String ClassDef, String Method)
        {
            return Method;
        }

        public override String CallBase(String Method)
        {
            return Method;
        }

        public override String CreateFormula(String FmlaText, TFormulaSpan Span)
        {
            if (Span.IsOneCell)
            {
                return "TFormula: " + GetString(FmlaText);
            }

            return 
                "Array TFormula (Rows: " + Span.RowSpan.ToString() + " Cols: " + Span.ColSpan.ToString() + "): " + FmlaText;
        }

        public override void SaveTheFile(Boolean SaveXls, Boolean SaveCsv, Boolean SavePxl, Boolean SavePdf, Boolean SaveHtml, Boolean SaveBmp, Boolean AspNetCode)
        {
        }

        public override void SetCellValue(Int32 row, Int32 col, String val)
        {
            WriteLine(Indent + new TCellAddress(row, col).CellRef + ": " + val);
        }

        public override void SetCellFormat(String VarName, Int32 row, Int32 col, TFlxFormat fmt, TFlxApplyFormat Apply, Boolean FormatFromStyle)
        {
            WriteLine(Indent + new TCellAddress(row, col).CellRef + ": " + CalcFormat(fmt, Apply));
        }

        private String CalcFormat(TFlxFormat fmt, TFlxApplyFormat Apply)
        {
            StringBuilder Result = new StringBuilder();
            //Font
           // Result.Append(GetFont(fmt.Font, Apply.Font));

            //Borders
            StringBuilder Result2 = new StringBuilder();
            if (Apply.Borders.Left)
            {
                Append(Result2, "LEFT: Color: " + GetColor(fmt.Borders.Left.Color, false) + " Style: " + GetColor(fmt.Borders.Left.Color, false));
            }
            if (Apply.Borders.Right)
            {
                Append(Result2, "RIGHT: Color: " + GetColor(fmt.Borders.Right.Color, false) + " Style: " + GetColor(fmt.Borders.Right.Color, false));
            }
            if (Apply.Borders.Top)
            {
                Append(Result2, "TOP: Color: " + GetColor(fmt.Borders.Top.Color, false) + " Style: " + GetColor(fmt.Borders.Top.Color, false));
            }

            if (Apply.Borders.Bottom)
            {
                Append(Result2, "BOTTOM: Color: " + GetColor(fmt.Borders.Bottom.Color, false) + " Style: " + GetColor(fmt.Borders.Bottom.Color, false));
            }

            if (Apply.Borders.Diagonal && fmt.Borders.DiagonalStyle == TFlxDiagonalBorder.DiagDown || fmt.Borders.DiagonalStyle == TFlxDiagonalBorder.Both)
            {
                Append(Result2, "DIAGONAL DOWN: Color: " + GetColor(fmt.Borders.Diagonal.Color, false) + " Style: " + GetColor(fmt.Borders.Diagonal.Color, false));
            }

            if (Apply.Borders.Diagonal && fmt.Borders.DiagonalStyle == TFlxDiagonalBorder.DiagUp || fmt.Borders.DiagonalStyle == TFlxDiagonalBorder.Both)
            {
                Append(Result2, "DIAGONAL UP: Color: " + GetColor(fmt.Borders.Diagonal.Color, false) + " Style: " + GetColor(fmt.Borders.Diagonal.Color, false));
            }


            if (Result2.Length > 0)
            {
                Result.Append("  Borders: {");
                Result.Append(Result2);
                Result.Append("}");
            }

/*
            //Pattern
            if (Apply.FillPattern.Pattern)
            {
                Result.Append(WriteLine(Indent + VarName + ".FillPattern.Pattern " + EqualStr + " " + GetEnum(fmt.FillPattern.Pattern) + Eol);
            }

            //we need to apply fg and bg too for Excel 2003.
            if (Apply.FillPattern.FgColor) WriteLine(Indent + VarName + ".FillPattern.FgColor " + EqualStr + " " + GetColor(fmt.FillPattern.FgColor, true) + Eol);
            if (Apply.FillPattern.BgColor) WriteLine(Indent + VarName + ".FillPattern.BgColor " + EqualStr + " " + GetColor(fmt.FillPattern.BgColor, true) + Eol);
            if (Apply.FillPattern.Gradient && fmt.FillPattern.Pattern == TFlxPatternStyle.Gradient)
            {
                string GradientStops = NullString;
                if (fmt.FillPattern.Gradient != null)
                {
                    GradientStops = WriteGradientStops(fmt.FillPattern.Gradient);
                }
                WriteLine(Indent + VarName + ".FillPattern.Gradient " + EqualStr + " " + GetGradient(GradientStops, fmt.FillPattern.Gradient) + Eol);
            }

            //Parent Style
            string DisplayName = GetStyleName(fmt);
            if (Apply.ParentStyle) WriteLine(Indent + VarName + ".ParentStyle " + EqualStr + " " + DisplayName + Eol);

            //Others
            if (Apply.Format) WriteLine(Indent + VarName + ".Format " + EqualStr + " " + GetString(fmt.Format) + Eol);

            if (Apply.HAlignment) WriteLine(Indent + VarName + ".HAlignment " + EqualStr + " " + GetEnum(fmt.HAlignment) + Eol);
            if (Apply.VAlignment) WriteLine(Indent + VarName + ".VAlignment " + EqualStr + " " + GetEnum(fmt.VAlignment) + Eol);

            if (Apply.Locked) WriteLine(Indent + VarName + ".Locked " + EqualStr + " " + GetBool(fmt.Locked) + Eol);
            if (Apply.Hidden) WriteLine(Indent + VarName + ".Hidden " + EqualStr + " " + GetBool(fmt.Hidden) + Eol);
            if (Apply.WrapText) WriteLine(Indent + VarName + ".WrapText " + EqualStr + " " + GetBool(fmt.WrapText) + Eol);
            if (Apply.ShrinkToFit) WriteLine(Indent + VarName + ".ShrinkToFit " + EqualStr + " " + GetBool(fmt.ShrinkToFit) + Eol);
            if (Apply.Rotation) WriteLine(Indent + VarName + ".Rotation " + EqualStr + " " + fmt.Rotation + Eol);
            if (Apply.Indent) WriteLine(Indent + VarName + ".Indent " + EqualStr + " " + fmt.Indent + Eol);
            if (Apply.ReadingOrder) WriteLine(Indent + VarName + ".ReadingOrder " + EqualStr + " " + GetEnum(fmt.ReadingOrder) + Eol);
            if (Apply.Lotus123Prefix) WriteLine(Indent + VarName + ".Lotus123Prefix " + EqualStr + " " + GetBool(fmt.Lotus123Prefix) + Eol);
            */
            return Result.ToString();
        }

        private void Append(StringBuilder result2, String v)
        {
            if (result2.Length > 0)
            {
                result2.Append(", ");
            }
            result2.Append("(");
            result2.Append(v);
            result2.Append(")");
        }
    }
}
