using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.Utils
{
    public class ConfigurationConstants
    {

        public static List<String> EmailsToSendBill = new List<String>() { "fabiangomezvalverde@gmail.com", "ivangomez15@yahoo.com", "ivangomez1505@gmail.com" };
        public static string MailBillSubject = "Factura #{0} ";

        public static string BillNameFile = "FacturaN{0}.pdf";
        public static Boolean NumberToNameInCapitalLetters = true;
    }
}
