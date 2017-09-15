using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.Abstraction.Mail
{
    public class AttachmentMail
    {
        public String FileName { get; set; }
        public Stream FileData { get; set; }
    }
}
