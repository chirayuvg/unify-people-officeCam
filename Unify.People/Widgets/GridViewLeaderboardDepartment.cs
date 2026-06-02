using AppStudio.Framework.Core;
using AppStudio.Model;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using Unify.Data;
using Unify.People.Models;

namespace Unify.People.Widgets
{
    public class GridViewLeaderboardDepartment<TModel> : GridView<TModel> where TModel : class, new()
    {
        public GridViewLeaderboardDepartment()
        {
            OnLoadData += GridViewLeaderboardDepartment_OnLoadData;
            Toolbar.Title = "This Week's Avg Working Hours";
        }

        private async Task<LoadResult> GridViewLeaderboardDepartment_OnLoadData(DataSourceLoadOptions loadOptions, string? parentId)
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

            string departmentName = (cardView.Data.DepartmentName == null || cardView.Data.DepartmentName == "") ? "" : cardView.Data.DepartmentName;

            var peopleDb = CoreServices.GetService<PeopleDb>();
            var query = peopleDb.AttendanceLogs
                .Where(a => //a.TenantId == TenantId
                    a.DateTime.Date >= fromDate
                    && a.DateTime.Date <= toDate
                );

            if (!string.IsNullOrEmpty(departmentName))
            {
                query = query.Where(a => a.Department == departmentName);
            }

            var results = query
                .AsEnumerable()
                .GroupBy(m => new { m.Name, m.Department, Day = m.DateTime.Date })
                .Select(g => new
                {
                    g.Key.Name,
                    g.Key.Department,
                    g.Key.Day,
                    TotalMinutes = g.Sum(x => x.Time)
                })
                .GroupBy(x => new { x.Name, x.Department })
                .Select(g =>
                {
                    int numberOfDays = (toDate - fromDate).Days;

                    var totalMinutes = g.Sum(x => x.TotalMinutes);
                    var totalDays = numberOfDays;// g.Count();
                    var avgDailyHours = totalDays > 0
                        ? (decimal)totalMinutes / 60m / totalDays
                        : 0m;

                    var avgDailyWorkingHours = Math.Round(avgDailyHours, 2);

                    return new LeaderboardEmployeeData
                    {
                        EmployeeName = g.Key.Name,
                        Department = g.Key.Department,
                        AvgDailyWorkingHours = avgDailyWorkingHours
                    };
                })
                .Where(m => m.AvgDailyWorkingHours > 0)
                .ToList();

            results = results.OrderByDescending(m => m.AvgDailyWorkingHours).ToList();

            var departmentWiseAverage = results
                .GroupBy(x => new { x.Department })
                .Select(g => new
                {
                    Department = g.Key.Department,
                    AvgDailyWorkingHours = Math.Round(g.Average(x => x.AvgDailyWorkingHours), 2)
                });


            var results2 = new List<LeaderboardDepartmentData2>();


            foreach (var item in departmentWiseAverage)
            {
                int hours = (int)item.AvgDailyWorkingHours;
                int minutes = (int)Math.Round((item.AvgDailyWorkingHours - hours) * 60);

                // Format as "H:mm" (e.g., "8:30", "7:12")
                string timeFormatted = $"{hours}:{minutes:D2}";

                var emp = new LeaderboardDepartmentData2
                {
                    Department = item.Department,
                    AvgDailyWorkingHours = timeFormatted
                };
                results2.Add(emp);
            }

            var loadResult = DataSourceLoader.Load(results2, loadOptions);
           // var loadResult = DataSourceLoader.Load(departmentWiseAverage, loadOptions);

            return loadResult;
        }
    }
}
