using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.Test
{
    public class Utils
    {

        static string[] ones = new string[] { "", "Uno", "Dos", "Tres", "Cuatro", "Cinco", "Seis", "Siete", "Ocho", "Nueve" };
        static string[] teens = new string[] { "Diez", "Once", "Doce", "Trece", "Catorce", "Quince", "Dieciseis", "Diecisiete", "Dieciocho", "Diecinueve" };
        static string[] tens = new string[] { "Veinte", "Treinta", "Cuarenta", "Cincuenta", "Sesenta", "Setenta", "Ochenta", "Noventa" };
        static string[] thousandsGroups = { "", " Mil", " Millones", " Billones" };

        private static string FriendlyInteger(int n, string leftDigits, int thousands)
        {
            if (n == 0)
            {
                return leftDigits;
            }

            string friendlyInt = leftDigits;

            if (friendlyInt.Length > 0)
            {
                friendlyInt += " ";
            }

            if (n < 10)
            {
                friendlyInt += ones[n];
            }
            else if (n < 20)
            {
                friendlyInt += teens[n - 10];
            }
            else if (n < 100)
            {
                friendlyInt += FriendlyInteger(n % 10, tens[n / 10 - 2], 0);
            }
            else if (n < 1000)
            {
                friendlyInt += FriendlyInteger(n % 100, (ones[n / 100] + " Ciento"), 0);
            }
            else
            {
                friendlyInt += FriendlyInteger(n % 1000, FriendlyInteger(n / 1000, "", thousands + 1), 0);
                if (n % 1000 == 0)
                {
                    return friendlyInt;
                }
            }

            return friendlyInt + thousandsGroups[thousands];
        }

        public static string IntegerToWritten(int number)
        {
            if (number == 0)
            {
                return "Cero";
            }
            else if (number < 0)
            {
                return "Negative " + IntegerToWritten(-number);
            }

            return FriendlyInteger(number, "", 0);
        }

        public static Dictionary<string, string> BillToDictionary(Model.Bill ActualBill)
        {
            var values = new Dictionary<string, string>();

            //Products Values
            for (int cont = 0; cont < ActualBill.AssignClient.Products.Count; cont++)
            {

                values.Add("productAmount" + cont,
                    ActualBill.AssignClient.Products[cont].Amount + "");

                values.Add("productCode" + cont,
                    ActualBill.AssignClient.Products[cont].Code + "");

                values.Add("productDescription" + cont,
                    ActualBill.AssignClient.Products[cont].Description + "");

                values.Add("productPrice" + cont,
                    ActualBill.AssignClient.Products[cont].Price + "");

                values.Add("productTotalCost" + cont,
                    ActualBill.AssignClient.Products[cont].Amount *
                      ActualBill.AssignClient.Products[cont].Price + "");


            }

            //Client Info
            values.Add("clientId",
                    ActualBill.AssignClient.ClientId);
            values.Add("clientName",
                    ActualBill.AssignClient.Name);
            values.Add("clientDirection",
                    ActualBill.AssignClient.Direction);
            values.Add("clientTelephone",
                    ActualBill.AssignClient.Telephone);
            values.Add("clientEmail",
                    ActualBill.AssignClient.Email);
            values.Add("clientLegalId",
                    ActualBill.AssignClient.LegalId);
            values.Add("clientDiscount",
                string.Format("DESCUENTO({0}%)", ActualBill.AssignClient.DiscountPercentage));
            values.Add("clientTaxes",
                string.Format("IMPUESTO VENTAS({0}%)", ActualBill.AssignClient.TaxesPercentage));
            values.Add("clientTerm",
                    ActualBill.AssignClient.Term + " dias");


            //Bill Info
            values.Add("billDate",
                    ActualBill.BillDate.ToString("g"));
            values.Add("billId",
                    "N° " + ActualBill.Id);
            values.Add("billTotalProducts",
                    "¢" + ActualBill.subTotalProducts);
            values.Add("billDescountAmount",
                    "¢" + ActualBill.discountAmount);
            values.Add("billTotalAfterDescount",
                    "¢" + ActualBill.totalAfterDiscount);
            values.Add("billTaxesAmount",
                    "¢" + ActualBill.taxesToPay);
            values.Add("billTotal",
                    "¢" + ActualBill.TotalToPay);

            values.Add("billTotalInText",
                   Utils.IntegerToWritten(((int)ActualBill.TotalToPay)));

            return values;
        }
    }
}
