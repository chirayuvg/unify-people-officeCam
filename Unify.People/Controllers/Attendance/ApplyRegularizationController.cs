using AppStudio.Framework.Core;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Unify.Data;
using Unify.People.Models;

namespace Unify.People.Controllers.Attendance
{
    [Authorize("CookiePolicy")]
    public class ApplyRegularizationController : PageController
    {
        private readonly ILogger<ApplyRegularizationController> _logger;
        private CardView<DateOnlyFilterData> cardView;

        public override void Init()
        {
            Page.SetTitle("Apply Regularization");

            cardView = this.AddCardView<DateOnlyFilterData>().SetControlStyle(CardViewStyle.Filter);
            cardView.ControlStyle = CardViewStyle.Filter;
            cardView.SubmitOnDataChange = true;
            cardView.LabelPosition = LabelPosition.Left;
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

            var gridView = this.AddGridView<AttendanceList>()
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
                );
            gridView.Css.SetHeight("calc(100vh - 300px)");
            gridView.Fields(field =>
            {
                field.ClearFields();

                field.AddField(f => f.Date).ColumnWidth = 100;
                field.AddField(f => f.Day).ColumnWidth = 200;
                field.AddField(f => f.FirstLogin).ColumnWidth = 200;
                field.AddField(f => f.FirstLogin).ColumnWidth = 200;
                field.AddField(f => f.WorkingTime).ColumnWidth = 200;
                field.AddField(f => f.AttendanceStatus).ColumnWidth = 200;
            });
            gridView.OnLoadData += GridView_OnLoadData;

            var btnRegularize = gridView.AddButton("Regularize");
            btnRegularize.OnClick += BtnRegularize_OnClick;

            //var btnLeave = gridView.AddButton("Leave");
            //btnLeave.OnClick += BtnLeave_OnClick;
        }

        private async Task CardView_OnSubmit(DateOnlyFilterData model, IFormFileCollection files)
        {
            model.GetDates();

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

        private async Task<DevExtreme.AspNet.Data.ResponseModel.LoadResult> GridView_OnLoadData(DataSourceLoadOptions loadOptions, string? parentId)
        {
            var cardView = Page.FindControl<CardView<DateOnlyFilterData>>();
            var TenantId = CoreServices.CurrentUser.GetTenantId();
            DateTime fromDate =
                cardView.Data.FromDate == DateOnly.MinValue
                ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
                : DateTime.Parse(cardView.Data.FromDate.ToString("yyyy-MM-dd"));
            DateTime toDate =
                cardView.Data.ToDate == DateOnly.MinValue
                ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
                : DateTime.Parse(cardView.Data.ToDate.ToString("yyyy-MM-dd"));

            Guid employeeId = new Guid("e34c325c-e299-4441-c18b-08dd3c5b8b41");

            //  Generate all dates between fromDate and toDate
            var allDates = Enumerable.Range(0, DateOnly.FromDateTime(toDate).DayNumber - DateOnly.FromDateTime(fromDate).DayNumber + 1)
                .Select(offset => fromDate.AddDays(offset))
                .ToList();

            //  Query AttendanceLogs for the given employee in range
            var peopleDb = CoreServices.GetService<PeopleDb>();
            var logs = await peopleDb.AttendanceLogs
                .Where(a => a.EmployeeId == employeeId
                         && a.DateTime.Date >= fromDate.Date
                         && a.DateTime.Date <= toDate.Date
                ).ToListAsync();

            //  Group logs by date
            var grouped = logs
                .GroupBy(l => DateOnly.FromDateTime(l.DateTime))
                .ToDictionary(g => g.Key, g => g.ToList());

            //  Build final AttendanceList
            var results = allDates.Select(date =>
            {
                if (grouped.TryGetValue(DateOnly.FromDateTime(date), out var dayLogs))
                {
                    var firstLogin = dayLogs
                        .Where(l => l.Action == "CheckIn")
                        .OrderBy(l => l.DateTime)
                        .Select(l => TimeOnly.FromDateTime(l.DateTime))
                        .FirstOrDefault();

                    var lastLogout = dayLogs
                        .Where(l => l.Action == "CheckOut")
                        .OrderByDescending(l => l.DateTime)
                        .Select(l => TimeOnly.FromDateTime(l.DateTime))
                        .FirstOrDefault();

                    var totalMinutes = dayLogs.Sum(l => (int)l.Time);
                    var workingTime = TimeSpan.FromMinutes(totalMinutes);
                    var formattedWorkingTime = $"{(int)workingTime.TotalHours:D2}:{workingTime.Minutes:D2}";

                    var anyLog = dayLogs.First();

                    return new AttendanceList
                    {
                        Id = anyLog.Id,
                        EmployeeId = anyLog.EmployeeId.ToString(),
                        EmployeeName = anyLog.Name,
                        Date = DateOnly.FromDateTime(date),
                        Day = date.DayOfWeek.ToString(),
                        FirstLogin = firstLogin,
                        LastLogout = lastLogout,
                        WorkingTime = formattedWorkingTime,
                        AttendanceStatus = "Present"
                    };
                }
                else
                {
                    // No logs for this day
                    return new AttendanceList
                    {
                        Id = null,
                        EmployeeId = null,
                        EmployeeName = null,
                        Date = DateOnly.FromDateTime(date),
                        Day = date.DayOfWeek.ToString(),
                        FirstLogin = null,
                        LastLogout = null,
                        WorkingTime = "",
                        AttendanceStatus = "Absent"
                    };
                }
            })
            //  Only keep records where working time < 8 hours
            .Where(r =>
            {
                if (TimeSpan.TryParse(r.WorkingTime, out var wt))
                {
                    return wt.TotalHours < 8;
                }
                return true; // keep "00:00" entries too
            })
            //  Order by date ascending
            .OrderBy(r => r.Date)
            .ToList();

            return DataSourceLoader.Load(results, loadOptions);
        }

        private void BtnRegularize_OnClick(ButtonItem obj)
        {
            var userId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetUserId();
            string AttendanceId = obj.RecordId ?? Guid.NewGuid().ToString();

            UiAction.ShowPopup(
                  "Regularization Time"
                , "ApplyRegularizationTime"
                , "Index"
                , new { EmployeeId = userId, AttendanceId = AttendanceId, RegularizationDate = "" });
        }

        //private void BtnLeave_OnClick(ButtonItem obj)
        //{
        //    var userId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetUserId();

        //    UiAction.ShowPopup(
        //          "Leave Details"
        //        , "ApplyLeave"
        //        , "Index"
        //        , new { EmployeeId = userId });
        //}
    }

    public class AttendanceList
    {
        public Guid? Id { get; set; }

        public string? EmployeeId { get; set; }

        public string? EmployeeName { get; set; }

        public DateOnly? Date { get; set; }

        public string? Day { get; set; }

        public TimeOnly? FirstLogin { get; set; }

        public TimeOnly? LastLogout { get; set; }

        public string? WorkingTime { get; set; }

        public string? AttendanceStatus { get; set; }
    }
}
