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
using com.Goval.FacturaDigital.Model;
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
        string fileName = "FacturaGoval.xlsx";
        string pdfResultName = "FacturaN{0}.pdf";

        #region Interface Implementation
        public async Task<Stream> CreateAndRunReport(Dictionary<string,string> pBill,string pBillNumber)
        {
            ExcelEngine excelEngine = new ExcelEngine();
            IApplication application = excelEngine.Excel;
            string billReportName = string.Format(pdfResultName, pBillNumber);

            application.DefaultVersion = ExcelVersion.Excel2013;

            string resourcePath = "com.Goval.FacturaDigital.Droid.Reports."+ fileName;
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
            obj["api_key"] = com.Goval.FacturaDigital.Utils.ConfigurationConstants.PDFGeneratorKey;
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
    }
}