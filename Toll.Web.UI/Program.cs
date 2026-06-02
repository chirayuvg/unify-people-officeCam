using AppBuilder.Framework.Core;

using AppBuilder.Framework.MVC;
using Toll.Web.UI;
using Toll.Web.UI.Controllers;
using AppBuilder.Framework.Mvc;
using Toll.Web.UI.App;
using Toll.Web.UI.Services;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Google;
using Toll.Web.UI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;



var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<IdentityDb>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityDb")));
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
        .AddEntityFrameworkStores<IdentityDb>()
        .AddDefaultTokenProviders();
builder.AddFramework<App>();
builder.Services.AddScoped<UserService>();
var app = builder.Build();

app.UseRouting();
app.UseFramework<App>();
app.Run();