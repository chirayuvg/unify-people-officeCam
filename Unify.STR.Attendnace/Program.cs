using AppStudio.Framework.MVC;
using DTC.Unify.HttpClientService;
using Microsoft.AspNetCore.Identity;
using Unify.STR.Attendance.App;

var builder = WebApplication.CreateBuilder(args);

builder.AddFramework<App>();
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequiredUniqueChars = 1;
});


var app = builder.Build();
app.UseHttpsRedirection();
app.UseRouting();
app.UseFramework<App>();
app.Run();
