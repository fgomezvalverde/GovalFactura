using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using com.Goval.FacturaDigital.Droid.DependencyServices;
using System.Security.Cryptography.X509Certificates;
using com.Goval.FacturaDigital.Abstraction.Mail;
using System.Net.Mail;
using System.Net;
using com.Goval.FacturaDigital.Abstraction.DependencyServices;

[assembly: Xamarin.Forms.Dependency(typeof(MailService))]
namespace com.Goval.FacturaDigital.Droid.DependencyServices
{
    public class MailService : IMailService
    {
        private readonly string MailAddress = "goval.automatic@gmail.com";
        private readonly string UserName = "goval.automatic";
        private readonly string Password = "gomezvalverde5";

        public bool SendMail(String pSubject,String pBody,List<string> pCopyEmails, List<AttachmentMail> pAttachments = null, Boolean pIsHtmlBody = false)
        {
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
            

            try
            {
                mail.From = new MailAddress(MailAddress);
                if (pCopyEmails != null && pCopyEmails.Any())
                {
                    foreach (var address in pCopyEmails)
                    {
                        mail.To.Add(address);
                    }
                }


                if (pAttachments != null && pAttachments.Any())
                {
                    foreach (var attachment in pAttachments)
                    {
                        Attachment newAttachment = new Attachment(attachment.FileData, attachment.FileName);
                        mail.Attachments.Add(newAttachment);
                    }
                }

                mail.Subject = pSubject;
                mail.Body = pBody;
                mail.IsBodyHtml = pIsHtmlBody;
                SmtpServer.Port = 587;  //gmail default port
                SmtpServer.Credentials = new System.Net.NetworkCredential(UserName, Password);
                SmtpServer.EnableSsl = true;
                ServicePointManager.ServerCertificateValidationCallback = delegate (object sender, X509Certificate certificate, X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
                {
                    return true;
                };
                SmtpServer.Send(mail);
                Toast.MakeText(Application.Context, "Correo Enviado a Destino", ToastLength.Short).Show();
                return true;
            }

            catch (Exception ex)
            {
                Toast.MakeText(Application.Context, ex.ToString(), ToastLength.Long);
                return false;
            }
        }
    }
}