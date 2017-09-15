using com.Goval.FacturaDigital.Amazon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.Utils
{
    public static class BillSecurity
    {
        public async static Task<List<Model.Bill>> GetBillList()
        {
            var billList = await DynamoDBManager.GetInstance().GetItemsAsync<Model.Bill>();
            if (billList != null && billList.Count != 0)
            {
                if (billList.Exists(bill => bill.Id == App.StarterBillNumber))
                {
                    var billResult = billList.Where(bill => bill.Id != App.StarterBillNumber);

                    if (billResult != null)
                    {
                        return billResult.ToList();
                    }
                    else
                    {
                        return new List<Model.Bill>();
                    }
                }
                else
                {
                    throw new Exception("GetBillList.No estaba el Bill base con el Numbero:" + App.StarterBillNumber);
                }
            }
            else
            {
                return null;
            }
        }

        public async static Task SaveBaseBill()
        {
            await DynamoDBManager.GetInstance().SaveAsync<Model.Bill>(
                new Model.Bill {Id = App.StarterBillNumber });

        }

        public async static Task<int> GetNextBillNumber()
        {
            var billList = await DynamoDBManager.GetInstance().GetItemsAsync<Model.Bill>();
            if (billList != null && billList.Count != 0)
            {
                if (billList.Exists(bill => bill.Id == App.StarterBillNumber))
                {
                    var billResult = billList.OrderByDescending(bill => bill.Id);
                    return billResult.First().Id + 1;
                }
                else
                {
                    throw new Exception("GetNextBillNumber.No estaba el Bill base con el Numbero:" + App.StarterBillNumber);
                }
            }
            else
            {
                return 0;
            }
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
                    ActualBill.AssignClient.Products[cont].Description);

                values.Add("productPrice" + cont, "¢" +
                    Utils.FormatNumericToString(ActualBill.AssignClient.Products[cont].Price));

                values.Add("productTotalCost" + cont, "¢" +
                    Utils.FormatNumericToString(ActualBill.AssignClient.Products[cont].Amount *
                      ActualBill.AssignClient.Products[cont].Price));


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

            if (ActualBill.AssignClient.Term > 0)
            {
                values.Add("clientTerm",
                   ActualBill.AssignClient.Term + " dias");

                values.Add("billExpiration",
                    ActualBill.BillDate.AddDays(ActualBill.AssignClient.Term).ToString("g"));
            }
            else
            {
                values.Add("clientTerm",
                   "contado");
            }



            //Bill Info
            values.Add("billDate",
                    ActualBill.BillDate.ToString("g"));
            values.Add("billPurchaseOrder",
                    ActualBill.PurchaseOrder);
            values.Add("billId",
                    "N° " + ActualBill.Id);
            values.Add("billTotalProducts",
                    "¢" + Utils.FormatNumericToString(ActualBill.subTotalProducts));
            values.Add("billDescountAmount",
                    "¢" + Utils.FormatNumericToString(ActualBill.discountAmount));
            values.Add("billTotalAfterDescount",
                    "¢" + Utils.FormatNumericToString(ActualBill.totalAfterDiscount));
            values.Add("billTaxesAmount",
                    "¢" + Utils.FormatNumericToString(ActualBill.taxesToPay));
            values.Add("billTotal",
                    "¢" + Utils.FormatNumericToString(ActualBill.TotalToPay));


            Utils util = new Utils();

            values.Add("billTotalInText",
                   util.IntegerToWritten((ActualBill.TotalToPay.ToString("0.##"))));

            return values;
        }
    }
}
