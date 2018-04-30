using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.DataContracts.Model
{
    public class AppConfiguration
    {
        
        public long StartBillNumber { get; set; }
        
        public string Base64Logotype { get; set; }
        
        public string EmailScheme { get; set; }

        public bool IsPremiumAccount { get; set; } = false;

        public decimal Credits { get; set; } = 0;
    }
}
