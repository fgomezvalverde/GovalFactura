using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.BusinessProxy.Models
{
    public class ProductRequest : DataContracts.BaseModel.BaseRequest
    {
        
        public DataContracts.Model.Product UserProduct { get; set; }
        
        public long UserId { get; set; }
    }
}
