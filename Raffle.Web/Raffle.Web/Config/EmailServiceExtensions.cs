using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raffle.Core;
using Raffle.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raffle.Web.Config
{
    public static class EmailServiceExtensions
    {
        public static void AddEmailService(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<SendGridEmailSenderOptions>(config.GetSection("SendGrid"));
            services.Configure<MailKitEmailSenderOptions>(config.GetSection("MailKit"));

            if (config["Flags:MailService"]?.ToLower() == "mailkit")
            {
                services.AddTransient<IRaffleEmailSender, MailKitRaffleEmailSender>();
                services.AddTransient<IEmailSender, MailKitEmailSender>();
            }
            else
            {
                services.AddTransient<IRaffleEmailSender, SendGridRaffleEmailSender>();
                services.AddTransient<IEmailSender, SendGridEmailSender>();
            }
        }
    }
}
