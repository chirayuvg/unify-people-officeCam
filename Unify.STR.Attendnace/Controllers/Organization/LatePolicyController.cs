using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;
using Unify.STR.Attendance.Data.SQL;

namespace Unify.STR.Attendance.Controllers.Organization
{
    public class LatePolicyController : PageController
    {
        private FormView<LatePolicy> formView;

        public override void Init()
        {
            this.Page.Title = "Late Policy List";

            formView = this.AddFormView<LatePolicy>();
            formView.OnSubmit += FormView_OnSubmit;

            var gridView = this.AddGridView<LatePolicy>()
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

        private async Task FormView_OnSubmit(LatePolicy model, IFormFileCollection files)
        {
            if (model.AppliesTo == 0)
            {
                //formView.GetFieldControl(m => m.Department).IsReadOnly = true;
                formView.GetFieldControl(m => m.Employee).IsReadOnly = true;
            }
        }
    }
}
