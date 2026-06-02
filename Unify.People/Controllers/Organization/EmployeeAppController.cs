using AppStudio.Framework.Core;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;
using DTC.Unify.HttpClientService;
using Pgvector;
using System.ComponentModel.DataAnnotations;
using Unify.Data.SQL;
using Unify.People.Services;

namespace Unify.People.Controllers.Organization
{
    public class EmployeeAppController : PageController
    {
        private readonly EmployeeService _employeeService;


        public override void Init()
        {
            this.Page.Title = "Employees";

            var formView = this.AddFormView<Employee>().SetLayout(FormLayout.Tab);

            var EmployeeImageFormView = formView.FindControl<FormView<EmployeeImage>>() as FormView<EmployeeImage>;
            EmployeeImageFormView.OnBeforeInsert += EmployeeImageFormView_OnBeforeInsert;
            // EmployeeImageFormView.OnBeforeDelete += EmployeeImageFormView_OnBeforeDelete;

            var gridView = this.AddGridView<Employee>()
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
        }

        private async Task<Result> EmployeeImageFormView_OnBeforeInsert(EmployeeImage entity, IFormFileCollection files, string parentId)
        {

            try
            {
                FaceService faceService = CoreServices.GetService<FaceService>();

                var embeddings = await faceService.GetEmbeddings(files[0]);
                Vector vector = new Vector(embeddings);

                entity.Embedding = vector;

            }
            catch (Exception ex)
            {
                var error = ex.Message;
            }

            return new Result();
        }
    }
}
