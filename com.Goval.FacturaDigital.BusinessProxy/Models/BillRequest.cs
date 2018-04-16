using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.BusinessProxy.Models
{
    public class BillRequest : DataContracts.BaseModel.BaseRequest
    {

        public DataContracts.Model.Bill ClientBill { get; set; }

        public long UserId { get; set; }
    }
}
