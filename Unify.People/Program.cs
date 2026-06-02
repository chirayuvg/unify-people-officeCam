using AppStudio.Framework.MVC;
using DTC.Unify.HttpClientService;
using Unify.People;
using Unify.People.App;
using Unify.People.RulesEngine;
using Unify.People.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddFramework<App>();



var app = builder.Build();
app.UseHttpsRedirection();
app.UseRouting();
app.UseFramework<App>();
app.Run();
