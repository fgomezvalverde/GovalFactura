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
        public string Code { get; set; }
        public string Description { get; set; }
        public double Price { get; set; } = 0;

        // This is to assign to a Client
        public Boolean IsUsed { get; set; }
    }
}
