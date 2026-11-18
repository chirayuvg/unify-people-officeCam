using Toll.Web.UI.Application;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<VehicleValidationService>();
var app = builder.Build();
app.Run();
