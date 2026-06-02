using AppStudio.Framework.Core;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml;
using System.ComponentModel.DataAnnotations;
  
using Unify.STR.Attendance.Data.SQL;
using Unify.STR.Attendance.Data.SQL;
using Unify.STR.Attendnace.Models;


namespace Unify.STR.Attendance.Controllers.Attendance
{

    public class ReportDepartmentController : PageController
    {
        private CardView<ReportDepartmentFilterData> cardView;

        public override void Init()
        {
            this.Page.Toolbar.AddButton("Download").OnClickExecute(m => m.DownloadFile("DepartmentReport.xlsx", "ReportDepartment", "Download"));

            cardView = this.AddCardView<ReportDepartmentFilterData>().SetControlStyle(CardViewStyle.Filter);
            cardView.ControlStyle = CardViewStyle.Filter;
            cardView.SubmitOnDataChange = true;
            cardView.LabelPosition = LabelPosition.Left;
            cardView.GetFieldControl(m => m.DivisionId).OnLoadLookupData += Department_OnLoadLookupData;
            cardView.GetFieldControl(m => m.DivisionName).IsVisible = false;
            cardView.OnSubmit += CardView_OnSubmit;

            var gridView = (GridView<ReportDepartmentMaster>)this.AddGridView<ReportDepartmentMaster>()
                .ConfigueUI(grid => grid
                    .ColumnAutoWidth(false)
                    .FilterRow(f => f.Visible(false))
                    .HeaderFilter(headerFilter => headerFilter.Visible(false))
                    .RowAlternationEnabled(true)
                    .Scrolling(scrolling => scrolling.ColumnRenderingMode(GridColumnRenderingMode.Virtual))
                    .SearchPanel(s => s.Visible(false))
                    .ShowBorders(true)
                    .ShowColumnHeaders(true)
                    .ShowRowLines(false).ShowColumnLines(false)
                    .WordWrapEnabled(true)
                    .Paging(paging => paging.Enabled(true).PageSize(5)) //  Enable paging with 5 rows per page
                    .Pager(pager =>
                    {
                        pager
                            .Visible(true)
                            .DisplayMode(GridPagerDisplayMode.Full)     //  Show full pagination with navigation
                            .ShowPageSizeSelector(false)                //  Remove option to change page size
                            .AllowedPageSizes(new JS("[5]"))            //  Only allow 5 rows per page
                            .ShowInfo(true)                             //  Show page numbers (e.g., "Page 1 of 8")
                            .ShowNavigationButtons(true);               //  Enable page navigation
                    })
                    .MasterDetail(md =>
                    {
                        md.Enabled(true);
                        md.Template(GenerateChildGridView(CoreServices.GetHtmlHelper()));
                    })
                );
            gridView.OnLoadData += GridView_OnLoadData;
        }

        private static RazorBlock GenerateChildGridView(IHtmlHelper html)
        {
            var grd = html
                .DevExtreme()
                .DataGrid<ReportDepartmentDetail>()
                .DataSource(new JS("data.reportDepartmentDetails"))
                .Columns(col =>
                {
                    col.Add().DataField("day").Caption("Day").Width(200);
                    col.Add().DataField("loginTime").Caption("Login Time").Width(200);
                    col.Add().DataField("logoutTime").Caption("Logout Time").Width(200);
                    col.Add().DataField("workingHour").Caption("Working Hour").Width(200);
                    col.Add().DataField("lateLogin").Caption("Late Login").Width(200);
                    col.Add().DataField("earlyLogout").Caption("Early Logout").Width(200);
                });

            return (item) => new HtmlString($"<div style='padding-left: 200px;'>{grd.ToTemplate()}</div>");
        }

        private async Task<LoadResult> Department_OnLoadLookupData(DataSourceLoadOptions loadOptions)
        {
            var _peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            var divisions = _peopleDb.Divisions.ToList();
            divisions.Add(new Division() { Id = Guid.Empty, Name = "--All--" });
            LoadResult loadResult = DataSourceLoader.Load(divisions, loadOptions);

            return loadResult;
        }

        private async Task CardView_OnSubmit(ReportDepartmentFilterData model, IFormFileCollection files)
        {
            model.GetDates();
            model.GetDepartmentName();
        }

        private async Task<LoadResult> GridView_OnLoadData(DataSourceLoadOptions loadOptions, string? parentId)
        {
            var cardView = this.Page.FindControl<CardView<ReportDepartmentFilterData>>();
            var TenantId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();
            DateTime fromDate =
                (cardView.Data.FromDate == DateTime.MinValue)
                ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
                : DateTime.Parse(cardView.Data.FromDate.ToString("yyyy-MM-dd"));
            DateTime toDate =
                (cardView.Data.ToDate == DateTime.MinValue)
                ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
                : DateTime.Parse(cardView.Data.ToDate.ToString("yyyy-MM-dd"));
            string departmentName = (cardView.Data.DivisionName == null || cardView.Data.DivisionName == "") ? "" : cardView.Data.DivisionName;

            LoadResult loadResult = new LoadResult();
            loadResult.data = await GetReportData(TenantId, fromDate, toDate, departmentName);

            return loadResult;
        }

