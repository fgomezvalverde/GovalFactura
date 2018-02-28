using Amazon.DynamoDBv2.DataModel;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.Model
{
    [DynamoDBTable("Product")]
    [AddINotifyPropertyChangedInterface]
    public class Product
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public double Price { get; set; } = 0;

        public string UnityType { get; set; }

        // This is to assign to a Client
        public Boolean IsUsed { get; set; }

        // This is to assign to a Bill
        public int Amount { get; set; } = 0;

        public double TotalCost { get; set; } = 0;
        public int UserId { get; set; }
    }
}
