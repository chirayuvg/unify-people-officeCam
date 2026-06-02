using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using Unify.CCC.Data;
using Microsoft.AspNetCore.Mvc;
using DevExtreme.AspNet.Mvc;

namespace Unify.CCC.Controllers
{
    public class TicketController : PageController
    {
        public override void Init()
        {
            this.Page.Title = "Ticket";

            var formView = this.AddFormView<Ticket>();

            var gridView = this.AddGridView<Ticket>()
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
    //public class TicketController : PageController<TicketPage>
    //{
    //    public TicketController()
    //    {
    //        this.Page.Title = "Ticket";
    //    }
    //}
    //public class TicketPage : PageModel
    //{
    //    public GridView<Ticket> GridView { get; set; } = new();
    //    public FormView<Ticket> FormView { get; set; } = new();
        
    //}
}
