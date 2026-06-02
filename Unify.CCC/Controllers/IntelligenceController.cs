
using Microsoft.AspNetCore.Mvc;
using Unify.CCC.Data;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;


namespace Unify.CCC.Controllers
{
    public class IntelligenceController : PageController
    {
        public override void Init()
        {
            this.Page.Title = "Intelligence";

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
        // public IntelligenceController()
        //{
        //    this.Page.Title = " Intelligence";
        //}
    }

    //public class IntelligenceController : PageController<IntelligencePage>
    //{
    //    public IntelligenceController()
    //    {
    //        this.Page.Title = "Intelligence";
    //    }
    //}
    //public class IntelligencePage : PageModel
    //{
    //    public GridView<Intelligence> GridView { get; set; } = new();
    //    public FormView<Intelligence> FormView { get; set; } = new();
    //}
    
}
