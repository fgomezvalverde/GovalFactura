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
                    {"02","Código del producto del comprador"},
                    {"03","Código del producto asignado por la industria"},
                    {"04","Código uso interno"},
                    {"05","Otros"}
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
                    {"kg","Kilogramo"},
                    {"s","Segundo"},
                    {"A","Ampere"},
                    {"K","Kelvin"},
                    {"mol","Mol"},
                    {"cd","Candela"},
                    {"m²","Metro cuadrado"},
                    {"m³","Metro cúbico"},
                    {"m/s","Metro por segundo"},
                    {"m/s²","Metro por segundo cuadrado"},
                    {"1/m","1 por metro"},
                    {"kg/m³","Kilogramo por metro cúbico"},
                    {"A/m²","Ampere por metro cuadrado"},
                    {"A/m","Ampere por metro"},
                    {"mol/m³","Mol por metro cúbico"},
                    {"cd/m²","Candela por metro cuadrado"},
                    {"1","Uno (indice de refracción)"},
                    {"rad","Radián" },
                    {"sr","Estereorradián"},
                    {"Hz","Hertz"},
                    {"N","Newton"},
                    {"Pa","Pascal"},
                    {"J","Joule"},
                    {"W","Watt"},
                    {"C","Coulomb"},
                    {"V","Volt"},
                    {"F","Farad"},
                    {"Ω","Ohm"},
                    {"S","Siemens"},
                    {"Wb","Weber"},
                    {"T","Tesla"},
                    {"H","Henry"},
                    {"°C","Grado Celsius"},
                    {"lm ","Lumen"},
                    {"lx ","Lux"},
                    {"Bq","Becquerel"},
                    {"Gy","Gray" },
                    {"Sv","Sievert"},
                    {"kat","Katal"},
                    {"Pa·s","Pascal segundo"},
                    {"N·m","Newton metro"},
                    {"N/m","Newton por metro"},
                    {"rad/s","Radián por segundo"},
                    {"rad/s²","Radián por segundo cuadrado"},
                    {"W/m²","Watt por metro cuadrado"},
                    {"J/K","Joule por kelvin "},
                    {"J/(kg·K)","Joule por kilogramo kelvin"},
                    {"J/kg","Joule por kilogramo"},
                    {"W/(m·K)","Watt por metro kevin"},
                    {"J/m³","Joule por metro cúbico"},
                    {"V/m","Volt por metro"},
                    {"C/m³","Coulomb por metro cúbico"},
                    {"C/m²","Coulomb por metro cuadrado"},
                    {"F/m","Farad por metro"},
                    {"H/m","Henry por metro"},
                    {"J/mol","Joule por mol"},
                    {"J/(mol·K)","Joule por mol kelvin"},
                    {"C/kg","Coulomb por kilogramo"},
                    {"Gy/s","Gray por segundo"},
                    {"W/sr","Watt por estereorradián"},
                    {"W/(m²·sr)","Watt por metro cuadrado estereorradián" },
                    {"kat/m³","Katal por metro cúbico"},
                    {"min","Minuto "},
                    {"h ","Hora"},
                    {"d","Día"},
                    {"º","Grado"},
                    {"´","Minuto"},
                    {"´´","Segundo"},
                    {"L","Litro"},
                    {"t","Tonelada"},
                    {"Np","Bel"},
                    {"eV","Electronvolt"},
                    {"u","Unidad de masa atómica unificada"},
                    {"ua","Unidad astronómica"},
                    {"Unid","Unidad"},
                    {"Gal","Galón"},
                    {"g","Gramo"},
                    {"Km ","Kilometro"},
                    {"ln","Pulgada"},
                    {"cm","Centímetro"},
                    {"mL","Mililitro"},
                    {"mm","Milímetro"},
                    {"Oz","Onzas "},
                    {"Otros","Otros"}
                };
            }

        }
  
        public static Dictionary<string, string> CurrencyType {
            get {
                return new Dictionary<string, string>()
                {
                    {"CRC","Colón costarricense"},
                    {"USD","Dólar Americano"}


                    /*{"AFN ","Afghani"},
                    {"ALL ","Lek"},
                    {"EUR ","Euro"},
                    {"DZD ","Dinar argelino"}
                    {"AOA ","Colón costarricense"},
                    {"XCD","Dólar Americano"}
                    {"SAR","Colón costarricense"},
                    {"ARS","Dólar Americano"}
                    {"CRC","Colón costarricense"},
                    {"USD","Dólar Americano"}
                    {"CRC","Colón costarricense"},
                    {"USD","Dólar Americano"}
                    {"CRC","Colón costarricense"},
                    {"CRC","Colón costarricense"},
                    {"CRC","Colón costarricense"},
                    {"CRC","Colón costarricense"},
                    {"CRC","Colón costarricense"},
                    {"CRC","Colón costarricense"},
                    {"CRC","Colón costarricense"},
                    {"CRC","Colón costarricense"},
                    {"CRC","Colón costarricense"},
                    {"CRC","Colón costarricense"},
                    {"CRC","Colón costarricense"},
                    {"CRC","Colón costarricense"},
                    {"CRC","Colón costarricense"},
                    {"CRC","Colón costarricense"},
                    {"CRC","Colón costarricense"},
                    {"CRC","Colón costarricense"},
                    {"CRC","Colón costarricense"},
                    {"CRC","Colón costarricense"},
                    {"USD","Dólar Americano"}
                    {"USD","Dólar Americano"}
                    {"USD","Dólar Americano"}
                    {"USD","Dólar Americano"}
                    {"USD","Dólar Americano"}
                    {"USD","Dólar Americano"}
                    {"USD","Dólar Americano"}
                    {"USD","Dólar Americano"}
                    {"USD","Dólar Americano"}
                    {"USD","Dólar Americano"}
                    {"USD","Dólar Americano"}
                    {"USD","Dólar Americano"}
                    {"USD","Dólar Americano"}
                    {"USD","Dólar Americano"}
                    {"USD","Dólar Americano"}
                    {"USD","Dólar Americano"}
                    {"USD","Dólar Americano"}
                    {"USD","Dólar Americano"}*/
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
                    { "03","Código del producto asignado por la industria " }
                };
            }

        }

        public static Dictionary<string, string> IdentificationType
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {"01","Cédula Física"},
                    {"02","Cédula Jurídica" },
                    {"03","DIMEX"},
                    {"04","NITE " }
                };
            }
        }
        public static Dictionary<string, string> SellConditions
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {"01","Contado"},
                    {"02","Crédito"},
                    {"03","Consignación"},
                    {"04","Apartado"},
                    {"05","Arrendamiento con opción de compra"},
                    {"06","Arrendamiento en función financiera"},
                    {"99","Otros"}
                };
            }
        }
        public static Dictionary<string, string> PayMethod
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {"01","Efectivo"},
                    {"02","Tarjeta"},
                    {"03","Cheque"},
                    {"04","Transferencia – depósito bancario"},
                    {"05","Recaudado por terceros"},
                    {"99","Otros"}
                };
            }
        }
        public static Dictionary<string, string> TaxType
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {"01","Impuesto General sobre las Ventas"},
                    {"02","Impuesto Selectivo de Consumo"},
                    {"03","Impuesto Único a los combustibles"},
                    {"04","Impuesto específico de Bebidas Alcohólicas"},
                    {"05","Impuesto Específico sobre las bebidas envasadas sin contenido alcohólico y jabones de tocador"},
                    {"06","Impuesto a los Productos de Tabaco"},
                    {"07","Servicio"},
                    {"12","Impuesto Especifico al Cemento"},
                    {"98","Otros"},
                    {"08","Impuesto General sobre las Ventas Diplomáticos"},
                    {"09","Impuesto General  sobre las Ventas Compras Autorizadas"},
                    {"10","Impuesto General sobre las ventas Instituciones Públicas y otros Organismos"},
                    {"11","Impuesto Selectivo de Consumo Compras Autorizadas"},
                    {"99","Otros"}
                };
            }
        }
        public static Dictionary<string, string> ReferenceCode
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {"01","Anula Documento de Referencia"},
                    {"02","Corrige texto documento de referencia"},
                    {"03","Corrige monto"},
                    {"04","Referencia a otro documento"},
                    {"05","Sustituye comprobante provisional por contingencia"},
                    {"99","Otros"}
                };
            }
        }
        public static Dictionary<string, string> ReferenceDocumentType
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {"01","Factura electrónica"},
                    {"02","Nota de débito electrónica"},
                    {"03","Nota de crédito electrónica"},
                    {"04","Tiquete electrónico"},
                    {"05","Nota de despacho"},
                    {"06","Contrato"},
                    {"07","Procedimiento"},
                    {"08","Comprobante emitido en contingencia"},
                    {"99","Otros"}
                };
            }
        }
        public static Dictionary<string, string> ExonerationOrAutorizationDocumentType
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {"01","Compras autorizadas"},
                    {"02","Ventas exentas a diplomáticos"},
                    {"03","Orden de compra (Instituciones Públicas y otros organismos)"},
                    {"04","Exenciones Dirección General de Hacienda"},
                    {"05","Zonas Francas"},
                    {"99","Otros"}
                };
            }
        }
        public static Dictionary<string, string> MessageCode
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {"1","Aceptado"},
                    {"2","*Aceptación parcial"},
                    {"3","Rechazado"}
                };
            }
        }
    }
}
