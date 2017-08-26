using Amazon.DynamoDBv2.DataModel;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.Model
{
    [DynamoDBTable("Client")]
    [AddINotifyPropertyChangedInterface]
    public class Client
    {
        public string ClientId { get; set; }

        public string Name { get; set; }

        public string Direction { get; set; }

        public string Telephone { get; set; }

        public string Email { get; set; }

        public string LegalId { get; set; }


        public List<Product> Products { get; set; } = new List<Product>();

        public double DiscountPercentage { get; set; } = 0;

        public double TaxesPercentage { get; set; } = 13;

        public int Term { get; set; } = 0;


        public void RemoveOnUsedProducts()
        {
            Products.RemoveAll(product => product.IsUsed == false);
        }
    }
}
