using AppStudio.Enterprise.Data;
using AppStudio.Framework.MVC;
using AppStudio.Model;
using DTC.Unify.HttpClientService;
using Unify.STR.Attendance.Controllers.Attendance;
using Unify.STR.Attendance.Controllers.Organization;
using Unify.STR.Attendance.Data.SQL;
using Unify.STR.Attendance.Services;

namespace Unify.STR.Attendance.App
{
    public class App : WebApp
    {
        public override void Init()
        {

            this.AddApplication("STR Attendance", "")
               .SetApplicationLogoUrl("/Images/sathyamangalamLogo.png");

            this.AddRole("Admin");
            this.AddRole("User");
            this.AddModule("Attendance", "Attendance Management", ["Admin", "User"]);
            this.AddModule("Visitor", "Visitor Management");

            this.SetLayout(LayoutTypes.AppLayout);


            this.AddDatabase<PeopleDb>(DatabaseType.PostgreSql);
            this.AddEnterpriseServices<ApplicationDb>(DatabaseType.PostgreSql);
            //this.AddSeedData<TestSeedData>();

            this.AddLoginPage()
                .SetLoginImageUrl("/Images/LoginImage.png")
               .AllowForgotPassword(true);

            this.AddRegistrationPage()
                .SetRegistrationType(RegistrationType.IndividalAndCompany)
                .RequiredConfirmedEmail(true)
                .ShowProductSelectionPage(true)
                .SetProductFilterTag("unify")
                .AddFreeTrail(true);


            var db = this.AddSubMenu("Dashboard", "fa-solid fa-industry");
            db.AddMenu<AttendanceDashboardAdminController>("Dashboard", "ms-Icon ms-Icon--Home");

            var att = this.AddSubMenu("Attendance", "fa-solid fa-industry");
            //att.AddMenu<TaskController>("Tasks", "fa-solid fa-list-check");
            att.AddMenu<AttendanceLogsController>("Logs", "fa-solid fa-camera");
            //att.AddMenu<AttendanceDashboardAdminController>("Dashboard", "fa-solid fa-building");
            //att.AddMenu<ReportDepartmentController>("Department Wise", "fa-solid fa-building");
            //att.AddMenu<ReportEmployeeController>("Employees Wise", "ms-Icon ms-Icon--Home");

            var org = this.AddSubMenu("Organization", "fa-solid fa-industry");
            org.AddMenu<EmployeeController>("Employee", "fa-solid fa-user");
            org.AddMenu<DesignationController>("Designation", "fa-solid fa-building");
            org.AddMenu<CircleController>("Circles", "fa-solid fa-building");
            org.AddMenu<DivisionController>("Divisions", "fa-solid fa-building");
            org.AddMenu<RangeController>("Ranges", "fa-solid fa-building");
            org.AddMenu<SectionController>("Sections", "fa-solid fa-building");
            org.AddMenu<BeatController>("Beats", "fa-solid fa-building");           
            org.AddMenu<EmployeeTypeController>("Employee Type", "fa-solid fa-users-gear");
            org.AddMenu<ShiftController>("Shift", "fa-solid fa-clock-rotate-left");
            org.AddMenu<LatePolicyController>("Late Policy", "fa-solid fa-hourglass-end");
        }

        public override void ConfigureServices(IHostApplicationBuilder builder)
        {
            //builder.Services.AddScoped<LocalStorageSettings>();
            //builder.Services.AddScoped<IStorageServices, LocalStorageServices>();
            //builder.Services.AddScoped<AIPersonService>();
            //builder.Services.AddSingleton<AttendanceRulesEngine>();
            //builder.Services.AddScoped<EmployeeService>();
            //builder.Services.AddScoped<ShiftService>();
            //builder.Services.AddScoped<LatePolicyService>();
            //builder.Services.AddScoped<AttendanceDashboardService>();
            //builder.Services.AddScoped<WidgetService>();
            //builder.Services.AddScoped<DashboardServices>();
            //builder.Services.AddSingleton<VisitorRulesEngine>();
            builder.Services.AddScoped<FaceService>();
            builder.Services.AddScoped<IdentityServices>();
            //builder.Services.AddScoped<AttendanceServices>();
        }
    }
}


