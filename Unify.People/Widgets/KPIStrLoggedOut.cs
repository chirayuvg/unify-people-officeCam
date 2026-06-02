using AppStudio.Framework.Core;
using AppStudio.Model;
using Unify.Data;
using Unify.People.Models;

namespace Unify.People.Widgets
{
    public class KPIStrLoggedOut : KpiView
    {
        public KPIStrLoggedOut()
        {
            OnLoadData += KpiLoggedOut_OnLoadData;
        }

        private async Task<KpiModel> KpiLoggedOut_OnLoadData()
        {
            var TenantId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();
            DateTime toDay = DateTime.Now;

            /*
            string sql =
                $"SELECT COUNT(*) AS CountRecord" +
                $"  FROM attendanceindex" +
                $" WHERE TenantId = '{TenantId}'" +
                $"   AND (YEAR(dateTime) = {toDay.Year} AND MONTH(dateTime) = {toDay.Month} AND DAY(dateTime) = {toDay.Day})" +
                $"   AND action = 'CheckOut' and lastLog = true";
            // $"   AND CAST(timeStamp AS Time) > CAST('18:30:00' AS Time)";
            */

            var peopleDb = CoreServices.GetService<PeopleDb>();
            var count = peopleDb.AttendanceLogs
                .Where(m =>  m.DateTime.Date == toDay.Date && m.Action == "CheckOut")
                .Count();
            var result = new AggregateIntegerData { Value = count };

            KpiModel kpiModel = new();
            kpiModel.Title = "Logged Out";
            kpiModel.Value = (result == null) ? "0" : result.Value.ToString();
            kpiModel.Color = KpiColor.Purple;
            kpiModel.Icon = "fa-solid fa-right-from-bracket";

            return kpiModel;
        }
    }
}
