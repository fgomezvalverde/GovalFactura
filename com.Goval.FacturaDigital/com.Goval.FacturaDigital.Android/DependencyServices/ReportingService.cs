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
using com.Goval.FacturaDigital.DependencyServices;
using com.Goval.FacturaDigital.Droid.DependencyServices;
using FlexCel.XlsAdapter;
using System.IO;
using FlexCel.Render;
using com.Goval.FacturaDigital.Model;
using Newtonsoft.Json.Linq;

[assembly: Xamarin.Forms.Dependency(typeof(ReportingService))]
namespace com.Goval.FacturaDigital.Droid.DependencyServices
{
    public class ReportingService : IReportingService
    {
        #region SpecialLogic
        private XlsFile OpenFile()
        {
            XlsFile reportFile = new XlsFile(true);

            using (var template = Android.App.Application.Context.Assets.Open("Factura.xlsx"))
            {
                using (var memTemplate = new MemoryStream())
                {
                    template.CopyTo(memTemplate);
                    memTemplate.Position = 0;
                    reportFile.Open(memTemplate);
                }
            }


            return reportFile;
        }

        private XlsFile EvaluateBill(Bill pBill,XlsFile pReport)
        {
            var billObj = JObject.FromObject(pBill);
            for (int namedVariableCount = 1; namedVariableCount <= pReport.NamedRangeCount; namedVariableCount++)
            {
                var namedVariable = pReport.GetNamedRange(namedVariableCount);
                if (namedVariable != null)
                {
                    var property = billObj[namedVariable.Name];
                    switch(property.Type)
                    {
                        case JTokenType.Boolean:
                            pReport.SetCellValue(namedVariable.Top, namedVariable.Left, property.Value<Boolean>());
                            break;
                        case JTokenType.Float:
                            pReport.SetCellValue(namedVariable.Top, namedVariable.Left, property.Value<float>());
                            break;
                        case JTokenType.String:
                            pReport.SetCellValue(namedVariable.Top, namedVariable.Left, property.Value<string>());
                            break;
                        case JTokenType.Integer:
                            pReport.SetCellValue(namedVariable.Top, namedVariable.Left, property.Value<double>());
                            break;
                        case JTokenType.Date:
                            var date = property.Value<DateTime>();
                            pReport.SetCellValue(namedVariable.Top, namedVariable.Left, date.ToString());
                            break;
                        default:
                            pReport.SetCellValue(namedVariable.Top, namedVariable.Left, property.ToString());
                            break;

                    }
                }
            }

            pReport.Recalc();
            return pReport;
        }

        #endregion

        #region Interface Implementation
        public void RunReport(Bill pBill)
        {
            var reportFile = OpenFile();
            reportFile = EvaluateBill(pBill, reportFile);
            string path = System.IO.Path.Combine(
                Android.OS.Environment.ExternalStorageDirectory.Path, "result.pdf"
                );
            using (FlexCelPdfExport pdf = new FlexCelPdfExport(reportFile, true))
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    pdf.FontMapping = FlexCel.Pdf.TFontMapping.ReplaceAllFonts;
                    pdf.Export(fs);
                }
            }
            Java.IO.File file = new Java.IO.File(path);
            Intent intent = new Intent(Intent.ActionView);
            intent.SetDataAndType(Android.Net.Uri.FromFile(file), "application/pdf");
            intent.SetFlags(ActivityFlags.NewTask);
            Android.App.Application.Context.StartActivity(intent);
        }
#endregion
    }
}