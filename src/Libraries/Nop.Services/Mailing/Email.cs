using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using System.Reflection.Metadata;
using System.IO;
using System.Net.Mime;

namespace Nop.Services.Mailing
{
    class Email
    {
        MailMessage _mail;
        SmtpClient _smtpServer;
        private string _emailOrigen;
        private string _emailDestino;
        private string _contrasena;
        private string _subject;
        private string _body;
        private Attachment _attachement;

        public Email() {
            _mail = new MailMessage();
            _smtpServer = new SmtpClient("smtp.gmail.com");
            _smtpServer.Port = 587;
            _smtpServer.EnableSsl = true;
        }

        public void SetEmailOrigen(string emailOrigen,string contrasena) {
            _emailOrigen = emailOrigen;
            _contrasena = contrasena;
            _mail.From = new MailAddress(emailOrigen);
            _smtpServer.Credentials = new NetworkCredential(emailOrigen, contrasena);
        }

        public void SetEmailDestino(string emailDestino)
        {
            _emailDestino = emailDestino;
            _mail.To.Add(emailDestino);
        }

        public void SetSubject(string subject)
        {
            _subject = subject;
            _mail.Subject = subject;
        }

        public void SetBody(string body)
        {
            _subject = body;
            _mail.Body = body;
        }

        public void SetAttachment(Stream contentStream, ContentType contentType)
        {
            if(contentStream != null && contentType != null)
            {
                _attachement = new Attachment(contentStream, contentType);
                _mail.Attachments.Add(_attachement);
            }
        }


        public void Send() {
            _smtpServer.Send(_mail);
        }
    }
}