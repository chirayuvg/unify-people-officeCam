
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;

using Microsoft.AspNetCore.Mvc;
using UnifyNow.Forest.Data.SQL;

namespace UnifyNow.Forest.Controllers
{
    public class CamerasController : PageController
    {
        public override void Init()
        {
            this.Page.Title = "Cameras";

            var formView = this.AddFormView<Camera>();

            var gridView = this.AddGridView<Camera>()
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

    //public class CameraPage : PageModel
    //{
    //    public GridView<Camera> GridView { get; set; } = new();

    //    public FormView<Camera> FormView { get; set; } = new();
    //}
}