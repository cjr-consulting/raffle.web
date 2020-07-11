using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

using Raffle.Core;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raffle.Web.Services
{
    public class SendGridRaffleEmailSender : IRaffleEmailSender
    {
        readonly string fromEmail;
        readonly string fromName;
        readonly string apiKey;

        public SendGridRaffleEmailSender(IOptions<SendGridEmailSenderOptions> options)
        {
            apiKey = options.Value.ApiKey;
            fromName = options.Value.Name;
            fromEmail = options.Value.Email;
        }

        public async Task SendEmailAsync(string toEmail, string toName, string subject, string textBody, string htmlBody)
        {
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromEmail, fromName);
            var to = new EmailAddress(toEmail, toName);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, textBody, htmlBody);
            // msg.SetClickTracking(false, false);
            var response = await client.SendEmailAsync(msg);
        }
    }
}
