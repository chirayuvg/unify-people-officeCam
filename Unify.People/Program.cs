using Unify.People.Application.Attendance;
using Unify.People.Application.Organisation;
using Unify.People.Application.Visitors;

var builder = WebApplication.CreateBuilder(args);

// The host is intentionally presentation- and persistence-free during the
// migration. These services contain only framework-free application rules.
builder.Services.AddSingleton<OrganisationValidationService>();
builder.Services.AddSingleton<AttendanceValidationService>();
builder.Services.AddSingleton<VisitorMeetingValidationService>();

var app = builder.Build();
app.Run();
