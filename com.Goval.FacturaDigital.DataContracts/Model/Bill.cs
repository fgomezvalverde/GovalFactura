using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.DataContracts.Model
{
    public class Bill
    {
        
        public string Status { get; set; }
        
        public string PurchaseOrderCode { get; set; }
        
        public decimal TotalAfterDiscount { get; set; }
        
        public decimal TaxesToPay { get; set; }
        
        public decimal SubTotalProducts { get; set; }
        
        public decimal DiscountAmount { get; set; }
        
        public decimal TotalToPay { get; set; }
        
        public long ClientId { get; set; }
        
        public string XMLReceivedFromHacienda { get; set; }
        
        public string SoldProductsJSON { get; set; }
        
        public long BillId { get; set; }
        
        public int HaciendaFailCounter { get; set; }
        
        public System.DateTime LastSendDate { get; set; }
        
        public System.DateTime EmissionDate { get; set; }
        
        public string DocumentKey { get; set; }
        
        public string ConsecutiveNumber { get; set; }
        
        public string SellCondition { get; set; }
        
        public string CreditTerm { get; set; }
        
        public string PaymentMethod { get; set; }
        
        public string DiscountNature { get; set; }
        
        public string TaxCode { get; set; }

        
        public bool HaveExoneration { get; set; }
        
        public decimal ExonerationAmount { get; set; }
        
        public Exoneration ExonerationData { get; set; }
    }
}
