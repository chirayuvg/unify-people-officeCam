using AppStudio.Enterprise.Data;
using AppStudio.Framework.Core;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.ComponentModel.DataAnnotations;
using Unify.Data;
using Unify.Data.SQL;
using Unify.People.Models;
using Unify.People.Services;

namespace Unify.People.Controllers.Attendance
{
    public class ReportDepartmentController : PageController
    {
        private readonly EmployeeService _employeeService;

        private readonly ILogger<ReportDepartmentController> _logger;
        private CardView<DepartmentFilterData> cardView;

        public ReportDepartmentController(ILogger<ReportDepartmentController> logger, EmployeeService employeeService)
        {
            _logger = logger;
            _employeeService = employeeService;
        }

        public override void Init()
        {
            this.Page.SetTitle("Department Wise Report");
            this.Page.Toolbar.AddButton("Download").OnClickExecute(m => m.DownloadFile("DepartmentReport.xlsx", "ReportDepartment", "Download"));

            cardView = this.AddCardView<DepartmentFilterData>().SetControlStyle(CardViewStyle.Filter);
            cardView.ControlStyle = CardViewStyle.Filter;
            cardView.SubmitOnDataChange = true;
            cardView.LabelPosition = LabelPosition.Left;
            cardView.GetFieldControl(m => m.DepartmentId).OnLoadLookupData += Department_OnLoadLookupData;
            cardView.GetFieldControl(m => m.DepartmentName).IsVisible = false;
            cardView.OnSubmit += CardView_OnSubmit;

            if (cardView.Data.Period == Period.CustomDateRange)
            {
                cardView.GetFieldControl(m => m.FromDate).IsReadOnly = false;
                cardView.GetFieldControl(m => m.ToDate).IsReadOnly = false;
            }
            else
            {
                cardView.GetFieldControl(m => m.FromDate).IsReadOnly = true;
                cardView.GetFieldControl(m => m.ToDate).IsReadOnly = true;
            }

            var gridView = (GridView<ReportDepartmentMaster>)this.AddGridView<ReportDepartmentMaster>()
                .ConfigueUI(
                    grid => grid
                        .ColumnAutoWidth(false)
                        .FilterRow(f => f.Visible(false))
                        .HeaderFilter(headerFilter => headerFilter.Visible(false))
                        .RowAlternationEnabled(true)
                        .Scrolling(scrolling => scrolling.ColumnRenderingMode(GridColumnRenderingMode.Virtual))
                        .SearchPanel(s => s.Visible(true).HighlightCaseSensitive(false))
                        .ShowBorders(true)
                        .ShowColumnHeaders(true)
                        .ShowColumnLines(false)
                        .ShowRowLines(false)
                        .WordWrapEnabled(true).Width("100%")
                        .Paging(paging => paging.Enabled(true).PageSize(10))
                        .Pager(pager =>
                        {
                            pager
                                .Visible(true)
                                .DisplayMode(GridPagerDisplayMode.Full)
                                .ShowPageSizeSelector(false)
                                .AllowedPageSizes(new JS("[5]"))
                                .ShowInfo(true)
                                .ShowNavigationButtons(true);
                        })
                        .MasterDetail(md =>
                        {
                            md.Enabled(true);
                            md.Template(GenerateChildGridView(CoreServices.GetHtmlHelper()));
                        })
                );
            gridView.Css.SetHeight("calc(100vh - 300px)");
            gridView.Fields(field =>
            {
                field.ClearFields();

                field.AddField(f => f.DepartmentName);
                field.AddField(f => f.EmployeeName);
                field.AddField(f => f.EmployeeCode);
                field.AddField(f => f.AvgDailyWorkingTime);
                field.AddField(f => f.LateLoginCount);
                field.AddField(f => f.EarlyLogoutCount);
            });
            gridView.OnLoadData += GridView_OnLoadData;
        }

