using AppStudio.Enterprise.Data;
using AppStudio.Framework.Core;
using AppStudio.Model;
using Unify.Data;
using Unify.People.Models;
using Unify.People.Services;

namespace Unify.People.Widgets
{
    public class KpiAverageHours : KpiView
    {
        private readonly EmployeeService _employeeService;

        public KpiAverageHours(EmployeeService employeeService)
        {
            _employeeService = employeeService;
            OnLoadData += KpiAverageHours_OnLoadData;
        }

        private async Task<KpiModel> KpiAverageHours_OnLoadData()
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
                )
                .GroupBy(g => new { g.EmployeeId })
                .Select(g => new AggregateDecimalData
                {
                    Value = g.Sum(a => (decimal)a.Time)
                })
                .FirstOrDefault()?.Value ?? 0;

            var daysCount = peopleDb.AttendanceLogs
                .Where(m =>
                    m.EmployeeId == employee.Id &&
                    m.DateTime.Month == toDay.Month &&
                    m.DateTime.Year == toDay.Year)
                .Select(m => m.DateTime.Date)
                .Distinct()
                .Count();

            if (daysCount > 0) groupedData = groupedData / daysCount;

            int totalSeconds = (int)(groupedData * 60);
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;

            string totalTime = $"{hours:D2}:{minutes:D2}";
            var result = new AggregateStringData { Value = totalTime };

            KpiModel kpiModel = new();
            kpiModel.Title = "Average Hours (This Month)";
            kpiModel.Value = (result == null) ? "0" : result.Value;
            kpiModel.Color = KpiColor.Indigo;
            kpiModel.Icon = "fa-solid fa-clock-rotate-left";

            return kpiModel;
        }
    }
}
