using System;
using System.Collections.Generic;
using System.Text;

namespace com.Goval.FacturaDigital.DataContracts.BaseModel
{
    public class BaseRequest
    {
        public string UserName { get; set; }
        
        public string ApplicationId { get; set; }
        
        public string DeviceId { get; set; }
        
        public string SSOT { get; set; }
    }
}
