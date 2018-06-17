using com.Goval.FacturaDigital.BusinessProxy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.BusinessProxy.Ubication
{
    public class GetDistritos : Abstraction.BaseProxy.BaseProxy<UbicationRequest, UbicationResponse>
    {
        public override string OperationoAddress { get => "/BusinessService.svc/json/GetDistritos"; }
    }
}
