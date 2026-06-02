using AppStudio.Framework.Core;
using AppStudio.Model;
using DevExtreme.AspNet.Data.ResponseModel;
using Unify.Data;
using Unify.People.Models;

namespace Unify.People.Widgets
{
    public class GridViewLeaderboardWorkingDays<TModel> : GridView<TModel> where TModel : class, new()
    {
        public GridViewLeaderboardWorkingDays()
        {
            OnLoadData += GridViewLeaderboardWorkingDays_OnLoadData;
            Toolbar.Title += "Working Days";
        }

        private async Task<DevExtreme.AspNet.Data.ResponseModel.LoadResult> GridViewLeaderboardWorkingDays_OnLoadData(DevExtreme.AspNet.Mvc.DataSourceLoadOptions loadOptions, string? parentId)
        {
            var cardView = this.Page.FindControl<CardView<DashboardFilterData>>();
            var TenantId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();
            DateTime fromDate =
                (cardView.Data.FromDate == DateOnly.MinValue)
                ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
                : DateTime.Parse(cardView.Data.FromDate.ToString("yyyy-MM-dd"));
            DateTime toDate =
                (cardView.Data.ToDate == DateOnly.MinValue)
                ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
                : DateTime.Parse(cardView.Data.ToDate.ToString("yyyy-MM-dd"));
            var peopleDb = CoreServices.GetService<PeopleDb>();
            var results = peopleDb.AttendanceLogs
                .Where(a => a.Action == "CheckIn" &&
                a.DateTime.Date >= fromDate &&
                a.DateTime.Date <= toDate)
                    .GroupBy(a => a.Name)
                    .Select(g => new
                    {
                        Name = g.Key,
                        WorkingDays = g
                            .Select(x => x.DateTime.Date)
                            .Distinct()
                            .Count()
                    })
                    .ToList();

            int totalCount = results.Count;
            int pageSize = 5;
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            int currentPage = (loadOptions.Skip / pageSize) + 1;

            results = results.Skip(loadOptions.Skip).Take(loadOptions.Take).ToList();

            return new LoadResult
            {
                data = results,
                totalCount = totalCount
            };
        }
    }
}
