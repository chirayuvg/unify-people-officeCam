
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using UnifyNow.Forest.Data.SQL;

namespace UnifyNow.Forest.Controllers
{
    public class AlertRulesController : PageController
    {
        public override void Init()
        {
            this.Page.Title = "AlertRules";

            var formView = this.AddFormView<AlertRule>();

            var gridView = this.AddGridView<AlertRule>()
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

    //public class AlertRulePage : PageModel
    //{
    //    public AlertRulePage()
    //    {
    //        this.Title = "Alert Rule";
    //    }

    //    public GridView<AlertRule> GridView { get; set; } = new();

    //    public FormView<AlertRule> FormView { get; set; } = new();
    //}
}