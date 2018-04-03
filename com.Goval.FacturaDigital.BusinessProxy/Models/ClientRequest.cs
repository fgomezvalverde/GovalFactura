using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.BusinessProxy.Models
{
    public class ClientRequest : DataContracts.BaseModel.BaseRequest
    {
        public DataContracts.Model.Client UserClient { get; set; }
        public long UserId { get; set; }
    }
}
