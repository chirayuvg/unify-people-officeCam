using AppStudio.Framework.Core;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;
using Unify.Data;
using Unify.Data.SQL;

namespace Unify.People.Controllers.Organization
{
    public class ShiftController : PageController
    {
        private FormView<Shift> formView;
        private GridView<Shift> gridView;
        public override void Init()
        {
            this.Page.SetTitle("Shift List");

            formView = this.AddFormView<Shift>();
            formView.OnBeforeInsert += FormView_OnBeforeInsert;
            formView.OnBeforeUpdate += FormView_OnBeforeUpdate;
            formView.OnBeforeDelete += FormView_OnBeforeDelete;

            gridView = this.AddGridView<Shift>()
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
                field.AddField(f => f.StartTime).ColumnWidth = 400;
                field.AddField(f => f.EndTime).ColumnWidth = 100;
            });
            gridView.SetDefaultSort("Name", false);
        }

        public override Task ProcessAsync()
        {
            formView.OnBeforeInsert += FormView_OnBeforeInsert;
            formView.OnBeforeUpdate += FormView_OnBeforeUpdate;
            formView.OnBeforeDelete += FormView_OnBeforeDelete;

            return base.ProcessAsync();
        }

        private async Task<Result> FormView_OnBeforeInsert(Shift entity, IFormFileCollection files, string parentId)
        {
            var result = new Result(); // Create a new result

            try
            {
                //  Check Duplicate
                var peopleDb = CoreServices.GetService<PeopleDb>();
                var shiftName = peopleDb.Shifts
                    .Where(m => m.Name.ToLower() == entity.Name.ToLower())
                    .FirstOrDefault()?.Name ?? "";

                if (entity.Name.ToLower() == shiftName.ToLower())
                {
                    UiAction.AddJsCode("alert('Shift " + entity.Name + " already exists. Cannot insert it again.');");
                    result.AddError("ShiftId", "Cannot Insert: Shift " + entity.Name + " already exists");
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

        private async Task<Result> FormView_OnBeforeUpdate(Shift entity, IFormFileCollection files, string parentId)
        {
            var result = new Result(); // Create a new result

            try
            {
                //  Check Duplicate
                var peopleDb = CoreServices.GetService<PeopleDb>();
                var shiftName = peopleDb.Shifts
                    .Where(m =>
                        m.Name.ToLower() == entity.Name.ToLower()
                        &&
                        m.Id != entity.Id
                    )
                    .FirstOrDefault()?.Name ?? "";

                if (entity.Name.ToLower() == shiftName.ToLower())
                {
                    UiAction.AddJsCode("alert('Shift " + entity.Name + " already exists. Cannot insert it again.');");
                    result.AddError("ShiftId", "Cannot Insert: Shift " + entity.Name + " already exists");
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
                        a.ShiftId == Guid.Parse(id.ToString())
                    )
                    .ToList();

                if (employee.Any())
                {
                    UiAction.AddJsCode("alert('This Shift has existing reference, hence cannot be deleted');");
                    result.AddError("ShiftId", "Cannot delete:" + employee.FirstOrDefault().ShiftId + " reference exists in employees. ");
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
