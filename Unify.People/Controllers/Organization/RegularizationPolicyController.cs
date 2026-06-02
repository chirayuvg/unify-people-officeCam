using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;
using Unify.Data.SQL;

namespace Unify.People.Controllers.Organization
{
    public class RegularizationPolicyController : PageController
    {
        private FormView<RegularizationPolicy> formView;

        public override void Init()
        {
            this.Page.SetTitle("Regularization Policy List");

            formView = this.AddFormView<RegularizationPolicy>();

            var gridView = this.AddGridView<RegularizationPolicy>()
                .SetFormView(formView)
                .ConfigueUI(
                    grid => grid
                        .ColumnAutoWidth(false)
                        .FilterRow(f => f.Visible(false))
                        .HeaderFilter(headerFilter => headerFilter.Visible(false))
                        .RowAlternationEnabled(true)
                        .Scrolling(scrolling => scrolling.ColumnRenderingMode(GridColumnRenderingMode.Virtual))
                        .SearchPanel(s => s.Visible(true).HighlightCaseSensitive(false))
                        .ShowBorders(true)
                        .ShowColumnHeaders(true)
                        .ShowColumnLines(false)
                        .ShowRowLines(false)
                        .WordWrapEnabled(true).Width("100%")
                        .Paging(paging => paging.Enabled(true).PageSize(10))
                        .Pager(pager =>
                        {
                            pager
                                .Visible(true)
                                .DisplayMode(GridPagerDisplayMode.Full)
                                .ShowPageSizeSelector(false)
                                .AllowedPageSizes(new JS("[5]"))
                                .ShowInfo(true)
                                .ShowNavigationButtons(true);
                        })
                );
            gridView.Css.SetHeight("calc(100vh - 200px)");
            gridView.Fields(field =>
            {
                field.ClearFields();

                field.AddField(f => f.Name).ColumnWidth = 100;
                field.AddField(f => f.QuotaPerMonth).ColumnWidth = 100;
            });
            gridView.SetDefaultSort("Name", false);
        }
    }
}
