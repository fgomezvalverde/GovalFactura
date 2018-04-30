using com.Goval.FacturaDigital.DataContracts.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.BusinessProxy.Models
{
    public class SignupRequest
    {
        public User User { get; set; } = new User();

        public AppConfiguration UserAppConfiguration { get; set; } = new AppConfiguration();
    }
}
