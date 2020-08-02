using MailKit.Net.Smtp;

using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

using MimeKit;
using MimeKit.Text;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Raffle.Web.Services
{
    public class MailKitEmailSender : IEmailSender
    {
        readonly string fromEmail;
        readonly string fromName;
        readonly string password;
        readonly bool enableSsl;
        readonly string incomingServer;
        readonly string outgoingServer;
        readonly int smtpPort;

        public MailKitEmailSender(IOptions<MailKitEmailSenderOptions> options)
        {
            password = options.Value.Password;
            fromName = options.Value.Name;
            fromEmail = options.Value.Email;
            enableSsl = options.Value.EnableSsl;
            incomingServer = options.Value.IncomingServer;
            outgoingServer = options.Value.OutgoingServer;
            smtpPort = options.Value.SmtpPort;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.To.Add(MailboxAddress.Parse(email));
            message.From.Add(new MailboxAddress(fromName, fromEmail));

            message.Subject = subject;
            //We will say we are sending HTML. But there are options for plaintext etc. 
            message.Body = new TextPart(TextFormat.Html)
            {
                Text = htmlMessage
            };

            //Be careful that the SmtpClient class is the one from Mailkit not the framework!
            using (var emailClient = new SmtpClient())
            {
                //The last parameter here is to use SSL (Which you should!)
                await emailClient.ConnectAsync(outgoingServer, smtpPort, enableSsl);

                //Remove any OAuth functionality as we won't be using it. 
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

                await emailClient.AuthenticateAsync(fromEmail, password);

                await emailClient.SendAsync(message);

                await emailClient.DisconnectAsync(true);
            }
        }
    }
}
