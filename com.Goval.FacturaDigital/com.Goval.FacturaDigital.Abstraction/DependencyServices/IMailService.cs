using com.Goval.FacturaDigital.Abstraction.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.Abstraction.DependencyServices
{
    public interface IMailService
    {
        Boolean SendMail(String pSubject, String pBody, List<string> pCopyEmails, List<AttachmentMail> pAttachments = null, Boolean pIsHtmlBody = false);
    }
}
