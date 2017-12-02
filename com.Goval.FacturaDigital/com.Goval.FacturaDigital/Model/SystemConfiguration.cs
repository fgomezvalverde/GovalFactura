using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.Model
{
    public class SystemConfiguration
    {
        public string Id { get; set; }  
        public string PdfGeneratorKey { get; set; }
        public List<String> EmailsToSendBill { get; set; }
        public Boolean SendBillToClientEmail { get; set; } = false;
    }

    public class EmailContact
    {
        public string MyProperty { get; set; }
    }
}