        [HttpPost]
        public async Task<IActionResult> Download()
        {
            var cardView = this.Page.FindControl<CardView<ReportDepartmentFilterData>>();
            var TenantId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();
            DateTime fromDate =
                (cardView.Data.FromDate == DateTime.MinValue)
                ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
                : DateTime.Parse(cardView.Data.FromDate.ToString("yyyy-MM-dd"));
            DateTime toDate =
                (cardView.Data.ToDate == DateTime.MinValue)
                ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
                : DateTime.Parse(cardView.Data.ToDate.ToString("yyyy-MM-dd"));
            string departmentName = (cardView.Data.DivisionName == null || cardView.Data.DivisionName == "") ? "" : cardView.Data.DivisionName;

            List<ReportDepartmentMaster> results = await GetReportData(TenantId, fromDate, toDate, departmentName);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Report");

                //  ReportEmployeeData Column Name
                worksheet.Cells[1, 1].Value = "Division";
                worksheet.Cells[1, 2].Value = "Employee Name";
                worksheet.Cells[1, 3].Value = "Employee Code";
                worksheet.Cells[1, 4].Value = "Avg Daily Working Hours";
                //worksheet.Cells[1, 5].Value = "Late Logins";
                //worksheet.Cells[1, 6].Value = "Early Logouts";

                //  Dynamically add headers for ReportEmployeeDetails
                int actionStartCol = 5;
                int maxActions = 0;

                //  Find maximum actions to define the column headers
                foreach (var record in results)
                {
                    maxActions = Math.Max(maxActions, record.ReportDepartmentDetails.Count);
                }

                //  ReportEmployeeDetail Column Name
                for (int i = 0; i < maxActions; i++)
                {
                    worksheet.Cells[1, actionStartCol + (i * 6)].Value = $"Day {i + 1}";
                    worksheet.Cells[1, actionStartCol + (i * 6) + 1].Value = $"Login Time {i + 1}";
                    worksheet.Cells[1, actionStartCol + (i * 6) + 2].Value = $"Logout Time {i + 1}";
                    worksheet.Cells[1, actionStartCol + (i * 6) + 3].Value = $"Working Hour {i + 1}";
                    //worksheet.Cells[1, actionStartCol + (i * 6) + 4].Value = $"Late Login {i + 1}";
                    //worksheet.Cells[1, actionStartCol + (i * 6) + 5].Value = $"Early Logout {i + 1}";
                }

                int row = 2;

                //  Add ReportEmployeeData Records
                foreach (var record in results)
                {
                    worksheet.Cells[row, 1].Value = record.Department;
                    worksheet.Cells[row, 2].Value = record.EmployeeName;
                    worksheet.Cells[row, 3].Value = record.EmployeeCode;
                    worksheet.Cells[row, 4].Value = record.AvgDailyWorkingHours;
                    //worksheet.Cells[row, 7].Value = record.LateLogins;
                    //worksheet.Cells[row, 8].Value = record.EarlyLogouts;

                    //  Add ReportEmployeeDetails Records
                    for (int i = 0; i < record.ReportDepartmentDetails.Count; i++)
                    {
                        worksheet.Cells[row, actionStartCol + (i * 6)].Value = record.ReportDepartmentDetails[i].Day;
                        worksheet.Cells[row, actionStartCol + (i * 6) + 1].Value = record.ReportDepartmentDetails[i].LoginTime;
                        worksheet.Cells[row, actionStartCol + (i * 6) + 2].Value = record.ReportDepartmentDetails[i].LogoutTime;
                        worksheet.Cells[row, actionStartCol + (i * 6) + 3].Value = record.ReportDepartmentDetails[i].WorkingHour;
                        //worksheet.Cells[row, actionStartCol + (i * 6) + 4].Value = record.ReportDepartmentDetails[i].LateLogin;     // ? "Yes" : "No";
                        //worksheet.Cells[row, actionStartCol + (i * 6) + 5].Value = record.ReportDepartmentDetails[i].EarlyLogout;   // ? "Yes" : "No";
                    }

                    row++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "EmployeeReport.xlsx");
            }
        }

