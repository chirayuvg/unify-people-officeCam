using AppStudio.Framework.Core;
using AppStudio.Model;
using Microsoft.EntityFrameworkCore;
using Unify.Data;
using Unify.People.Models;

namespace Unify.People.Widgets
{
    public class KpiExitsRecorded : KpiView
    {
        public KpiExitsRecorded()
        {
            OnLoadData += KpiEarlyLogOuts_OnLoadData;
        }

        private async Task<KpiModel> KpiEarlyLogOuts_OnLoadData()
        {
            var TenantId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();
            DateTime toDay = DateTime.Now;

            var peopleDb = CoreServices.GetService<PeopleDb>();
            var groupedData = await peopleDb.AttendanceLogs
                .Where(a => a.DateTime.Date == toDay.Date)
                .GroupBy(a => a.Name)
                .ToListAsync(); // This moves the data to memory
            var result = groupedData
                .Select(g => g
                    .OrderByDescending(x => x.DateTime)
                    .FirstOrDefault()
                )
                .Count(x => x != null && x.Action == "CheckOut");

            KpiModel kpiModel = new();
            kpiModel.Title = "Exits Recorded (Today)";
            kpiModel.Value = result.ToString();
            kpiModel.Color = KpiColor.Orange;
            kpiModel.Icon = "fa-solid fa-door-open";

            return kpiModel;
        }
    }
}
