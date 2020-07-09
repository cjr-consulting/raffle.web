using Microsoft.AspNetCore.Identity.UI.Services;

using SendGrid;
using SendGrid.Helpers.Mail;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Raffle.Web.Services
{
    public class SendGridEmailSender : IEmailSender
    {
        readonly SendGridClient client;
        readonly string fromEmail;
        readonly string fromName;

        public SendGridEmailSender(
            SendGridClient client,
            string email,
            string name)
        {
            this.fromName = name;
            this.fromEmail = email;
            this.client = client;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var from = new EmailAddress(fromEmail, fromName);
            var to = new EmailAddress(email);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, htmlMessage, htmlMessage);
            msg.SetClickTracking(false, false);
            var response = await client.SendEmailAsync(msg);
        }
    }
}
