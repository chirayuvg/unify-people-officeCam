
using Microsoft.AspNetCore.Mvc;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;

using UnifyNow.Forest.Data.SQL;

namespace UnifyNow.Forest.Controllers
{
    public class DesignationController : PageController
    {
        public override void Init()
        {
            this.Page.Title = "Designations";

            var formView = this.AddFormView<Designation>();

            var gridView = this.AddGridView<Designation>()
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

    //public class DesignationPage : PageModel
    //{
    //    public GridView<Designation> GridView { get; set; } = new();

    //    public FormView<Designation> FormView { get; set; } = new();
    //}
}