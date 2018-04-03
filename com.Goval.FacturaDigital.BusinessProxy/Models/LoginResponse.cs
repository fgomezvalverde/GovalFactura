using System;
using System.Collections.Generic;
using System.Text;

namespace com.Goval.FacturaDigital.BusinessProxy.Models
{
    public class LoginResponse: DataContracts.BaseModel.BaseResponse
    {
        public DataContracts.Model.AppConfiguration UserConfiguration { get; set; }
        public DataContracts.Model.User UserInformation { get; set; }
    }
}
