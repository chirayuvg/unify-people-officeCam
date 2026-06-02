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
using Unify.STR.Attendnace.Models;

namespace Unify.STR.Attendance.Controllers.Attendance
{

    public class ReportEmployeeController : PageController
    {
        private CardView<ReportEmployeeFilterData> cardView;

        public override void Init()
        {
            this.Page.Toolbar.AddButton("Download").OnClickExecute(m => m.DownloadFile("DivisionReport.xlsx", "ReportDivision", "Download"));

            cardView = this.AddCardView<ReportEmployeeFilterData>().SetControlStyle(CardViewStyle.Filter);
            cardView.ControlStyle = CardViewStyle.Filter;
            cardView.SubmitOnDataChange = true;
            cardView.LabelPosition = LabelPosition.Left;
            cardView.GetFieldControl(m => m.EmployeeId).OnLoadLookupData += Employee_OnLoadLookupData;
            cardView.GetFieldControl(m => m.EmployeeName).IsVisible = false;
            cardView.OnSubmit += CardView_OnSubmit;

            var gridView = (GridView<ReportEmployeeMaster>)this.AddGridView<ReportEmployeeMaster>()
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
                .DataGrid<ReportEmployeeDetail>()
                .DataSource(new JS("data.reportEmployeeDetails"))
                .Columns(col =>
                {
                    col.Add().DataField("action").Caption("Action").Width(200);
                    col.Add().DataField("timeStamp").Caption("Time Stamp").Width(200);
                    col.Add().DataField("workingMinute").Caption("Working Minute").Width(200);
                });

            return (item) => new HtmlString($"<div style='padding-left: 250px;'>{grd.ToTemplate()}</div>");
        }

        private async Task<LoadResult> Employee_OnLoadLookupData(DataSourceLoadOptions loadOptions)
        {
            var _peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            var employees = _peopleDb.Employees.ToList();
            employees.Add(new Employee() { Id = Guid.Empty, Name = "--All--" });
            LoadResult loadResult = DataSourceLoader.Load(employees, loadOptions);

            return loadResult;
        }

        private async Task CardView_OnSubmit(ReportEmployeeFilterData model, IFormFileCollection files)
        {
            model.GetDates();
            model.GetEmployeeName();
        }

        private async Task<LoadResult> GridView_OnLoadData(DataSourceLoadOptions loadOptions, string? parentId)
        {
            var cardView = this.Page.FindControl<CardView<ReportEmployeeFilterData>>();
            var TenantId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();
            DateTime fromDate =
                (cardView.Data.FromDate == DateTime.MinValue)
                ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
                : DateTime.Parse(cardView.Data.FromDate.ToString("yyyy-MM-dd"));
            DateTime toDate =
                (cardView.Data.ToDate == DateTime.MinValue)
                ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
                : DateTime.Parse(cardView.Data.ToDate.ToString("yyyy-MM-dd"));
            string employeeName = (cardView.Data.EmployeeName == null || cardView.Data.EmployeeName == "") ? "" : cardView.Data.EmployeeName;

            LoadResult loadResult = new LoadResult();
            loadResult.data = await GetReportData(TenantId, fromDate, toDate, employeeName);

            return loadResult;
        }

