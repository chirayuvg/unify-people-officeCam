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
using Unify.People.RulesEngine;
using Unify.People.Services;

namespace Unify.People.Controllers.Attendance
{
    public class ReportEmployeeController : PageController
    {
        private readonly EmployeeService _employeeService;

        private readonly ILogger<ReportEmployeeController> _logger;
        private CardView<EmployeeFilterData> cardView;

        public ReportEmployeeController(ILogger<ReportEmployeeController> logger, EmployeeService employeeService)
        {
            _logger = logger;
            _employeeService = employeeService;
        }

        public override void Init()
        {
            var _applicationDb = AppStudio.Framework.Core.CoreServices.GetService<ApplicationDb>();
            var userId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetUserId();
            var tenantUser = _applicationDb.TenantUsers.FirstOrDefault(m => m.Id == userId);

            if (tenantUser.Roles.Any(r => (r == "Super Admin") || (r == "Admin")))
            {
                this.Page.SetTitle("Employee Wise Report");
            }
            else
            {
                this.Page.SetTitle("Report");
            }
            this.Page.Toolbar.AddButton("Download").OnClickExecute(m => m.DownloadFile("EmployeeReport.xlsx", "ReportEmployee", "Download"));

            cardView = this.AddCardView<EmployeeFilterData>().SetControlStyle(CardViewStyle.Filter);
            cardView.ControlStyle = CardViewStyle.Filter;
            cardView.SubmitOnDataChange = true;
            cardView.LabelPosition = LabelPosition.Left;
            cardView.GetFieldControl(m => m.EmployeeId).OnLoadLookupData += Employee_OnLoadLookupData;
            cardView.GetFieldControl(m => m.EmployeeName).IsVisible = false;
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

            var gridView = (GridView<ReportEmployeeMaster>)this.AddGridView<ReportEmployeeMaster>()
                .ConfigueUI(
                    grid => grid
                        .ColumnAutoWidth(false)
                        .FilterRow(f => f.Visible(false))
                        .HeaderFilter(headerFilter => headerFilter.Visible(false))
                        .RowAlternationEnabled(true)
                        .Scrolling(scrolling => scrolling.ColumnRenderingMode(GridColumnRenderingMode.Virtual))
                        .SearchPanel(s => s.Visible(false))
                        .ShowBorders(true)
                        .ShowColumnHeaders(true)
                        .ShowColumnLines(false)
                        .ShowRowLines(false)
                        .WordWrapEnabled(true).Width("100%")
                        .Paging(paging => paging.Enabled(true).PageSize(10)) //  Enable paging with 5 rows per page
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
            gridView.Css.SetHeight("calc(100vh - 300px)");
            gridView.Fields(field =>
            {
                field.ClearFields();

                field.AddField(f => f.EmployeeName);
                field.AddField(f => f.DepartmentName);
                field.AddField(f => f.EmployeeCode);
                field.AddField(f => f.Date);
                field.AddField(f => f.LoginTime);
                field.AddField(f => f.LogoutTime);
                field.AddField(f => f.TotalWorkingHour);
                field.AddField(f => f.LateLogin);
                field.AddField(f => f.EarlyLogout);
            });
            gridView.OnLoadData += GridView_OnLoadData;

            //var btnRegularize = gridView.AddButton("Regularize");
            //btnRegularize.OnClick += BtnRegularize_OnClick;

            //var btnLeave = gridView.AddButton("Leave");
            //btnLeave.OnClick += BtnLeave_OnClick;
        }

        public override Task ProcessAsync()
        {
            var cardView = this.Page.FindControl<CardView<EmployeeFilterData>>();
            cardView.GetFieldControl(m => m.EmployeeId).OnLoadLookupData += Employee_OnLoadLookupData;
            cardView.GetFieldControl(m => m.EmployeeName).IsVisible = false;

            var _peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            var _applicationDb = AppStudio.Framework.Core.CoreServices.GetService<ApplicationDb>();
            var userId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetUserId();
            var tenantUser = _applicationDb.TenantUsers.FirstOrDefault(m => m.Id == userId);

            if (tenantUser.Roles.Any(r => (r == "Super Admin") || (r == "Admin")))
            {
                cardView.GetFieldControl(m => m.EmployeeId).IsReadOnly = false;
            }
            else
            {
                cardView.GetFieldControl(m => m.EmployeeId).IsReadOnly = true;
            }

            return base.ProcessAsync();
        }

        private static RazorBlock GenerateChildGridView(IHtmlHelper html)
        {
            var grd = html
                .DevExtreme()
                .DataGrid<ReportEmployeeDetail>()
                .DataSource(new JS("data.reportEmployeeDetails"))
                .Columns(col =>
                {
                    col.Add().DataField("action").Caption("Action").Width(135);
                    col.Add().DataField("checkInTimeStamp").Caption("CheckIn Time").Width(140);
                    col.Add().DataField("checkOutTimeStamp").Caption("CheckOut Time").Width(150);
                    col.Add().DataField("workingHour").Caption("Working Hour").Width(150);
                });

            return (item) => new HtmlString($"<div style='padding-left: 500px;'>{grd.ToTemplate()}</div>");
        }

        private async Task<LoadResult> Employee_OnLoadLookupData(DataSourceLoadOptions loadOptions)
        {
            var _peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            var employees = _peopleDb.Employees.ToList();
            employees.Add(new Employee() { Id = Guid.Empty, Name = "--All--" });
            LoadResult loadResult = DataSourceLoader.Load(employees, loadOptions);

            return loadResult;
        }

        private async Task CardView_OnSubmit(EmployeeFilterData model, IFormFileCollection files)
        {
            model.GetDates();
            model.GetEmployeeName();

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
            var cardView = this.Page.FindControl<CardView<EmployeeFilterData>>();
            var TenantId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();
            DateTime fromDate =
                (cardView.Data.FromDate == DateOnly.MinValue)
                ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
                : DateTime.Parse(cardView.Data.FromDate.ToString("yyyy-MM-dd"));
            DateTime toDate =
                (cardView.Data.ToDate == DateOnly.MinValue)
                ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
                : DateTime.Parse(cardView.Data.ToDate.ToString("yyyy-MM-dd"));
            string employeeName = GetEmployeeName();//(cardView.Data.EmployeeName == null || cardView.Data.EmployeeName == "") ? "" : cardView.Data.EmployeeName;

            var results = await GetReportData(TenantId, fromDate, toDate, employeeName);

            return DataSourceLoader.Load(results, loadOptions);
        }

        public string GetEmployeeName()
        {
            var cardView = this.Page.FindControl<CardView<EmployeeFilterData>>();
            var _peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            string EmployeeName = (cardView.Data.EmployeeId == Guid.Empty) ? "" : _peopleDb.Employees.Where(m => m.Id == cardView.Data.EmployeeId).FirstOrDefault().Name;
            return EmployeeName;
        }

        [HttpPost]
        public async Task<IActionResult> Download()
        {
            var cardView = this.Page.FindControl<CardView<EmployeeFilterData>>();
            var TenantId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();
            DateTime fromDate =
                (cardView.Data.FromDate == DateOnly.MinValue)
                ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
                : DateTime.Parse(cardView.Data.FromDate.ToString("yyyy-MM-dd"));
            DateTime toDate =
                (cardView.Data.ToDate == DateOnly.MinValue)
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
                worksheet.Cells[1, 3].Value = "Department";
                worksheet.Cells[1, 4].Value = "Date";
                worksheet.Cells[1, 5].Value = "Login Time";
                worksheet.Cells[1, 6].Value = "Logout Time";
                worksheet.Cells[1, 7].Value = "Total Hours";
                worksheet.Cells[1, 8].Value = "Late Login";
                worksheet.Cells[1, 9].Value = "Early Logout";

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
                    worksheet.Cells[1, actionStartCol + (i * 3) + 1].Value = $"CheckIn Time {i + 1}";
                    worksheet.Cells[1, actionStartCol + (i * 3) + 2].Value = $"CheckOut Time {i + 1}";
                    worksheet.Cells[1, actionStartCol + (i * 3) + 3].Value = $"WorkingMinute {i + 1}";
                }

                int row = 2;

                //  Add ReportEmployeeData Records
                foreach (var record in results)
                {
                    worksheet.Cells[row, 1].Value = record.EmployeeName;
                    worksheet.Cells[row, 2].Value = record.EmployeeCode;
                    worksheet.Cells[row, 3].Value = record.DepartmentName;
                    worksheet.Cells[row, 4].Value = record.Date.ToString("dd-MM-yyyy");
                    worksheet.Cells[row, 5].Value = record.LoginTime;
                    worksheet.Cells[row, 6].Value = record.LogoutTime;
                    worksheet.Cells[row, 7].Value = record.TotalWorkingHour;
                    worksheet.Cells[row, 8].Value = record.LateLogin;   // ? "Yes" : "No";
                    worksheet.Cells[row, 9].Value = record.EarlyLogout; // ? "Yes" : "No";

                    //  Add ReportEmployeeDetails Records
                    for (int i = 0; i < record.ReportEmployeeDetails.Count; i++)
                    {
                        worksheet.Cells[row, actionStartCol + (i * 3)].Value = record.ReportEmployeeDetails[i].Action;
                        worksheet.Cells[row, actionStartCol + (i * 3) + 1].Value = record.ReportEmployeeDetails[i].CheckInTimeStamp;
                        worksheet.Cells[row, actionStartCol + (i * 3) + 2].Value = record.ReportEmployeeDetails[i].CheckOutTimeStamp;
                        worksheet.Cells[row, actionStartCol + (i * 3) + 3].Value = record.ReportEmployeeDetails[i].WorkingHour;
                    }

                    row++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "EmployeeReport.xlsx");
            }
        }

