using AppStudio.Framework.Core;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;
using Unify.Data;
using Unify.Data.SQL;

namespace Unify.People.Controllers.Organization
{
    public class DesignationController : PageController
    {
        private FormView<Designation> formView;
        private GridView<Designation> gridView;

        public override void Init()
        {
            this.Page.SetTitle("Designation List");

            formView = this.AddFormView<Designation>();
            formView.OnBeforeDelete += FormView_OnBeforeDelete;
            formView.OnBeforeInsert += FormView_OnBeforeInsert;
            formView.OnBeforeUpdate += FormView_OnBeforeUpdate;

            gridView = this.AddGridView<Designation>()
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
            gridView.SetDefaultSort("Name", false);
        }

        public override Task ProcessAsync()
        {
            formView.OnBeforeInsert += FormView_OnBeforeInsert;
            formView.OnBeforeUpdate += FormView_OnBeforeUpdate;
            formView.OnBeforeDelete += FormView_OnBeforeDelete;

            return base.ProcessAsync();
        }

        private async Task<Result> FormView_OnBeforeInsert(Designation entity, IFormFileCollection files, string parentId)
        {
            var result = new Result(); // Create a new result

            try
            {
                //  Check Duplicate
                var peopleDb = CoreServices.GetService<PeopleDb>();
                var designationName = peopleDb.Designations
                    .Where(m => m.Name.ToLower() == entity.Name.ToLower())
                    .FirstOrDefault()?.Name ?? "";

                if (entity.Name.ToLower() == designationName.ToLower())
                {
                    UiAction.AddJsCode("alert('Designation " + entity.Name + " already exists. Cannot insert it again.');");
                    result.AddError("DesignationId", "Cannot Insert: Designation " + entity.Name + " already exists");
                    result.IsSuccess = false;
                    return result;
                }
            }
            catch (Exception ex)
            {
                UiAction.AddJsCode(ex.Message.ToString());
                result.AddError(ex.Message.ToString());
                result.IsSuccess = false;
                return result;
            }

            result.IsSuccess = true;
            return result;
        }

        private async Task<Result> FormView_OnBeforeUpdate(Designation entity, IFormFileCollection files, string parentId)
        {
            var result = new Result(); // Create a new result

            try
            {
                //  Check Duplicate
                var peopleDb = CoreServices.GetService<PeopleDb>();
                var designationName = peopleDb.Designations
                    .Where(m =>
                        m.Name.ToLower() == entity.Name.ToLower()
                        &&
                        m.Id != entity.Id
                    )
                    .FirstOrDefault()?.Name ?? "";

                if (entity.Name.ToLower() == designationName.ToLower())
                {
                    UiAction.AddJsCode("alert('Designation " + entity.Name + " already exists. Cannot insert it again.');");
                    result.AddError("DesignationId", "Cannot Insert: Designation " + entity.Name + " already exists");
                    result.IsSuccess = false;
                    return result;
                }
            }
            catch (Exception ex)
            {
                UiAction.AddJsCode(ex.Message.ToString());
                result.AddError(ex.Message.ToString());
                result.IsSuccess = false;
                return result;
            }

            result.IsSuccess = true;
            return result;
        }

        private async Task<Result> FormView_OnBeforeDelete(object id)
        {
            var result = new Result(); // Create a new result

            try
            {
                var peopleDb = CoreServices.GetService<PeopleDb>();
                var employee = peopleDb.Employees
                    .Where(a =>
                        a.DesignationId == Guid.Parse(id.ToString())
                    )
                    .ToList();

                if (employee.Any())
                {
                    UiAction.AddJsCode("alert('This Designation has existing reference, hence cannot be deleted');");
                    result.AddError("DesignationId", "Cannot delete:" + employee.FirstOrDefault().DesignationId + " reference exists in employees. ");
                    result.IsSuccess = false;
                    return result;
                }
            }
            catch (Exception ex)
            {
                UiAction.AddJsCode(ex.Message.ToString());
                result.AddError(ex.Message.ToString());
                result.IsSuccess = false;
                return result;
            }

            result.IsSuccess = true;
            return result;
        }
    }
}
