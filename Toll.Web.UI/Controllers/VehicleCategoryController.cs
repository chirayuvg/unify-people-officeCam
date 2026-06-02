using AppBuilder.Framework.MVC.Controllers;
using AppBuilder.Model;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Toll.Web.UI.Entity;

namespace Toll.Web.UI.Controllers
{
    public class VehicleCategoryController : PageController<VehicleCategoryPage>
    {

    }

    public class VehicleCategoryPage : PageModel
    {
        public VehicleCategoryPage()
        {
            this.Title = "Vehicle Category";
        }
        public GridView<VehicleCategory> GridView { get; set; } = new();
        public FormView<VehicleCategory> FormView { get; set; } = new();
    }
} 
