using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.BusinessProxy.Bill
{
    public class InvalidateBill : Abstraction.BaseProxy.BaseProxy<Models.DebitCreditNoteBillRequest, DataContracts.BaseModel.BaseResponse>
    {
        public override string OperationoAddress { get => "/BusinessService.svc/json/InvalidateBill"; }
    }
}
