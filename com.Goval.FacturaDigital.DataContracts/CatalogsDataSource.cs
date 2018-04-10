using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.DataContracts.Catalogs
{
    public static class CatalogsDataSource
    {
        public static Dictionary<string, string> ProductType
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {"01","Código del producto del vendedor"},
                    { "02","Código del producto del comprador"},
                    { "03","Código del producto asignado por la industria"},
                    { "04","Código uso interno"},
                    { "05","Otros"}
                };
            }
        }

        public static Dictionary<string, string> MeasurementUnit
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {"Sp","Servicios Profesionales"},
                    {"m","Metro"},
                    {"kg","Segundo"},
                    {"A","Ampere"},
                    {"K","Kelvin"},
                    {"mol","Mol"},
                    {"cd","Candela"},
                    {"m²","metro cuadrado"},
                    {"m³","metro cúbico"},
                    {"m/s","metro por segundo"},
                    {"m/s²","metro por segundo cuadrado"},
                    {"1/m","1 por metro"},
                    {"kg/m³","kilogramo por metro cúbico"},
                    {"A/m²","ampere por metro cuadrado"},
                    {"A/m","ampere por metro"},
                    {"mol/m³","mol por metro cúbico"},
                    {"cd/m²","candela por metro cuadrado"},
                    {"1","uno (indice de refracción)"},
                    {"rad","radián" },
                    {"sr","estereorradián"},
                    {"Hz","hertz"},
                    {"N","newton"},
                    {"Pa","pascal"},
                    {"J","Joule"},
                    {"W","Watt"},
                    {"C","coulomb"},
                    {"V","volt"},
                    {"F","farad"},
                    {"Ω","ohm"},
                    {"S","siemens"},
                    {"Wb","weber "},
                    {"T","tesla "},
                    {"H","henry "},
                    {"°C","grado Celsius"},
                    {"lm ","lumen"},
                    {"lx ","lux"},
                    {"Bq","Becquerel"}
                };
            }

        }
  
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
