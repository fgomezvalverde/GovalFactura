using com.Goval.FacturaDigital.DataContracts.BaseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.BusinessProxy.Models
{
    public class UbicationResponse : BaseResponse
    {
        public Dictionary<string, string> DirectionCodesDictionary { get; set; }
    }
}