        public override Task ProcessAsync()
        {
            var cardView = this.Page.FindControl<CardView<DepartmentFilterData>>();
            cardView.GetFieldControl(m => m.DepartmentName).IsVisible = false;

            var _peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            var _applicationDb = AppStudio.Framework.Core.CoreServices.GetService<ApplicationDb>();
            var userId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetUserId();
            var tenantUser = _applicationDb.TenantUsers.FirstOrDefault(m => m.Id == userId);

            if (tenantUser.Roles.Any(r => (r == "Super Admin") || (r == "Admin")))
            {
                cardView.GetFieldControl(m => m.DepartmentId).IsReadOnly = false;
            }
            else
            {
                cardView.GetFieldControl(m => m.DepartmentId).IsReadOnly = true;
            }

            return base.ProcessAsync();
        }

        private static RazorBlock GenerateChildGridView(IHtmlHelper html)
        {
            var grd = html
                .DevExtreme()
                .DataGrid<ReportDepartmentDetail>()
                .DataSource(new JS("data.reportDepartmentDetails"))
                .Columns(col =>
                {
                    col.Add().DataField("day").Caption("Day").Width(150);
                    col.Add().DataField("loginTime").Caption("Login Time").Width(150);
                    col.Add().DataField("logoutTime").Caption("Logout Time").Width(150);
                    col.Add().DataField("workingTime").Caption("Working Time").Width(150);
                    col.Add().DataField("lateLogin").Caption("Late Login").Width(150).Alignment(HorizontalAlignment.Right);
                    col.Add().DataField("earlyLogout").Caption("Early Logout").Width(150).Alignment(HorizontalAlignment.Right);
                });

            return (item) => new HtmlString($"<div style='padding-left: 200px;'>{grd.ToTemplate()}</div>");
        }

        private async Task<LoadResult> Department_OnLoadLookupData(DataSourceLoadOptions loadOptions)
        {
            var _peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            var departments = _peopleDb.Departments.ToList();
            departments.Add(new Department() { Id = Guid.Empty, Name = "--All--" });
            LoadResult loadResult = DataSourceLoader.Load(departments, loadOptions);
            return loadResult;
        }

        private async Task CardView_OnSubmit(DepartmentFilterData model, IFormFileCollection files)
        {
            model.GetDates();
            model.GetDepartmentName();

            if (cardView.Data.Period == Period.CustomDateRange)
            {
                cardView.GetFieldControl(m => m.FromDate).IsReadOnly = false;
                cardView.GetFieldControl(m => m.ToDate).IsReadOnly = false;
            }
            else
            {
                cardView.GetFieldControl(m => m.FromDate).IsReadOnly = true;
                cardView.GetFieldControl(m => m.ToDate).IsReadOnly = true;
            }
        }

        private async Task<LoadResult> GridView_OnLoadData(DataSourceLoadOptions loadOptions, string? parentId)
        {
            var cardView = this.Page.FindControl<CardView<DepartmentFilterData>>();
            var TenantId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();
            DateTime fromDate =
                (cardView.Data.FromDate == DateOnly.MinValue)
                ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
                : DateTime.Parse(cardView.Data.FromDate.ToString("yyyy-MM-dd"));
            DateTime toDate =
                (cardView.Data.ToDate == DateOnly.MinValue)
                ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
                : DateTime.Parse(cardView.Data.ToDate.ToString("yyyy-MM-dd"));
            string departmentName = GetDepartmentName();// (cardView.Data.DepartmentName == null || cardView.Data.DepartmentName == "") ? "" : cardView.Data.DepartmentName;

            var results = await GetReportData(TenantId, fromDate, toDate, departmentName);

            return DataSourceLoader.Load(results, loadOptions);
        }

        public string GetDepartmentName()
        {
            var cardView = this.Page.FindControl<CardView<DepartmentFilterData>>();
            var _peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            string DepartmentName = (cardView.Data.DepartmentId == Guid.Empty) ? "" : _peopleDb.Departments.Where(m => m.Id == cardView.Data.DepartmentId).FirstOrDefault().Name;
            return DepartmentName;
        }

