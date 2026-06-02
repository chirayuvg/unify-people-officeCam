using AppStudio.Enterprise.Data;
using AppStudio.Framework.Core;
using AppStudio.Model;
using Unify.Data;
using Unify.People.Models;
using Unify.People.Services;

namespace Unify.People.Widgets
{
    public class KpiTotalEarlyLogout : KpiView
    {
        private readonly EmployeeService _employeeService;

        public KpiTotalEarlyLogout(EmployeeService employeeService)
        {
            _employeeService = employeeService;
            OnLoadData += KpiTotalEarlyLogout_OnLoadData;
        }

        private async Task<KpiModel> KpiTotalEarlyLogout_OnLoadData()
        {
            var TenantId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();
            var UserId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetUserId();
            TenantUser tenantUser = await Entity.FindByIdAsync<TenantUser>(UserId);
            var employee = await _employeeService.GetEmployeeByEmailAsync(TenantId, tenantUser.Email);
            DateTime toDay = DateTime.Now;

            var peopleDb = CoreServices.GetService<PeopleDb>();
            var groupedData = peopleDb.AttendanceLogs
                .Where(m =>
                       m.EmployeeId == employee.Id
                    && m.DateTime.Date.Month == toDay.Date.Month
                    && m.DateTime.Date.Year == toDay.Date.Year
                    && m.EarlyLogout
                )
                .Count();

            var result = new AggregateIntegerData { Value = groupedData };

            KpiModel kpiModel = new();
            kpiModel.Title = "Total Early Logout (This Month)";
            kpiModel.Value = (result == null) ? "0" : result.Value.ToString();
            kpiModel.Color = KpiColor.Orange;
            kpiModel.Icon = "fa-solid fa-door-open";

            return kpiModel;
        }
    }
}
