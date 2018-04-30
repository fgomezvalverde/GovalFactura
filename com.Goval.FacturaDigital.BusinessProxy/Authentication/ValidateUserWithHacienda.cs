using com.Goval.FacturaDigital.Abstraction.BaseProxy;
using com.Goval.FacturaDigital.DataContracts.BaseModel;
using com.Goval.FacturaDigital.DataContracts.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.BusinessProxy.Authentication
{
    public class ValidateUserWithHacienda : BaseProxy<Models.UserValidationRequest, BaseResponse>
    {
        public override string OperationoAddress { get => "/BusinessService.svc/json/ValidateUserWithHacienda"; }
    }
}
