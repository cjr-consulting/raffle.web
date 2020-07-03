using Raffle.Core;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raffle.Web.Services
{
    public class SendGridEmailSender : IEmailSender
    {
        readonly string apiKey;
        readonly string fromEmail;
        readonly string fromName;

        public SendGridEmailSender(
            string apiKey,
            string fromEmail,
            string fromName)
        {
            this.fromName = fromName;
            this.fromEmail = fromEmail;
            this.apiKey = apiKey;
        }

        public async Task SendEmailAsync(string toEmail, string toName, string subject, string textBody, string htmlBody)
        {
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromEmail, fromName);
            var to = new EmailAddress(toEmail, toName);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, textBody, htmlBody);
            var response = await client.SendEmailAsync(msg);
        }
    }
}
