using AppStudio.Framework.Core;
using AppStudio.Model;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using Elastic.Clients.Elasticsearch;
using Unify.Data;
using Unify.People.Controllers;
using Unify.People.Models;
using Unify.People.RulesEngine;

namespace Unify.People.Widgets
{
    public class GridViewLeaderboardEmployee<TModel> : GridView<TModel> where TModel : class, new()
    {
        private ElasticsearchClient _elasticClient;

        public GridViewLeaderboardEmployee()
        {
            OnLoadData += GridViewLeaderboardEmployee_OnLoadData;
        }

        private async Task<LoadResult> GridViewLeaderboardEmployee_OnLoadData(DataSourceLoadOptions loadOptions, string? parentId)
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
            //       //$"     , CAST(dateTime AS DATE) AS WorkDate" +
            //       $" , ROUND((ROUND( DATE_DIFF('hour', MIN(timeStamp), MAX(timeStamp)) / CAST(COUNT(DISTINCT CAST(dateTime AS DATE)) AS FLOAT), 2 ) / 3.5 ),2)  AS AvgDailyWorkingHours" +
            //       //$"  , DATE_DIFF('hour', MIN(timeStamp), MAX(timeStamp))  AS AvgDailyWorkingHours" +
            //       // $"     , (SUM(time)/60) AS AvgDailyWorkingHours" +
            //       $"  FROM attendanceindex" +
            //       $" WHERE TenantId = '{TenantId}'";
            //if (!string.IsNullOrEmpty(departmentName)) sql += $"   AND department = '{departmentName}'";
            //sql += $"   AND (dateTime >= '{fromDate}' AND dateTime <= '{toDate}')";
            //sql += $" GROUP BY name, department " +
            //       $" HAVING ROUND( DATE_DIFF('hour', MIN(timeStamp), MAX(timeStamp)) / CAST(COUNT(DISTINCT CAST(dateTime AS DATE)) AS FLOAT), 2 ) > 0";


            var peopleDb = CoreServices.GetService<PeopleDb>();


            //var final = peopleDb.AttendanceLogs
            //    .Where(a =>
            //        a.DateTime.Date >= fromDate.Date &&
            //        a.DateTime.Date <= toDate.Date &&
            //        (string.IsNullOrEmpty(departmentName) || a.Department == departmentName))
            //    .AsEnumerable() // Switch to in-memory LINQ to avoid expression tree issues
            //    .GroupBy(a => new { a.EmployeeId, a.Name, a.Department })
            //    .Select(g =>
            //    {
            //        var totalMinutes = g.Sum(x => x.Time);
            //        var totalDays = g.Select(x => x.DateTime.Date).Distinct().Count();
            //        var avgDailyHours = totalDays > 0 ? Math.Round((decimal)totalMinutes / 60 / totalDays, 2) : 0;

            //        return new LeaderboardEmployeeData
            //        {
            //            EmployeeName = g.Key.Name,
            //            Department = g.Key.Department,
            //            AvgDailyWorkingHours = avgDailyHours
            //        };
            //    })
            //    .Where(x => x.AvgDailyWorkingHours > 0)
            //    .OrderByDescending(x => x.AvgDailyWorkingHours)
            //    .ToList();

            var attendanceLogs = peopleDb.AttendanceLogs
    .Where(a => //a.TenantId == TenantId
         a.DateTime >= fromDate
        && a.DateTime <= toDate
        && (a.EmployeeId.ToString() != "00000000-0000-0000-0000-000000000000")
            );
            var reportAttendanceIndex = attendanceLogs
                .Select(a => new ReportAttendanceIndex
                {
                    EmployeeId = a.EmployeeId.ToString(),
                    Name = a.Name,
                    Code = a.Code,
                    Department = a.Department,
                    DateTime = a.DateTime,
                    Action = a.Action,
                    FirstLogin = a.FirstLogin,
                    LastLog = a.LastLog,
                    LateLogin = a.LateLogin,
                    EarlyLogout = a.EarlyLogout,
                    Time = (decimal)a.Time,
                    TimeStamp = a.TimeStamp,
                })
                .ToList();

