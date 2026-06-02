using AppStudio.Framework.Core;
using AppStudio.Model;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using Unify.Data;
using Unify.People.Models;

namespace Unify.People.Widgets
{
    public class GridViewLeaderboardLateLogin<TModel> : GridView<TModel> where TModel : class, new()
    {
        public GridViewLeaderboardLateLogin()
        {
            OnLoadData += GridViewLeaderboardLateLogin_OnLoadData;
            Toolbar.Title = "This Week's Late Logins";
        }

        private async Task<LoadResult> GridViewLeaderboardLateLogin_OnLoadData(DataSourceLoadOptions loadOptions, string? parentId)
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
            //       $"     , COUNT(*) AS CountLateLogin" +
            //       $"  FROM attendanceindex" +
            //       $" WHERE TenantId = '{TenantId}'";
            //if (!string.IsNullOrEmpty(departmentName)) sql += $"   AND department = '{departmentName}'";
            //sql += $"   AND (dateTime >= '{fromDate}' AND dateTime <= '{toDate}')";
            //sql += $"   AND action = 'CheckIn'" +
            //       $"   AND firstLogin = true" +
            //       $"   AND (HOUR(DATE_ADD('HOUR', 5, DATE_ADD('MINUTE', 30, timeStamp))) > 9 OR (HOUR(DATE_ADD('HOUR', 5, DATE_ADD('MINUTE', 30, timeStamp))) = 9 AND MINUTE(DATE_ADD('HOUR', 5, DATE_ADD('MINUTE', 30, timeStamp))) >= 30))" +
            //       //$"   AND startTime > '09:30:00'" +
            //       $" GROUP BY name, department " +
            //       $"HAVING COUNT(*) > 0";


            var peopleDb = CoreServices.GetService<PeopleDb>();
            var query = peopleDb.AttendanceLogs
                .Where(a => //a.TenantId == TenantId
                     a.DateTime.Date >= fromDate
                    && a.DateTime.Date <= toDate
                    && a.Action == "CheckIn"
                    && a.FirstLogin == true);
            if (!string.IsNullOrEmpty(departmentName)) query = query.Where(a => a.Department == departmentName);
            var results = query
                .AsEnumerable()
                .Where(m =>
                {
                    var adjustedTime = m.TimeStamp.AddHours(5).AddMinutes(30);
                    return adjustedTime.Hour > 9 || (adjustedTime.Hour == 9 && adjustedTime.Minute >= 30);
                })
                .GroupBy(a => new { a.Name, a.Department })
                .Select(m => new LeaderboardLateLoginData
                {
                    EmployeeName = m.Key.Name,
                    Department = m.Key.Department,
                    CountLateLogin = m.Count()
                })
                .Where(m => m.CountLateLogin > 0)
                .ToList();

            // Apply sorting and pagination at the database level
            results = results.OrderByDescending(m => m.CountLateLogin).ToList();

            return DevExtreme.AspNet.Data.DataSourceLoader.Load(results, loadOptions);
        }
    }
}
