using UnifyNow.Forest.Application;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<AlertRuleValidationService>();

var app = builder.Build();
app.Run();
