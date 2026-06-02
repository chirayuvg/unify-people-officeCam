using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using UnifyNow.Forest.Data.SQL;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;


namespace UnifyNow.Forest.Controllers
{
    public class MemeberController : PageController
    {
        public override void Init()
        {
            this.Page.Title = "Members";

            var formView = this.AddFormView<Member>();

            var gridView = this.AddGridView<Member>()
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

    //public class MemeberPage : PageModel
    //{
    //    public MemeberPage()
    //    {
    //        this.Title = "Members";
    //    }
    //    public GridView<Member> GridView { get; set; } = new();
    //    public FormView<Member> FormView { get; set; } = new();
    //private void Alertsgrid_OnBeforeRender(DevExtreme.AspNet.Mvc.Builders.DataGridBuilder<object> gridView)
    //    {
    //        gridView.ID(Name)
    //            .ShowColumnLines(false)
    //            .RowAlternationEnabled(true)
    //            .ShowRowLines(false)
    //            .ShowBorders(true)
    //            .HeaderFilter(headerFilter => headerFilter.Visible(false))
    //            .SearchPanel(s => s.Visible(false))
    //                                .FilterRow(filterRow => filterRow
    //    .Visible(false)
    //    .ApplyFilter(GridApplyFilterMode.Auto)
    //)

    //            .ColumnWidth(Mode.Auto)
    //            .WordWrapEnabled(true).Width("100%");
    //    }
    //}
}
