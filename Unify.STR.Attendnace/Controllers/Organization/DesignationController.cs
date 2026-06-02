using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Unify.STR.Attendance.Data.SQL;

namespace Unify.STR.Attendance.Controllers.Organization
{
    public class DesignationController : PageController
    {
        public override void Init()
        {
            this.Page.Title = "Desiginations";

            var formView = this.AddFormView<Desigination>();

            var gridView = this.AddGridView<Desigination>()
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