        [HttpPost]
        public async Task<IActionResult> Download()
        {
            var cardView = this.Page.FindControl<CardView<ReportEmployeeFilterData>>();
            var TenantId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();
            DateTime fromDate =
                (cardView.Data.FromDate == DateTime.MinValue)
                ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
                : DateTime.Parse(cardView.Data.FromDate.ToString("yyyy-MM-dd"));
            DateTime toDate =
                (cardView.Data.ToDate == DateTime.MinValue)
                ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
                : DateTime.Parse(cardView.Data.ToDate.ToString("yyyy-MM-dd"));
            string employeeName = (cardView.Data.EmployeeName == null || cardView.Data.EmployeeName == "") ? "" : cardView.Data.EmployeeName;

            List<ReportEmployeeMaster> results = await GetReportData(TenantId, fromDate, toDate, employeeName);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Report");

                //  ReportEmployeeData Column Name
                worksheet.Cells[1, 1].Value = "Employee Name";
                worksheet.Cells[1, 2].Value = "Employee Code";
                worksheet.Cells[1, 3].Value = "Division";
                worksheet.Cells[1, 4].Value = "Date";
                worksheet.Cells[1, 5].Value = "Login Time";
                worksheet.Cells[1, 6].Value = "Logout Time";
                worksheet.Cells[1, 7].Value = "Total Hours";
                

                //  Dynamically add headers for ReportEmployeeDetails
                int actionStartCol = 5;
                int maxActions = 0;

                //  Find maximum actions to define the column headers
                foreach (var record in results)
                {
                    maxActions = Math.Max(maxActions, record.ReportEmployeeDetails.Count);
                }

                //  ReportEmployeeDetail Column Name
                for (int i = 0; i < maxActions; i++)
                {
                    worksheet.Cells[1, actionStartCol + (i * 3)].Value = $"Action {i + 1}";
                    worksheet.Cells[1, actionStartCol + (i * 3) + 1].Value = $"TimeStamp {i + 1}";
                    worksheet.Cells[1, actionStartCol + (i * 3) + 2].Value = $"WorkingMinute {i + 1}";
                }

                int row = 2;

                //  Add ReportEmployeeData Records
                foreach (var record in results)
                {
                    worksheet.Cells[row, 1].Value = record.EmployeeName;
                    worksheet.Cells[row, 2].Value = record.EmployeeCode;
                    worksheet.Cells[row, 3].Value = record.Division;
                    worksheet.Cells[row, 4].Value = record.Date;
                    worksheet.Cells[row, 5].Value = record.LoginTime;
                    worksheet.Cells[row, 6].Value = record.LogoutTime;
                    worksheet.Cells[row, 7].Value = record.TotalHour;

                    //  Add ReportEmployeeDetails Records
                    for (int i = 0; i < record.ReportEmployeeDetails.Count; i++)
                    {
                        worksheet.Cells[row, actionStartCol + (i * 3)].Value = record.ReportEmployeeDetails[i].Action;
                        worksheet.Cells[row, actionStartCol + (i * 3) + 1].Value = record.ReportEmployeeDetails[i].TimeStamp;
                        worksheet.Cells[row, actionStartCol + (i * 3) + 2].Value = record.ReportEmployeeDetails[i].WorkingMinute;
                    }

                    row++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "EmployeeReport.xlsx");
            }
        }