        public async Task<List<ReportEmployeeMaster>> GetReportData(Guid TenantId, DateTime fromDate, DateTime toDate, string employeeName)
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

                if (!string.IsNullOrEmpty(employeeName)) attendanceLogs = attendanceLogs.Where(a => a.Name == employeeName);

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

                //  Prepare Employee Master Data List
                //  ------------------------------------------------------------
                List<ReportEmployeeMaster> employeeMaster = reportAttendanceIndex
                    .Where(w => (employeeName == "" || w.Name == employeeName))
                    .GroupBy(g => new { g.EmployeeId, g.Name, g.Code, g.Department, g.DateTime.Date })
                    .Select(g => new ReportEmployeeMaster
                    {
                        EmployeeId = g.Key.EmployeeId,
                        EmployeeName = g.Key.Name.ToString(),
                        EmployeeCode = g.Key.Code.ToString(),
                        DepartmentName = g.Key.Department.ToString(),
                        Date = DateOnly.FromDateTime(g.Key.Date),
                        TotalWorkingMinute = g.Sum(a => (decimal)a.Time),
                        LateLogin = g.Any(a => a.Action == "CheckIn" && a.FirstLogin && a.LateLogin) ? "Yes" : "",
                        EarlyLogout = g.Any(a => a.Action == "CheckOut" && a.LastLog && a.EarlyLogout) ? "Yes" : ""
                    })
                    .ToList();
                //  ============================================================

