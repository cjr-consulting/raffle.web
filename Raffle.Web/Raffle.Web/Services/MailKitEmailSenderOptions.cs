using System;
using System.Linq;

namespace Raffle.Web.Services
{
    public class MailKitEmailSenderOptions
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string IncomingServer { get; set; }
        public string OutgoingServer { get; set; }
        public int SmtpPort { get; set; }
        public bool EnableSsl { get; set; }
    }
}
