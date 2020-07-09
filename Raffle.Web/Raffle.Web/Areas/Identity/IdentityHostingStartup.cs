using Microsoft.AspNetCore.Hosting;

using System;

[assembly: HostingStartup(typeof(Raffle.Web.Areas.Identity.IdentityHostingStartup))]
namespace Raffle.Web.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
            });
        }
    }
}