using AppStudio.Enterprise.Data;
using AppStudio.Framework.MVC;
using AppStudio.Model;
using DTC.Unify.HttpClientService;
using Unify.Data.SQL;
using Unify.People.Controllers.Attendance;
using Unify.People.Controllers.Organization;
using Unify.People.Controllers.Password;
using Unify.People.Controllers.Visitors;
using Unify.People.RulesEngine;
using Unify.People.Services;
using EmployeeService = Unify.People.Services.EmployeeService;
using LatePolicyService = Unify.People.Services.LatePolicyService;
using ShiftService = Unify.People.Services.ShiftService;

namespace Unify.People.App
{
    public class App : WebApp
    {
        public override void Init()
        {

            this.AddApplication("OfficeCAM", "")
                .SetApplicationLogoUrl("/Images/cam-360.svg");

            this.AddRole("Admin");
            this.AddRole("User");
            //this.AddMessaging().UseRabbitMQ();
            this.AddModule("Attendance", "Attendance Management", ["Admin", "User"]);
            this.AddModule("Visitor", "Visitor Management");

            this.SetLayout(LayoutTypes.AppLayout);

            this.AddDatabase<Unify.Data.PeopleDb>(DatabaseType.PostgreSql);
            this.AddEnterpriseServices<ApplicationDb>(DatabaseType.PostgreSql);
            //this.AddDatabase<CCCDb>(DatabaseType.PostgreSql);
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

            this.AddMenu<AttendanceController>("Kiosk", "fa-solid fa-person-booth").SetRoles("Admin");

            var att = this.AddSubMenu("Attendance", "fa-solid fa-user-check");
            att.AddMenu<AttendanceLogsController>("Logs", "");

            att.AddMenu<AttendanceDashboardAdminController>("Dashboard", "").SetRoles("Admin");
            att.AddMenu<AttendanceDashboardEmployeeController>("Dashboard", "").SetRoles("User");

            att.AddMenu<ApplyLeaveController>("Apply Leave", "").SetRoles("User");
            att.AddMenu<LeaveApprovalController>("Leave Approval", "").SetRoles("Admin");

            att.AddMenu<ReportDepartmentController>("Department Wise", "").SetRoles("Admin");
            att.AddMenu<ReportEmployeeController>("Employees Wise", "").SetRoles("Admin");
            att.AddMenu<ReportEmployeeController>("Report", "").SetRoles("User");
            //att.AddMenu<ApplyRegularizationController>("Apply Regularization", "");

            var visManage = this.AddSubMenu("Visitor Management", "fa-solid fa-address-card");
            visManage.AddMenu<AppointmentController>("Appointment", "");
            visManage.AddMenu<MeetingController>("Meeting", "");
            //visManage.AddMenu<VisitorDashboardController>("Dashboard", "ms-Icon ms-Icon--ViewDashboard");

            //var vis =  this.AddSubMenu("Visitor Management", "fa-solid fa-industry");
            //vis.AddMenu<VisitorDashboardController>("Dashboard", "ms-Icon ms-Icon--Home");
            //vis.AddMenu<AppointmentController>("Appointment", "fa-solid fa-building");
            //vis.AddMenu<VisitorController>("Visitor", "fa-solid fa-camera");
            //vis.AddMenu<VisitorMeetingController>("Meeting", "fa-solid fa-building");
            //vis.AddMenu<MeetingPurposeController>("Meeting Purpose", "fa-solid fa-building");
            //vis.AddMenu<MeetingPlaceController>("Meeting Place", "ms-Icon ms-Icon--Home");

            var org = this.AddSubMenu("Organization", "fa-solid fa-users-gear");
            org.SetRoles("Admin");
            org.AddMenu<EmployeeController>("Employee", "");
            org.AddMenu<DepartmentController>("Department", "");
            org.AddMenu<DesignationController>("Designation", "");
            org.AddMenu<EmployeeTypeController>("Employee Type", "");
            org.AddMenu<ShiftController>("Shift", "");
            org.AddMenu<LatePolicyController>("Late Policy", "");
            org.AddMenu<LeavePolicyController>("Leave Policy", "");
            org.AddMenu<RegularizationPolicyController>("Regularization Policy", "");

            org.AddMenu<VisitorController>("Visitor", "");
            org.AddMenu<MeetingPurposeController>("Meeting Purpose", "");
            org.AddMenu<MeetingPlaceController>("Meeting Place", "");

            //org.AddMenu<DoorStatusController>("Door Status", "");

            //this.AddMenu<ApplyRegularizationController>("Regularization", "fa-solid fa-calendar-check");
            this.AddMenu<ChangePasswordController>("Change Password", "fa-solid fa-key");
        }

        public override void ConfigureServices(IHostApplicationBuilder builder)
        {
            builder.Services.AddScoped<LocalStorageSettings>();
            builder.Services.AddScoped<IStorageServices, LocalStorageServices>();
            builder.Services.AddScoped<AIPersonService>();
            builder.Services.AddScoped<InferenceServices>();
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
            builder.Services.AddScoped<ConsumeServices>();
            //builder.Services.AddScoped<CameraServices>();
        }
    }
}
