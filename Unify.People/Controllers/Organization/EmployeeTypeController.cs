using AppStudio.Framework.Core;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;
using Unify.Data;
using Unify.Data.SQL;

namespace Unify.People.Controllers.Organization
{
    public class EmployeeTypeController : PageController
    {
        private FormView<EmployeeType> formView;
        private GridView<EmployeeType> gridView;

        public override void Init()
        {
            this.Page.SetTitle("Employee Type List");

            formView = this.AddFormView<EmployeeType>();
            formView.OnBeforeDelete += FormView_OnBeforeDelete;
            formView.OnBeforeInsert += FormView_OnBeforeInsert;
            formView.OnBeforeUpdate += FormView_OnBeforeUpdate;

            gridView = this.AddGridView<EmployeeType>()
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

        private async Task<Result> FormView_OnBeforeInsert(EmployeeType entity, IFormFileCollection files, string parentId)
        {
            var result = new Result(); // Create a new result

            try
            {
                //  Check Duplicate
                var peopleDb = CoreServices.GetService<PeopleDb>();
                var employeeTypeName = peopleDb.EmployeeTypes
                    .Where(m => m.Name.ToLower() == entity.Name.ToLower())
                    .FirstOrDefault()?.Name ?? "";

                if (entity.Name.ToLower() == employeeTypeName.ToLower())
                {
                    UiAction.AddJsCode("alert('Employee Type " + entity.Name + " already exists. Cannot insert it again.');");
                    result.AddError("EmployeeTypeId", "Cannot Insert: Employee Type " + entity.Name + " already exists");
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

        private async Task<Result> FormView_OnBeforeUpdate(EmployeeType entity, IFormFileCollection files, string parentId)
        {
            var result = new Result(); // Create a new result
            try
            {
                //  Check Duplicate
                var peopleDb = CoreServices.GetService<PeopleDb>();
                var employeeTypeName = peopleDb.EmployeeTypes
                    .Where(m =>
                        m.Name.ToLower() == entity.Name.ToLower()
                        &&
                        m.Id != entity.Id
                    )
                    .FirstOrDefault()?.Name ?? "";

                if (entity.Name.ToLower() == employeeTypeName.ToLower())
                {
                    UiAction.AddJsCode("alert('Employee Type " + entity.Name + " already exists. Cannot insert it again.');");
                    result.AddError("EmployeeTypeId", "Cannot Insert: Employee Type " + entity.Name + " already exists");
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
                        a.EmployeeTypeId == Guid.Parse(id.ToString())
                    )
                    .ToList();

                if (employee.Any())
                {
                    UiAction.AddJsCode("alert('This EmployeeType has existing reference, hence cannot be deleted');");
                    result.AddError("EmployeeTypeId", "Cannot delete:" + employee.FirstOrDefault().EmployeeTypeId + " reference exists in employees. ");
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
