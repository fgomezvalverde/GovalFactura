using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.DataContracts.Model
{
    public class Exoneration
    {
        
        public long ExonerationId { get; set; }
        
        public string DocumentType { get; set; }
        
        public string DocumentNumber { get; set; }
        
        public string InstitutionName { get; set; }
        
        public System.DateTime EmissionDate { get; set; }
        
        public decimal TaxAmount { get; set; }
        
        public int PurchasePercentaje { get; set; }
    }
}
