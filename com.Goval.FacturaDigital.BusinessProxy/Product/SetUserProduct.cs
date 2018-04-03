using com.Goval.FacturaDigital.BusinessProxy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.BusinessProxy.Product
{
    public class SetUserProduct : Abstraction.BaseProxy.BaseProxy<ProductRequest, ProductResponse>
    {
        public override string OperationoAddress { get => "/BusinessService.svc/json/SetUserProduct"; }
    }
}
