using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Raffle.Core;
using Raffle.Core.Commands;
using Raffle.Core.Data;
using Raffle.Core.Models;
using Raffle.Core.Queries;
using Raffle.Core.Repositories;
using Raffle.Core.Shared;
using Raffle.Web.Data;
using Raffle.Web.Services;

using SendGrid;

using System;
using System.Linq;

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

            services.AddApplicationInsightsTelemetry();
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
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            services.Configure<SendGridEmailSenderOptions>(Configuration.GetSection("SendGrid"));

            string managerEmail = Configuration["raffleManager:Email"];
            string managerName = Configuration["raffleManager:Name"];

            services.AddTransient<IRaffleEmailSender, SendGridRaffleEmailSender>();
            services.AddTransient<IEmailSender, SendGridEmailSender>();

            services.AddScoped<ICommandHandler<AddRaffleItemCommand>>(services => new AddRaffleItemCommandHandler(dbConnectionString));
            services.AddScoped<ICommandHandler<UpdateRaffleItemCommand>>(services => new UpdateRaffleItemCommandHandler(dbConnectionString));
            services.AddScoped<IQueryHandler<GetRaffleOrderQuery, RaffleOrder>>(services => new GetRaffleOrderQueryHandler(dbConnectionString));

            services.AddScoped<ICommandHandler<UpdateOrderCommand>>(services => new UpdateOrderCommandHandler(dbConnectionString));
            services.AddScoped<IQueryHandler<StartRaffleOrderQuery, int>>(services => new StartRaffleOrderQueryHandler(dbConnectionString));
            services.AddScoped<ICommandHandler<CompleteRaffleOrderCommand>>(services => new CompleteRaffleOrderCommandHandler(
                dbConnectionString, 
                services.GetService<IRaffleEmailSender>(),
                services.GetService<EmbeddedResourceReader>(),
                new EmailAddress(managerEmail, managerName)));

            services.AddScoped<IQueryHandler<GetRaffleOrdersQuery, GetRaffleOrdersResult>>(sevices => new GetRaffleOrdersQueryHandler(dbConnectionString));
            
            services.AddScoped<IRaffleItemRepository>(services => new RaffleItemRepository(dbConnectionString));
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
            if (env.IsDevelopment())
            {
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
            });
        }
    }
}
