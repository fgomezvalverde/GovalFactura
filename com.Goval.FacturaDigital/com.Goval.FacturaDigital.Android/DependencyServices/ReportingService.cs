using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Goval.FacturaDigital;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using com.Goval.FacturaDigital.Droid.DependencyServices;
using FlexCel.XlsAdapter;
using System.IO;
using FlexCel.Render;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using com.Goval.FacturaDigital.Droid.Utils;
using com.Goval.FacturaDigital.Abstraction.DependencyServices;

[assembly: Xamarin.Forms.Dependency(typeof(ReportingService))]
namespace com.Goval.FacturaDigital.Droid.DependencyServices
{
    public class ReportingService : IReportingService
    {
        string pdfResultName = "FacturaN{0}.pdf";

        #region Interface Implementation

        public void SaveAndOpenFile(string pFileName, byte[] pData)
        {
            SaveAndroid androidSave = new SaveAndroid();
            using (MemoryStream vStream = new MemoryStream(pData))
            {
                if (pFileName.Contains(".pdf"))
                {
                    androidSave.Save(pFileName, "application/pdf", vStream);
                }
                
            }
                
        }
        #endregion
    }
}