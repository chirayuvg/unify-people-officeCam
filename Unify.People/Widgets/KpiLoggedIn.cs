using AppStudio.Framework.Core;
using AppStudio.Model;
using Unify.Data;
using Unify.People.Models;

namespace Unify.People.Widgets
{
    public class KpiLoggedIn : KpiView
    {
        public KpiLoggedIn()
        {
            OnLoadData += KpiLoggedIn_OnLoadData;
        }

        private async Task<KpiModel> KpiLoggedIn_OnLoadData()
        {
            var TenantId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();
            DateTime toDay = DateTime.Now;

            /*
            string sql =
                $"SELECT GREATEST(COUNT(DISTINCT employeeId),0) AS CountRecord" +
                $"  FROM attendanceindex" +
                $" WHERE TenantId = '{TenantId}'" +
                $"   AND (YEAR(dateTime) = {toDay.Year} AND MONTH(dateTime) = {toDay.Month} AND DAY(dateTime) = {toDay.Day})";
            */

            var peopleDb = CoreServices.GetService<PeopleDb>();
            var count = peopleDb.AttendanceLogs
                .Where(m =>  m.DateTime.Date == toDay.Date && m.Action == "CheckIn")
                .Select(m => m.EmployeeId)
                .Distinct()
                .Count();
            var result = new AggregateIntegerData { Value = count };

            KpiModel kpiModel = new();
            kpiModel.Title = "Logged In (Today)";
            kpiModel.Value = (result == null) ? "0" : result.Value.ToString();
            kpiModel.Color = KpiColor.SeaGreen;
            kpiModel.Icon = "fa-solid fa-right-to-bracket";

            return kpiModel;
        }
    }
}
