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
        public string ClientName { get; set; } = "Fabian Gomez";
        public string ProductOneName { get; set; } = "Mecate 6mts";
        public int CostTest { get; set; } = 2000;
        public double FloatTest { get; set; } = 3.555;
        public Boolean BoolTest { get; set; } = true;
        public DateTime Today { get; set; } = DateTime.Now;
    }
}
