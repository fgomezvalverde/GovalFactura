using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.BusinessProxy.Models
{
    public class UserValidationRequest : DataContracts.BaseModel.BaseRequest
    {
        public DataContracts.Model.User User { get; set; } = new DataContracts.Model.User();
    }
}
