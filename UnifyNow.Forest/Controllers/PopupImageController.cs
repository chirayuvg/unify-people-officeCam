using AppStudio.Framework.Identity.Models;
using AppStudio.Framework.MVC;
using AppStudio.Model;

using Microsoft.AspNetCore.Mvc;
using UnifyNow.Forest.Data.SQL;
using AppStudio.Framework.MVC.Controllers;
using DevExtreme.AspNet.Mvc;
using Monitor = UnifyNow.Forest.Data.SQL.Monitor;

namespace UnifyNow.Forest.Controllers
{
    public class PopupImageController : PageController
    {
        public override void Init()
        {
            this.Page.Title = "Image";

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

    //public class PopupImagePage : PageModel
    //{
    //    //public TileView

    //    public GridView<Department> GridView { get; set; } = new();

    //    public PopupImagePage()
    //    {
    //        this.OnPageLoad += PopupImagePage_OnPageLoad;
    //    }

    //    private async Task PopupImagePage_OnPageLoad()
    //    {
    //        //Dictionary<string, string> param = this.Parameters.ToDictionary();

    //        //string id = this.Parameters["id"];
    //    }
    //}
}
