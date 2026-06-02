using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Unify.Common.Identity.Data;

namespace Unify.Common.Extender
{
    public static class IdentityExtender
    {

        public static void AddSingleSignon(this WebApplicationBuilder builder)
        {

            string connectionString = builder.Configuration.GetConnectionString("IdentityDb") ?? "";

            builder.Services.AddDbContext<IdentityDb>(options => options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<IdentityDb>();
            builder.Services.AddControllersWithViews();

            builder.Services.AddDataProtection()
                     .PersistKeysToDbContext<IdentityDb>()
                     .SetApplicationName("SharedCookieApp");

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = ".AspNet.SharedCookie";
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                options.LoginPath = "/Account/Login";
                //options.Cookie.Domain = ".unifynow.ai";
            });

        }

    }
}