            var results = reportAttendanceIndex
    .Where(a => a.Time > 0) // Exclude zero time entries
    .GroupBy(a => new { a.Name, a.Department, Date = a.DateTime.Date }) // Daily grouping
    .Select(g => new
    {
        g.Key.Name,
        g.Key.Department,
        DailyHours = g.Sum(x => x.Time) / 60m  // Minutes to hours
    })
    .GroupBy(e => new { e.Name, e.Department }) // Group by employee + department
    .Select(empGroup => new
    {
        Name = empGroup.Key.Name,
        Department = empGroup.Key.Department,
        AvgDailyWorkingHours = Math.Round(empGroup.Average(x => x.DailyHours), 2)
    });
    //.OrderByDescending(x => x.AvgDailyWorkingHours);
    //.ToList();



            //        var query = peopleDb.AttendanceLogs
            //.Where(a =>
            //    a.DateTime.Date >= fromDate.Date &&
            //    a.DateTime.Date <= toDate.Date &&
            //    !string.IsNullOrEmpty(a.Department) &&
            //    a.Time > 0) // Exclude zero time records
            //.GroupBy(a => new { a.Name, a.Department })
            //.Select(g => new
            //{
            //    Department = g.Key.Department,
            //    Name = g.Key.Name,
            //    TotalMinutes = g.Sum(x => x.Time),
            //    DistinctDays = g.Select(x => x.DateTime.Date).Distinct().Count()
            //})
            //.Where(x => x.DistinctDays > 0)
            //.Select(x => new
            //{
            //    Department = x.Department,
            //    EmpName = x.Name,
            //    AvgDailyHours = Math.Round((decimal)x.TotalMinutes / 60m / x.DistinctDays, 2)
            //})
            //.ToList();
            //        var final = query.Select(a => new LeaderboardEmployeeData
            //        {
            //            EmployeeName = a.EmpName,
            //            Department = a.Department,
            //            AvgDailyWorkingHours = a.AvgDailyHours
            //        }).ToList();

            //        var final = peopleDb.AttendanceLogs
            //.Where(a =>
            //    // a.TenantId == TenantId && // Uncomment if needed
            //    a.DateTime.Date >= fromDate &&
            //    a.DateTime.Date <= toDate &&
            //    (string.IsNullOrEmpty(departmentName) || a.Department == departmentName))
            //.AsEnumerable()
            //.GroupBy(g => new { g.EmployeeId, g.Name, g.Code, g.Department })
            //.Select(empGroup =>
            //{
            //    // For each employee, get daily working hours per day
            //    var dailyHours = empGroup
            //        .GroupBy(x => x.DateTime.Date)
            //        .Select(dayGroup => dayGroup.Sum(x => (decimal)x.Time) / 60m); // minutes to hours

            //    var avgDailyWorkingHours = dailyHours.Any()
            //        ? Math.Round(dailyHours.Average(), 2)
            //        : 0;

            //    return new LeaderboardEmployeeData
            //    {
            //        EmployeeName = empGroup.Key.Name,
            //        Department = empGroup.Key.Department,
            //        AvgDailyWorkingHours = avgDailyWorkingHours
            //    };
            //})
            //.Where(m => m.AvgDailyWorkingHours > 0)
            //.ToList();

            //        var final = peopleDb.AttendanceLogs
            //.Where(a =>
            //    // a.TenantId == TenantId && // Uncomment if needed
            //    a.DateTime.Date >= fromDate &&
            //    a.DateTime.Date <= toDate &&
            //    (string.IsNullOrEmpty(departmentName) || a.Department == departmentName))
            //.GroupBy(g => new { g.EmployeeId, g.Name, g.Code, g.Department, Date = g.DateTime.Date })
            //.Select(g => new LeaderboardEmployeeData
            //{
            //    EmployeeName = g.Key.Name,
            //    Department = g.Key.Department,
            //    AvgDailyWorkingHours = Math.Round(g.Sum(a => (decimal)a.Time) / 60m, 2)
            //})
            //.Where(m => m.AvgDailyWorkingHours > 0)
            //.ToList();

