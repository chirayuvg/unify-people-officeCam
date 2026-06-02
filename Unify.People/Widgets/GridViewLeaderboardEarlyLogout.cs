using AppStudio.Framework.Core;
using AppStudio.Model;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using Unify.Data;
using Unify.People.Models;

namespace Unify.People.Widgets
{
    public class GridViewLeaderboardEarlyLogout<TModel> : GridView<TModel> where TModel : class, new()
    {
        public GridViewLeaderboardEarlyLogout()
        {
            OnLoadData += GridViewLeaderboardEarlyLogout_OnLoadData;
            Toolbar.Title = "This Week's Early Logouts";
        }

        private async Task<LoadResult> GridViewLeaderboardEarlyLogout_OnLoadData(DataSourceLoadOptions loadOptions, string? parentId)
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


            //string sql =
            //       $"SELECT name AS EmployeeName" +
            //       $"     , department AS Department" +
            //       $"     , COUNT(*) AS CountEarlyLogout" +
            //       $"  FROM attendanceindex";// +
            //     //  $" WHERE TenantId = '{TenantId}'";
            //if (!string.IsNullOrEmpty(departmentName)) sql += $"   AND department = '{departmentName}'";
            //sql += $"   AND (dateTime >= '{fromDate}' AND dateTime <= '{toDate}')";
            //sql += $"   AND action = 'CheckOut'" +
            //       $"   AND lastLog = true" +
            //       $"   AND (HOUR(DATE_ADD('HOUR', 5, DATE_ADD('MINUTE', 30, timeStamp))) < 18 OR (HOUR(DATE_ADD('HOUR', 5, DATE_ADD('MINUTE', 30, timeStamp))) = 18 AND MINUTE(DATE_ADD('HOUR', 5, DATE_ADD('MINUTE', 30, timeStamp))) <= 30))" +
            //       //$"   AND CAST(timeStamp AS Time) < CAST('18:30:00' AS Time)" +
            //       //$"   AND earlyLogout = true" +
            //       $" GROUP BY name, department " +
            //       $"HAVING COUNT(*) > 0";


            var peopleDb = CoreServices.GetService<PeopleDb>();

            var query = peopleDb.AttendanceLogs
    .Where(m => m.DateTime.Date >= fromDate && m.DateTime.Date <= toDate
        && m.Action == "CheckOut" && m.LastLog == true && m.EarlyLogout == true);
            if (!string.IsNullOrEmpty(departmentName)) query = query.Where(m => m.Department == departmentName);
            var results = query
                .AsEnumerable()
                //.Where(m =>
                //{
                //    var adjustedTime = m.TimeStamp.AddMinutes(330);
                //    return adjustedTime.Hour < 18 || (adjustedTime.Hour == 18 && adjustedTime.Minute <= 30);
                //})
                .GroupBy(m => new { m.Name, m.Department })
                .Select(m => new LeaderboardEarlyLogoutData
                {
                    //DateOnly = DateOnly.FromDateTime(m.Key.Date),
                    EmployeeName = m.Key.Name,
                    Department = m.Key.Department,
                    CountEarlyLogout = m.Count()
                })
                .Where(m => m.CountEarlyLogout > 0)
                .ToList();
            //var query = peopleDb.AttendanceLogs
            //    .Where(m => //m.TenantId == TenantId
            //         m.DateTime.Date >= fromDate.Date
            //        && m.DateTime.Date <= toDate.Date
            //        && m.Action == "CheckOut"
            //        && m.LastLog == true);

            //if (!string.IsNullOrEmpty(departmentName))
            //{
            //    query = query.Where(m => m.Department == departmentName);
            //}

            //var results = query
            //    .AsEnumerable()
            //    .Where(m => m.Action == "CheckOut" && m.LastLog == true && m.EarlyLogout == true)
            //    //{
            //    //    var adjustedTime = m.TimeStamp;
            //    //    return adjustedTime.Hour < 18 || (adjustedTime.Hour == 18 && adjustedTime.Minute <= 30);
            //    //}

            //    .GroupBy(m => new { m.Name, m.Department })
            //    .Select(m => new LeaderboardEarlyLogoutData
            //    {
            //        EmployeeName = m.Key.Name,
            //        Department = m.Key.Department,
            //        CountEarlyLogout = m.Count()
            //    })
            //    .Where(m => m.CountEarlyLogout > 0)
            //    .ToList();

            // Sorting by CountEarlyLogout in descending order
            results = results.OrderByDescending(m => m.CountEarlyLogout).ToList();

            //  Server-Side Pagination Handling
            int totalCount = results.Count;
            int pageSize = 5;
            results = results.Skip(loadOptions.Skip).Take(loadOptions.Take).ToList();

            return new LoadResult
            {
                data = results,
                totalCount = totalCount
            };
        }
    }
}
