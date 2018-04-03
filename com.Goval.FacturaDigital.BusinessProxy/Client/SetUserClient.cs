using com.Goval.FacturaDigital.BusinessProxy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.BusinessProxy.Client
{
    public class SetUserClient : Abstraction.BaseProxy.BaseProxy<ClientRequest, ClientResponse>
    {
        public override string OperationoAddress { get => "/BusinessService.svc/json/SetUserClient"; }
    }
}
