using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

using SendGrid;
using SendGrid.Helpers.Mail;

using System.Threading.Tasks;

namespace Raffle.Web.Services
{
    public class SendGridEmailSender : IEmailSender
    {
        readonly string fromEmail;
        readonly string fromName;
        readonly string apiKey;

        public SendGridEmailSender(IOptions<SendGridEmailSenderOptions> options)
        {
            apiKey = options.Value.ApiKey;
            fromName = options.Value.Name;
            fromEmail = options.Value.Email;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromEmail, fromName);
            var to = new EmailAddress(email);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, htmlMessage, htmlMessage);
            // msg.SetClickTracking(false, false);
            var response = await client.SendEmailAsync(msg);
        }
    }
}
