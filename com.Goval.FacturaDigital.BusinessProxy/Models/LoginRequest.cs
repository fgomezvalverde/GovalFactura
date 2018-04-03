using System;
using System.Collections.Generic;
using System.Text;

namespace com.Goval.FacturaDigital.BusinessProxy.Models
{
    public class LoginRequest : DataContracts.BaseModel.BaseRequest
    {
        public string Password { get; set; }
    }
}
