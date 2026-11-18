using Unify.STR.Attendance.Application;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<AttendanceImportValidationService>();
var app = builder.Build();
app.Run();
