using AppStudio.Framework.Core;
using AppStudio.Model;
using Unify.Data;
using Unify.People.Models;

namespace Unify.People.Widgets
{
    public class KpiLateLogins : KpiView
    {
        public KpiLateLogins()
        {
            OnLoadData += KpiLateLogins_OnLoadData;
        }

        private async Task<KpiModel> KpiLateLogins_OnLoadData()
        {
            var TenantId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();
            DateTime toDay = DateTime.Now;

            /*
            string sql =
                $"SELECT GREATEST(COUNT(*),0) AS CountRecord" +
                $"  FROM attendanceindex" +
                $" WHERE TenantId = '{TenantId}'" +
                $"   AND (YEAR(dateTime) = {toDay.Year} AND MONTH(dateTime) = {toDay.Month} AND DAY(dateTime) = {toDay.Day})" +
                $"   AND action = 'CheckIn' and firstLogin = true " +
                $" AND (HOUR(DATE_ADD('MINUTE', 330, timeStamp)) > 9 OR (HOUR(DATE_ADD('MINUTE', 330, timeStamp)) = 9 AND MINUTE(DATE_ADD('MINUTE', 330, timeStamp)) >= 30))";

            //$"   AND CAST(timeStamp AS Time) > CAST('09:30:00' AS Time)";
            // $"   AND lateLogin = 'true'";
            */

            var peopleDb = CoreServices.GetService<PeopleDb>();

            var count = peopleDb.AttendanceLogs
                .Where(m =>  m.DateTime.Date == toDay.Date && m.Action == "CheckIn" && m.FirstLogin == true)
                .AsEnumerable()
                .Where(m =>
                {
                    var adjusted = m.TimeStamp;
                    return adjusted.Hour > 9 || (adjusted.Hour == 9 && adjusted.Minute >= 30);
                })
                .Count();
            var result = new AggregateIntegerData { Value = count };

            KpiModel kpiModel = new();
            kpiModel.Title = "Late Logins (Today)";
            kpiModel.Value = (result == null) ? "0" : result.Value.ToString();
            kpiModel.Color = KpiColor.Red;
            kpiModel.Icon = "fa-solid fa-clock";

            return kpiModel;
        }
    }
}
