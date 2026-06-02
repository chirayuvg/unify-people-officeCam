
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using Unify.CCC.Data;
using DevExtreme.AspNet.Mvc;

namespace Unify.CCC.Controllers
{

    public class NotificationController : PageController
    {
        public override void Init()
        {
            this.Page.Title = "Notifications";

            var formView = this.AddFormView<Notification>();

            var gridView = this.AddGridView<Notification>()
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

        //public NotificationController()
        //{
        //    this.Page.Title = " Notifications";
        //}
    }
    //public class NotificationController : PageController<NotificationPage>
    //{
    //    public NotificationController()
    //    {
    //        this.Page.Title = "Notifications";
    //    }
    //}
    //public class NotificationPage : PageModel
    //{
    //    public GridView<Notification> GridView { get; set; } = new();
    //    public FormView<Notification> FormView { get; set; } = new();
       
    //}
}

