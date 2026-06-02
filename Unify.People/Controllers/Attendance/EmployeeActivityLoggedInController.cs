using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using Unify.Data;

namespace Unify.People.Controllers.Attendance
{
    public class EmployeeActivityLoggedInController : PageController
    {
        private GridView<EmployeeActivity> gridView;

        public override void Init()
        {
            // GridView for Employee details
            gridView = this.AddGridView<EmployeeActivity>();
            gridView
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
            gridView.Css.SetHeight("calc(100vh - 275px)");
            gridView.Fields(field =>
            {
                field.ClearFields();

                field.AddField(f => f.EmployeeName);
                field.AddField(f => f.EmployeeCode);
                field.AddField(f => f.Department);

                var dateTime = field.AddField(f => f.TimeStamp);
                //dateTime.Caption = "Login Time";
            })
            .SetDefaultSort("EmployeeName", false);

            gridView.OnLoadData += GridView_OnLoadData;
        }

        private async Task<LoadResult> GridView_OnLoadData(DataSourceLoadOptions loadOptions, string? parentId)
        {
            var peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            var today = DateTime.Now.Date;
            List<EmployeeActivity> listEmployeesActivity;
            var groupedAttendanceLog = peopleDb.AttendanceLogs
                .Where(a => a.DateTime.Date == today)
                .GroupBy(a => a.EmployeeId)
                .Select(g => g
                    .OrderByDescending(a => a.DateTime)
                    .FirstOrDefault()
                )
                .ToList();
            listEmployeesActivity =
                (
                    from a in groupedAttendanceLog
                    join e in peopleDb.Employees on a.EmployeeId equals e.Id
                    join d in peopleDb.Departments on e.DepartmentId equals d.Id
                    select new EmployeeActivity
                    {
                        EmployeeName = e.Name,
                        EmployeeCode = e.Code,
                        Department = d.Name,
                        TimeStamp = a.DateTime.ToString("HH:mm:ss")
                    }
                )
                .ToList();
            return DataSourceLoader.Load(listEmployeesActivity, loadOptions);
        }
    }
}
