using AppStudio.Enterprise.Data;
using AppStudio.Framework.Core;
using AppStudio.Model;
using Unify.Data;
using Unify.People.Models;
using Unify.People.Services;

namespace Unify.People.Widgets
{
    public class KpiLogInTime : KpiView
    {
        private readonly EmployeeService _employeeService;

        public KpiLogInTime(EmployeeService employeeService)
        {
            _employeeService = employeeService;
            OnLoadData += KpiLogInTime_OnLoadData;
        }

        private async Task<KpiModel> KpiLogInTime_OnLoadData()
        {
            var TenantId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();
            var UserId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetUserId();
            TenantUser tenantUser = await Entity.FindByIdAsync<TenantUser>(UserId);
            var employee = await _employeeService.GetEmployeeByEmailAsync(TenantId, tenantUser.Email);
            DateTime toDay = DateTime.Now;

            var peopleDb = CoreServices.GetService<PeopleDb>();
            DateTime? logInTime = peopleDb.AttendanceLogs
                .Where(m =>
                       m.EmployeeId == employee.Id
                    && m.DateTime.Date == toDay.Date
                    && m.Action == "CheckIn"
                    && m.FirstLogin
                )
                .OrderBy(m => m.DateTime)
                .FirstOrDefault()?.DateTime;

            var result = new AggregateStringData { Value = logInTime?.ToString("dd-MM-yyyy HH:mm:ss") ?? "No Login" };

            KpiModel kpiModel = new();
            kpiModel.Title = "Login Time (Today)";
            kpiModel.Value = (result == null) ? "" : result.Value;
            kpiModel.Color = KpiColor.SeaGreen;
            kpiModel.Icon = "fa-solid fa-clock";

            return kpiModel;
        }
    }
}
