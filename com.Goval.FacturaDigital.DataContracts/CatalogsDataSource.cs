using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.DataContracts
{
    public static class CatalogsDataSource
    {
        public static Dictionary<string, string> CurrencyType {
            get {
                return new Dictionary<string, string>()
                {
                    {"CRC","Colón costarricense"},{"USD","Dólar Americano" }
                };
            }
                
        }
        public static Dictionary<string, string> CountryType
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {"1","San José"},{"2","Alajuela" }
                };
            }

        }

        public static Dictionary<string, string> CodeProductOrServiceType
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {"01","Código del producto del vendedor"},{"02","Código del producto del comprador" },
                    { "03","Código del producto asignado por la industria " }
                };
            }

        }
    }
}
