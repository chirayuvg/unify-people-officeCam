using AppStudio.Framework.Core;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;
using Unify.Data;
using Unify.Data.SQL;

namespace Unify.People.Controllers.Visitors
{
    public class MeetingPlaceController : PageController
    {
        private FormView<MeetingPlace> formView;
        private GridView<MeetingPlace> gridView;

        public override void Init()
        {
            this.Page.SetTitle("Meeting Place List");

            formView = this.AddFormView<MeetingPlace>();
            formView.OnBeforeDelete += FormView_OnBeforeDelete;
            formView.OnBeforeInsert += FormView_OnBeforeInsert;
            formView.OnBeforeUpdate += FormView_OnBeforeUpdate;

            gridView = this.AddGridView<MeetingPlace>()
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
            formView.OnBeforeDelete += FormView_OnBeforeDelete;
            formView.OnBeforeInsert += FormView_OnBeforeInsert;
            formView.OnBeforeUpdate += FormView_OnBeforeUpdate;

            return base.ProcessAsync();
        }

        private async Task<Result> FormView_OnBeforeInsert(MeetingPlace entity, IFormFileCollection files, string parentId)
        {
            var result = new Result(); // Create a new result

            try
            {
                //  Check Duplicate
                var peopleDb = CoreServices.GetService<PeopleDb>();
                var meetingPlaceName = peopleDb.MeetingPlaces
                    .Where(m => m.Name.ToLower() == entity.Name.ToLower())
                    .FirstOrDefault()?.Name ?? "";

                if (entity.Name.ToLower() == meetingPlaceName.ToLower())
                {
                    UiAction.AddJsCode("alert('Meeting Place " + entity.Name + " already exists. Cannot insert it again.');");
                    result.AddError("MeetingPlaceId", "Cannot Insert: Meeting Place " + entity.Name + " already exists");
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

        private async Task<Result> FormView_OnBeforeUpdate(MeetingPlace entity, IFormFileCollection files, string parentId)
        {
            var result = new Result(); // Create a new result

            try
            {
                //  Check Duplicate
                var peopleDb = CoreServices.GetService<PeopleDb>();
                var meetingPlaceName = peopleDb.MeetingPlaces
                    .Where(m =>
                        m.Name.ToLower() == entity.Name.ToLower()
                        &&
                        m.Id != entity.Id
                    )
                    .FirstOrDefault()?.Name ?? "";

                if (entity.Name.ToLower() == meetingPlaceName.ToLower())
                {
                    UiAction.AddJsCode("alert('Meeting Place " + entity.Name + " already exists. Cannot insert it again.');");
                    result.AddError("MeetingPlaceId", "Cannot Insert: Meeting Place " + entity.Name + " already exists");
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
                var app = peopleDb.Appointments
                    .Where(a =>
                        a.MeetingPlaceId == Guid.Parse(id.ToString()) && a.Status != "Closed"

                    )
                    .ToList();

                if (app.Any())
                {
                    UiAction.AddJsCode("alert('This Meeting Place has existing reference, hence cannot be deleted');");
                    result.AddError("MeetingPlaceId", "Cannot delete:" + app.FirstOrDefault().MeetingPlaceId + " reference exists in appointmenta. ");
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
