using com.Goval.FacturaDigital.BusinessProxy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.BusinessProxy.CreditNote
{
    public class GetUserCreditNotes : Abstraction.BaseProxy.BaseProxy<BillRequest, BillResponse>
    {
        public override string OperationoAddress { get => "/BusinessService.svc/json/GetUserCreditNotes"; }
    }
}
