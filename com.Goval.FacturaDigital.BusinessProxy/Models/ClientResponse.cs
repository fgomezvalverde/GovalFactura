using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.BusinessProxy.Models
{
    public class ClientResponse : DataContracts.BaseModel.BaseResponse
    {
        public List<DataContracts.Model.Client> UserClients { get; set; } = new List<DataContracts.Model.Client>();
    }
}
