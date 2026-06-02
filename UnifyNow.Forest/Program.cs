using AppStudio.Framework.Mvc;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;

using Microsoft.EntityFrameworkCore;
using UnifyNow.Forest;
using UnifyNow.Forest.App;
using UnifyNow.Forest.CoreServices;
using UnifyNow.Forest.Data.SQL;
using UnifyNow.Forest.HttpClientService;
using UnifyNow.Forest.RulesEngine;
using UnifyNow.Forest.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<CameraServices>();
builder.Services.AddScoped<AreasServices>();
builder.Services.AddScoped<AnimalServices>();
builder.Services.AddScoped<LogServices>();
builder.Services.AddScoped<AreaServices>();
builder.Services.AddScoped<PresetServices>();
builder.Services.AddScoped<AIForestService>();
builder.Services.AddScoped<GalleryServices>();
builder.Services.AddScoped<AlertServices>();
builder.Services.AddScoped<InferenceServices>();
builder.Services.AddScoped<TicketServices>();
builder.Services.AddScoped<WhatsappService>();
builder.Services.AddScoped<SMSServices>();
builder.Services.AddSingleton<ForestRulesEngine>();
builder.Services.AddScoped<LocalStorageSettings>();
builder.Services.AddScoped<IStorageServices, LocalStorageServices>();

builder.Services.AddHostedService<SaveDataRabbitMQBackgroundService>();
builder.Services.AddScoped<Unify.CCC.Services.CameraServices>();


builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<AutoMappings>();
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        builder => builder.WithOrigins("http://localhost", "http://localhost:3000", "http://localhost:3001", "http://192.168.0.125:7300") // Allow localhost on different ports
                          .AllowAnyMethod()  // Allow any HTTP method (GET, POST, etc.)
                          .AllowAnyHeader()); // Allow any headers
});


builder.AddFramework<App>();

var app = builder.Build();
app.UseCors("AllowLocalhost");
app.UseRouting();
app.UseFramework<App>();
app.Run();