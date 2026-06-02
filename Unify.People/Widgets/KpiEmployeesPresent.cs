using AppStudio.Framework.Core;
using AppStudio.Model;
using Microsoft.EntityFrameworkCore;
using Unify.Data;
using Unify.People.Models;

namespace Unify.People.Widgets
{
    public class KpiEmployeesPresent : KpiView
    {
        public KpiEmployeesPresent()
        {
            OnLoadData += KpiEmployeesPresent_OnLoadData;
        }

        private async Task<KpiModel> KpiEmployeesPresent_OnLoadData()
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
                .Count(x => x != null && x.Action == "CheckIn");

            KpiModel kpiModel = new();
            kpiModel.Title = "Employees Present (Today)";
            kpiModel.Value = result.ToString();
            kpiModel.Color = KpiColor.RoyalBlue;
            kpiModel.Icon = "fa-solid fa-people-group";

            return kpiModel;
        }
    }
}
