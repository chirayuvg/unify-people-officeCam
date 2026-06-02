using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;
using Unify.STR.Attendance.Data.SQL;

namespace Unify.STR.Attendance.Controllers.Organization
{
    public class ShiftController : PageController
    {
        public override void Init()
        {
            this.Page.Title = "Appointments";

            var formView = this.AddFormView<Shift>();

            var gridView = this.AddGridView<Shift>()
                .SetFormView(formView)
                .ConfigueUI(
                    grid => grid
                        .ColumnWidth(Mode.Auto)
                        .FilterRow(filterRow => filterRow
                            .ApplyFilter(GridApplyFilterMode.Auto)
                            .Visible(false)
                        )
                        .HeaderFilter(headerFilter => headerFilter.Visible(false))
                        .RowAlternationEnabled(true)
                        .SearchPanel(s => s.Visible(false))
                        .ShowBorders(true)
                        .ShowColumnLines(false)
                        .ShowRowLines(false)
                        .WordWrapEnabled(true).Width("100%")
                );
        }
    }
}
