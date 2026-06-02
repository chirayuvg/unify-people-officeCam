using AppStudio.Enterprise.Data;
using AppStudio.Framework.Core;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;
using Unify.Data;
using Unify.People.Models;
using Unify.People.Services;
using Unify.People.Widgets;

namespace Unify.People.Controllers.Attendance
{
    public class AttendanceDashboardEmployeeController : PageController
    {
        private readonly EmployeeService _employeeService;

        private CardView<DashboardFilterData> cardView;

        private ChartViewEmployeeWiseDailyWorkingHours chartViewEmployeeWiseDailyWorkingHours;
        private GridViewEmployeeWiseDepartmentAvgHour<LeaderboardEmployeeData> gridViewEmployeeWiseDepartmentAvgHour;

        private GridViewEmployeeWiseMyMeetings<MyMeeting> gridViewEmployeeWiseMyMeetings;
        private GridViewEmployeeWiseTimeSheet<EmployeeWiseTimeSheet> gridViewEmployeeWiseTimeSheet;

        public AttendanceDashboardEmployeeController(EmployeeService employeeService )
        {
            _employeeService = employeeService;
        }

        public override void Init()
        {
            this.Page.Title = "Live Analytics";
            this.Page.ShowScrollBar = true;

            var kpiLogInTime = new KpiLogInTime(_employeeService).SetId("kpiLogInTime");
            //kpiLogInTime.Data.Value = "-";
            kpiLogInTime.Data.Color = KpiColor.SeaGreen;
            kpiLogInTime.Data.Icon = "fa-solid fa-clock";

            var kpiTotalLateLogIns = new KpiTotalLateLogIns(_employeeService).SetId("kpiTotalLateLogIns");
           // kpiTotalLateLogIns.Data.Value = "-";
            kpiTotalLateLogIns.Data.Color = KpiColor.SeaGreen;
            kpiTotalLateLogIns.Data.Icon = "fa-solid fa-stopwatch";

            var kpiTotalHours = new KpiTotalHours(_employeeService).SetId("kpiTotalHours");
           // kpiTotalHours.Data.Value = "-";
            kpiTotalHours.Data.Color = KpiColor.RoyalBlue;
            kpiTotalHours.Data.Icon = "fa-solid fa-hourglass-half";

            var kpiAverageHours = new KpiAverageHours(_employeeService).SetId("kpiAverageHours");
           // kpiAverageHours.Data.Value = "-";
            kpiAverageHours.Data.Color = KpiColor.Indigo;
            kpiAverageHours.Data.Icon = "fa-solid fa-clock-rotate-left";

            var kpiTotalEarlyLogout = new KpiTotalEarlyLogout(_employeeService).SetId("kpiTotalEarlyLogout");
           // kpiTotalEarlyLogout.Data.Value = "-";
            kpiTotalEarlyLogout.Data.Color = KpiColor.Orange;
            kpiTotalEarlyLogout.Data.Icon = "fa-solid fa-door-open";

            this.AddColumns().Controls(c =>
            {
                c.AddControl(kpiLogInTime);
                c.AddControl(kpiTotalLateLogIns);
                c.AddControl(kpiTotalHours);
                c.AddControl(kpiAverageHours);
                c.AddControl(kpiTotalEarlyLogout);
            });

            cardView = this.AddCardView<DashboardFilterData>().SetControlStyle(CardViewStyle.Filter);
            cardView.Title = "Trends";
            cardView.ControlStyle = CardViewStyle.Filter;
            cardView.SubmitOnDataChange = true;
            cardView.LabelPosition = LabelPosition.Left;
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

            this.AddColumns().Controls(c =>
            {
                chartViewEmployeeWiseDailyWorkingHours = c.AddControl<ChartViewEmployeeWiseDailyWorkingHours>();
                gridViewEmployeeWiseDepartmentAvgHour = c.AddControl<GridViewEmployeeWiseDepartmentAvgHour<LeaderboardEmployeeData>>();
                gridViewEmployeeWiseDepartmentAvgHour
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
                        .Columns(c =>
                        {
                            c.Add().DataField("employeeName").Width("30%").Alignment(HorizontalAlignment.Center);
                            c.Add().DataField("department").Width("30%").Alignment(HorizontalAlignment.Center);
                            c.Add().DataField("avgDailyWorkingHours").Width("40%").Alignment(HorizontalAlignment.Center);
                        })
                    )
                    .Fields(field =>
                    {
                        field.ClearFields();
                    });
            });

            gridViewEmployeeWiseMyMeetings = this.AddControl<GridViewEmployeeWiseMyMeetings<MyMeeting>>();
            gridViewEmployeeWiseMyMeetings.SetEmployeeService(_employeeService);
            gridViewEmployeeWiseMyMeetings
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
                    .Columns(c =>
                    {
                        c.Add().DataField("date").Width("40%").Alignment(HorizontalAlignment.Center);
                        c.Add().DataField("primaryVisitor").Width("30%").Alignment(HorizontalAlignment.Center);
                        c.Add().DataField("status").Width("30%").Alignment(HorizontalAlignment.Center);
                        c.Add().DataField("startTime").Width("30%").Alignment(HorizontalAlignment.Center);
                        c.Add().DataField("endTime").Width("30%").Alignment(HorizontalAlignment.Center);
                    })
                ).Fields(field =>
                {
                    field.ClearFields();
                });

