using System;
using System.Collections.Generic;
using System.Text;

namespace com.Goval.FacturaDigital.DataContracts.BaseModel
{
    public class BaseResponse
    {
        
        public Boolean IsSuccessful { get; set; } = false;
        
        public string UserMessage { get; set; }
        
        public string TechnicalMessage { get; set; }
        
        public int Code { get; set; }

        public string SSOT { get; set; }
    }
}