            //var query = peopleDb.AttendanceLogs
            //    .Where(a => //a.TenantId == TenantId
            //         a.DateTime.Date >= fromDate
            //        && a.DateTime.Date <= toDate);

            //if (!string.IsNullOrEmpty(departmentName))
            //{
            //    query = query.Where(a => a.Department == departmentName);
            //};
            //var query1 = query                    .GroupBy(g => new { g.EmployeeId, g.Name, g.Code, g.Department, g.DateTime.Date })
            //        .Select(g => new ReportEmployeeMaster
            //        {
            //            EmployeeId = g.Key.EmployeeId.ToString(),
            //            EmployeeName = g.Key.Name.ToString(),
            //            EmployeeCode = g.Key.Code.ToString(),
            //            DepartmentName = g.Key.Department.ToString(),
            //            Date = DateOnly.FromDateTime(g.Key.Date),
            //            TotalHour = Math.Round(g.Sum(a => (decimal)a.Time / 60), 2),
            //        })
            //        .ToList();
            //var final = query1.Select(m =>
            //{
            //    return new LeaderboardEmployeeData
            //    {
            //        EmployeeName = m.EmployeeName,
            //        Department = m.DepartmentName,
            //        AvgDailyWorkingHours = m.TotalHour
            //    };

            //})
            //.Where(m => m.AvgDailyWorkingHours > 0)
            //    .ToList();
            //        var query1 = query
            //.GroupBy(m => new { m.EmployeeId, m.Name, m.Department, m.DateTime })
            //.Select(m => new LeaderboardEmployeeData
            //{
            //    EmployeeName = m.Key.Name,
            //    Department = m.Key.Department,
            //    AvgDailyWorkingHours = m.Sum(x => x.Time) / 60m // Assuming Time is in minutes
            //})
            //.Where(m => m.AvgDailyWorkingHours > 0)
            //.ToList();

            //        var results = query
            //            .AsEnumerable()
            //            .GroupBy(m => new { m.EmployeeId,m.Name, m.Code, m.Department, m.DateTime.Date })
            //            .Select(m =>
            //            {
            //                var minTime = m.Min(x => x.TimeStamp);
            //                var maxTime = m.Max(x => x.TimeStamp);
            //                //var totalDays = m.Select(x => x.DateTime.Date).Distinct().Count();

            //                var TotalHour = Math.Round(m.Sum(a => (decimal)a.Time / 60), 2);

            //                //var totalWorkHours = m
            //                //    .Where(x => x.Time > 0)
            //                //    .Sum(x => x.Time / 60m); // Convert minutes to hours as decimal
            //                var totalDays = m.Select(x => x.DateTime.Date).Distinct().Count();
            //                var avgDailyWorkingHours = totalDays > 0
            //? Math.Round(TotalHour / totalDays, 2)
            //: 0;

            //                // var totalHours = (decimal)(maxTime - minTime).TotalHours;
            //                // var avgDailyWorkingHours = totalDays > 0 ? Math.Round(Math.Round(totalHours / totalDays, 2) / 3.5m, 2) : 0;

            //                return new LeaderboardEmployeeData
            //                {
            //                    EmployeeName = m.Key.Name,
            //                    Department = m.Key.Department,
            //                    AvgDailyWorkingHours = avgDailyWorkingHours
            //                };
            //            })
            //            .Where(m => m.AvgDailyWorkingHours > 0)
            //            .ToList();

            //  Sorting & Pagination (Handled Server-Side)
            results = results.OrderByDescending(m => m.AvgDailyWorkingHours).ToList();

            var loadResult = DataSourceLoader.Load(results, loadOptions);

            return loadResult;
        }
    }
}
