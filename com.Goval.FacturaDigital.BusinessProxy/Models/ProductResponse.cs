using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.BusinessProxy.Models
{
    public class ProductResponse : DataContracts.BaseModel.BaseResponse
    {

        public List<DataContracts.Model.Product> UserProducts { get; set; } = new List<DataContracts.Model.Product>();
    }
}
