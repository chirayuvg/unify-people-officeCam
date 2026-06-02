
using Unify.CCC.Data;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;


namespace Unify.CCC.Controllers
{

    public class ChannelController : PageController
    {
        public override void Init()
        {
            this.Page.Title = "Camera Configuration";

            var formView = this.AddFormView<Channel>();

            var gridView = this.AddGridView<Channel>()
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
        //  public ChannelController()
        //{
        //    this.Page.Title = "Camera Configuration";
        //}
    }
 }
