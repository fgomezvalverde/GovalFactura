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
using Syncfusion.XlsIO;
using System.Reflection;
using System.Net;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf;
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
        public async Task<Stream> CreateAndRunReport(Dictionary<string,string> pBill,string pBillNumber,string pExcelFileName)
        {
            ExcelEngine excelEngine = new ExcelEngine();
            IApplication application = excelEngine.Excel;
            string billReportName = string.Format(pdfResultName, pBillNumber);

            application.DefaultVersion = ExcelVersion.Excel2013;

            string resourcePath = "com.Goval.FacturaDigital.Droid.Reports."+ pExcelFileName;
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream fileStream = assembly.GetManifestResourceStream(resourcePath);

            IWorkbook workbook = application.Workbooks.Open(fileStream);
            IWorksheet sheet = workbook.Worksheets[0];

            foreach (Syncfusion.XlsIO.Implementation.NameImpl namedVariable in workbook.Names)
            {
                string value = "";
                if (pBill.TryGetValue(namedVariable.Address, out value))
                {
                    sheet.Range[namedVariable.AddressLocal].Text = value;
                }
                else
                {
                    Console.WriteLine("ERROR", "ESTO NO PUEDE PASAR");
                }
            }

            MemoryStream stream = new MemoryStream();
            workbook.SaveAs(stream);
            workbook.Close();
            excelEngine.Dispose();
            var data = stream.ToArray();
            var stringData = Convert.ToBase64String(data);
            JObject obj = new JObject();
            //obj["api_key"] = com.Goval.FacturaDigital.Utils.ConfigurationConstants.PDFGeneratorKey;
            obj["document"] = stringData;
            try
            {
                var httpClient = new HttpClient();
                var uri = new Uri("http://getoutpdf.com/api/convert/document-to-pdf");
                var content = new StringContent(obj.ToString(), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync(uri, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var jobjectResponse = JObject.Parse(responseString);
                    var stringBase64 = jobjectResponse["pdf_base64"].Value<String>();
                    var responseBytes = Convert.FromBase64String(stringBase64);
                    try
                    {
                        SaveAndroid androidSave = new SaveAndroid();
                        var streamResult = new MemoryStream(responseBytes);
                        androidSave.Save(billReportName, "application/pdf", streamResult);
                        return streamResult;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                    
            }

            catch (WebException exception)
            {
                string responseText;
                using (var reader = new StreamReader(exception.Response.GetResponseStream()))
                {
                    responseText = reader.ReadToEnd();
                    Console.WriteLine(responseText);
                }
            }
            return null;
        }
        #endregion


        #region Flexcel
        public async Task<Stream> CreateAndRunReport1(Dictionary<string, string> pBill, string pBillNumber, string pExcelFileName)
        {
            XlsFile reportFile = new XlsFile(true);
            using (var template = Android.App.Application.Context.Assets.Open(pExcelFileName))
            {
                using (var memTemplate = new MemoryStream())
                {
                    template.CopyTo(memTemplate);
                    memTemplate.Position = 0;
                    reportFile.Open(memTemplate);
                }
            }
            var billObj = JObject.FromObject(pBill);
            for (int namedVariableCount = 1; namedVariableCount <= reportFile.NamedRangeCount; namedVariableCount++)
            {
                var namedVariable = reportFile.GetNamedRange(namedVariableCount);
                if(namedVariable != null)
                {
                    var property = billObj[namedVariable.Name];
                    if (property != null)
                    {
                        switch (property.Type)
                        {
                            case JTokenType.Boolean:
                                reportFile.SetCellValue(namedVariable.Top, namedVariable.Left, property.Value<Boolean>());
                                break;
                            case JTokenType.Float:
                                reportFile.SetCellValue(namedVariable.Top, namedVariable.Left, property.Value<float>());
                                break;
                            case JTokenType.String:
                                reportFile.SetCellValue(namedVariable.Top, namedVariable.Left, property.Value<string>());
                                break;
                            case JTokenType.Integer:
                                reportFile.SetCellValue(namedVariable.Top, namedVariable.Left, property.Value<double>());
                                break;
                            case JTokenType.Date:
                                var date = property.Value<DateTime>();
                                reportFile.SetCellValue(namedVariable.Top, namedVariable.Left, date.ToString());
                                break;
                            default:
                                reportFile.SetCellValue(namedVariable.Top, namedVariable.Left, property.ToString());
                                break;
                        }
                    }
                }
            }
            reportFile.Recalc();

            string path = System.IO.Path.Combine(
            Android.OS.Environment.ExternalStorageDirectory.Path, "result.pdf");
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
            Stream stream = File.OpenRead(path);
            return stream;
        }

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