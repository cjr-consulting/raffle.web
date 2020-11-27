using MediatR;

using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Raffle.Core;
using Raffle.Core.Cache;
using Raffle.Core.Models;
using Raffle.Core.Shared;
using Raffle.Web.Config;

using Raffle.Web.Data;
using Raffle.Web.Services;

using StackifyLib;

using System;
using System.Collections.Generic;

namespace Raffle.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var dbConnectionString = Configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    dbConnectionString));
            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential 
                // cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                // requires using Microsoft.AspNetCore.Http;
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.Secure = CookieSecurePolicy.SameAsRequest;
            });

            services.AddSignalR();
            services.AddApplicationInsightsTelemetry();
            services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>(
                (module, o) => { module.EnableSqlCommandTextInstrumentation = true; });
            services.AddControllersWithViews();
            services.AddRazorPages();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
            });
            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);

                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            services.AddMediatR(typeof(Startup), typeof(RaffleDbConfiguration), typeof(NotifyRaffleRunRaffleItemUpdate));

            services.AddTransient<RaffleRunHub>();
            services.AddEmailService(Configuration);

            var section = Configuration.GetSection("Flags");
            var flags = new Dictionary<string, string>();
            foreach (var child in section.GetChildren())
            {
                flags.Add(child.Key, child.Value);
            }

            services.AddSingleton(x => new FlagManager(flags));

            services.AddSingleton(services => new RaffleDbConfiguration { ConnectionString = dbConnectionString });
            services.AddSingleton(services => new EmailAddress(Configuration["raffleManager:Email"], Configuration["raffleManager:Name"]));

            services.AddAzureBlobStorageService(Configuration.GetSection("ImageStorage"));
            services.AddRaffleItem();
            services.AddRaffleItemCache();
            services.AddSingleton<ICacheManager, CacheManager>();

            services.AddSingleton<EmbeddedResourceReader>();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Administration", policy =>
                    policy.Requirements.Add(new RolesAuthorizationRequirement(new[] { "Admins" })));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.ConfigureStackifyLogging(Configuration);

            if (env.IsDevelopment())
            {
                app.UseMiddleware<StackifyMiddleware.RequestTracerMiddleware>();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
                endpoints.MapHub<RaffleRunHub>("/RaffleRunHub");
            });
        }
    }
}
