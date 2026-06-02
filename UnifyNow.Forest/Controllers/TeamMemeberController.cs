using AppStudio.Framework.Core;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using UnifyNow.Forest.Data.SQL;

namespace UnifyNow.Forest.Controllers
{
    public class TeamMemeberController : PageController
    {
        public override void Init()
        {
            this.Page.Title = "TeamMembers";

            var formView = this.AddFormView<TeamMember>();

            var gridView = this.AddGridView<TeamMember>()
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

    //public class TeamMemeberPage : PageModel
    //{
    //    public GridView<TeamMember> GridView { get; set; } = new();
    //    public FormView<TeamMember> FormView { get; set; } = new();
    //}
}




