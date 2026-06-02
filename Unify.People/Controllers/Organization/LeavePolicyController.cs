using AppStudio.Framework.Core;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;
using Unify.Data;
using Unify.Data.SQL;

namespace Unify.People.Controllers.Organization
{
    public class LeavePolicyController : PageController
    {
        private FormView<LeavePolicy> formView;

        public override void Init()
        {
            this.Page.SetTitle("Leave Policy List");

            formView = this.AddFormView<LeavePolicy>();
            formView.GetFieldControl(m => m.SystemGenerated).IsVisible = false;
            formView.OnBeforeDelete += FormView_OnBeforeDelete;
            formView.OnSubmit += FormView_OnSubmit;

            var gridView = this.AddGridView<LeavePolicy>()
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

                field.AddField(f => f.Name).ColumnWidth = 200;
                field.AddField(f => f.QuotaPerYear).ColumnWidth = 200;
                field.AddField(f => f.AllowCarryForward).ColumnWidth = 200;
                field.AddField(f => f.CarryForwardLimit).ColumnWidth = 200;
                //field.AddField(f => f.SystemGenerated).ColumnWidth = 200;
            });
            gridView.SetDefaultSort("Name", false);
        }

        private async Task<Result> FormView_OnBeforeDelete(object id)
        {
            var result = new Result();

            var peopleDb = CoreServices.GetService<PeopleDb>();
            var leavePolicy = peopleDb.LeavePolicies.Where(a => a.Id == Guid.Parse(id.ToString())).SingleOrDefault();

            if (leavePolicy.SystemGenerated)
            {
                UiAction.AddJsCode("alert('This Leave Policy is SYSTEM GENERATED. Can not be deleted.');");
                result.AddError("LeavePolicyId", "Can not Delete: System Generated");
                result.IsSuccess = false;
            }
            else
            {
                result.IsSuccess = true;
            }

            return result;
        }

        public override Task ProcessAsync()
        {
            var SystemGenerated = formView.Data.SystemGenerated;
            formView.GetFieldControl("Name").IsReadOnly = SystemGenerated;

            return base.ProcessAsync();
        }

        private async Task FormView_OnSubmit(LeavePolicy model, IFormFileCollection files)
        {
            var CarryForwardLimitControl = formView.GetFieldControl("CarryForwardLimit");
            CarryForwardLimitControl.IsReadOnly = !model.AllowCarryForward;
        }
    }
}
