using AppStudio.Framework.Core;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using Microsoft.EntityFrameworkCore;
using Unify.Data;

namespace Unify.People.Controllers.Attendance
{
    public class EmployeeActivityExitsRecordedController : PageController
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
                //dateTime.Caption = "Exit Time";
            })
            .SetDefaultSort("Name", false);

            gridView.OnLoadData += GridView_OnLoadData;
        }

        private async Task<LoadResult> GridView_OnLoadData(DataSourceLoadOptions loadOptions, string? parentId)
        {
            var TenantId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();
            DateTime toDay = DateTime.Now;

            var peopleDb = CoreServices.GetService<PeopleDb>();
            var groupedData = await peopleDb.AttendanceLogs
                .Where(a => a.DateTime.Date == toDay.Date)
                .GroupBy(a => a.Name)
                .ToListAsync(); // This moves the data to memory
            var result = groupedData
                .Select(g => g
                    .OrderByDescending(x => x.DateTime)
                    .FirstOrDefault()
                )
                .Where(a => a != null && a.Action == "CheckOut")
                .Select(ea => new EmployeeActivity
                {
                    EmployeeName = ea.Name,
                    EmployeeCode = ea.Code,
                    Department = ea.Department,
                    TimeStamp = ea.DateTime.ToString("HH:mm:ss")
                })
                .ToList();

            return DataSourceLoader.Load(result, loadOptions);
        }
    }
}
