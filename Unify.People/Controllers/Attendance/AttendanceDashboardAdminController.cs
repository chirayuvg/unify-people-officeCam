using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using Unify.Data;
using Unify.Data.SQL;
using Unify.People.Models;
using Unify.People.Widgets;

namespace Unify.People.Controllers.Attendance
{
    public class AttendanceDashboardAdminController : PageController
    {
        private CardView<DashboardFilterData> cardView;

        private ChartViewAverageWorkingHours chartViewAverageWorkingHours;

        private GridViewLeaderboardEmployee<LeaderboardEmployeeData> gridViewLeaderboardEmployee;
        private GridViewLeaderboardDepartment<LeaderboardDepartmentData> gridViewLeaderboardDepartment;

        private ChartViewLateLogins chartViewLateLogins;
        private ChartViewEarlyLogouts chartViewEarlyLogouts;

        private GridViewLeaderboardLateLogin<LeaderboardLateLoginData> gridViewLeaderboardLateLogin;
        private GridViewLeaderboardEarlyLogout<LeaderboardEarlyLogoutData> gridViewLeaderboardEarlyLogout;

        public override void Init()
        {
            this.Page.Title = "Live Analytics";
            this.Page.ShowScrollBar = true;

            var kpiLoggedIn = new KpiLoggedIn().SetId("kpiLoggedIn");
            kpiLoggedIn.Data.Value = "-";
            kpiLoggedIn.Data.Color = KpiColor.SeaGreen;
            kpiLoggedIn.Data.Icon = "fa-solid fa-right-to-bracket";
            kpiLoggedIn.OnClick += KpiLoggedIn_OnClick;

            var kpiLateLogins = new KpiLateLogins().SetId("kpiLateLogins");
            kpiLateLogins.Data.Value = "-";
            kpiLateLogins.Data.Color = KpiColor.Red;
            kpiLateLogins.Data.Icon = "fa-solid fa-clock";
            kpiLateLogins.OnClick += KpiLateLogins_OnClick;

            var kpiEmployeesPresent = new KpiEmployeesPresent().SetId("kpiEmployeesPresent");
            kpiEmployeesPresent.Data.Value = "-";
            kpiEmployeesPresent.Data.Color = KpiColor.RoyalBlue;
            kpiEmployeesPresent.Data.Icon = "fa-solid fa-people-group";
            kpiEmployeesPresent.OnClick += KpiEmployeesPresent_OnClick;

            var kpiExitsRecorded = new KpiExitsRecorded().SetId("kpiExitsRecorded");
            kpiExitsRecorded.Data.Value = "-";
            kpiExitsRecorded.Data.Color = KpiColor.Orange;
            kpiExitsRecorded.Data.Icon = "fa-solid fa-door-open";
            kpiExitsRecorded.OnClick += KpiExitsRecorded_OnClick;

            this.AddColumns().Controls(c =>
            {
                c.AddControl(kpiLoggedIn);
                c.AddControl(kpiLateLogins);
                c.AddControl(kpiEmployeesPresent);
                c.AddControl(kpiExitsRecorded);
            });

            cardView = this.AddCardView<DashboardFilterData>().SetControlStyle(CardViewStyle.Filter);
            cardView.Title = "Trends";
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

            chartViewAverageWorkingHours = this.AddControl<ChartViewAverageWorkingHours>();

            //string leaderboardLabel = $@"
            //        <div style=""width: 100%; height: 50px; border: 2px solid black; padding: 5px; text-align: center; font-size: 18px; background-color: white;"">
            //            <center><span style=""font-size:24px;font-weight: 700;"">Leaderboard</span></center>
            //        </div>
            //        ";
            //this.AddHtml();

            this.AddColumns().Controls(c =>
            {
                gridViewLeaderboardEmployee = c.AddControl<GridViewLeaderboardEmployee<LeaderboardEmployeeData>>();
                gridViewLeaderboardEmployee
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

                gridViewLeaderboardDepartment = c.AddControl<GridViewLeaderboardDepartment<LeaderboardDepartmentData>>();
                gridViewLeaderboardDepartment
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
                            c.Add().DataField("department").Width("50%").Alignment(HorizontalAlignment.Center);
                            c.Add().DataField("avgDailyWorkingHours").Width("50%").Alignment(HorizontalAlignment.Center);
                        })
                    )
                    .Fields(field =>
                    {
                        field.ClearFields();
                    });
            });

            this.AddColumns().Controls(c =>
            {
                chartViewLateLogins = c.AddControl<ChartViewLateLogins>();
                chartViewEarlyLogouts = c.AddControl<ChartViewEarlyLogouts>();
            });

            this.AddColumns().Controls(c =>
            {
                gridViewLeaderboardLateLogin = c.AddControl<GridViewLeaderboardLateLogin<LeaderboardLateLoginData>>();

                gridViewLeaderboardLateLogin
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
                            c.Add().DataField("employeeName").Width("40%").Alignment(HorizontalAlignment.Center);
                            c.Add().DataField("department").Width("30%").Alignment(HorizontalAlignment.Center);
                            c.Add().DataField("countLateLogin").Width("30%").Alignment(HorizontalAlignment.Center);
                        })


                    ).Fields(field =>
                    {
                        field.ClearFields();
                    });

                gridViewLeaderboardEarlyLogout = c.AddControl<GridViewLeaderboardEarlyLogout<LeaderboardEarlyLogoutData>>();
                gridViewLeaderboardEarlyLogout
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
                            c.Add().DataField("employeeName").Width("40%").Alignment(HorizontalAlignment.Center);
                            c.Add().DataField("department").Width("30%").Alignment(HorizontalAlignment.Center);
                            c.Add().DataField("countEarlyLogout").Width("30%").Alignment(HorizontalAlignment.Center);
                        })
                    ).Fields(field =>
                    {
                        field.ClearFields();
                    });
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
                            el.style.fontSize = '18px';
                            el.style.fontWeight = 'bold';
                            el.style.fontFamily = 'Segoe UI, sans-serif';
                            el.style.lineHeight = '1.4';
                            el.style.marginLeft = '0';
                            el.style.marginRight = '0';
                        });
                    });

                    // Handle DevExpress Chart SVG Titles
                    document.querySelectorAll('.dxc-title text').forEach(el => {
                            el.style.fontSize = '18px';
                            el.style.fontWeight = 'bold';
                            el.style.fontFamily = 'Segoe UI, sans-serif';
                            el.style.lineHeight = '1.4';
                            el.style.marginLeft = '0';
                            el.style.marginRight = '0';
                    });

                document.querySelectorAll('.dx-toolbar-item-content div').forEach(el => {
                            el.style.fontSize = '18px';
                            el.style.fontWeight = 'bold';
                            el.style.fontFamily = 'Segoe UI, sans-serif';
                            el.style.lineHeight = '1.4';
                            el.style.marginLeft = '0';
                            el.style.marginRight = '0';
                });

                })();
            ");

            PrepareComponentTitle();
        }

        private void KpiLoggedIn_OnClick(KpiModel obj)
        {
            UiAction.ShowPopup("Employee Activity - Logged In", "EmployeeActivityLoggedIn");
        }

        private void KpiLateLogins_OnClick(KpiModel obj)
        {
            UiAction.ShowPopup("Employee Activity - Late Logins", "EmployeeActivityLateLogins");
        }

        private void KpiEmployeesPresent_OnClick(KpiModel obj)
        {
            UiAction.ShowPopup("Employee Activity - Employees Present", "EmployeeActivityEmployeesPresent");
        }

        private void KpiExitsRecorded_OnClick(KpiModel obj)
        {
            UiAction.ShowPopup("Employee Activity - Exits Recorded", "EmployeeActivityExitsRecorded");
        }

        private async Task<DevExtreme.AspNet.Data.ResponseModel.LoadResult> Department_OnLoadLookupData(DevExtreme.AspNet.Mvc.DataSourceLoadOptions loadOptions)
        {
            var _peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            var departments = _peopleDb.Departments.ToList();
            departments.Add(new Department() { Id = Guid.Empty, Name = "--All--" });
            LoadResult loadResult = DataSourceLoader.Load(departments, loadOptions);
            return loadResult;
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
            this.chartViewAverageWorkingHours.Title = GetPeriodInText(cardView) + "Avg Daily Working Hours for " + departmentName + " Department";
            this.gridViewLeaderboardEmployee.Toolbar.Title = GetPeriodInText(cardView) + "Avg Working Hours for " + departmentName + " Dept Employee-wise";
            this.gridViewLeaderboardDepartment.Toolbar.Title = GetPeriodInText(cardView) + "Avg Working Hours Department Wise";
            this.chartViewLateLogins.Title = GetPeriodInText(cardView) + "Late Logins for " + departmentName + " Department";
            this.chartViewEarlyLogouts.Title = GetPeriodInText(cardView) + "Early Logouts for " + departmentName + " Department";
            this.gridViewLeaderboardLateLogin.Toolbar.Title = GetPeriodInText(cardView) + "Late Logins Stats. for " + departmentName + " Department";
            this.gridViewLeaderboardEarlyLogout.Toolbar.Title = GetPeriodInText(cardView) + "Early Logouts Stats. for " + departmentName + " Department";
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

    public class DepartmentForDropdown
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }

        public DepartmentForDropdown(Guid? Id, string Name)
        {
            this.Id = Id;
            this.Name = Name;
        }
    }

    public class EmployeeActivity
    {
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public string Department { get; set; }
        public string TimeStamp { get; set; }
    }
}
