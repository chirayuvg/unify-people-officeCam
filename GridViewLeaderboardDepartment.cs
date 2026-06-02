using AppStudio.Framework.Core;
using AppStudio.Model;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using Elastic.Clients.Elasticsearch;
using Unify.Data;
using Unify.People.Controllers;
using Unify.People.Models;

namespace Unify.People.Widgets
{
    public class GridViewLeaderboardDepartment<TModel> : GridView<TModel> where TModel : class, new()
    {
        private ElasticsearchClient _elasticClient;

        public GridViewLeaderboardDepartment()
        {
            OnLoadData += GridViewLeaderboardDepartment_OnLoadData;
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


            //string sql =
            //       $"SELECT department AS Department" +
            //       $" , ROUND((ROUND( DATE_DIFF('hour', MIN(timeStamp), MAX(timeStamp)) / CAST(COUNT(DISTINCT CAST(dateTime AS DATE)) AS FLOAT), 2 ) / 3.5),2)  AS AvgDailyWorkingHours" +
            //       //$" , DATE_DIFF('minute', MIN(timeStamp), MAX(timeStamp))/60 AS AvgDailyWorkingHours" +
            //       //$"     , (SUM(time)/60) AS AvgDailyWorkingHours" +
            //       $"  FROM attendanceindex" +
            //       $" WHERE TenantId = '{TenantId}'";
            //sql += $"   AND (dateTime >= '{fromDate}' AND dateTime <= '{toDate}')";
            //sql += $" GROUP BY department " +
            //       $"HAVING ROUND( DATE_DIFF('hour', MIN(timeStamp), MAX(timeStamp)) / CAST(COUNT(DISTINCT CAST(dateTime AS DATE)) AS FLOAT), 2 ) > 0";


            var peopleDb = CoreServices.GetService<PeopleDb>();


            var query = peopleDb.AttendanceLogs
    .Where(a =>
        a.DateTime.Date >= fromDate &&
        a.DateTime.Date <= toDate &&
        !string.IsNullOrEmpty(a.Department));



            //var results = peopleDb.AttendanceLogs
            //    .Where(m => //m.TenantId == TenantId
            //         m.DateTime.Date >= fromDate.Date
            //        && m.DateTime.Date <= toDate.Date)
            //    .GroupBy(m => m.Department)
            //    .AsEnumerable()
            //    .Select(m =>
            //    {
            //        var minTime = m.Min(x => x.TimeStamp);
            //        var maxTime = m.Max(x => x.TimeStamp);
            //        var totalDays = m.Select(x => x.DateTime.Date).Distinct().Count();

            //        var totalHourDiff = (decimal)(maxTime - minTime).TotalHours;
            //        var avgDailyWorkingHours = totalDays > 0 ? Math.Round(totalHourDiff / totalDays, 2) : 0;

            //        return new LeaderboardDepartmentData
            //        {
            //            Department = m.Key,
            //            AvgDailyWorkingHours = avgDailyWorkingHours
            //        };
            //    })
            //    .Where(m => m.AvgDailyWorkingHours > 0);

            // Query filtered by department (if provided)

            //        var query = peopleDb.AttendanceLogs
            //.Where(a =>
            //    a.DateTime.Date >= fromDate &&
            //    a.DateTime.Date <= toDate);

            //        var departmentResults = query
            //            .GroupBy(a => new { a.Department, a.EmployeeId })
            //            .Select(empGroup => new
            //            {
            //                Department = empGroup.Key.Department,
            //                TotalHours = empGroup.Sum(x => (decimal)x.Time) / 60, // minutes to hours
            //                DaysPresent = empGroup.Select(x => x.DateTime.Date).Distinct().Count()
            //            })
            //            .Where(x => x.DaysPresent > 0)
            //            .GroupBy(x => x.Department)
            //            .Select(deptGroup => new LeaderboardDepartmentData
            //            {
            //                Department = deptGroup.Key,
            //                AvgDailyWorkingHours = Math.Round(
            //                    deptGroup.Average(x => x.TotalHours / x.DaysPresent), 2)
            //            })
            //            .Where(d => d.AvgDailyWorkingHours > 0)
            //            .OrderByDescending(d => d.AvgDailyWorkingHours);
            //            //.ToList();

            var perEmployeeAverages = query
    .GroupBy(a => new { a.Department, a.EmployeeId })
    .Select(empGroup => new
    {
        empGroup.Key.Department,
        TotalHours = empGroup.Sum(x => (decimal)x.Time) / 60m, // Convert to hours
        DistinctDays = empGroup.Select(x => x.DateTime.Date).Distinct().Count()
    })
    .Where(x => x.DistinctDays > 0)
    .Select(x => new
    {
        x.Department,
        AvgHoursPerDay = x.TotalHours / x.DistinctDays
    });

            // Step 2: Group by Department and average over employee-level averages
            var departmentAverages = perEmployeeAverages
                .GroupBy(x => x.Department)
                .Select(deptGroup => new LeaderboardDepartmentData
                {
                    Department = deptGroup.Key,
                    AvgDailyWorkingHours = Math.Round(deptGroup.Average(e => e.AvgHoursPerDay), 2)
                })
                .Where(d => d.AvgDailyWorkingHours > 0)
                .OrderByDescending(d => d.AvgDailyWorkingHours).Distinct().Count();

            var departmentAverages2 = perEmployeeAverages
    .GroupBy(x => x.Department)
    .Select(deptGroup => new LeaderboardDepartmentData
    {
        Department = deptGroup.Key,
        AvgDailyWorkingHours = Math.Round(deptGroup.Average(e => e.AvgHoursPerDay), 2)
    })
    .Where(d => d.AvgDailyWorkingHours > 0)
    .OrderByDescending(d => d.AvgDailyWorkingHours).ToList();
            //.ToList();
            //       var results2 = departmentAverages.Distinct();

            int totalCount = departmentAverages;
            int pageSize = 5;
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            int currentPage = (loadOptions.Skip / pageSize) + 1;
            var results2 = departmentAverages2;
            results2 = results2.Skip(loadOptions.Skip).Take(loadOptions.Take).ToList();

            return new LoadResult
            {
                data = results2,
                totalCount = totalCount
            };
        }
    }
}