        public async Task<List<ReportEmployeeMaster>> GetReportData(Guid TenantId, DateTime fromDate, DateTime toDate, string employeeName)
        {
            /*
            //  Fetch parameter based all the records from database
            //  ------------------------------------------------------------
            string sql =
                   $"SELECT employeeId, name, code, Division, dateTime, action, firstLogin, lastLog, lateLogin, earlyLogout, time, timeStamp" +
                   $"  FROM attendanceindex" +
                   $" WHERE TenantId = '{TenantId}'";
            if (employeeName != "") sql += $"   AND name = '{employeeName}'";
            sql += $"   AND (dateTime >= '{fromDate}' AND dateTime <= '{toDate}')";
            //  ============================================================
            */

            toDate = toDate.Date.AddDays(1).AddTicks(-1);

            var peopleDb = CoreServices.GetService<PeopleDb>();
            var query = peopleDb.AttendanceLogs
                .Where(a => //a.TenantId == TenantId
                     a.DateTime >= fromDate
                    && a.DateTime <= toDate
                    && (a.EmployeeId.ToString() != "00000000-0000-0000-0000-000000000000")
                );

            if (!string.IsNullOrEmpty(employeeName)) query = query.Where(a => a.Name == employeeName);

            var reportAttendanceIndex = query
                .Select(a => new ReportAttendanceIndex
                {
                    EmployeeId = a.EmployeeId.ToString(),
                    Name = a.Name,
                    Code = a.Code,
                    Division = a.Division,
                    DateTime = a.DateTime,
                    Action = a.Action,
                    //FirstLogin = a.FirstLogin,
                    //LastLog = a.LastLog,
                    //LateLogin = a.LateLogin,
                    //EarlyLogout = a.EarlyLogout,
                    //Time = (int)a.Time,
                    //TimeStamp = a.TimeStamp,
                })
                .ToList();

            //  Prepare Master Data List
            //  ------------------------------------------------------------
            List<ReportEmployeeMaster> results = reportAttendanceIndex
                .Where(w => (employeeName == "" || w.Name == employeeName))
                .GroupBy(g => new {
                    EmployeeId = g.EmployeeId ?? "",
                    Name = g.Name ?? "Unknown",
                    Action = g.Action ?? "Unknown",
                    Code = g.Code ?? "Unknown",
                    Division = g.Division ?? "Unknown",
                    DateTime = g.DateTime
                })
                .Select(g => new ReportEmployeeMaster
                {
                    EmployeeId = g.Key.EmployeeId.ToString(),
                    EmployeeName = g.Key.Name.ToString(),
                    Action = g.Key.Action.ToString(),
                    EmployeeCode = g.Key.Code.ToString(),
                    Division = g.Key.Division.ToString(),
                    DateTime = g.Key.DateTime,
                    //TotalHour = Math.Round(g.Sum(a => (decimal)a.Time / 60), 2)
                })
                .ToList();
            //  ============================================================

            //  Prepare Detail Data List for all Employee
            //  ------------------------------------------------------------
            var employeeActions = reportAttendanceIndex
                .Select(s => new
                {
                    Date = DateOnly.FromDateTime(s.DateTime),
                    Action = s.Action,
                    TimeStamp = TimeOnly.FromDateTime(s.TimeStamp),
                    WorkingMinute = s.Time,
                })
                .OrderBy(o => o.Date)
                .ToList();
            //  ============================================================

            //  For each record in Master Data, fill with the Detail data
            foreach (ReportEmployeeMaster result in results)
            {
                //  Prepare some of Master data
                //  --------------------------------------------------
                //DateOnly date = DateOnly.Parse(result.Date);
                DateTime date = DateTime.Parse(result.Date);
                string timeOnlyString = TimeOnly.FromDateTime(date).ToString("HH:mm");
                DateTime date2 = result.DateTime;
                //result.Date = date.ToString("dd") + "-" + date.ToString("MMM") + "-" + date.ToString("yy");
                //TimeOnly time = TimeOnly.FromDateTime(result.Date);
                //            TimeOnly LoginTime = reportAttendanceIndex
                //.Where(w => w.Action == "CheckIn" && w.FirstLogin == true && w.DateTime == date2)
                //.Select(w => TimeOnly.FromDateTime(date))
                //.FirstOrDefault();
                TimeOnly LoginTime = reportAttendanceIndex
                .Where(w =>
                    w.Action == "CheckIn" &&
                    w.DateTime.Date == date2.Date // ensure same calendar day
                )
                .OrderBy(w => w.TimeStamp)
                .Select(w => TimeOnly.FromDateTime(w.TimeStamp))
                .FirstOrDefault();

                //TimeOnly LoginTime = reportAttendanceIndex
                //    .Where(w => //DateOnly.FromDateTime(w.DateTime) == DateOnly.Parse(result.Date)
                //        w.Action == "CheckIn" && w.FirstLogin == true)

                //    .Select(s => TimeOnly.FromDateTime(s.DateTime).AddHours(5).AddMinutes(30))
                //    .FirstOrDefault();
                result.LoginTime = LoginTime.ToString("HH:mm");

                //TimeOnly LogoutTime = reportAttendanceIndex
                //    .Where(w => //DateOnly.FromDateTime(w.DateTime) == DateOnly.Parse(result.Date)
                //        w.Action == "CheckOut" && w.LastLog == true && w.DateTime == date2)

                //    .Select(m => TimeOnly.FromDateTime(date))
                //    .FirstOrDefault();

                TimeOnly LogoutTime = reportAttendanceIndex
    .Where(w =>
        w.Action == "CheckOut" &&
        w.DateTime.Date == date2.Date // Compare only date
    )
    .OrderByDescending(w => w.TimeStamp)
    .Select(w => TimeOnly.FromDateTime(w.TimeStamp))
    .FirstOrDefault();

                result.LogoutTime = LogoutTime.ToString("HH:mm");
                //  ==================================================

                //  Get the list of transaction of the specific Employee
                //  --------------------------------------------------
                var actionList = employeeActions
                    .Where(w => w.Date == DateOnly.FromDateTime(DateTime.Parse(result.Date)))
                    .OrderBy(o => o.TimeStamp).ToList();
                //  ==================================================

                //  Prepare the list of Detail data
                //  --------------------------------------------------
                List<ReportEmployeeDetail> reportEmployeeDetails = new List<ReportEmployeeDetail>();

                foreach (var row in actionList)
                {
                    ReportEmployeeDetail reportEmployeeDetail = new ReportEmployeeDetail();

                    reportEmployeeDetail.Action = row.Action;
                    reportEmployeeDetail.TimeStamp = row.TimeStamp.ToString("HH:mm:ss");
                    reportEmployeeDetail.WorkingMinute = row.WorkingMinute;

                    reportEmployeeDetails.Add(reportEmployeeDetail);
                }

                result.ReportEmployeeDetails = reportEmployeeDetails;
                //  ==================================================
            }

            results = results
                .OrderBy(m => m.EmployeeName)
                .ThenBy(o => o.Division)
                .ThenBy(m => m.Date)
                .ToList();

            return results;
        }
    }

    public class ReportEmployeeFilterData
    {
        public Guid EmployeeId { get; set; } = Guid.Empty;
        public Employee? Employee { get; set; }

        public Period Period { get; set; } = Period.ThisWeek;

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public string? EmployeeName { get; set; } = "";

        public ReportEmployeeFilterData()
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

        public void GetEmployeeName()
        {
            var _peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            EmployeeName = (EmployeeId == Guid.Empty) ? "" : _peopleDb.Employees.Where(m => m.Id == EmployeeId).FirstOrDefault().Name;
        }
    }

    public class ReportEmployeeMaster
    {
        [Key]
        public string EmployeeId { get; set; }

        public string EmployeeName { get; set; }

        public string Action { get; set; }


        public string EmployeeCode { get; set; }

        public string Division { get; set; }

        public string Date { get; set; }
        public DateTime DateTime { get; set; }


        public string LoginTime { get; set; }

        public string LogoutTime { get; set; }

        public decimal TotalHour { get; set; }


        public List<ReportEmployeeDetail> ReportEmployeeDetails { get; set; }
    }

    public class ReportEmployeeDetail
    {
        public string Action { get; set; }

        public string TimeStamp { get; set; }

        public int WorkingMinute { get; set; }
    }

    public class EmployeeForDropdown
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }

        public EmployeeForDropdown(Guid? Id, string Name)
        {
            this.Id = Id;
            this.Name = Name;
        }
    }
    //public class ReportEmployeeController : PageController  
    //{
    //    private CardView<ReportEmployeeFilterData> cardView;

    //    public override void Init()
    //    {
    //        this.Page.Toolbar.AddButton("Download").OnclickDownload("DivisionReport.xlsx", "ReportDivision", "Download");

    //        cardView = this.AddCardView<ReportEmployeeFilterData>().SetControlStyle(CardViewStyle.Filter);
    //        cardView.ControlStyle = CardViewStyle.Filter;
    //        cardView.SubmitOnDataChange = true;
    //        cardView.LabelPosition = LabelPosition.Left;
    //        cardView.GetFieldControl(m => m.EmployeeId).OnLoadLookupData += Employee_OnLoadLookupData;
    //        cardView.GetFieldControl(m => m.EmployeeName).IsVisible = false;
    //        cardView.OnSubmit += CardView_OnSubmit;

    //        var gridView = (GridView<ReportEmployeeMaster>)this.AddGridView<ReportEmployeeMaster>()
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
    //            .DataGrid<ReportEmployeeDetail>()
    //            .DataSource(new JS("data.reportEmployeeDetails"))
    //            .Columns(col =>
    //            {
    //                col.Add().DataField("action").Caption("Action").Width(200);
    //                col.Add().DataField("timeStamp").Caption("Time Stamp").Width(200);
    //                col.Add().DataField("workingMinute").Caption("Working Minute").Width(200);
    //            });

    //        return (item) => new HtmlString($"<div style='padding-left: 250px;'>{grd.ToTemplate()}</div>");
    //    }

    //    private async Task<LoadResult> Employee_OnLoadLookupData(DataSourceLoadOptions loadOptions)
    //    {
    //        var _peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
    //        var employees = _peopleDb.Employees.ToList();
    //        employees.Add(new Employee() { Id = Guid.Empty, Name = "--All--" });
    //        LoadResult loadResult = DataSourceLoader.Load(employees, loadOptions);

    //        return loadResult;
    //    }

    //    private async Task CardView_OnSubmit(ReportEmployeeFilterData model, IFormFileCollection files)
    //    {
    //        model.GetDates();
    //        model.GetEmployeeName();
    //    }

    //    private async Task<LoadResult> GridView_OnLoadData(DataSourceLoadOptions loadOptions, string? parentId)
    //    {
    //        var cardView = this.Page.FindControl<CardView<ReportEmployeeFilterData>>();
    //        var TenantId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();
    //        DateTime fromDate =
    //            (cardView.Data.FromDate == DateTime.MinValue)
    //            ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
    //            : DateTime.Parse(cardView.Data.FromDate.ToString("yyyy-MM-dd"));
    //        DateTime toDate =
    //            (cardView.Data.ToDate == DateTime.MinValue)
    //            ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
    //            : DateTime.Parse(cardView.Data.ToDate.ToString("yyyy-MM-dd"));
    //        string employeeName = (cardView.Data.EmployeeName == null || cardView.Data.EmployeeName == "") ? "" : cardView.Data.EmployeeName;

    //        LoadResult loadResult = new LoadResult();
    //        loadResult.data = await GetReportData(TenantId, fromDate, toDate, employeeName);

    //        return loadResult;
    //    }

    //    [HttpPost]
    //    public async Task<IActionResult> Download()
    //    {
    //        var cardView = this.Page.FindControl<CardView<ReportEmployeeFilterData>>();
    //        var TenantId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();
    //        DateTime fromDate =
    //            (cardView.Data.FromDate == DateTime.MinValue)
    //            ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
    //            : DateTime.Parse(cardView.Data.FromDate.ToString("yyyy-MM-dd"));
    //        DateTime toDate =
    //            (cardView.Data.ToDate == DateTime.MinValue)
    //            ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
    //            : DateTime.Parse(cardView.Data.ToDate.ToString("yyyy-MM-dd"));
    //        string employeeName = (cardView.Data.EmployeeName == null || cardView.Data.EmployeeName == "") ? "" : cardView.Data.EmployeeName;

    //        List<ReportEmployeeMaster> results = await GetReportData(TenantId, fromDate, toDate, employeeName);

    //        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

    //        using (var package = new ExcelPackage())
    //        {
    //            var worksheet = package.Workbook.Worksheets.Add("Report");

    //            //  ReportEmployeeData Column Name
    //            worksheet.Cells[1, 1].Value = "Employee Name";
    //            worksheet.Cells[1, 2].Value = "Employee Code";
    //            worksheet.Cells[1, 3].Value = "Division";
    //            worksheet.Cells[1, 4].Value = "Date";
    //            worksheet.Cells[1, 5].Value = "Login Time";
    //            worksheet.Cells[1, 6].Value = "Logout Time";
    //            worksheet.Cells[1, 7].Value = "Total Hours";
    //            worksheet.Cells[1, 8].Value = "Late Login";
    //            worksheet.Cells[1, 9].Value = "Early Logout";

    //            //  Dynamically add headers for ReportEmployeeDetails
    //            int actionStartCol = 5;
    //            int maxActions = 0;

    //            //  Find maximum actions to define the column headers
    //            foreach (var record in results)
    //            {
    //                maxActions = Math.Max(maxActions, record.ReportEmployeeDetails.Count);
    //            }

    //            //  ReportEmployeeDetail Column Name
    //            for (int i = 0; i < maxActions; i++)
    //            {
    //                worksheet.Cells[1, actionStartCol + (i * 3)].Value = $"Action {i + 1}";
    //                worksheet.Cells[1, actionStartCol + (i * 3) + 1].Value = $"TimeStamp {i + 1}";
    //                worksheet.Cells[1, actionStartCol + (i * 3) + 2].Value = $"WorkingMinute {i + 1}";
    //            }

    //            int row = 2;

    //            //  Add ReportEmployeeData Records
    //            foreach (var record in results)
    //            {
    //                worksheet.Cells[row, 1].Value = record.EmployeeName;
    //                worksheet.Cells[row, 2].Value = record.EmployeeCode;
    //                worksheet.Cells[row, 3].Value = record.Division;
    //                worksheet.Cells[row, 4].Value = record.Date;
    //                worksheet.Cells[row, 5].Value = record.LoginTime;
    //                worksheet.Cells[row, 6].Value = record.LogoutTime;
    //                worksheet.Cells[row, 7].Value = record.TotalHour;
    //                worksheet.Cells[row, 8].Value = record.LateLogin;   // ? "Yes" : "No";
    //                worksheet.Cells[row, 9].Value = record.EarlyLogout; // ? "Yes" : "No";

    //                //  Add ReportEmployeeDetails Records
    //                for (int i = 0; i < record.ReportEmployeeDetails.Count; i++)
    //                {
    //                    worksheet.Cells[row, actionStartCol + (i * 3)].Value = record.ReportEmployeeDetails[i].Action;
    //                    worksheet.Cells[row, actionStartCol + (i * 3) + 1].Value = record.ReportEmployeeDetails[i].TimeStamp;
    //                    worksheet.Cells[row, actionStartCol + (i * 3) + 2].Value = record.ReportEmployeeDetails[i].WorkingMinute;
    //                }

    //                row++;
    //            }

    //            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

    //            return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "EmployeeReport.xlsx");
    //        }
    //    }

    //    public async Task<List<ReportEmployeeMaster>> GetReportData(Guid TenantId, DateTime fromDate, DateTime toDate, string employeeName)
    //    {
    //        /*
    //        //  Fetch parameter based all the records from database
    //        //  ------------------------------------------------------------
    //        string sql =
    //               $"SELECT employeeId, name, code, Division, dateTime, action, firstLogin, lastLog, lateLogin, earlyLogout, time, timeStamp" +
    //               $"  FROM attendanceindex" +
    //               $" WHERE TenantId = '{TenantId}'";
    //        if (employeeName != "") sql += $"   AND name = '{employeeName}'";
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

    //        if (!string.IsNullOrEmpty(employeeName)) query = query.Where(a => a.Name == employeeName);

    //        var reportAttendanceIndex = query
    //            .Select(a => new ReportAttendanceIndex
    //            {
    //                EmployeeId = a.EmployeeId.ToString(),
    //                Name = a.Name,
    //                Code = a.Code,
    //                Division = a.Division,
    //                DateTime = a.DateTime,
    //                Action = a.Action,
    //                FirstLogin = a.FirstLogin,
    //                LastLog = a.LastLog,
    //                LateLogin = a.LateLogin,
    //                EarlyLogout = a.EarlyLogout,
    //                Time = (int)a.Time,
    //                TimeStamp = a.TimeStamp,
    //            })
    //            .ToList();

    //        //  Prepare Master Data List
    //        //  ------------------------------------------------------------
    //        List<ReportEmployeeMaster> results = reportAttendanceIndex
    //            .Where(w => (employeeName == "" || w.Name == employeeName))
    //            .GroupBy(g => new { g.EmployeeId, g.Name, g.Action, g.Code, g.Division, g.TimeStamp, g.DateTime })
    //            .Select(g => new ReportEmployeeMaster
    //            {
    //                EmployeeId = g.Key.EmployeeId.ToString(),
    //                EmployeeName = g.Key.Name.ToString(),
    //                Action = g.Key.Action.ToString(),
    //                EmployeeCode = g.Key.Code.ToString(),
    //                Division = g.Key.Division.ToString(),
    //                Date = g.Key.TimeStamp.ToString(),
    //                DateTime = g.Key.DateTime,
    //                TotalHour = Math.Round(g.Sum(a => (decimal)a.Time / 60), 2),
    //                LateLogin = g.Any(a => a.Action == "CheckIn" && a.FirstLogin && a.LateLogin) ? "Late Login" : "",
    //                EarlyLogout = g.Any(a => a.Action == "CheckOut" && !a.FirstLogin && a.EarlyLogout) ? "Early Logout" : ""
    //            })
    //            .ToList();
    //        //  ============================================================

    //        //  Prepare Detail Data List for all Employee
    //        //  ------------------------------------------------------------
    //        var employeeActions = reportAttendanceIndex
    //            .Select(s => new
    //            {
    //                Date = DateOnly.FromDateTime(s.DateTime),
    //                Action = s.Action,
    //                TimeStamp = TimeOnly.FromDateTime(s.TimeStamp),
    //                WorkingMinute = s.Time,
    //            })
    //            .OrderBy(o => o.Date)
    //            .ToList();
    //        //  ============================================================

    //        //  For each record in Master Data, fill with the Detail data
    //        foreach (ReportEmployeeMaster result in results)
    //        {
    //            //  Prepare some of Master data
    //            //  --------------------------------------------------
    //            //DateOnly date = DateOnly.Parse(result.Date);
    //            DateTime date = DateTime.Parse(result.Date);
    //            string timeOnlyString = TimeOnly.FromDateTime(date).ToString("HH:mm");
    //            DateTime date2 = result.DateTime;
    //            //result.Date = date.ToString("dd") + "-" + date.ToString("MMM") + "-" + date.ToString("yy");
    //            //TimeOnly time = TimeOnly.FromDateTime(result.Date);
    //            //            TimeOnly LoginTime = reportAttendanceIndex
    //            //.Where(w => w.Action == "CheckIn" && w.FirstLogin == true && w.DateTime == date2)
    //            //.Select(w => TimeOnly.FromDateTime(date))
    //            //.FirstOrDefault();
    //            TimeOnly LoginTime = reportAttendanceIndex
    //            .Where(w =>
    //                w.Action == "CheckIn" &&
    //                w.FirstLogin == true &&
    //                w.DateTime.Date == date2.Date // ensure same calendar day
    //            )
    //            .OrderBy(w => w.TimeStamp)
    //            .Select(w => TimeOnly.FromDateTime(w.TimeStamp))
    //            .FirstOrDefault();

    //            //TimeOnly LoginTime = reportAttendanceIndex
    //            //    .Where(w => //DateOnly.FromDateTime(w.DateTime) == DateOnly.Parse(result.Date)
    //            //        w.Action == "CheckIn" && w.FirstLogin == true)

    //            //    .Select(s => TimeOnly.FromDateTime(s.DateTime).AddHours(5).AddMinutes(30))
    //            //    .FirstOrDefault();
    //            result.LoginTime = LoginTime.ToString("HH:mm");

    //            //TimeOnly LogoutTime = reportAttendanceIndex
    //            //    .Where(w => //DateOnly.FromDateTime(w.DateTime) == DateOnly.Parse(result.Date)
    //            //        w.Action == "CheckOut" && w.LastLog == true && w.DateTime == date2)

    //            //    .Select(m => TimeOnly.FromDateTime(date))
    //            //    .FirstOrDefault();

    //            TimeOnly LogoutTime = reportAttendanceIndex
    //.Where(w =>
    //    w.Action == "CheckOut" &&
    //    w.LastLog == true &&
    //    w.DateTime.Date == date2.Date // Compare only date
    //)
    //.OrderByDescending(w => w.TimeStamp)
    //.Select(w => TimeOnly.FromDateTime(w.TimeStamp))
    //.FirstOrDefault();

    //            result.LogoutTime = LogoutTime.ToString("HH:mm");
    //            //  ==================================================

    //            //  Get the list of transaction of the specific Employee
    //            //  --------------------------------------------------
    //            var actionList = employeeActions
    //                .Where(w => w.Date == DateOnly.FromDateTime(DateTime.Parse(result.Date)))
    //                .OrderBy(o => o.TimeStamp).ToList();
    //            //  ==================================================

    //            //  Prepare the list of Detail data
    //            //  --------------------------------------------------
    //            List<ReportEmployeeDetail> reportEmployeeDetails = new List<ReportEmployeeDetail>();

    //            foreach (var row in actionList)
    //            {
    //                ReportEmployeeDetail reportEmployeeDetail = new ReportEmployeeDetail();

    //                reportEmployeeDetail.Action = row.Action;
    //                reportEmployeeDetail.TimeStamp = row.TimeStamp.ToString("HH:mm:ss");
    //                reportEmployeeDetail.WorkingMinute = row.WorkingMinute;

    //                reportEmployeeDetails.Add(reportEmployeeDetail);
    //            }

    //            result.ReportEmployeeDetails = reportEmployeeDetails;
    //            //  ==================================================
    //        }

    //        results = results
    //            .OrderBy(m => m.EmployeeName)
    //            .ThenBy(o => o.Division)
    //            .ThenBy(m => m.Date)
    //            .ToList();

    //        return results;
    //    }
    //}

    //public class ReportEmployeeFilterData
    //{
    //    public Guid EmployeeId { get; set; } = Guid.Empty;
    //    public Employee? Employee { get; set; }

    //    public Period Period { get; set; } = Period.ThisWeek;

    //    public DateTime FromDate { get; set; }

    //    public DateTime ToDate { get; set; }

    //    public string? EmployeeName { get; set; } = "";

    //    public ReportEmployeeFilterData()
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

    //    public void GetEmployeeName()
    //    {
    //        var _peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
    //        EmployeeName = (EmployeeId == Guid.Empty) ? "" : _peopleDb.Employees.Where(m => m.Id == EmployeeId).FirstOrDefault().Name;
    //    }
    //}

    //public class ReportEmployeeMaster
    //{
    //    [Key]
    //    public string EmployeeId { get; set; }

    //    public string EmployeeName { get; set; }

    //    public string Action { get; set; }


    //    public string EmployeeCode { get; set; }

    //    public string Division { get; set; }

    //    public string Date { get; set; }
    //    public DateTime DateTime { get; set; }


    //    public string LoginTime { get; set; }

    //    public string LogoutTime { get; set; }

    //    public decimal TotalHour { get; set; }

    //    public string LateLogin { get; set; }

    //    public string EarlyLogout { get; set; }

    //    public List<ReportEmployeeDetail> ReportEmployeeDetails { get; set; }
    //}

    //public class ReportEmployeeDetail
    //{
    //    public string Action { get; set; }

    //    public string TimeStamp { get; set; }

    //    public int WorkingMinute { get; set; }
    //}

    //public class EmployeeForDropdown
    //{
    //    public Guid? Id { get; set; }
    //    public string Name { get; set; }

    //    public EmployeeForDropdown(Guid? Id, string Name)
    //    {
    //        this.Id = Id;
    //        this.Name = Name;
    //    }
    //}
}