        public async Task<List<ReportDepartmentMaster>> GetReportData(Guid TenantId, DateTime fromDate, DateTime toDate, string divisionName)
        {
            /*
            //  Fetch parameter based all the records from database
            //  ------------------------------------------------------------
            string sql =
                   $"SELECT employeeId, name, code, department, dateTime, action, firstLogin, lastLog, lateLogin, earlyLogout, time, timeStamp" +
                   $"  FROM attendanceindex" +
                   $" WHERE TenantId = '{TenantId}'";
            if (departmentName != "") sql += $"   AND department = '{departmentName}'";
            sql += $"   AND (dateTime >= '{fromDate}' AND dateTime <= '{toDate}')";
            //  ============================================================
            */

            toDate = toDate.Date.AddDays(1).AddTicks(-1);

            var peopleDb = CoreServices.GetService<PeopleDb>();
            var query = peopleDb.AttendanceLogs
                .Where(a => //a.TenantId == TenantId
                     a.DateTime >= fromDate
                    && a.DateTime <= toDate
                    && (a.EmployeeId.ToString() != null)
                );

            if (!string.IsNullOrEmpty(divisionName)) 
                query = query.Where(a => a.Division == divisionName);
            var res = query.ToList();
            var reportAttendanceIndex = query
                .Select(a => new ReportAttendanceIndex
                {
                    EmployeeId = a.EmployeeId.ToString(),
                    Name = a.Name,
                    Code = a.Code,
                    Division = a.Division,
                    DateTime = a.DateTime,
                    Action = a.Action,                
                    //Time = (int)a.Time,
                })
                .ToList();

            //  Prepare Master Data List
            //  ------------------------------------------------------------
            List<ReportDepartmentMaster> results = reportAttendanceIndex
                .Where(w => (divisionName == "" || w.Division == divisionName))
                .GroupBy(g => new { g.EmployeeId, g.Division, g.Name, g.Action, g.Code })
                .Select(g => new ReportDepartmentMaster
                {
                    EmployeeId = g.Key.EmployeeId.ToString(),
                    Department = g.Key.Division,
                    EmployeeName = g.Key.Name,
                    Action = g.Key.Action,
                    EmployeeCode = g.Key.Code,

                    AvgDailyWorkingHours = (decimal)Math.Round(
                        g.GroupBy(a => a.DateTime.Date)
                         .Select(dayGroup => (dayGroup.Max(a => a.TimeStamp) - dayGroup.Min(a => a.TimeStamp)).TotalHours)
                         .DefaultIfEmpty(0)
                         .Average(), 2),

                    //LateLogins = g.Count(a => a.Action == "CheckIn" && a.FirstLogin && a.LateLogin),
                    //EarlyLogouts = g.Count(a => a.Action == "CheckOut" && !a.FirstLogin && a.EarlyLogout),
                })
                .ToList();
            //  ============================================================

            //  Prepare Detail Data List for all Employee
            //  ------------------------------------------------------------
            string employeeIds = string.Join(",", results.Select(r => $"'{r.EmployeeId.Trim()}'"));
            var employeeIdList = employeeIds.Replace("'", "").Split(",");

            var employeeHours = reportAttendanceIndex
                .Where(w => employeeIdList.Contains(w.EmployeeId.Trim()))
                .GroupBy(g => new { g.EmployeeId, g.Division, g.Name, g.Action, g.Code, g.TimeStamp, g.DateTime })
                .Select(g => new GroupedEmployee
                {
                    EmployeeId = g.Key.EmployeeId,
                    Division = g.Key.Division,
                    EmployeeName = g.Key.Name,
                    Action = g.Key.Action,
                    EmployeeCode = g.Key.Code,
                    Date = g.Key.TimeStamp.ToString(),
                    DateTime = g.Key.DateTime,
                    WorkingHour = Math.Round(g.Sum(e => (decimal)e.Time / 60), 2)
                })
                .ToList();
            //  ============================================================

            //  For each record in Master Data, fill with the Detail data
            foreach (ReportDepartmentMaster result in results)
            {
                //  Get the list of transaction of the specific Employee
                //  --------------------------------------------------
                var hourList = employeeHours
                    .Where(w => w.EmployeeId == result.EmployeeId)
                    .OrderBy(o => o.Date)
                    .ToList();
                //  ==================================================

                //  Prepare the list of Detail data
                //  --------------------------------------------------
                List<ReportDepartmentDetail> reportDepartmentHours = new List<ReportDepartmentDetail>();

                foreach (var row in hourList)
                {
                    ReportDepartmentDetail reportDepartmentDetail = new();

                    reportDepartmentDetail.Day = DateOnly.FromDateTime(row.DateTime).ToString();

                    DateTime date = DateTime.Parse(row.Date);
                    string timeOnlyString = TimeOnly.FromDateTime(date).ToString("HH:mm");
                    DateOnly date2 = DateOnly.FromDateTime(row.DateTime);

                   

                    TimeOnly LoginTime = reportAttendanceIndex
                        .Where(w => w.EmployeeId == row.EmployeeId
                            && DateOnly.FromDateTime(w.DateTime) == date2
                            && w.Action == "CheckIn")
                        .OrderBy(w => w.TimeStamp)
                        .Select(s => TimeOnly.FromDateTime(s.TimeStamp)) // if UTC to IST
                        .FirstOrDefault();

                    reportDepartmentDetail.LoginTime = LoginTime.ToString("HH:mm");

                   

                    TimeOnly LogoutTime = reportAttendanceIndex
    .Where(w => w.EmployeeId == row.EmployeeId
        && DateOnly.FromDateTime(w.DateTime) == date2
        && w.Action == "CheckOut")
    .OrderByDescending(w => w.TimeStamp)
    .Select(s => TimeOnly.FromDateTime(s.TimeStamp)) // if UTC to IST
    .FirstOrDefault();

                    reportDepartmentDetail.LogoutTime = LogoutTime.ToString("HH:mm");

                    reportDepartmentDetail.WorkingHour = row.WorkingHour;

                    bool LateLogin = reportAttendanceIndex
                        .Where(w => w.EmployeeId == row.EmployeeId
                            && DateOnly.FromDateTime(w.DateTime) == date2
                        )
                        .Any();
                   

                    reportDepartmentHours.Add(reportDepartmentDetail);
                }

                result.ReportDepartmentDetails = reportDepartmentHours;
                //  ==================================================
            }

            results = results
                .OrderBy(o => o.Department)
                .ThenBy(o => o.EmployeeName)
                .ToList();

            return results;
        }
    }

    public class ReportDepartmentFilterData
    {
        public Guid DivisionId { get; set; } = Guid.Empty;
        public Division? Division { get; set; }

        public Period Period { get; set; } = Period.ThisWeek;

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public string? DivisionName { get; set; } = "";

        public ReportDepartmentFilterData()
        {
            DateTime toDay = DateTime.Today;

            FromDate = toDay.AddDays(-(int)toDay.DayOfWeek + (int)System.DayOfWeek.Monday);
            ToDate = FromDate.AddDays(6);
        }

        public void GetDates()
        {
            DateTime fromDate = DateTime.Today;
            DateTime toDate = DateTime.Today;

            switch (Period)
            {
                case Period.ThisWeek:
                    fromDate = fromDate.AddDays(-(int)fromDate.DayOfWeek + (int)System.DayOfWeek.Monday);
                    toDate = fromDate.AddDays(6);
                    break;
                case Period.ThisMonth:
                    fromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                    break;
                case Period.LastMonth:
                    fromDate = new DateTime(DateTime.Now.AddMonths(-1).Year, DateTime.Now.AddMonths(-1).Month, 1);
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                    break;
                case Period.CustomDateRange:
                    fromDate = DateTime.Parse(FromDate.ToString());
                    toDate = DateTime.Parse(ToDate.ToString());
                    break;
                default:
                    break;
            }

            FromDate = DateTime.Parse(fromDate.ToString("yyyy-MM-dd"));
            ToDate = DateTime.Parse(toDate.ToString("yyyy-MM-dd"));
        }

        public void GetDepartmentName()
        {
            var _peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            DivisionName = (DivisionId == Guid.Empty) ? "" : _peopleDb.Divisions.Where(m => m.Id == DivisionId).FirstOrDefault().Name;
        }
    }

    public class ReportDepartmentMaster
    {
        [Key]
        public string EmployeeId { get; set; }

        public string Department { get; set; }

        public string EmployeeName { get; set; }

        public string EmployeeCode { get; set; }

        public string Action { get; set; }
        public decimal AvgDailyWorkingHours { get; set; }


        public List<ReportDepartmentDetail> ReportDepartmentDetails { get; set; }
    }

    public class ReportDepartmentDetail
    {
        public string Day { get; set; }

        public string LoginTime { get; set; }

        public string LogoutTime { get; set; }

        public decimal WorkingHour { get; set; }

    }

    public class ReportAttendanceIndex
    {
        public string EmployeeId { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }

        public string Division { get; set; }

        public DateTime DateTime { get; set; }

        public string Action { get; set; }

        public int Time { get; set; }

        public DateTime TimeStamp { get; set; }
    }

    public class GroupedEmployee
    {
        public string EmployeeId { get; set; }

        public string Division { get; set; }

        public string EmployeeName { get; set; }

        public string Action { get; set; }
        public string EmployeeCode { get; set; }

        public string Date { get; set; }

        public DateTime DateTime { get; set; }
        public decimal WorkingHour { get; set; }
    }
    //public class ReportDepartmentController : PageController
    //{
    //    private CardView<ReportDepartmentFilterData> cardView;

    //    public override void Init()
    //    {
    //        this.Page.Toolbar.AddButton("Download").OnclickDownload("DepartmentReport.xlsx", "ReportDepartment", "Download");

    //        cardView = this.AddCardView<ReportDepartmentFilterData>().SetControlStyle(CardViewStyle.Filter);
    //        cardView.ControlStyle = CardViewStyle.Filter;
    //        cardView.SubmitOnDataChange = true;
    //        cardView.LabelPosition = LabelPosition.Left;
    //        cardView.GetFieldControl(m => m.DepartmentId).OnLoadLookupData += Department_OnLoadLookupData;
    //        cardView.GetFieldControl(m => m.DepartmentName).IsVisible = false;
    //        cardView.OnSubmit += CardView_OnSubmit;

    //        var gridView = (GridView<ReportDepartmentMaster>)this.AddGridView<ReportDepartmentMaster>()
    //            .ConfigueUI(grid => grid
    //                .ColumnAutoWidth(false)
    //                .FilterRow(f => f.Visible(false))
    //                .HeaderFilter(headerFilter => headerFilter.Visible(false))
    //                .RowAlternationEnabled(true)
    //                .Scrolling(scrolling => scrolling.ColumnRenderingMode(GridColumnRenderingMode.Virtual))
    //                .SearchPanel(s => s.Visible(false))
    //                .ShowBorders(true)
    //                .ShowColumnHeaders(true)
    //                .ShowRowLines(false).ShowColumnLines(false)
    //                .WordWrapEnabled(true)
    //                .Paging(paging => paging.Enabled(true).PageSize(5)) //  Enable paging with 5 rows per page
    //                .Pager(pager =>
    //                {
    //                    pager
    //                        .Visible(true)
    //                        .DisplayMode(GridPagerDisplayMode.Full)     //  Show full pagination with navigation
    //                        .ShowPageSizeSelector(false)                //  Remove option to change page size
    //                        .AllowedPageSizes(new JS("[5]"))            //  Only allow 5 rows per page
    //                        .ShowInfo(true)                             //  Show page numbers (e.g., "Page 1 of 8")
    //                        .ShowNavigationButtons(true);               //  Enable page navigation
    //                })
    //                .MasterDetail(md =>
    //                {
    //                    md.Enabled(true);
    //                    md.Template(GenerateChildGridView(CoreServices.GetHtmlHelper()));
    //                })
    //            );
    //        gridView.OnLoadData += GridView_OnLoadData;
    //    }

    //    private static RazorBlock GenerateChildGridView(IHtmlHelper html)
    //    {
    //        var grd = html
    //            .DevExtreme()
    //            .DataGrid<ReportDepartmentDetail>()
    //            .DataSource(new JS("data.reportDepartmentDetails"))
    //            .Columns(col =>
    //            {
    //                col.Add().DataField("day").Caption("Day").Width(200);
    //                col.Add().DataField("loginTime").Caption("Login Time").Width(200);
    //                col.Add().DataField("logoutTime").Caption("Logout Time").Width(200);
    //                col.Add().DataField("workingHour").Caption("Working Hour").Width(200);
    //                col.Add().DataField("lateLogin").Caption("Late Login").Width(200);
    //                col.Add().DataField("earlyLogout").Caption("Early Logout").Width(200);
    //            });

    //        return (item) => new HtmlString($"<div style='padding-left: 200px;'>{grd.ToTemplate()}</div>");
    //    }

    //    private async Task<LoadResult> Department_OnLoadLookupData(DataSourceLoadOptions loadOptions)
    //    {
    //        var _peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
    //        var departments = _peopleDb.Departments.ToList();
    //        departments.Add(new Department() { Id = Guid.Empty, Name = "--All--" });
    //        LoadResult loadResult = DataSourceLoader.Load(departments, loadOptions);

    //        return loadResult;
    //    }

    //    private async Task CardView_OnSubmit(ReportDepartmentFilterData model, IFormFileCollection files)
    //    {
    //        model.GetDates();
    //        model.GetDepartmentName();
    //    }

    //    private async Task<LoadResult> GridView_OnLoadData(DataSourceLoadOptions loadOptions, string? parentId)
    //    {
    //        var cardView = this.Page.FindControl<CardView<ReportDepartmentFilterData>>();
    //        var TenantId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();
    //        DateTime fromDate =
    //            (cardView.Data.FromDate == DateTime.MinValue)
    //            ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
    //            : DateTime.Parse(cardView.Data.FromDate.ToString("yyyy-MM-dd"));
    //        DateTime toDate =
    //            (cardView.Data.ToDate == DateTime.MinValue)
    //            ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
    //            : DateTime.Parse(cardView.Data.ToDate.ToString("yyyy-MM-dd"));
    //        string departmentName = (cardView.Data.DepartmentName == null || cardView.Data.DepartmentName == "") ? "" : cardView.Data.DepartmentName;

    //        LoadResult loadResult = new LoadResult();
    //        loadResult.data = await GetReportData(TenantId, fromDate, toDate, departmentName);

    //        return loadResult;
    //    }

    //    [HttpPost]
    //    public async Task<IActionResult> Download()
    //    {
    //        var cardView = this.Page.FindControl<CardView<ReportDepartmentFilterData>>();
    //        var TenantId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();
    //        DateTime fromDate =
    //            (cardView.Data.FromDate == DateTime.MinValue)
    //            ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
    //            : DateTime.Parse(cardView.Data.FromDate.ToString("yyyy-MM-dd"));
    //        DateTime toDate =
    //            (cardView.Data.ToDate == DateTime.MinValue)
    //            ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
    //            : DateTime.Parse(cardView.Data.ToDate.ToString("yyyy-MM-dd"));
    //        string departmentName = (cardView.Data.DepartmentName == null || cardView.Data.DepartmentName == "") ? "" : cardView.Data.DepartmentName;

    //        List<ReportDepartmentMaster> results = await GetReportData(TenantId, fromDate, toDate, departmentName);

    //        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

    //        using (var package = new ExcelPackage())
    //        {
    //            var worksheet = package.Workbook.Worksheets.Add("Report");

    //            //  ReportEmployeeData Column Name
    //            worksheet.Cells[1, 1].Value = "Department";
    //            worksheet.Cells[1, 2].Value = "Employee Name";
    //            worksheet.Cells[1, 3].Value = "Employee Code";
    //            worksheet.Cells[1, 4].Value = "Avg Daily Working Hours";
    //            worksheet.Cells[1, 5].Value = "Late Logins";
    //            worksheet.Cells[1, 6].Value = "Early Logouts";

    //            //  Dynamically add headers for ReportEmployeeDetails
    //            int actionStartCol = 5;
    //            int maxActions = 0;

    //            //  Find maximum actions to define the column headers
    //            foreach (var record in results)
    //            {
    //                maxActions = Math.Max(maxActions, record.ReportDepartmentDetails.Count);
    //            }

    //            //  ReportEmployeeDetail Column Name
    //            for (int i = 0; i < maxActions; i++)
    //            {
    //                worksheet.Cells[1, actionStartCol + (i * 6)].Value = $"Day {i + 1}";
    //                worksheet.Cells[1, actionStartCol + (i * 6) + 1].Value = $"Login Time {i + 1}";
    //                worksheet.Cells[1, actionStartCol + (i * 6) + 2].Value = $"Logout Time {i + 1}";
    //                worksheet.Cells[1, actionStartCol + (i * 6) + 3].Value = $"Working Hour {i + 1}";
    //                worksheet.Cells[1, actionStartCol + (i * 6) + 4].Value = $"Late Login {i + 1}";
    //                worksheet.Cells[1, actionStartCol + (i * 6) + 5].Value = $"Early Logout {i + 1}";
    //            }

    //            int row = 2;

    //            //  Add ReportEmployeeData Records
    //            foreach (var record in results)
    //            {
    //                worksheet.Cells[row, 1].Value = record.Department;
    //                worksheet.Cells[row, 2].Value = record.EmployeeName;
    //                worksheet.Cells[row, 3].Value = record.EmployeeCode;
    //                worksheet.Cells[row, 4].Value = record.AvgDailyWorkingHours;
    //                worksheet.Cells[row, 7].Value = record.LateLogins;
    //                worksheet.Cells[row, 8].Value = record.EarlyLogouts;

    //                //  Add ReportEmployeeDetails Records
    //                for (int i = 0; i < record.ReportDepartmentDetails.Count; i++)
    //                {
    //                    worksheet.Cells[row, actionStartCol + (i * 6)].Value = record.ReportDepartmentDetails[i].Day;
    //                    worksheet.Cells[row, actionStartCol + (i * 6) + 1].Value = record.ReportDepartmentDetails[i].LoginTime;
    //                    worksheet.Cells[row, actionStartCol + (i * 6) + 2].Value = record.ReportDepartmentDetails[i].LogoutTime;
    //                    worksheet.Cells[row, actionStartCol + (i * 6) + 3].Value = record.ReportDepartmentDetails[i].WorkingHour;
    //                    worksheet.Cells[row, actionStartCol + (i * 6) + 4].Value = record.ReportDepartmentDetails[i].LateLogin;     // ? "Yes" : "No";
    //                    worksheet.Cells[row, actionStartCol + (i * 6) + 5].Value = record.ReportDepartmentDetails[i].EarlyLogout;   // ? "Yes" : "No";
    //                }

    //                row++;
    //            }

    //            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

    //            return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "EmployeeReport.xlsx");
    //        }
    //    }

    //    public async Task<List<ReportDepartmentMaster>> GetReportData(Guid TenantId, DateTime fromDate, DateTime toDate, string departmentName)
    //    {
    //        /*
    //        //  Fetch parameter based all the records from database
    //        //  ------------------------------------------------------------
    //        string sql =
    //               $"SELECT employeeId, name, code, department, dateTime, action, firstLogin, lastLog, lateLogin, earlyLogout, time, timeStamp" +
    //               $"  FROM attendanceindex" +
    //               $" WHERE TenantId = '{TenantId}'";
    //        if (departmentName != "") sql += $"   AND department = '{departmentName}'";
    //        sql += $"   AND (dateTime >= '{fromDate}' AND dateTime <= '{toDate}')";
    //        //  ============================================================
    //        */

    //        toDate = toDate.Date.AddDays(1).AddTicks(-1);

    //        var peopleDb = CoreServices.GetService<PeopleDb>();
    //        var query = peopleDb.AttendanceLogs
    //            .Where(a => //a.TenantId == TenantId
    //                 a.DateTime >= fromDate
    //                && a.DateTime <= toDate
    //                && (a.EmployeeId.ToString() != "00000000-0000-0000-0000-000000000000")
    //            );

    //        if (!string.IsNullOrEmpty(departmentName)) query = query.Where(a => a.Department == departmentName);

    //        var reportAttendanceIndex = query
    //            .Select(a => new ReportAttendanceIndex
    //            {
    //                EmployeeId = a.EmployeeId.ToString(),
    //                Name = a.Name,
    //                Code = a.Code,
    //                Department = a.Department,
    //                DateTime = a.DateTime,
    //                Action = a.Action,
    //                FirstLogin = a.FirstLogin,
    //                LastLog = a.LastLog,
    //                LateLogin = a.LateLogin,
    //                EarlyLogout = a.EarlyLogout,
    //                Time = (int)a.Time,
    //                TimeStamp = a.TimeStamp,
    //                //FaceImage = a.FaceImage

    //            })
    //            .ToList();

    //        //  Prepare Master Data List
    //        //  ------------------------------------------------------------
    //        List<ReportDepartmentMaster> results = reportAttendanceIndex
    //            .Where(w => (departmentName == "" || w.Department == departmentName))
    //            .GroupBy(g => new { g.EmployeeId, g.Department, g.Name, g.Action, g.Code })
    //            .Select(g => new ReportDepartmentMaster
    //            {
    //                EmployeeId = g.Key.EmployeeId.ToString(),
    //                Department = g.Key.Department,
    //                EmployeeName = g.Key.Name,
    //                Action = g.Key.Action,
    //                EmployeeCode = g.Key.Code,

    //                AvgDailyWorkingHours = (decimal)Math.Round(
    //                    g.GroupBy(a => a.DateTime.Date)
    //                     .Select(dayGroup => (dayGroup.Max(a => a.TimeStamp) - dayGroup.Min(a => a.TimeStamp)).TotalHours)
    //                     .DefaultIfEmpty(0)
    //                     .Average(), 2),

    //                LateLogins = g.Count(a => a.Action == "CheckIn" && a.FirstLogin && a.LateLogin),
    //                EarlyLogouts = g.Count(a => a.Action == "CheckOut" && !a.FirstLogin && a.EarlyLogout),
    //            })
    //            .ToList();
    //        //  ============================================================

    //        //  Prepare Detail Data List for all Employee
    //        //  ------------------------------------------------------------
    //        string employeeIds = string.Join(",", results.Select(r => $"'{r.EmployeeId.Trim()}'"));
    //        var employeeIdList = employeeIds.Replace("'", "").Split(",");

    //        var employeeHours = reportAttendanceIndex
    //            .Where(w => employeeIdList.Contains(w.EmployeeId.Trim()))
    //            .GroupBy(g => new { g.EmployeeId, g.Department, g.Name, g.Action, g.Code, g.TimeStamp, g.DateTime })
    //            .Select(g => new GroupedEmployee
    //            {
    //                EmployeeId = g.Key.EmployeeId,
    //                Department = g.Key.Department,
    //                EmployeeName = g.Key.Name,
    //                Action = g.Key.Action,
    //                EmployeeCode = g.Key.Code,
    //                Date = g.Key.TimeStamp.ToString(),
    //                DateTime = g.Key.DateTime,
    //                WorkingHour = Math.Round(g.Sum(e => (decimal)e.Time / 60), 2)
    //            })
    //            .ToList();
    //        //  ============================================================

    //        //  For each record in Master Data, fill with the Detail data
    //        foreach (ReportDepartmentMaster result in results)
    //        {
    //            //  Get the list of transaction of the specific Employee
    //            //  --------------------------------------------------
    //            var hourList = employeeHours
    //                .Where(w => w.EmployeeId == result.EmployeeId)
    //                .OrderBy(o => o.Date)
    //                .ToList();
    //            //  ==================================================

    //            //  Prepare the list of Detail data
    //            //  --------------------------------------------------
    //            List<ReportDepartmentDetail> reportDepartmentHours = new List<ReportDepartmentDetail>();

    //            foreach (var row in hourList)
    //            {
    //                ReportDepartmentDetail reportDepartmentDetail = new();

    //                reportDepartmentDetail.Day = DateOnly.FromDateTime(row.DateTime).ToString();

    //                DateTime date = DateTime.Parse(row.Date);
    //                string timeOnlyString = TimeOnly.FromDateTime(date).ToString("HH:mm");
    //                DateOnly date2 = DateOnly.FromDateTime(row.DateTime);

    //                //TimeOnly LoginTime = reportAttendanceIndex
    //                //    .Where(w => w.EmployeeId == row.EmployeeId
    //                //        && DateOnly.FromDateTime(w.DateTime) == date2
    //                //        && (w.Action == "CheckIn" && w.FirstLogin == true)
    //                //    )
    //                //    .Select(s => TimeOnly.FromDateTime(date).AddHours(5).AddMinutes(30))
    //                //    .FirstOrDefault();
    //                //reportDepartmentDetail.LoginTime = LoginTime.ToString("HH:mm");

    //                TimeOnly LoginTime = reportAttendanceIndex
    //.Where(w => w.EmployeeId == row.EmployeeId
    //    && DateOnly.FromDateTime(w.DateTime) == date2
    //    && w.Action == "CheckIn")
    //.OrderBy(w => w.TimeStamp)
    //.Select(s => TimeOnly.FromDateTime(s.TimeStamp)) // if UTC to IST
    //.FirstOrDefault();

    //                reportDepartmentDetail.LoginTime = LoginTime.ToString("HH:mm");

    //                //TimeOnly LogoutTime = reportAttendanceIndex
    //                //    .Where(w => w.EmployeeId == row.EmployeeId
    //                //        && DateOnly.FromDateTime(w.DateTime) == date2
    //                //        && (w.Action == "CheckOut" && w.LastLog == true)
    //                //    )
    //                //    .Select(s => TimeOnly.FromDateTime(date).AddHours(5).AddMinutes(30))
    //                //    .FirstOrDefault();
    //                //reportDepartmentDetail.LogoutTime = LogoutTime.ToString("HH:mm");

    //                TimeOnly LogoutTime = reportAttendanceIndex
    //.Where(w => w.EmployeeId == row.EmployeeId
    //    && DateOnly.FromDateTime(w.DateTime) == date2
    //    && w.Action == "CheckOut")
    //.OrderByDescending(w => w.TimeStamp)
    //.Select(s => TimeOnly.FromDateTime(s.TimeStamp)) // if UTC to IST
    //.FirstOrDefault();

    //                reportDepartmentDetail.LogoutTime = LogoutTime.ToString("HH:mm");

    //                reportDepartmentDetail.WorkingHour = row.WorkingHour;

    //                bool LateLogin = reportAttendanceIndex
    //                    .Where(w => w.EmployeeId == row.EmployeeId
    //                        && DateOnly.FromDateTime(w.DateTime) == date2
    //                        && w.LateLogin == true
    //                    )
    //                    .Any();
    //                reportDepartmentDetail.LateLogin = LateLogin ? "Late Login" : "";

    //                bool EarlyLogout = reportAttendanceIndex
    //                    .Where(w => w.EmployeeId == row.EmployeeId
    //                        && DateOnly.FromDateTime(w.DateTime) == date2
    //                        && w.EarlyLogout == true
    //                    )
    //                    .Any();
    //                reportDepartmentDetail.EarlyLogout = EarlyLogout ? "Early Logout" : "";

    //                reportDepartmentHours.Add(reportDepartmentDetail);
    //            }

    //            result.ReportDepartmentDetails = reportDepartmentHours;
    //            //  ==================================================
    //        }

    //        results = results
    //            .OrderBy(o => o.Department)
    //            .ThenBy(o => o.EmployeeName)
    //            .ToList();

    //        return results;
    //    }
    //}

    //public class ReportDepartmentFilterData
    //{
    //    public Guid DepartmentId { get; set; } = Guid.Empty;
    //    public Department? Department { get; set; }

    //    public Period Period { get; set; } = Period.ThisWeek;

    //    public DateTime FromDate { get; set; }

    //    public DateTime ToDate { get; set; }

    //    public string? DepartmentName { get; set; } = "";

    //    public ReportDepartmentFilterData()
    //    {
    //        DateTime toDay = DateTime.Today;

    //        FromDate = toDay.AddDays(-(int)toDay.DayOfWeek + (int)System.DayOfWeek.Monday);
    //        ToDate = FromDate.AddDays(6);
    //    }

    //    public void GetDates()
    //    {
    //        DateTime fromDate = DateTime.Today;
    //        DateTime toDate = DateTime.Today;

    //        switch (Period)
    //        {
    //            case Period.ThisWeek:
    //                fromDate = fromDate.AddDays(-(int)fromDate.DayOfWeek + (int)System.DayOfWeek.Monday);
    //                toDate = fromDate.AddDays(6);
    //                break;
    //            case Period.ThisMonth:
    //                fromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
    //                toDate = fromDate.AddMonths(1).AddDays(-1);
    //                break;
    //            case Period.LastMonth:
    //                fromDate = new DateTime(DateTime.Now.AddMonths(-1).Year, DateTime.Now.AddMonths(-1).Month, 1);
    //                toDate = fromDate.AddMonths(1).AddDays(-1);
    //                break;
    //            case Period.CustomDateRange:
    //                fromDate = DateTime.Parse(FromDate.ToString());
    //                toDate = DateTime.Parse(ToDate.ToString());
    //                break;
    //            default:
    //                break;
    //        }

    //        FromDate = DateTime.Parse(fromDate.ToString("yyyy-MM-dd"));
    //        ToDate = DateTime.Parse(toDate.ToString("yyyy-MM-dd"));
    //    }

    //    public void GetDepartmentName()
    //    {
    //        var _peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
    //        DepartmentName = (DepartmentId == Guid.Empty) ? "" : _peopleDb.Departments.Where(m => m.Id == DepartmentId).FirstOrDefault().Name;
    //    }
    //}

    //public class ReportDepartmentMaster
    //{
    //    [Key]
    //    public string EmployeeId { get; set; }

    //    public string Department { get; set; }

    //    public string EmployeeName { get; set; }

    //    public string EmployeeCode { get; set; }

    //    public string Action { get; set; }
    //    public decimal AvgDailyWorkingHours { get; set; }

    //    public long LateLogins { get; set; }

    //    public long EarlyLogouts { get; set; }

    //    public List<ReportDepartmentDetail> ReportDepartmentDetails { get; set; }
    //}

    //public class ReportDepartmentDetail
    //{
    //    public string Day { get; set; }

    //    public string LoginTime { get; set; }

    //    public string LogoutTime { get; set; }

    //    public decimal WorkingHour { get; set; }

    //    public string LateLogin { get; set; }

    //    public string EarlyLogout { get; set; }
    //}

    //public class ReportAttendanceIndex
    //{
    //    public string EmployeeId { get; set; }

    //    public string Name { get; set; }

    //    public string Code { get; set; }

    //    public string Department { get; set; }

    //    public DateTime DateTime { get; set; }

    //    public string Action { get; set; }

    //    public bool FirstLogin { get; set; }

    //    public bool LastLog { get; set; }

    //    public bool LateLogin { get; set; }

    //    public bool EarlyLogout { get; set; }

    //    public int Time { get; set; }

    //    public DateTime TimeStamp { get; set; }
    //}

    //public class GroupedEmployee
    //{
    //    public string EmployeeId { get; set; }

    //    public string Department { get; set; }

    //    public string EmployeeName { get; set; }

    //    public string Action { get; set; }
    //    public string EmployeeCode { get; set; }

    //    public string Date { get; set; }

    //    public DateTime DateTime { get; set; }
    //    public decimal WorkingHour { get; set; }
    //}
}
