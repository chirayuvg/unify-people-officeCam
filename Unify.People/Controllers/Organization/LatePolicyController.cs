using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;
using Unify.Data.SQL;

namespace Unify.People.Controllers.Organization
{
    public class LatePolicyController : PageController
    {
        private FormView<LatePolicy> formView;

        public override void Init()
        {
            this.Page.SetTitle("Late Policy List");

            formView = this.AddFormView<LatePolicy>();
            formView.OnSubmit += FormView_OnSubmit;

            var gridView = this.AddGridView<LatePolicy>()
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
                field.AddField(f => f.AppliesTo).ColumnWidth = 100;
                field.AddField(f => f.DepartmentId).ColumnWidth = 100;
                field.AddField(f => f.EmployeeId).ColumnWidth = 100;
                field.AddField(f => f.LateLogin).ColumnWidth = 100;
                field.AddField(f => f.MaxLateLogin).ColumnWidth = 100;
                field.AddField(f => f.EarlyLogout).ColumnWidth = 100;
                field.AddField(f => f.MaxEarlyLogout).ColumnWidth = 100;
            });
            gridView.SetDefaultSort("Name", false);

            formView.GetFieldControl(m => m.AppliesTo).SubmitOnDataChange = true; ;
        }

        public override Task ProcessAsync()
        {
            var appliesToValue = formView.Data.AppliesTo;

            var departmentControl = formView.GetFieldControl("DepartmentId");
            var employeeControl = formView.GetFieldControl("EmployeeId");

            if (appliesToValue == AppliesTo.All)
            {
                departmentControl.IsVisible = false;
                employeeControl.IsVisible = false;
            }
            else if (appliesToValue == AppliesTo.Department)
            {
                departmentControl.IsVisible = true;
                employeeControl.IsVisible = false;
            }
            else if (appliesToValue == AppliesTo.Employee)
            {
                departmentControl.IsVisible = false;
                employeeControl.IsVisible = true;
            }
            else
            {
                departmentControl.IsVisible = false;
                employeeControl.IsVisible = false;
            }

            return base.ProcessAsync();
        }

        private async Task FormView_OnSubmit(LatePolicy model, IFormFileCollection files)
        {
            if (model.AppliesTo == 0)
            {
                formView.GetFieldControl(m => m.Department).IsReadOnly = true;
                formView.GetFieldControl(m => m.Employee).IsReadOnly = true;
            }
        }
    }
}