                //  Calculate TotalTime
                //  ------------------------------------------------------------
                foreach (var item in employeeMaster)
                {
                    int totalSeconds = (int)(item.TotalWorkingMinute * 60);
                    TimeSpan timeSpan = TimeSpan.FromSeconds(totalSeconds);
                    item.TotalWorkingHour = timeSpan.ToString(@"hh\:mm");
                }
                //  ============================================================

                //  For each record in Master Data, fill with the Detail data
                foreach (ReportEmployeeMaster employee in employeeMaster)
                {
                    //  Prepare some of Master data
                    //  --------------------------------------------------
                    DateTime date = employee.Date.ToDateTime(TimeOnly.MinValue);

                    TimeOnly LoginTime = reportAttendanceIndex
                    .Where(w =>
                        w.EmployeeId == employee.EmployeeId
                        && w.Action == "CheckIn"
                        && w.FirstLogin == true
                        && w.DateTime.Date == date.Date // ensure same calendar day
                    )
                    .OrderBy(w => w.TimeStamp)
                    .Select(w => TimeOnly.FromDateTime(w.TimeStamp))
                    .FirstOrDefault();

                    employee.LoginTime = LoginTime.ToString("HH:mm:ss");

                    TimeOnly LogoutTime = reportAttendanceIndex
                        .Where(w =>
                            w.EmployeeId == employee.EmployeeId
                            && w.Action == "CheckOut"
                            //&& w.LastLog == true
                            && w.DateTime.Date == date.Date // Compare only date
                        )
                        .OrderByDescending(w => w.TimeStamp)
                        .Select(w => TimeOnly.FromDateTime(w.TimeStamp))
                        .FirstOrDefault();

                    employee.LogoutTime = LogoutTime.ToString("HH:mm:ss");
                    //  ==================================================

                    //  Get the list of transaction of the specific Employee
                    //  --------------------------------------------------
                    var actionList = reportAttendanceIndex
                        .Where(w =>
                            w.EmployeeId == employee.EmployeeId
                            && w.DateTime.Date == employee.Date.ToDateTime(TimeOnly.MinValue)
                        )
                        .OrderBy(o => o.TimeStamp).ToList();
                    //  ==================================================

                    //  Prepare the list of Detail data
                    //  --------------------------------------------------
                    List<ReportEmployeeDetail> reportEmployeeDetails = new List<ReportEmployeeDetail>();

                    foreach (var row in actionList)
                    {
                        ReportEmployeeDetail reportEmployeeDetail = new ReportEmployeeDetail();

                        reportEmployeeDetail.Action = row.Action;

                        if (row.Action.ToLower() == "checkin")
                            reportEmployeeDetail.CheckInTimeStamp = row.TimeStamp.ToString("HH:mm:ss");
                        else
                            reportEmployeeDetail.CheckOutTimeStamp = row.TimeStamp.ToString("HH:mm:ss");

                        reportEmployeeDetail.WorkingMinute = row.Time;
                        //TotalHour = Math.Round(g.Sum(a => (decimal)a.Time / 60), 2),

                        reportEmployeeDetails.Add(reportEmployeeDetail);
                    }

                    //  Calculate TotalTime
                    //  ------------------------------------------------------------
                    foreach (var item in reportEmployeeDetails)
                    {
                        int totalSeconds = (int)(item.WorkingMinute * 60);
                        TimeSpan timeSpan = TimeSpan.FromSeconds(totalSeconds);
                        item.WorkingHour = timeSpan.ToString(@"hh\:mm");
                    }
                    //  ============================================================

                    employee.ReportEmployeeDetails = reportEmployeeDetails;
                    //  ==================================================
                }

