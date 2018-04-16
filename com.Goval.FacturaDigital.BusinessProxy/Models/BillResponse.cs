using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.BusinessProxy.Models
{
    public class BillResponse : DataContracts.BaseModel.BaseResponse
    {
        public byte[] PdfInvoice { get; set; }
    }
}
