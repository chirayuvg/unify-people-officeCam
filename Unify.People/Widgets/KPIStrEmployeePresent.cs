using AppStudio.Framework.Core;
using AppStudio.Model;
using Unify.Data;
using Unify.People.Models;

namespace Unify.People.Widgets
{
    public class KPIStrEmployeePresent : KpiView
    {
        public KPIStrEmployeePresent()
        {
            OnLoadData += KpiEmployeesPresent_OnLoadData;
        }

        private async Task<KpiModel> KpiEmployeesPresent_OnLoadData()
        {
            var TenantId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();
            DateTime toDay = DateTime.Now;

            /*
            string sql =
                $"SELECT (" +
                $"          COUNT(DISTINCT CASE WHEN action = 'CheckIn' THEN name END)" +
                $"          -" +
                $"          COUNT(DISTINCT CASE WHEN (action = 'CheckOut' AND CAST(timeStamp AS Time) > CAST('18:30:00' AS Time)) THEN name END)" +
                $"       ) AS CountRecord" +
                $"  FROM attendanceindex" +
                $" WHERE TenantId = '{TenantId}'" +
                $"   AND (YEAR(dateTime) = {toDay.Year} AND MONTH(dateTime) = {toDay.Month} AND DAY(dateTime) = {toDay.Day})";
            */

            var peopleDb = CoreServices.GetService<PeopleDb>();
            var filtered = peopleDb.AttendanceLogs
                .Where(a =>  a.DateTime.Date == toDay.Date)
                .ToList();
            var grouped = filtered
                .GroupBy(a => a.Name)
                .Select(g =>
                {
                    bool hasEntry = g.Any(x => x.Action == "CheckIn");
                    bool hasLateExit = g.Any(x => x.Action == "CheckOut" && x.TimeStamp.TimeOfDay > new TimeSpan(18, 30, 0));
                    return new { Include = hasEntry && !hasLateExit };
                })
                .Count(x => x.Include);
            var result = new AggregateIntegerData { Value = grouped };

            KpiModel kpiModel = new();
            kpiModel.Title = "Employees Present (Today)";
            kpiModel.Value = (result == null) ? "0" : result.Value.ToString();
            kpiModel.Color = KpiColor.RoyalBlue;
            kpiModel.Icon = "fa-solid fa-people-group";

            return kpiModel;
        }
    }
}

