using Amazon.DynamoDBv2.DataModel;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.Model
{
    [DynamoDBTable("Bill")]
    [AddINotifyPropertyChangedInterface]
    public class Bill
    {
        public int Id { get; set; }

        public Client AssignClient { get; set; }

        public DateTime BillDate { get; set; }

        public string PurchaseOrder { get; set; } = string.Empty;

        public double subTotalProducts { get; set; } = 0;
        public double discountAmount { get; set; } = 0;
        public double taxesToPay { get; set; } = 0;
        public double totalAfterDiscount { get; set; } = 0;
        public double TotalToPay { get; set; } = 0;

        public string Status { get; set; }= Enum.GetName(BillStatus.Aprobada.GetType(), BillStatus.Aprobada);
    }
}
