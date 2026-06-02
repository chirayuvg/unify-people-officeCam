using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;

using Microsoft.AspNetCore.Mvc;
using UnifyNow.Forest.Data.SQL;

namespace UnifyNow.Forest.Controllers
{
    public class DepartmentController : PageController
    {
        public override void Init()
        {
            this.Page.Title = "Departments";

            var formView = this.AddFormView<Department>();

            var gridView = this.AddGridView<Department>()
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

    //public class DepartmentPage : PageModel
    //{
    //    public GridView<Department> GridView { get; set; } = new();

    //    public FormView<Department> FormView { get; set; } = new();
    //}
}
