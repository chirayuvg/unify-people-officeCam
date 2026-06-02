using AppStudio.Framework.Core;
using AppStudio.Model;
using MimeKit;
using Unify.Data;
using Unify.People.Models;

namespace Unify.People.Widgets
{
    public class KPIStrMonthly : KpiView
    {
        public KPIStrMonthly()
        {
            OnLoadData += KPIStrLoggedIn_OnLoadData;
        }

        private async Task<KpiModel> KPIStrLoggedIn_OnLoadData()
        {
            var TenantId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();
            DateTime fromDate = DateTime.Today;
            DateTime toDate = DateTime.Today;
            fromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            toDate = fromDate.AddMonths(1).AddDays(-1);

            /*
            string sql =
                $"SELECT GREATEST(COUNT(DISTINCT employeeId),0) AS CountRecord" +
                $"  FROM attendanceindex" +
                $" WHERE TenantId = '{TenantId}'" +
                $"   AND (YEAR(dateTime) = {toDay.Year} AND MONTH(dateTime) = {toDay.Month} AND DAY(dateTime) = {toDay.Day})";
            */

            var peopleDb = CoreServices.GetService<PeopleDb>();
            var count = peopleDb.AttendanceLogs
                .Where(m => m.DateTime.Date >= fromDate.Date && m.DateTime.Date <= toDate.Date && m.Action == "CheckIn")
                .Select(m => m.EmployeeId)
                .Distinct()
                .Count();
            var result = new AggregateIntegerData { Value = count };
            int day = DateTime.Now.Day;
            int res = 0;
            if (result != null)
            {
                 res = result.Value / day;
            }

                KpiModel kpiModel = new();
            kpiModel.Title = "Average Employees of month";
            kpiModel.Value = res.ToString();
            kpiModel.Color = KpiColor.SeaGreen;
            kpiModel.Icon = "fa-solid fa-right-to-bracket";

            return kpiModel;
        }
    }
}