        [HttpPost]
        public async Task<IActionResult> Download()
        {
            var cardView = this.Page.FindControl<CardView<DepartmentFilterData>>();
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

            List<ReportDepartmentMaster> results = await GetReportData(TenantId, fromDate, toDate, departmentName);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Report");

                //  ReportEmployeeData Column Name
                worksheet.Cells[1, 1].Value = "Department";
                worksheet.Cells[1, 2].Value = "Employee Name";
                worksheet.Cells[1, 3].Value = "Employee Code";
                worksheet.Cells[1, 4].Value = "Avg Daily Working Hours";
                worksheet.Cells[1, 5].Value = "Late Logins";
                worksheet.Cells[1, 6].Value = "Early Logouts";

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
                    worksheet.Cells[1, actionStartCol + (i * 6) + 4].Value = $"Late Login {i + 1}";
                    worksheet.Cells[1, actionStartCol + (i * 6) + 5].Value = $"Early Logout {i + 1}";
                }

                int row = 2;

                //  Add ReportEmployeeData Records
                foreach (var record in results)
                {
                    worksheet.Cells[row, 1].Value = record.DepartmentName;
                    worksheet.Cells[row, 2].Value = record.EmployeeName;
                    worksheet.Cells[row, 3].Value = record.EmployeeCode;
                    worksheet.Cells[row, 4].Value = record.AvgDailyWorkingHours;
                    worksheet.Cells[row, 7].Value = record.LateLoginCount;
                    worksheet.Cells[row, 8].Value = record.EarlyLogoutCount;

                    //  Add ReportEmployeeDetails Records
                    for (int i = 0; i < record.ReportDepartmentDetails.Count; i++)
                    {
                        worksheet.Cells[row, actionStartCol + (i * 6)].Value = record.ReportDepartmentDetails[i].Day;
                        worksheet.Cells[row, actionStartCol + (i * 6) + 1].Value = record.ReportDepartmentDetails[i].LoginTime;
                        worksheet.Cells[row, actionStartCol + (i * 6) + 2].Value = record.ReportDepartmentDetails[i].LogoutTime;
                        worksheet.Cells[row, actionStartCol + (i * 6) + 3].Value = record.ReportDepartmentDetails[i].WorkingTime;
                        worksheet.Cells[row, actionStartCol + (i * 6) + 4].Value = record.ReportDepartmentDetails[i].LateLogin;     // ? "Yes" : "No";
                        worksheet.Cells[row, actionStartCol + (i * 6) + 5].Value = record.ReportDepartmentDetails[i].EarlyLogout;   // ? "Yes" : "No";
                    }

                    row++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "EmployeeReport.xlsx");
            }
        }

        public async Task<List<ReportDepartmentMaster>> GetReportData(Guid TenantId, DateTime fromDate, DateTime toDate, string departmentName)
        {
            try
            {
                var UserId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetUserId();
                toDate = toDate.Date.AddDays(1).AddTicks(-1);

                TenantUser tenantUser = await Entity.FindByIdAsync<TenantUser>(UserId);
                var peopleDb = CoreServices.GetService<PeopleDb>();
                IQueryable<AttendanceLog> attendanceLogs;

                if (tenantUser.Roles.Any(r => (r == "Super Admin") || (r == "Admin")))
                {
                    attendanceLogs = peopleDb.AttendanceLogs
                        .Where(a =>
                               a.DateTime >= fromDate
                            && a.DateTime <= toDate
                            && (a.EmployeeId.ToString() != "00000000-0000-0000-0000-000000000000")
                        );
                }
                else
                {
                    var employee = await _employeeService.GetEmployeeByEmailAsync(TenantId, tenantUser.Email);

                    attendanceLogs = peopleDb.AttendanceLogs
                        .Where(a =>
                               a.DateTime >= fromDate
                            && a.DateTime <= toDate
                            && a.EmployeeId == employee.Id
                        );
                }

                if (!string.IsNullOrEmpty(departmentName)) attendanceLogs = attendanceLogs.Where(a => a.Department == departmentName);

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

                //  Prepare Department Master Data List
                //  ------------------------------------------------------------
                List<ReportDepartmentMaster> departmentMaster = reportAttendanceIndex
                    .Where(w => (departmentName == "" || w.Department == departmentName))
                    .GroupBy(g => new { g.EmployeeId, g.Department, g.Name, g.Code })
                    .Select(g => new ReportDepartmentMaster
                    {
                        EmployeeId = g.Key.EmployeeId.ToString(),
                        DepartmentName = g.Key.Department,
                        EmployeeName = g.Key.Name,
                        EmployeeCode = g.Key.Code,

                        AvgDailyWorkingHours = (decimal)Math.Round(
                            g.GroupBy(a => a.DateTime.Date)
                             .Select(dayGroup => dayGroup.Sum(e => (decimal)e.Time / 60))
                             .DefaultIfEmpty(0)
                             .Average(), 2),
                        AvgDailyWorkingTime = $"{(int)(Math.Round(g.GroupBy(a => a.DateTime.Date).Select(dayGroup => dayGroup.Sum(e => (decimal)e.Time / 60)).DefaultIfEmpty(0).Average(), 2))}:{(int)Math.Round((Math.Round(g.GroupBy(a => a.DateTime.Date).Select(dayGroup => dayGroup.Sum(e => (decimal)e.Time / 60)).DefaultIfEmpty(0).Average(), 2) % 1) * 60):D2}",
                        LateLoginCount = g.Count(a => a.Action == "CheckIn" && a.FirstLogin && a.LateLogin),
                        EarlyLogoutCount = g.Count(a => a.EarlyLogout),
                    })
                    .ToList();
                //  ============================================================

                //  Prepare Detail Data List for all Employee
                //  ------------------------------------------------------------
                string employeeIds = string.Join(",", departmentMaster.Select(r => $"'{r.EmployeeId.Trim()}'"));
                var employeeIdList = employeeIds.Replace("'", "").Split(",");

                var employeeHours = reportAttendanceIndex
                    .Where(w => employeeIdList.Contains(w.EmployeeId.Trim()))
                    .GroupBy(g => new { g.EmployeeId, g.Department, g.Name, g.Code, g.DateTime.Date })
                    .Select(g => new GroupedEmployee
                    {
                        EmployeeId = g.Key.EmployeeId,
                        Department = g.Key.Department,
                        EmployeeName = g.Key.Name,
                        EmployeeCode = g.Key.Code,
                        Date = DateOnly.FromDateTime(g.Key.Date),
                        WorkingHour = Math.Round(g.Sum(e => (decimal)e.Time / 60), 2),
                        WorkingTime = $"{(int)(Math.Floor(Math.Round(g.Sum(e => (decimal)e.Time / 60), 2)))}:{(int)Math.Round((Math.Round(g.Sum(e => (decimal)e.Time / 60), 2) % 1) * 60):D2}"

                    })
                    .ToList();
                //  ============================================================

                //  For each record in Master Data, fill with the Detail data
                foreach (ReportDepartmentMaster department in departmentMaster)
                {
                    //  Get the list of transaction of the specific Employee
                    //  --------------------------------------------------
                    var hourList = employeeHours
                        .Where(w => w.EmployeeId == department.EmployeeId)
                        .OrderBy(o => o.Date)
                        .ToList();
                    //  ==================================================

                    //  Prepare the list of Detail data
                    //  --------------------------------------------------
                    List<ReportDepartmentDetail> reportDepartmentHours = new List<ReportDepartmentDetail>();

                    foreach (var row in hourList)
                    {
                        ReportDepartmentDetail reportDepartmentDetail = new();

                        reportDepartmentDetail.Day = row.Date.ToString();

                        TimeOnly LoginTime = reportAttendanceIndex
                            .Where(w =>
                                w.EmployeeId == row.EmployeeId
                                && DateOnly.FromDateTime(w.DateTime) == row.Date
                                && w.Action == "CheckIn"
                            )
                            .OrderBy(w => w.TimeStamp)
                            .Select(s => TimeOnly.FromDateTime(s.TimeStamp)) // if UTC to IST
                            .FirstOrDefault();

                        reportDepartmentDetail.LoginTime = LoginTime.ToString("HH:mm");

                        TimeOnly LogoutTime = reportAttendanceIndex
                            .Where(w =>
                                w.EmployeeId == row.EmployeeId
                                && DateOnly.FromDateTime(w.DateTime) == row.Date
                                && w.Action == "CheckOut"
                            )
                            .OrderByDescending(w => w.TimeStamp)
                            .Select(s => TimeOnly.FromDateTime(s.TimeStamp)) // if UTC to IST
                            .FirstOrDefault();

                        reportDepartmentDetail.LogoutTime = LogoutTime.ToString("HH:mm");

                        var totalhours = row.WorkingHour;
                        int hours = (int)totalhours;
                        int minutes = (int)Math.Round((totalhours - hours) * 60);

                        // Format as "H:mm" (e.g., "8:30", "7:12")
                        string timeFormatted = $"{hours}:{minutes:D2}";

                       // reportDepartmentDetail.WorkingHour = row.WorkingHour;
                        reportDepartmentDetail.WorkingTime = timeFormatted;
                        bool LateLogin = reportAttendanceIndex
                            .Where(w => w.EmployeeId == row.EmployeeId
                                && DateOnly.FromDateTime(w.DateTime) == row.Date
                                && w.LateLogin == true
                            )
                            .Any();
                        reportDepartmentDetail.LateLogin = LateLogin ? "Yes" : "";

                        bool EarlyLogout = reportAttendanceIndex
                            .Where(w =>
                                w.EmployeeId == row.EmployeeId
                                && DateOnly.FromDateTime(w.DateTime) == row.Date
                                && w.EarlyLogout == true
                            )
                            .Any();
                        reportDepartmentDetail.EarlyLogout = EarlyLogout ? "Yes" : "";

                        reportDepartmentHours.Add(reportDepartmentDetail);
                    }

                    department.ReportDepartmentDetails = reportDepartmentHours;
                    //  ==================================================
                }

                departmentMaster = departmentMaster
                    .OrderBy(o => o.DepartmentName)
                    .ThenBy(o => o.EmployeeName)
                    .ToList();

                return departmentMaster;
            }
            catch (Exception ex)
            {
                var ex1 = new ServiceException("Something went wrong", ex);
                _logger.LogError(ex1, null);

                return new List<ReportDepartmentMaster>();
            }
        }
    }

    public class ReportDepartmentMaster
    {
        [Key]
        public string EmployeeId { get; set; }

        public string DepartmentName { get; set; }

        public string EmployeeName { get; set; }

        public string EmployeeCode { get; set; }

        public string Action { get; set; }

        public decimal AvgDailyWorkingHours { get; set; }

        public string AvgDailyWorkingTime { get; set; }

        public long LateLoginCount { get; set; }

        public long EarlyLogoutCount { get; set; }

        public List<ReportDepartmentDetail> ReportDepartmentDetails { get; set; }
    }

    public class ReportDepartmentDetail
    {
        public string Day { get; set; }

        public string LoginTime { get; set; }

        public string LogoutTime { get; set; }

       // public decimal WorkingHour { get; set; }

        public string WorkingTime { get; set; }

        public string LateLogin { get; set; }

        public string EarlyLogout { get; set; }
    }

    public class ReportAttendanceIndex
    {
        public string EmployeeId { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }

        public string Department { get; set; }

        public DateTime DateTime { get; set; }

        public string Action { get; set; }

        public bool FirstLogin { get; set; }

        public bool LastLog { get; set; }

        public bool LateLogin { get; set; }

        public bool EarlyLogout { get; set; }

        public decimal Time { get; set; }

        public DateTime TimeStamp { get; set; }
    }

    public class GroupedEmployee
    {
        public string EmployeeId { get; set; }

        public string Department { get; set; }

        public string EmployeeName { get; set; }

        //public string Action { get; set; }

        public string EmployeeCode { get; set; }

        public DateOnly Date { get; set; }

        public DateTime DateTime { get; set; }
        public decimal WorkingHour { get; set; }

        public string WorkingTime { get; set; }

    }
}
