using com.Goval.FacturaDigital.Abstraction.BaseProxy;
using com.Goval.FacturaDigital.BusinessProxy.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.Goval.FacturaDigital.BusinessProxy.Authentication
{
    public class ValidateUserClient : BaseProxy<LoginRequest, LoginResponse>
    {
        public override string OperationoAddress { get => "/BusinessService.svc/json/ValidateUser"; }
    }
}
