
using Microsoft.AspNetCore.Mvc;
using UnifyNow.Forest.Data.SQL;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;


namespace UnifyNow.Forest.Controllers
{
    public class PresetController : PageController
    {
        public override void Init()
        {
            this.Page.Title = "Presets";

            var formView = this.AddFormView<Preset>();

            var gridView = this.AddGridView<Preset>()
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

    //public class PresetPage : PageModel
    //{
    //    public GridView<Preset> GridView { get; set; } = new();

    //    public FormView<Preset> FormView { get; set; } = new();
    //}
}