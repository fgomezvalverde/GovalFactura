using com.Goval.FacturaDigital.DataContracts.BaseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.BusinessProxy.Models
{
    public class UbicationRequest : BaseRequest
    {
        public int ProvinciaCode { get; set; }
        public int DistritoCode { get; set; }
        public int CantonCode { get; set; }
    }
}
