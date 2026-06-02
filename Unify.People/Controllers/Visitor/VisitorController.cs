using AppStudio.Framework.Core;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;
using DTC.Unify.HttpClientService;
using Pgvector;
using Unify.Data;
using Unify.Data.SQL;

namespace Unify.People.Controllers.Visitors
{
    public class VisitorController : PageController
    {
        private FormView<Visitor> formView;
        private GridView<Visitor> gridView;

        public override void Init()
        {
            this.Page.SetTitle("Visitor List");

            formView = this.AddFormView<Visitor>().SetLayout(FormLayout.Tab);
            formView.OnBeforeDelete += FormView_OnBeforeDelete;

            var VisitorImageFormView = formView.FindControl<FormView<VisitorImage>>() as FormView<VisitorImage>;
            VisitorImageFormView.OnBeforeInsert += VisitorImageFormView_OnBeforeInsert;
            VisitorImageFormView.OnBeforeUpdate += VisitorImageFormView_OnBeforeUpdate;

            gridView = this.AddGridView<Visitor>()
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
                field.AddField(f => f.MobileNo).ColumnWidth = 100;
                field.AddField(f => f.Company).ColumnWidth = 200;
                field.AddField(f => f.Address).ColumnWidth = 100;
                field.AddField(f => f.Email).ColumnWidth = 100;
            });
            gridView.SetDefaultSort("Name", false);
        }

        public override Task ProcessAsync()
        {
            formView.OnBeforeDelete += FormView_OnBeforeDelete;

            return base.ProcessAsync();
        }

        private async Task<Result> FormView_OnBeforeDelete(object id)
        {
            var result = new Result(); // Create a new result

            try
            {
                var peopleDb = CoreServices.GetService<PeopleDb>();

                DateTime fromDate = DateTime.Today.AddDays(-7);
                DateTime toDate = DateTime.Today;

                var getImage = peopleDb.VisitorImages
                    .Where(a =>
                        a.VisitorId == Guid.Parse(id.ToString())
                    )
                    .ToList();
                if (getImage.Any())
                {
                    UiAction.AddJsCode("alert('Please delete all the images before deleting the visitor');");
                    result.AddError("VisitorId", "Cannot delete: Please delete all the images before deleting the visitor");
                    result.IsSuccess = false;
                    return result;
                }

                var relatedLogs = peopleDb.VisitorLogs
                    .Where(a =>
                        a.VisitorId == Guid.Parse(id.ToString())
                    )
                    .ToList();
                if (relatedLogs.Any())
                {
                    UiAction.AddJsCode("alert('This record exists in attendance logs, hence cannot be deleted');");
                    result.AddError("EmployeeId", "Cannot Delete: Record exists for this visitor in visitor logs");
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

        private async Task<Result> VisitorImageFormView_OnBeforeInsert(VisitorImage entity, IFormFileCollection files, string parentId)
        {
            var result = new Result(); // Create a new result

            try
            {
                FaceService faceService = CoreServices.GetService<FaceService>();
                var embeddings = await faceService.GetEmbeddings(files[0]);

                if (embeddings == null)
                {
                    UiAction.AddJsCode("alert('Image does not contain a valid human face. Please upload a clear image of a person.');");
                    result.AddError("VisitorId", "Image does not contain a valid human face. Please upload a clear image of a person.");
                    result.IsSuccess = false;
                    return result;
                }
                else
                {
                    Vector vector = new Vector(embeddings);
                    entity.Embedding = vector;
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

        private async Task<Result> VisitorImageFormView_OnBeforeUpdate(VisitorImage entity, IFormFileCollection files, string parentId)
        {
            var result = new Result(); // Create a new result

            try
            {
                FaceService faceService = CoreServices.GetService<FaceService>();
                var embeddings = await faceService.GetEmbeddings(files[0]);

                if (embeddings == null)
                {
                    UiAction.AddJsCode("alert('Image does not contain a valid human face. Please upload a clear image of a person.');");
                    result.AddError("VisitorId", "Image does not contain a valid human face. Please upload a clear image of a person.");
                    result.IsSuccess = false;
                    return result;
                }
                else
                {
                    Vector vector = new Vector(embeddings);
                    entity.Embedding = vector;
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
