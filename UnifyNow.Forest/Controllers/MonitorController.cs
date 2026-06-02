using AppStudio.Framework.Identity.Models;
using AppStudio.Framework.MVC;
using AppStudio.Model;

using Microsoft.AspNetCore.Mvc;
using UnifyNow.Forest.Data.SQL;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;

using Monitor = UnifyNow.Forest.Data.SQL.Monitor;

namespace UnifyNow.Forest.Controllers
{
    public class MonitorController : PageController
    {
        public override void Init()
        {
            this.Page.Title = "Monitors";

            var formView = this.AddFormView<Monitor>();

            var gridView = this.AddGridView<Monitor>()
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

    //public class MonitorPage : PageModel
    //{
    //    public GridView<Monitor> GridView { get; set; } = new();

    //    public FormView<Monitor> FormView { get; set; } = new();
    //}
}
