using AppStudio.Enterprise.Data;
using AppStudio.Framework.Core;
using AppStudio.Model;
using DevExtreme.AspNet.Data;
using Unify.Data;
using Unify.People.Models;
using Unify.People.Services;

namespace Unify.People.Widgets
{
    public class GridViewEmployeeWiseDepartmentAvgHour<TModel> : GridView<TModel> where TModel : class, new()
    {

        public GridViewEmployeeWiseDepartmentAvgHour()
        {
            OnLoadData += GridViewEmployeeWiseDepartmentAvgHour_OnLoadData;
        }

        private async Task<DevExtreme.AspNet.Data.ResponseModel.LoadResult> GridViewEmployeeWiseDepartmentAvgHour_OnLoadData(DevExtreme.AspNet.Mvc.DataSourceLoadOptions loadOptions, string? parentId)
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
            var UserId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetUserId();
            TenantUser tenantUser = await Entity.FindByIdAsync<TenantUser>(UserId);
            var _employeeService = AppStudio.Framework.Core.CoreServices.GetService<EmployeeService>();
            var employee = await _employeeService.GetEmployeeByEmailAsync(TenantId, tenantUser.Email);

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
                    var totalMinutes = g.Sum(x => x.TotalMinutes);
                    var totalDays = g.Select(x => x.Day).Distinct().Count();  // <-- FIXED
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

            var results2 = new List<LeaderboardEmployeeData2>();


            foreach (var item in results)
            {
                int hours = (int)item.AvgDailyWorkingHours;
                int minutes = (int)Math.Round((item.AvgDailyWorkingHours - hours) * 60);

                // Format as "H:mm" (e.g., "8:30", "7:12")
                string timeFormatted = $"{hours}:{minutes:D2}";

                var emp = new LeaderboardEmployeeData2
                {
                    EmployeeName = item.EmployeeName,
                    Department = item.Department,
                    AvgDailyWorkingHours = timeFormatted
                };
                results2.Add(emp);
            }

            var loadResult = DataSourceLoader.Load(results2, loadOptions);

            return loadResult;
        }
    }
}
