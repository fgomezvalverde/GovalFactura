using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.DataContracts.Model
{
    public class Client
    {
        
        public long ClientId { get; set; }
        
        public string Name { get; set; }
        
        public int DefaultDiscountPercentage { get; set; }
        
        public int DefaultTaxesPercentage { get; set; }
        
        public string PhoneNumber { get; set; }
        
        public string ClientLegalNumber { get; set; }
        
        public string LocationDescription { get; set; }
        
        public string Email { get; set; }
        
        public int DefaultPaymentTerm { get; set; }
        
        public string ComercialName { get; set; }
        
        public bool IsIndependentPerson { get; set; }
        
        public string LastName { get; set; }
        
        public string SecondName { get; set; }
        
        public string IdentificationType { get; set; }
        
        public string ForeignIdentification { get; set; }
        
        public string ProvinciaCode { get; set; }
        
        public string CantonCode { get; set; }
        
        public string DistritoCode { get; set; }
        
        public string BarrioCode { get; set; }
        
        public string PhoneNumberCountryCode { get; set; }
        
        public string FaxCountryCode { get; set; }
        
        public string Fax { get; set; }


        
        public List<Product> ClientProducts { get; set; } = new List<Product>();
    }
}
