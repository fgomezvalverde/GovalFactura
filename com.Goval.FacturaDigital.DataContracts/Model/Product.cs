using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.DataContracts.Model
{
    public class Product
    {
        
        public long ProductId { get; set; }
        
        public decimal Price { get; set; }
        
        public string MeasurementUnit { get; set; }
        
        public string Description { get; set; }
        
        public string Name { get; set; }
        
        public string CurrencyType { get; set; }
        
        public string BarCode { get; set; }
        
        public string ProductType { get; set; }
        
        public string ProductCode { get; set; }
        
        public string MeasurementUnitType { get; set; }
        
        public long ProductByClientId { get; set; }
        
        public int ProductQuantity { get; set; }
    }
}
