using AppBuilder.Framework.MVC.Controllers;
using AppBuilder.Model;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Toll.Web.UI.Entity;

namespace Toll.Web.UI.Controllers
{
    public class ValidateVehicleController : PageController<ValidateVehiclePage>
    {

    }

    public class ValidateVehiclePage : PageModel
    {
        public ValidateVehiclePage()
        {
            this.Title = "Vehicle Validation";
            this.GridView.AllowAdd(false);
            this.FormView.AllowAdd(false);
            this.FormView.AllowEdit(false);
            this.FormView.AllowDelete(false);
        }
        public GridView<ValidateVehicle> GridView { get; set; } = new();
        public FormView<ValidateVehicle> FormView { get; set; } = new();
    }
} 