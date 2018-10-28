using com.Goval.FacturaDigital.DataContracts.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.BusinessProxy.Models
{
    public class DebitCreditNoteBillRequest : DataContracts.BaseModel.BaseRequest
    {
        public DataContracts.Model.Bill ClientBill { get; set; }

        public User User { get; set; }

        public ReferenceDocument ReferenceDocument { get; set; }
    }
}
