using System;
using System.Collections.Generic;
using System.Text;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;

namespace Chat7.Controllers
{
    public class SendEmail
    {
  
        public void Send(string to, string body, string subject)
        {

            var mailMessage = new MimeMessage();
            mailMessage.From.Add(new MailboxAddress("7 Chats", "chatnonreply@gmail.com"));
            mailMessage.To.Add(new MailboxAddress(to, to));
            mailMessage.Subject = subject;
            mailMessage.Body = new TextPart("plain")
            {
                Text = body
            };

            using (var smtpClient = new SmtpClient())
            {
                smtpClient.Connect("smtp.gmail.com", 465, true);
                smtpClient.Authenticate("chatnonreply@gmail.com", "N2_3LG_wSwHU5Ks");
                smtpClient.Send(mailMessage);
                smtpClient.Disconnect(true);
            }

        }
    }
}