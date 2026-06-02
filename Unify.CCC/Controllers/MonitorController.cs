
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using Unify.CCC.Data;
using Unify.CCC.Services;
//using Unify.CCC.Data;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;
using Monitor = Unify.CCC.Data.Monitor;
using AppStudio.Framework.Core;
using Microsoft.AspNetCore.Http;


namespace Unify.CCC.Controllers
{

   

     public class MonitorController : PageController
    {
        private FormView<Monitor> formView;

        public override void Init()
        {
            this.Page.Title = "Monitor";

            var formView = this.AddFormView<Monitor>();

            formView.OnInsert += FormView_OnInsert;
            formView.OnUpdate += FormView_OnUpdate;

        

            var gridView = this.AddGridView<Monitor>()
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

       

    //public class MonitorController : PageController<MonitorPage>
    //{

    //}
    //public class MonitorPage : PageModel
    //{
    //    public GridView<Monitor> GridView { get; set; } = new();
    //    public FormView<Monitor> FormView { get; set; } = new();

    //    public MonitorPage()
    //    {
    //        this.Title = "Monitor";
    //        this.FormView.OnInsert += FormView_OnInsert;
    //        this.FormView.OnUpdate += FormView_OnUpdate;
    //        //this.FormView.OnAfterInsert += FormView_OnAfterInsert;
    //        //this.FormView.OnAfterUpdate += FormView_OnAfterUpdate;
    //    }

        private async Task FormView_OnAfterUpdate(Monitor monitor, IFormFileCollection files, string parentId)
        {
            var _cameraServices = AppStudio.Framework.Core.CoreServices.GetService<CameraServices>();
            await _cameraServices.UpdateMonitorAsync(monitor);
        }

        private async Task FormView_OnAfterInsert(Monitor entity, IFormFileCollection files, string parentId)
        {
            string data = string.Empty;
        }

        private async Task<Result> FormView_OnInsert(Monitor monitor, IFormFileCollection files, string parentId)
        {
            try
            {
              var _cameraServices = AppStudio.Framework.Core.CoreServices.GetService<CameraServices>();
                await _cameraServices.SaveMonitorAsync(monitor);
                await _cameraServices.GetCameraConfigurationByIdAsync(monitor.CameraId);
            }
            catch (Exception ex)
            {
 
            }

            return new Result();
        }

        private async Task<Result> FormView_OnUpdate(Monitor monitor, IFormFileCollection files, string parentId)
        {
            try
            {
                var _cameraServices = AppStudio.Framework.Core.CoreServices.GetService<CameraServices>();
                await _cameraServices.UpdateMonitorAsync(monitor);
                await _cameraServices.GetCameraConfigurationByIdAsync(monitor.CameraId);
            }
            catch (Exception ex)
            {

            }

            return new Result();
        }
    }
}
