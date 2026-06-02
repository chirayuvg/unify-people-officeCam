using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using Unify.CCC.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;


namespace Unify.CCC.Controllers
{

    public class SegmentController : PageController
    {
        public override void Init()
        {
            this.Page.Title = "Segment";

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
    }
}

    //    public class SegmentController : PageController<SegmentPage>
    //{
    //    public SegmentController()
    //    {
    //        this.Page.Title = "Segment";
    //    }
    //}
    //public class SegmentPage : PageModel
    //{
    //    public GridView<Segment> GridView { get; set; } = new();
    //    public FormView<Segment> FormView { get; set; } = new();
        
    //}

