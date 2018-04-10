using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.DataContracts.Catalogs
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
        public static Dictionary<string, string> ProvinciaType
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {"1","San José"},{"2","Alajuela" }
                };
            }
        }

        public static Dictionary<string, string> CantonType
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {"1,1","San José"},{"1,2","Escazú" }
                };
            }
        }
        public static Dictionary<string, string> DistritoType
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {"1,1,1","CARMEN"},{"1,1,2","MERCED" }
                };
            }
        }
        public static Dictionary<string, string> BarrioType
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {"1,1,1,1","Amón"},{"1,1,1,1","Aranjuez" }
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
                    { "03","Código del producto asignado por la industria" }, {"04","Código uso interno" },
                 { "99","Otros" }
    };
            }

        }
    }
}