                employeeMaster = employeeMaster
                    .OrderBy(m => m.EmployeeName)
                    .ThenBy(m => m.Date)
                    .ToList();

                return employeeMaster;
            }
            catch (Exception ex)
            {
                var ex1 = new ServiceException("Something went wrong", ex);
                _logger.LogError(ex1, null);

                return new List<ReportEmployeeMaster>();
            }
        }

        ////private void BtnRegularize_OnClick(ButtonItem obj)
        ////{
        ////    var userId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetUserId();
        ////    string AttendanceId = obj.RecordId ?? "NULL";

        ////    UiAction.ShowPopup(
        ////          "Regularization Time"
        ////        , "ApplyRegularization"
        ////        , "Index"
        ////        , new { EmployeeId = userId, AttendanceId = AttendanceId, RegularizationDate = "" });
        ////}

        ////private void BtnLeave_OnClick(ButtonItem obj)
        ////{
        ////    var userId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetUserId();

        ////    UiAction.ShowPopup(
        ////          "Leave Details"
        ////        , "ApplyLeave"
        ////        , "Index"
        ////        , new { EmployeeId = userId });
        ////}
    }

    public class ReportEmployeeMaster
    {
        [Key]
        public string EmployeeId { get; set; }

        public string EmployeeName { get; set; }

        public string DepartmentName { get; set; }

        public string EmployeeCode { get; set; }

        public DateOnly Date { get; set; }

        public string LoginTime { get; set; }

        public string LogoutTime { get; set; }

        public decimal TotalWorkingMinute { get; set; }

        public string TotalWorkingHour { get; set; }

        public string LateLogin { get; set; }

        public string EarlyLogout { get; set; }

        public List<ReportEmployeeDetail> ReportEmployeeDetails { get; set; }
    }

    public class ReportEmployeeDetail
    {
        public string Action { get; set; }

        public string CheckInTimeStamp { get; set; }
        
        public string CheckOutTimeStamp { get; set; }

        public decimal WorkingMinute { get; set; }

        public string WorkingHour { get; set; }
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
}
