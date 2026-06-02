using AppStudio.Framework.Identity.Models;
using AppStudio.Framework.MVC;
using AppStudio.Model;

using Microsoft.AspNetCore.Mvc;
using UnifyNow.Forest.Data.SQL;
using AppStudio.Framework.MVC.Controllers;
using DevExtreme.AspNet.Mvc;

namespace UnifyNow.Forest.Controllers
{
    public class IntelligenceController : PageController
    {

        public override void Init()
        {
            this.Page.Title = "Intelligences";

            var formView = this.AddFormView<Intelligence>();

            var gridView = this.AddGridView<Intelligence>()
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

    //public class IntelligencePage : PageModel
    //{
    //    public GridView<Intelligence> GridView { get; set; } = new();

    //    public FormView<Intelligence> FormView { get; set; } = new();
    //}
}