            gridViewEmployeeWiseTimeSheet = this.AddControl<GridViewEmployeeWiseTimeSheet<EmployeeWiseTimeSheet>>();
            gridViewEmployeeWiseTimeSheet
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
                    .Columns(c =>
                    {
                        c.Add().DataField("date").Caption("Date").Width("20%").Alignment(HorizontalAlignment.Center);
                        c.Add().DataField("lateLogin").Caption("").Width("20%").Alignment(HorizontalAlignment.Center);
                        c.Add().DataField("firstLoginTime").Caption("Login Time").Width("20%").Alignment(HorizontalAlignment.Center);
                        c.Add().DataField("lastLogoutTime").Caption("Logout Time").Width("20%").Alignment(HorizontalAlignment.Center);
                        c.Add().DataField("earlyLogout").Caption("").Width("20%").Alignment(HorizontalAlignment.Center);
                    })


                ).Fields(field =>
                {
                    field.ClearFields();
                });

            UiAction.AddJsCode(@"
                setTimeout(function () {
                    window.location.reload();
                }, 300000);
            ");

            UiAction.AddJsCode(@"
                (function setUniformFontStyles() {
                    const selectors = [
                        '.dx-toolbar-label .dx-toolbar-item-content > div',
                        '.dx-datagrid .dx-header-row > td',
                        '.dx-datagrid .dx-row',
                        '.dx-toolbar .dx-item-content',
                        '.dx-datagrid .dx-datagrid-content .dx-row > td',
                        '.dx-card .dx-item-content',
                        '.dx-datagrid .dx-pager',
                        'h1, h2, h3, h4',
                        '.card-value',       
                        '.card-text'
                    ];

                    // Apply styles to regular HTML elements
                    selectors.forEach(selector => {
                        document.querySelectorAll(selector).forEach(el => {
                            el.style.fontSize = '15px';
                            el.style.fontFamily = 'Segoe UI, sans-serif';
                            el.style.lineHeight = '1.4';
                            el.style.marginLeft = '0';
                            el.style.marginRight = '0';
                        });
                    });

                    // Handle DevExpress Chart SVG Titles
                    document.querySelectorAll('.dxc-title text').forEach(el => {
                            el.style.fontSize = '15px';
                            el.style.fontFamily = 'Segoe UI, sans-serif';
                            el.style.lineHeight = '1.4';
                            el.style.marginLeft = '0';
                            el.style.marginRight = '0';
                    });

                document.querySelectorAll('.dx-toolbar-item-content div').forEach(el => {
                            el.style.fontSize = '15px';
                            el.style.fontFamily = 'Segoe UI, sans-serif';
                            el.style.lineHeight = '1.4';
                            el.style.marginLeft = '0';
                            el.style.marginRight = '0';
                });

                })();
            ");

            PrepareComponentTitle();
        }

        public override Task ProcessAsync()
        {
            var cardView = this.Page.FindControl<CardView<DashboardFilterData>>();
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

        private async Task CardView_OnSubmit(DashboardFilterData model, IFormFileCollection files)
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

            PrepareComponentTitle();
        }

        private void PrepareComponentTitle()
        {
            string departmentName = string.IsNullOrEmpty(cardView.Data.DepartmentName) ? "'All'" : "'" + cardView.Data.DepartmentName + "'";
            this.chartViewEmployeeWiseDailyWorkingHours.Title = GetPeriodInText(cardView) + "Daily Working Hours";
            this.gridViewEmployeeWiseDepartmentAvgHour.Toolbar.Title = GetPeriodInText(cardView) + "Avg Daily Working Hours of " + departmentName + " department";
            this.gridViewEmployeeWiseMyMeetings.Toolbar.Title = "My Meetings " + GetPeriodInTextForMeeting(cardView) ;
            this.gridViewEmployeeWiseTimeSheet.Toolbar.Title = GetPeriodInText(cardView) + "Late Logins and Early Logouts";
        }

        private string GetPeriodInTextForMeeting(CardView<DashboardFilterData> cardView)
        {
            string returnValue = "";

            switch (cardView.Data.Period)
            {
                case Period.ThisWeek: returnValue = "in This Week "; break;
                case Period.ThisMonth: returnValue = "in This Month "; break;
                case Period.LastMonth: returnValue = "in Last Month "; break;
                case Period.CustomDateRange:
                    returnValue =
                        "From " + cardView.Data.FromDate.ToString("dd/MMM")
                        + " To " + cardView.Data.ToDate.ToString("dd/MMM")
                        + " ";
                    break;
                default: break;
            }

            return returnValue;
        }

        private string GetPeriodInText(CardView<DashboardFilterData> cardView)
        {
            string returnValue = "";

            switch (cardView.Data.Period)
            {
                case Period.ThisWeek: returnValue = "This Week's "; break;
                case Period.ThisMonth: returnValue = "This Month's "; break;
                case Period.LastMonth: returnValue = "Last Month's "; break;
                case Period.CustomDateRange:
                    returnValue =
                        "From " + cardView.Data.FromDate.ToString("dd/MMM")
                        + " To " + cardView.Data.ToDate.ToString("dd/MMM")
                        + " ";
                    break;
                default: break;
            }

            return returnValue;
        }
    }
}
