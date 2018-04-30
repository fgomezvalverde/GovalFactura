using com.Goval.FacturaDigital.Abstraction.BaseProxy;
using com.Goval.FacturaDigital.BusinessProxy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.BusinessProxy.Authentication
{
    public class RegisterUser : BaseProxy<BusinessProxy.Models.SignupRequest, LoginResponse>
    {
        public override string OperationoAddress { get => "/BusinessService.svc/json/RegisterUser"; }
    }
}
