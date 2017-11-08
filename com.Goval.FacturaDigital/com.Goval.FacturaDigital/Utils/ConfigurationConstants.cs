using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.Utils
{
    public class ConfigurationConstants
    {
        public static string PDFGeneratorKey = "a09f3c3881f70d340e8746d136bb4c763299a6b6a7a824853f0ac442c16ef998";
        public static List<String> EmailsToSendBill = new List<String>() { "ivangomez15@yahoo.com", "ivangomez1505@gmail.com" };
        public static string MailBillSubject = "Factura #{0} ";
        public static Boolean SendBillToClientEmail = false;

        public static string BillNameFile = "FacturaN{0}.pdf";
        public static string DateTimeFormat = "d";
        public static CultureInfo Culture = new CultureInfo("pt-BR");
        public static Boolean NumberToNameInCapitalLetters = true;

        public static string BillExcelOriginalFormat = "FacturaGovalFormatoOriginal.xlsx";
        public static string BillExcelCopyFormat = "FacturaGovalFormatoCopia.xlsx";
        public static string BillExcelCanceledFormat = "FacturaGovalFormatoAnulado.xlsx";
    }
}
