using System;
using System.Threading.Tasks;

namespace Raffle.Core
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string toName, string subject, string textBody, string htmlBody);
    }
}
