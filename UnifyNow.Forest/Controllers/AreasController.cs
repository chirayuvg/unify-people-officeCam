
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;

using Microsoft.AspNetCore.Mvc;
using UnifyNow.Forest.Data.SQL;

namespace UnifyNow.Forest.Controllers
{
    public class AreasController : PageController
    {
        public override void Init()
        {
            this.Page.Title = "Areas";

            var formView = this.AddFormView<Area>();

            var gridView = this.AddGridView<Area>()
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

    //public class AreaPage : PageModel
    //{
    //    public AreaPage()
    //    {
    //        this.Title = "Area";
    //    }
    //    public GridView<Area> GridView { get; set; } = new();

    //    public FormView<Area> FormView { get; set; } = new();
    //}
}
