﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.DataContracts.Model
{
    public class User
    {

        
        public long UserId { get; set; }
        
        public string UserName { get; set; }
        
        public string Name { get; set; }
        
        public string LastName { get; set; }
        
        public string SecondName { get; set; }
        
        public string Email { get; set; }
        
        public string UserLegalNumber { get; set; }
        
        public string HaciendaUsername { get; set; }
        
        public string HaciendaPassword { get; set; }
        
        public string HaciendaCryptographicPIN { get; set; }
        
        public byte[] HaciendaCryptographicFile { get; set; }
        
        public string HaciendaCryptographicFileName { get; set; }
        
        public string Password { get; set; }
        
        public string PhoneNumber { get; set; }

        public Boolean HaciendaUserValidation { get; set; } = false;
        
        public string IdentificationType { get; set; }
        
        public string ComercialName { get; set; }
        
        public string ProvinciaCode { get; set; }
        
        public string CantonCode { get; set; }
        
        public string DistritoCode { get; set; }
        
        public string BarrioCode { get; set; }
        
        public string LocationDescription { get; set; }

        public string PhoneNumberCountryCode { get; set; } = "506";

        public string FaxCountryCode { get; set; } = "506";
        
        public string Fax { get; set; }
    }
}
