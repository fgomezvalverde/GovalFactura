using System;
using System.Linq;

namespace ApiMate
{
	sealed class AppConstants
	{
        public static readonly string[] ValidExtensionsDot = new string[]
        {
            ".XLS",
            ".XLT",
            ".XLSX",
            ".XLSM",
            ".XLTX",
            ".XLTM"
        };

        public static readonly string[] ValidExtensionsNoDot = (from p in ValidExtensionsDot select p.Substring(1)).ToArray();

        public const string langSelectedKey = "LangSelected";
        public const string fmlaSelectedKey = "FmlaSelected";


	}
}


