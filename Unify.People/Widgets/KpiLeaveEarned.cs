using AppStudio.Enterprise.Data;
using AppStudio.Framework.Core;
using AppStudio.Model;
using Microsoft.EntityFrameworkCore;
using Unify.Data;
using Unify.Data.SQL;
using Unify.People.Models;
using Unify.People.Services;

namespace Unify.People.Widgets
{
    public class KpiLeaveEarned : KpiView
    {
        public KpiLeaveEarned()
        {
            OnLoadData += KpiLeaveEarned_OnLoadData;
        }

        private async Task<KpiModel> KpiLeaveEarned_OnLoadData()
        {
            var TenantId = CoreServices.CurrentUser.GetTenantId();

            var peopleDb = CoreServices.GetService<PeopleDb>();
            LeavePolicy leavePolicy = peopleDb.LeavePolicies
                .Where(m => m.Name.ToUpper().StartsWith("EARNED"))
                .FirstOrDefault();

            var userId = CoreServices.CurrentUser.GetUserId();
            TenantUser tenantUser = await Entity.FindByIdAsync<TenantUser>(userId);
            var _employeeService = CoreServices.GetService<EmployeeService>();
            var employee = await _employeeService.GetEmployeeByEmailAsync(TenantId, tenantUser.Email);

            var totalDaysNullable = await peopleDb.LeaveLogs
                .Where(m => m.EmployeeId == employee.Id && m.LeavePolicyId == leavePolicy.Id)
                .Select(m => (int?)m.Days)
                .SumAsync();
            int totalDays = totalDaysNullable ?? 0;

            var result = new AggregateIntegerData { Value = (leavePolicy.QuotaPerYear - totalDays) };

            KpiModel kpiModel = new();
            kpiModel.Title = "Earned";
            kpiModel.Value = (result == null) ? "0" : result.Value.ToString();
            kpiModel.Color = KpiColor.SeaGreen;
            kpiModel.Icon = "fa-solid fa-briefcase";

            return kpiModel;
        }
    }
}
