using AppStudio.Enterprise.Data;
using AppStudio.Framework.MVC;
using AppStudio.Model;
using DTC.Unify.HttpClientService;
using Unify.Data;
using Unify.Data.SQL;
using Unify.People.Controllers;
using Unify.People.Controllers.Attendance;
using Unify.People.Controllers.Organization;
using Unify.People.RulesEngine;
using Unify.People.Services;

namespace Unify.People.App
{
    public class AppSTR : WebApp
    {
        public override void Init()
        {

            this.AddApplication("STR Attendance", "")
               .SetApplicationLogoUrl("/Images/cam-360.svg");

            this.AddRole("Admin");
            this.AddRole("User");
            this.AddModule("Attendance", "Attendance Management", ["Admin", "User"]);
            this.AddModule("Visitor", "Visitor Management");

            this.SetLayout(LayoutTypes.AppLayout);


            this.AddDatabase<PeopleDb>(DatabaseType.PostgreSql);
            this.AddEnterpriseServices<ApplicationDb>(DatabaseType.PostgreSql);
            this.AddSeedData<TestSeedData>();

            this.AddLoginPage()
                .SetLoginImageUrl("/Images/LoginImage.png")
               .AllowForgotPassword(true);

            this.AddRegistrationPage()
                .SetRegistrationType(RegistrationType.IndividalAndCompany)
                .RequiredConfirmedEmail(true)
                .ShowProductSelectionPage(true)
                .SetProductFilterTag("unify")
                .AddFreeTrail(true);

            this.AddMenu<AttendanceController>("Kiosk", "fa-solid fa-person-booth");

            var att = this.AddSubMenu("Attendence", "fa-solid fa-industry");
            att.AddMenu<AttendanceLogsController>("Logs", "fa-solid fa-camera");
            att.AddMenu<STRDashboardController>("Dashboard", "ms-Icon ms-Icon--Home");
            att.AddMenu<ReportDepartmentController>("Department Wise", "fa-solid fa-building");
            att.AddMenu<ReportEmployeeController>("Employees Wise", "ms-Icon ms-Icon--Home");



            var org = this.AddSubMenu("Organization", "fa-solid fa-industry");
            org.AddMenu<EmployeeAppController>("Employee", "fa-solid fa-user");
            org.AddMenu<DepartmentController>("Department", "fa-solid fa-building");
            org.AddMenu<DesignationController>("Designation", "fa-solid fa-address-card");
            org.AddMenu<EmployeeTypeController>("Employee Type", "fa-solid fa-users-gear");
            org.AddMenu<ShiftController>("Shift", "fa-solid fa-clock-rotate-left");
            org.AddMenu<LatePolicyController>("Late Policy", "fa-solid fa-hourglass-end");
        }

        public override void ConfigureServices(IHostApplicationBuilder builder)
        {
            builder.Services.AddScoped<LocalStorageSettings>();
            builder.Services.AddScoped<IStorageServices, LocalStorageServices>();
            builder.Services.AddScoped<AIPersonService>();
            builder.Services.AddSingleton<AttendanceRulesEngine>();
            builder.Services.AddScoped<EmployeeService>();
            builder.Services.AddScoped<ShiftService>();
            builder.Services.AddScoped<LatePolicyService>();
            builder.Services.AddScoped<AttendanceDashboardService>();
            builder.Services.AddScoped<WidgetService>();
            builder.Services.AddScoped<DashboardServices>();
            builder.Services.AddSingleton<VisitorRulesEngine>();
            builder.Services.AddScoped<FaceService>();
            builder.Services.AddScoped<AttendanceServices>();
            builder.Services.AddScoped<AppointmentServices>();
            builder.Services.AddScoped<VisitorLogServices>();
        }
    }
}
