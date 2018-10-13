using com.Goval.FacturaDigital.BusinessProxy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.BusinessProxy.Bill
{
    public class GetBillInvoice : Abstraction.BaseProxy.BaseProxy<BillRequest, BillResponse>
    {
        public override string OperationoAddress { get => "/BusinessService.svc/json/GetBillInvoice"; }
    }
}
